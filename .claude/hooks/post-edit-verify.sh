#!/usr/bin/env bash
# PostToolUse hook for Edit/Write. Auto-runs this project's lint/test commands, per PLAN.md §4.6.
# Deliberately generic: this harness template has no fixed language/stack, so the actual
# commands live in .claude/hooks/project-commands.sh (gitignored, per-project, not committed
# with the harness template itself). No-ops cleanly until that file exists and defines them —
# see CLAUDE.md / copilot-instructions.md "Project facts" for where to set this up per project.
set -uo pipefail

project_commands="$(dirname "${BASH_SOURCE[0]}")/project-commands.sh"

if [ ! -f "$project_commands" ]; then
  exit 0
fi

# shellcheck disable=SC1090
source "$project_commands"

# Bounds LINT_CMD/TEST_CMD so a hanging command can't stall every single edit forever — this hook
# runs after every Edit/Write, and a stuck lint/test run here has no other loop-guard watching it
# (implement-plan's retry cap counts *attempts*, not a single call that never returns).
# VERIFY_TIMEOUT_SECS is overridable in project-commands.sh; the default is generous enough for a
# real test suite, not just lint. Prefers GNU `timeout`/`gtimeout` when available; falls back to
# a manual background+kill loop since stock macOS's BSD userland ships neither.
VERIFY_TIMEOUT_SECS="${VERIFY_TIMEOUT_SECS:-300}"

run_with_timeout() {
  local cmd="$1"
  if command -v timeout >/dev/null 2>&1; then
    timeout "$VERIFY_TIMEOUT_SECS" bash -c "$cmd"
    return $?
  fi
  if command -v gtimeout >/dev/null 2>&1; then
    gtimeout "$VERIFY_TIMEOUT_SECS" bash -c "$cmd"
    return $?
  fi
  bash -c "$cmd" &
  local pid=$! waited=0
  while kill -0 "$pid" 2>/dev/null; do
    sleep 1
    waited=$((waited + 1))
    if [ "$waited" -ge "$VERIFY_TIMEOUT_SECS" ]; then
      kill -TERM "$pid" 2>/dev/null
      sleep 1
      kill -KILL "$pid" 2>/dev/null
      wait "$pid" 2>/dev/null
      echo "post-edit-verify: command exceeded ${VERIFY_TIMEOUT_SECS}s, killed (install coreutils for 'timeout'/'gtimeout' for cleaner handling)" >&2
      return 124
    fi
  done
  wait "$pid"
}

status=0

if [ -n "${LINT_CMD:-}" ]; then
  echo "--- post-edit-verify: running LINT_CMD ---" >&2
  run_with_timeout "$LINT_CMD" >&2 || status=1
fi

if [ -n "${TEST_CMD:-}" ]; then
  echo "--- post-edit-verify: running TEST_CMD ---" >&2
  run_with_timeout "$TEST_CMD" >&2 || status=1
fi

if [ "$status" -ne 0 ]; then
  echo "post-edit-verify: lint/test failed — see output above. Fix before continuing this step." >&2
  # Non-blocking (exit 0, not 2): this is feedback for the model to act on within its own
  # retry loop (implement-plan step 8), not a hard refusal — a failing test right after an
  # edit is expected mid-implementation, not a security violation.
fi

exit 0
