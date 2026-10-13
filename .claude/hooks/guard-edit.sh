#!/usr/bin/env bash
# PreToolUse hook for Edit/Write. Hard-blocks writes to secrets and internal git files,
# per PLAN.md §4.6. Enforcement, not a prompt-level suggestion.
set -euo pipefail

source "$(dirname "${BASH_SOURCE[0]}")/_json_field.sh"

input="$(cat)"
file_path="$(json_field "$input" file_path)"
if [ -z "$file_path" ]; then
  # NotebookEdit uses "notebook_path" instead of "file_path" — same file-identity concept, just
  # a different key, so it goes through every check below exactly like any other edit target.
  file_path="$(json_field "$input" notebook_path)"
fi

if [ -z "$file_path" ]; then
  exit 0
fi

base="$(basename "$file_path")"

block() {
  echo "BLOCKED by guard-edit.sh: $1 ($file_path)" >&2
  echo "This is a hard guardrail from PLAN.md §4.6 — it does not ask for confirmation, it refuses." >&2
  echo "If this file genuinely needs editing, do it yourself outside this session." >&2
  exit 2
}

case "$base" in
  .env|.env.*) block "refuses to edit secret/env files" ;;
esac

case "$file_path" in
  */.git/*|.git/*)      block "refuses to edit internal git files" ;;
  */secrets/*|secrets/*) block "refuses to edit files under a secrets/ directory" ;;
esac

# Self-tampering: refuse to edit the harness's own enforcement mechanism — the hook scripts and
# the settings.json that wires them in. Tampering here takes effect immediately, before any
# human reviews a diff, unlike every other file this harness touches (which is at least visible
# at Gate 2 before anything ships) — so this is deliberately NOT limited to the same "ask outside
# the session" pattern being merely advisory. project-commands.sh (the actual per-project
# lint/test config, not enforcement) is intentionally excluded — that file is meant to be edited
# as part of normal setup.
case "$base" in
  guard-bash.sh|guard-edit.sh|post-edit-verify.sh|_json_field.sh)
    case "$file_path" in
      */.claude/hooks/*|.claude/hooks/*) block "refuses to edit the harness's own enforcement hooks" ;;
    esac
    ;;
esac
case "$file_path" in
  */.claude/settings.json|.claude/settings.json) block "refuses to edit the harness's own hook wiring" ;;
esac

# Anything else git already ignores (build output, local config, etc.) is a softer signal —
# don't block it outright, but it's worth a heads-up rather than silent editing.
if git -C "$(dirname "$file_path")" rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  if git check-ignore -q "$file_path" 2>/dev/null; then
    echo "NOTE from guard-edit.sh: $file_path is gitignored — editing it won't show up in any diff review at Gate 2." >&2
  fi
fi

exit 0
