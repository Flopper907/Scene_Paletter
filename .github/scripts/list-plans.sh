#!/usr/bin/env bash
# Renders every .agent-plans/**/*.md (excluding templates) as a status table, by reading each
# file's YAML frontmatter. Sorted by priority (needs-input/blocked first, done/abandoned last,
# ties broken by `updated` descending) rather than filename order, so batch review sees what
# needs attention first. Also validates `depends_on`: duplicate ids and references to an id that
# doesn't exist among the plans found, then computes a dependency-safe execution order (Kahn's
# algorithm) among plans not yet `done`/`abandoned` — any cycle, whether a plan depending on
# itself, two plans depending on each other, or a longer A->B->C->A chain, shows up the same way:
# as plans that can never reach indegree 0, reported explicitly rather than silently omitted.
# Backs the list-plans skill (PLAN.md CRITIQUE.md item C1).
#
# Deliberately avoids bash 4+ features (associative arrays, `${var,,}`, mapfile) — macOS ships
# bash 3.2 as /bin/bash, and this script should run there without requiring a newer bash on PATH.
set -uo pipefail
cd "$(dirname "$0")/../.."

field() {
  # $1 = file, $2 = frontmatter key -> prints its scalar value, or "-" if absent/empty
  awk -v key="$2" '
    /^---$/ { fm++; next }
    fm == 1 {
      if ($0 ~ "^" key ":") {
        sub("^" key ":[ \t]*", "");
        gsub(/#.*/, "");
        gsub(/^[ \t]+|[ \t]+$/, "");
        print;
        found = 1;
      }
    }
    fm >= 2 { exit }
    END { if (!found) print "-" }
  ' "$1"
}

# Prints each depends_on entry on its own line (possibly zero lines). Handles both inline flow
# style (`depends_on: [a, b]` / `depends_on: []`) and block style (`depends_on:` followed by
# `  - a` / `  - b` lines).
dep_list() {
  awk '
    /^---$/ { fm++; next }
    fm == 2 { exit }
    fm == 1 && $0 ~ /^depends_on:/ {
      line = $0
      sub(/^depends_on:[ \t]*/, "", line)
      gsub(/#.*/, "", line)
      gsub(/^[ \t]+|[ \t]+$/, "", line)
      if (line ~ /^\[.*\]$/) {
        inner = line
        gsub(/^\[/, "", inner); gsub(/\]$/, "", inner)
        n = split(inner, parts, ",")
        for (i = 1; i <= n; i++) {
          v = parts[i]
          gsub(/^[ \t"'\'']+|[ \t"'\'']+$/, "", v)
          if (v != "") print v
        }
        capture = 0
      } else if (line == "") {
        capture = 1
      } else {
        print line
        capture = 0
      }
      next
    }
    fm == 1 && capture {
      if ($0 ~ /^[ \t]*-[ \t]*/) {
        v = $0
        sub(/^[ \t]*-[ \t]*/, "", v)
        gsub(/#.*/, "", v)
        gsub(/^[ \t"'\'']+|[ \t"'\'']+$/, "", v)
        if (v != "") print v
      } else if ($0 !~ /^[ \t]*$/) {
        capture = 0
      }
    }
  ' "$1"
}

status_priority() {
  case "$1" in
    needs-input) echo 0 ;;
    blocked)     echo 1 ;;
    approved)    echo 2 ;;
    in_progress) echo 3 ;;
    critiqued)   echo 4 ;;
    draft)       echo 5 ;;
    done)        echo 6 ;;
    abandoned)   echo 7 ;;
    *)           echo 8 ;;
  esac
}

# --- pass 1: read every plan's frontmatter into parallel indexed arrays (bash-3-safe: no
# associative arrays) --------------------------------------------------------------------------
ids=(); types=(); tiers=(); statuses=(); updateds=(); files=(); prios=()
dep_from=(); dep_to=()
n=0

shopt -s nullglob
for f in .agent-plans/*.md; do
  base="$(basename "$f")"
  case "$base" in
    _*) continue ;;
  esac
  id="$(field "$f" id)"
  ids[$n]="$id"
  types[$n]="$(field "$f" type)"
  tiers[$n]="$(field "$f" tier)"
  status="$(field "$f" status)"
  statuses[$n]="$status"
  updateds[$n]="$(field "$f" updated)"
  files[$n]="$f"
  prios[$n]="$(status_priority "$status")"
  n=$((n + 1))

  while IFS= read -r dep; do
    [ -n "$dep" ] || continue
    dep_from+=("$id")
    dep_to+=("$dep")
  done < <(dep_list "$f")
done

# --- pass 2: sort indices by (priority asc, updated desc) — plain selection sort, fine for the
# handful of plans a project realistically has open at once -------------------------------------
order=()
for ((i = 0; i < n; i++)); do order[$i]=$i; done
for ((i = 0; i < n - 1; i++)); do
  best=$i
  for ((j = i + 1; j < n; j++)); do
    oi=${order[$best]}; oj=${order[$j]}
    if [ "${prios[$oj]}" -lt "${prios[$oi]}" ]; then
      best=$j
    elif [ "${prios[$oj]}" -eq "${prios[$oi]}" ] && [[ "${updateds[$oj]}" > "${updateds[$oi]}" ]]; then
      best=$j
    fi
  done
  if [ "$best" -ne "$i" ]; then
    tmp=${order[$i]}; order[$i]=${order[$best]}; order[$best]=$tmp
  fi
done

printf '%-28s %-8s %-9s %-11s %-11s %s\n' "ID" "TYPE" "TIER" "STATUS" "UPDATED" "FILE"
printf '%-28s %-8s %-9s %-11s %-11s %s\n' "----------------------------" "--------" "---------" "-----------" "-----------" "----"
for ((k = 0; k < n; k++)); do
  i=${order[$k]}
  printf '%-28s %-8s %-9s %-11s %-11s %s\n' "${ids[$i]}" "${types[$i]}" "${tiers[$i]}" "${statuses[$i]}" "${updateds[$i]}" "${files[$i]}"
done

# --- dependency validation ----------------------------------------------------------------------
problems=0

# duplicate ids
for ((i = 0; i < n; i++)); do
  [ "${ids[$i]}" = "-" ] && continue
  for ((j = i + 1; j < n; j++)); do
    if [ "${ids[$i]}" = "${ids[$j]}" ]; then
      echo "DUPLICATE id: '${ids[$i]}' used by both ${files[$i]} and ${files[$j]}"
      problems=$((problems + 1))
    fi
  done
done

# dangling depends_on (references an id that doesn't match any plan found)
for ((k = 0; k < ${#dep_from[@]}; k++)); do
  from="${dep_from[$k]}"; to="${dep_to[$k]}"
  found=0
  for ((i = 0; i < n; i++)); do
    if [ "${ids[$i]}" = "$to" ]; then found=1; break; fi
  done
  if [ "$found" -eq 0 ]; then
    echo "DANGLING depends_on: '$from' depends on '$to', which doesn't match any plan's id"
    problems=$((problems + 1))
  fi
done

# --- execution order (Kahn's algorithm) among plans not yet done/abandoned ---------------------
# A done/abandoned plan's own ordering doesn't matter (nothing left to schedule), but it still
# satisfies anything that depends on it — excluded from the active set, but still makes a
# dependent's prerequisite count toward "satisfied" rather than "pending". Any plan whose
# indegree never reaches 0 is part of a cycle (self-loop, mutual pair, or a longer chain — Kahn's
# algorithm doesn't care which shape it is, it just can't schedule it), so full cycle detection
# falls out of computing the order rather than needing its own separate special-cased checks.
active_ids=(); active_indeg=()
for ((i = 0; i < n; i++)); do
  case "${statuses[$i]}" in
    done|abandoned) continue ;;
  esac
  active_ids[${#active_ids[@]}]="${ids[$i]}"
  active_indeg[${#active_indeg[@]}]=0
done
active_n=${#active_ids[@]}

find_active_index() {
  # $1 = plan id to look up among the active subset. Sets $__found_active_idx to its index, or
  # -1 if not found there (already done/abandoned, so its dependents treat it as satisfied — or
  # a dangling reference, already reported by the check above).
  local __target="$1" __ai
  __found_active_idx=-1
  for ((__ai = 0; __ai < active_n; __ai++)); do
    if [ "${active_ids[$__ai]}" = "$__target" ]; then
      __found_active_idx=$__ai
      return
    fi
  done
}

for ((k = 0; k < ${#dep_from[@]}; k++)); do
  find_active_index "${dep_from[$k]}"; from_idx=$__found_active_idx
  [ "$from_idx" -eq -1 ] && continue
  find_active_index "${dep_to[$k]}"; to_idx=$__found_active_idx
  [ "$to_idx" -eq -1 ] && continue
  active_indeg[$from_idx]=$((active_indeg[$from_idx] + 1))
done

queue=()
for ((i = 0; i < active_n; i++)); do
  [ "${active_indeg[$i]}" -eq 0 ] && queue[${#queue[@]}]=$i
done
exec_order=()
qhead=0
while [ "$qhead" -lt "${#queue[@]}" ]; do
  cur_idx=${queue[$qhead]}; qhead=$((qhead + 1))
  cur_id="${active_ids[$cur_idx]}"
  exec_order[${#exec_order[@]}]="$cur_id"
  for ((k = 0; k < ${#dep_from[@]}; k++)); do
    [ "${dep_to[$k]}" = "$cur_id" ] || continue
    find_active_index "${dep_from[$k]}"; succ_idx=$__found_active_idx
    [ "$succ_idx" -eq -1 ] && continue
    active_indeg[$succ_idx]=$((active_indeg[$succ_idx] - 1))
    [ "${active_indeg[$succ_idx]}" -eq 0 ] && queue[${#queue[@]}]=$succ_idx
  done
done

if [ "${#exec_order[@]}" -lt "$active_n" ]; then
  leftover=()
  for ((i = 0; i < active_n; i++)); do
    stuck=1
    for done_id in "${exec_order[@]}"; do
      [ "$done_id" = "${active_ids[$i]}" ] && stuck=0 && break
    done
    [ "$stuck" -eq 1 ] && leftover[${#leftover[@]}]="${active_ids[$i]}"
  done
  echo "CYCLE: these plans depend on each other (directly or through a chain) and have no valid execution order: ${leftover[*]}"
  problems=$((problems + 1))
fi

if [ "$problems" -eq 0 ]; then
  echo
  echo "Dependency check: OK (duplicate ids, dangling references, and dependency cycles of any"
  echo "length all checked)"
  if [ "$active_n" -gt 0 ]; then
    echo "Suggested execution order (dependency-safe, not-yet-done/abandoned plans only):"
    echo "  ${exec_order[*]}"
  fi
fi
