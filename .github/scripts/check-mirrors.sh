#!/usr/bin/env bash
# Verifies .github/ and .claude/ are complete, byte-identical duplicates of each other's content —
# every skill (including shared non-standalone docs like _plan-common.md, duplicated flat rather
# than nested since they're not independently invokable), every agent, and every script. Claude
# Code should never have to open a .github/ file to know what to do, and vice versa.
#
# Run this after editing any skill, agent, or script file, before considering the edit done.
# Exit 0 = everything matches. Exit 1 = drift found (listed on stdout) or a duplicate is missing.
set -uo pipefail
cd "$(dirname "$0")/../.."
shopt -s nullglob   # an empty .github/skills/lang/ must glob to nothing, not the literal pattern

drift=0

check_pair() {
  # $1 = source path, $2 = mirror/duplicate path, $3 = label for a missing duplicate
  if [ ! -f "$2" ]; then
    echo "MISSING $3: $2 (source: $1)"
    drift=1
    return
  fi
  if ! diff -q "$1" "$2" >/dev/null 2>&1; then
    echo "DRIFT: $1 and $2 differ"
    diff "$1" "$2" || true
    drift=1
  fi
}

# --- skills: named ones get Claude's nested SKILL.md form; shared non-standalone docs (leading
# underscore, e.g. _plan-common.md) get a flat duplicate instead, so they're not accidentally
# discoverable as an invokable skill on the Claude Code side -----------------------------------
for src in .github/skills/*.md; do
  name="$(basename "$src" .md)"
  case "$name" in
    _*) check_pair "$src" ".claude/skills/$name.md" "shared-doc duplicate" ;;
    *)  check_pair "$src" ".claude/skills/$name/SKILL.md" "skill mirror" ;;
  esac
done

# --- lang packs: .github/skills/lang/<name>.md -> .claude/skills/<name>/SKILL.md (flat on the
# Claude side, same as any other named skill — "lang/" is purely a .github-side organizing
# convention, not part of the mirror path) ---
for src in .github/skills/lang/*.md; do
  name="$(basename "$src" .md)"
  check_pair "$src" ".claude/skills/$name/SKILL.md" "lang-pack skill mirror"
done

# --- examples: reference docs, flat duplicates like a shared non-standalone skill doc (not
# invokable, so no nested SKILL.md form) ---
for src in .github/examples/*.md; do
  name="$(basename "$src")"
  check_pair "$src" ".claude/examples/$name" "example duplicate"
done

# --- plan templates: flat duplicates, same as examples/. Live here (not in .agent-plans/) so a
# fresh project's .agent-plans/ only ever holds that project's own real plans, never anything
# copied wholesale from this template repo ---
for src in .github/templates/*.md; do
  name="$(basename "$src")"
  check_pair "$src" ".claude/templates/$name" "template duplicate"
done

# --- agents: full duplicates, no thin pointers — Claude Code's subagent files must be
# self-sufficient on their own -------------------------------------------------------------------
for src in .github/agents/*.agent.md; do
  name="$(basename "$src" .agent.md)"
  check_pair "$src" ".claude/agents/$name.md" "agent duplicate"
done

# --- scripts: bash + PowerShell, both folders --------------------------------------------------
for src in .github/scripts/*; do
  name="$(basename "$src")"
  check_pair "$src" ".claude/scripts/$name" "script duplicate"
done

if [ "$drift" -eq 0 ]; then
  echo "All skills, agents, and scripts are identical between .github/ and .claude/."
fi

exit "$drift"
