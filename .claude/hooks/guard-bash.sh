#!/usr/bin/env bash
# PreToolUse hook for the Bash tool. Hard-blocks the destructive/irreversible command
# patterns PLAN.md §4.6 says must never run unattended, regardless of what the model asked for.
# This is enforcement, not a suggestion — it runs whether or not the model "meant well".
#
# Contract: reads the tool-call JSON on stdin, exits 2 (with a reason on stderr) to block,
# exits 0 to allow.
set -euo pipefail

source "$(dirname "${BASH_SOURCE[0]}")/_json_field.sh"

input="$(cat)"
command="$(json_field "$input" command)"

if [ -z "$command" ]; then
  exit 0
fi

block() {
  echo "BLOCKED by guard-bash.sh: $1" >&2
  echo "This is a hard guardrail from PLAN.md §4.6 — it does not ask for confirmation, it refuses." >&2
  echo "If this is genuinely intended, run it yourself in a terminal outside this session." >&2
  exit 2
}

# Collapse whitespace runs (including embedded newlines from a multi-line command) AND shell
# word-boundary metacharacters — ( ) ; & | " ' ` — to single spaces, so a word-boundary check
# like "(^| )rm( |$)" can't be dodged by anything that isn't a literal space next to the word:
# `bash -c "rm -rf x"` (quote before rm), `(rm -rf x)` (paren before rm), `true;rm -rf x` (no
# space before rm) all normalize to have a real space there. Deliberately NOT folding < / > —
# the redirect-overwrite check below looks for a literal '>' in $collapsed, which folding would
# destroy. $collapsed keeps original case; $norm additionally case-folds it — used for every
# check below except git-branch -D, where case is NOT cosmetic (-D force-deletes an unmerged
# branch, -d refuses to if it isn't merged — folding case would block the safe form too, so that
# one check runs against $collapsed instead).
collapsed="$(printf '%s' "$command" | tr -s $'();&|"\x27`[:space:]' ' ')"
norm="$(printf '%s' "$collapsed" | tr '[:upper:]' '[:lower:]')"

# Does $1 contain a short flag $2 either alone (-f) or bundled with others (-rf, -irf, -fr)?
# Checking this way (instead of an ordered substring like *" -f"*) means flag order/combination
# can't bypass the check — "-uf" and "-fu" both still contain "f".
has_short_flag() {
  printf '%s' "$1" | grep -qE -- "(^| )-[a-z]*$2[a-z]*( |\$)"
}

# rm -rf / -fr / -Rf / -irf / -r -f / --recursive --force, any order or combination. Note: this
# checks for recursive-flag-anywhere AND force-flag-anywhere in the whole command, not
# necessarily on the same "rm" invocation in a chained command line — a rare false positive
# (blocking something safe) is the deliberately-preferred failure mode here over a false
# negative (missing something destructive); the block message always names the manual escape
# hatch ("run it yourself outside this session").
if printf '%s' "$norm" | grep -qE -- '(^| )rm( |$)'; then
  has_recursive=0
  has_force=0
  if has_short_flag "$norm" r; then has_recursive=1; fi
  if printf '%s' "$norm" | grep -q -- '--recursive'; then has_recursive=1; fi
  if has_short_flag "$norm" f; then has_force=1; fi
  if printf '%s' "$norm" | grep -q -- '--force'; then has_force=1; fi
  if [ "$has_recursive" -eq 1 ] && [ "$has_force" -eq 1 ]; then
    block "rm with recursive + force flags is irreversible bulk deletion"
  fi
fi

# Destructive/irreversible git
if printf '%s' "$norm" | grep -q -- 'git reset' && printf '%s' "$norm" | grep -q -- '--hard'; then
  block "git reset --hard discards uncommitted work irreversibly"
fi
if printf '%s' "$norm" | grep -q -- 'git push'; then
  if printf '%s' "$norm" | grep -q -- '--force' || has_short_flag "$norm" f; then
    block "git push --force/-f can overwrite remote/shared history"
  fi
fi
if printf '%s' "$norm" | grep -q -- 'git clean'; then
  if printf '%s' "$norm" | grep -q -- '--force' || has_short_flag "$norm" f; then
    block "git clean -f permanently deletes untracked files"
  fi
fi
if printf '%s' "$collapsed" | grep -q -- 'git branch' && printf '%s' "$collapsed" | grep -q -- '\-D'; then
  block "git branch -D force-deletes a branch, discarding unmerged commits"
fi
if printf '%s' "$norm" | grep -q -- 'git rm'; then
  if has_short_flag "$norm" r || printf '%s' "$norm" | grep -q -- '--recursive'; then
    block "git rm -r removes a directory from git and the working tree — doesn't need -f to do it"
  fi
fi
if printf '%s' "$norm" | grep -qE -- '(^| )git checkout \.( |$)'; then
  block "git checkout . discards uncommitted changes to tracked files"
fi
if printf '%s' "$norm" | grep -qE -- '(^| )git restore \.( |$)'; then
  block "git restore . discards uncommitted changes to tracked files"
fi

# Self-tampering: same protection guard-edit.sh applies to the Edit/Write tool, extended here to
# the ways a shell command can achieve the same result without going through Edit/Write at all —
# plain rm (no -rf needed for a single file), shell redirection, chmod. Deliberately errs toward
# over-blocking (e.g. an unrelated chmod elsewhere in a chained command that also happens to
# mention one of these paths): tampering with the harness's own enforcement takes effect
# immediately, before any human reviews a diff, unlike everything else this guard is lenient
# about. project-commands.sh (real per-project config, not enforcement) is not included.
#
# Matches the bare protected filenames (word-bounded), not just the full ".claude/hooks/..."
# path — a full-path-only match is defeated by `cd .claude/hooks && rm guard-bash.sh` or
# `rm ../hooks/guard-bash.sh`, since neither produces the full path as one contiguous substring.
# The boundary class excludes "-"/"."/letters/digits (so it doesn't split "guard-bash.sh" at its
# own hyphen) but includes "/" and space, so both the full path and a cd-relative bare filename
# are caught the same way. settings.json is deliberately NOT bare-matched — unlike these hook
# script names, "settings.json" alone is a common filename in unrelated projects/tools, and
# bare-matching it would block legitimate work on a completely different settings.json.
protected_hooks_re='(^|[^a-z0-9_.-])(guard-bash\.sh|guard-edit\.sh|post-edit-verify\.sh|_json_field\.sh)([^a-z0-9_.-]|$)'
protected_settings_re='\.claude/settings\.json'
if printf '%s' "$norm" | grep -qE -- "$protected_hooks_re" || printf '%s' "$norm" | grep -qE -- "$protected_settings_re"; then
  if printf '%s' "$norm" | grep -qE -- '(^| )(rm|mv|chmod)( |$)' || printf '%s' "$collapsed" | grep -q '>'; then
    block "refuses to modify/delete/move/chmod the harness's own enforcement hooks or settings via a shell command"
  fi
fi

exit 0
