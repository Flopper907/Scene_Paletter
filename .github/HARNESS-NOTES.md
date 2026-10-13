# Harness notes — rationale (read on demand, not needed to operate)

Moved out of `copilot-instructions.md` on 2026-09-27 so the always-loaded rules stay short. Nothing
here is a rule; the rules are in `copilot-instructions.md` and the skill files.

## Why the gates are process discipline, not hooks
A `PreToolUse` hook sees one tool call, not the conversation, so it cannot tell "the human just
approved this commit" from "the model decided to commit". A blanket block would break the legitimate
"commit this". Gates 1/2 therefore hold because the model follows the rules — weaker than the
destructive-command blocks, which refuse regardless of context. Don't conflate the two kinds of "never".

## Why plan/implement run as subagents (Claude Code)
Research and implementation are the context-heaviest phases. Running them inline fills the main
session with detail it no longer needs once the plan file exists, and a full context degrades output.
The plan file is self-contained ("implement from cold") precisely so it can be the entire handoff.
Copilot CLI's `@agent` personas share the caller's context, so there everything runs inline and
context growth on long plans is an open cost.

## Why hooks protect themselves
Tampering with the enforcement layer takes effect immediately, before any diff review — so editing,
moving, redirect-overwriting or chmod-ing hook scripts / `settings.json` is hard-blocked like
destructive git. Copilot CLI has no hook equivalent yet; there the same rules are policy-only.

## Why `.github/` and `.claude/` are full duplicates
Each tool must be self-sufficient: Claude Code should never need to open a `.github/` file to know
what its own subagent does, and vice versa. `check-mirrors` keeps them from drifting.

## Why lessons are per area with a phase tag (2026-09-27)
Per-phase files (planning/implementing/bugfix) would duplicate most lessons, and a UI bugfix would load
every bugfix lesson including irrelevant areas. Per area + `[plan|impl|fix]` tag loads only what the
plan touches. The best lesson is one that no longer needs to be read: `lessons-curate` promotes
lessons into lint rules, tests (`architecture_test.dart`-style) or skill lines, then deletes them.

## Why no generic best-practice skills
The model already knows generic practice; a generic "frontend" skill costs tokens on every match and
adds little. Project skills contain only this repo's concrete conventions, file locations, footguns.

## Planned, not yet authored
`security-auditor`, `test-writer`, `summarizer` agents. Until they exist their duties live in the
`security` skill, `implement-plan`, and the `critic` agent — never reference them as if they ran.
