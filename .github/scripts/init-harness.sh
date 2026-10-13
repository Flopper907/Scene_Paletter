#!/usr/bin/env bash
# Bootstraps this harness (folders, templates, skills, agents, hooks, scripts) into an existing
# project. Safe to run against an "old" project with its own files already in place: never
# overwrites anything that already exists there — it reports what it skipped instead, so nothing
# of the target project's own history/config is silently clobbered.
#
# Usage: .github/scripts/init-harness.sh /path/to/existing/project
set -euo pipefail
shopt -s nullglob   # so an empty dir (e.g. .github/skills/lang/ with no packs yet) globs to
                    # nothing instead of the literal unmatched pattern, which copy_new would
                    # then try (and fail) to cp as a real file

harness_root="$(cd "$(dirname "$0")/../.." && pwd)"
target="${1:-}"

if [ -z "$target" ]; then
  echo "Usage: $0 /path/to/existing/project" >&2
  exit 1
fi
if [ ! -d "$target" ]; then
  echo "Not a directory: $target" >&2
  exit 1
fi
target="$(cd "$target" && pwd)"

created=()
skipped=()

copy_new() {
  # $1 = source (relative to harness_root), $2 = dest (relative to target)
  local src="$harness_root/$1" dst="$target/$2"
  if [ -e "$dst" ]; then
    skipped+=("$2 (already exists)")
    return
  fi
  mkdir -p "$(dirname "$dst")"
  cp "$src" "$dst"
  created+=("$2")
}

mkdir_report() {
  local dst="$target/$1"
  if [ -d "$dst" ]; then
    skipped+=("$1/ (already exists)")
  else
    mkdir -p "$dst"
    created+=("$1/")
  fi
}

echo "Initializing harness in: $target"
echo

# --- folders -----------------------------------------------------------
mkdir_report ".agent-plans"
mkdir_report ".agent-memory/architecture"
mkdir_report ".agent-memory/decisions"
mkdir_report ".agent-memory/code-map"

# --- memory templates ----------------------------------------------------
# Explicit, hardcoded calls — deliberately NOT a "copy everything found under .agent-memory/"
# loop. That was tried and reverted: .agent-memory/ in this harness's own repo is not a pure
# template source, it's a real, working directory that accumulates this project's own real
# content over time (decisions, lessons, architecture notes) — exactly like .agent-plans/ did
# (see git history: a real plan drafted for this repo leaked into every new project through the
# generalized loop before it was caught). .agent-memory/ currently holds nothing but templates and
# empty placeholders, which is *why* the same bug hasn't visibly triggered here yet — not because
# the generalized-loop approach was actually safe. lessons files and the architecture/ placeholder
# are `touch`ed fresh (never copied) specifically so the target always starts genuinely empty,
# regardless of what this repo's own copies may have accumulated by the time this runs.
copy_new ".agent-memory/decisions/_template.md" ".agent-memory/decisions/_template.md"
# Lessons files are generated fresh (header only, never copied) for the same reason.
mkdir_report ".agent-memory/lessons"
for area in general process ui state-arch data-platform testing build-ci; do
  lf="$target/.agent-memory/lessons/$area.md"
  [ -f "$lf" ] || { printf '# Lessons — %s
<!-- - [plan|impl|fix] <imperative rule> — <what went wrong / why>. (<plan-id>) · one line ≤200 chars · max 30 → lessons-curate -->

' "$area" > "$lf"; created+=(".agent-memory/lessons/$area.md"); }
done
[ -f "$target/.agent-memory/architecture/.gitkeep" ] || { touch "$target/.agent-memory/architecture/.gitkeep"; created+=(".agent-memory/architecture/.gitkeep"); }

# --- plan templates (canonical .github + native .claude duplicate) -------
for f in "$harness_root"/.github/templates/*.md; do
  name="$(basename "$f")"
  copy_new ".github/templates/$name" ".github/templates/$name"
done
for f in "$harness_root"/.claude/templates/*.md; do
  name="$(basename "$f")"
  copy_new ".claude/templates/$name" ".claude/templates/$name"
done

# --- usage doc — copied as HARNESS.md, never as README.md, so it can never clobber (or be
# clobbered by) the target project's own README -----------------------------------------------
copy_new "README.md" "HARNESS.md"
copy_new ".github/HARNESS-NOTES.md" ".github/HARNESS-NOTES.md"

# --- workspace rules (Copilot CLI canonical + Claude Code import) -------
copy_new "CLAUDE.md" "CLAUDE.md"
copy_new ".github/copilot-instructions.md" ".github/copilot-instructions.md"

# --- skills (canonical .github + native .claude duplicate) -----------------
for f in "$harness_root"/.github/skills/*.md; do
  name="$(basename "$f")"
  copy_new ".github/skills/$name" ".github/skills/$name"
done
for d in "$harness_root"/.claude/skills/*/; do
  name="$(basename "$d")"
  copy_new ".claude/skills/$name/SKILL.md" ".claude/skills/$name/SKILL.md"
done
for f in "$harness_root"/.claude/skills/*.md; do
  # flat duplicates of shared non-standalone docs (e.g. _plan-common.md) — not nested in a
  # SKILL.md dir since they're not independently invokable skills
  name="$(basename "$f")"
  copy_new ".claude/skills/$name" ".claude/skills/$name"
done
# --- reusable stack/language packs: .github/skills/lang/<name>.md — the Copilot CLI-side source
# for these (the .claude/skills/<name>/SKILL.md mirror is already covered by the loop above,
# since it lives flat under .claude/skills/*/ like any other skill) ---
for f in "$harness_root"/.github/skills/lang/*.md; do
  name="$(basename "$f")"
  copy_new ".github/skills/lang/$name" ".github/skills/lang/$name"
done

# --- example docs (reference material, not skills — flat duplicates like _plan-common.md) ------
for f in "$harness_root"/.github/examples/*.md; do
  name="$(basename "$f")"
  copy_new ".github/examples/$name" ".github/examples/$name"
done
for f in "$harness_root"/.claude/examples/*.md; do
  name="$(basename "$f")"
  copy_new ".claude/examples/$name" ".claude/examples/$name"
done

# --- agents ---------------------------------------------------------------
for f in "$harness_root"/.github/agents/*.agent.md; do
  name="$(basename "$f")"
  copy_new ".github/agents/$name" ".github/agents/$name"
done
for f in "$harness_root"/.claude/agents/*.md; do
  name="$(basename "$f")"
  copy_new ".claude/agents/$name" ".claude/agents/$name"
done

# --- Claude Code enforcement hooks ----------------------------------------
copy_new ".claude/settings.json" ".claude/settings.json"
for f in "$harness_root"/.claude/hooks/*; do
  name="$(basename "$f")"
  copy_new ".claude/hooks/$name" ".claude/hooks/$name"
done
chmod +x "$target"/.claude/hooks/*.sh 2>/dev/null || true

# --- scripts this project can run itself — both canonical (.github/scripts) and the duplicate
# (.claude/scripts), each in both bash and PowerShell form ------------------------------------
for f in "$harness_root"/.github/scripts/*; do
  name="$(basename "$f")"
  copy_new ".github/scripts/$name" ".github/scripts/$name"
done
for f in "$harness_root"/.claude/scripts/*; do
  name="$(basename "$f")"
  copy_new ".claude/scripts/$name" ".claude/scripts/$name"
done
chmod +x "$target"/.github/scripts/*.sh "$target"/.claude/scripts/*.sh 2>/dev/null || true

# --- .gitignore -------------------------------------------------------------
# Current harness default: .agent-plans/ and .agent-memory/ (except code-map/) are git-tracked —
# persisting plans/decisions is the whole point. Only the regeneratable code-map and the
# per-project hook config are excluded. If you actually want plans/memory kept out of git for this
# project, edit .gitignore yourself after running this — this script won't silently make that call.
gitignore="$target/.gitignore"
touch "$gitignore"
add_gitignore_line() {
  grep -qxF "$1" "$gitignore" 2>/dev/null || { echo "$1" >> "$gitignore"; created+=(".gitignore: $1"); }
}
add_gitignore_line ".agent-memory/code-map/"
add_gitignore_line ".claude/hooks/project-commands.sh"

# --- .gitattributes ---------------------------------------------------------
# Narrow, appended (never a whole-file copy_new that could skip entirely if the target already
# has its own .gitattributes) — pins just the harness's own mirrored file types to LF so
# check-mirrors.ps1's byte-identical comparison can't report false drift from a Windows checkout
# with CRLF on one side. Doesn't impose a line-ending policy on the rest of the project.
gitattributes="$target/.gitattributes"
touch "$gitattributes"
add_gitattributes_line() {
  grep -qxF "$1" "$gitattributes" 2>/dev/null || { echo "$1" >> "$gitattributes"; created+=(".gitattributes: $1"); }
}
add_gitattributes_line "*.sh text eol=lf"
add_gitattributes_line "*.ps1 text eol=lf"
add_gitignore_line ".claude/hooks/project-commands.sh"

echo "Created:"
if [ "${#created[@]}" -eq 0 ]; then echo "  (nothing — everything already existed)"; fi
for c in "${created[@]:-}"; do [ -n "$c" ] && echo "  + $c"; done

echo
echo "Skipped (already existed, left untouched):"
if [ "${#skipped[@]}" -eq 0 ]; then echo "  (nothing skipped)"; fi
for s in "${skipped[@]:-}"; do [ -n "$s" ] && echo "  - $s"; done

echo
echo "Next steps — see $target/HARNESS.md for the full guide:"
echo "  1. Fill in 'Project facts' (tech stack / code style / Definition of Done) in"
echo "     $target/.github/copilot-instructions.md."
echo "  2. Copy .claude/hooks/project-commands.sh.example to project-commands.sh and set"
echo "     LINT_CMD/TEST_CMD so post-edit-verify.sh actually runs this project's checks."
echo "  3. Code-map tooling is optional, not required to draft your first plan. When you do set"
echo "     it up, use the code-map skill (.github/skills/code-map.md) — it picks a tool per"
echo "     language and, if this folder holds more than one app/package (e.g. a React frontend"
echo "     next to a Flutter backend), generates a separate graph per unit instead of one merged"
echo "     graph. List each unit in 'Project facts' if there is more than one."
