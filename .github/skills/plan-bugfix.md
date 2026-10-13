---
name: plan-bugfix
description: Turns a bug report into a persisted plan file directly in .agent-plans/ (prefixed bug-) that proves root cause before proposing a fix. Never patches code directly — produces the plan that implement-plan later executes.
---

## When to use
Use when something that should work doesn't — an error, wrong output, crash, regression. If there's no existing broken behavior, just a new capability wanted, use `plan-feature` instead.

## Output
A single file: `.agent-plans/bug-<YYYY-MM-DD>-<slug>.md`, using `.github/templates/_template.md` (or `_template-quick.md` — see tier selection below; `.claude/templates/` mirror is identical), `type: bugfix`. Flat in `.agent-plans/`, no subfolder — the `bug-` prefix is what distinguishes it.

## What's specific to a bugfix plan (shared steps are in `_plan-common.md` — read it, this file doesn't repeat it)

1. **Reproduce first, theorize second.** Try to actually reproduce the bug — run the failing scenario, capture the actual error/output — before writing anything. If it can't be reproduced with the information given, that is itself a blocking question (see `_plan-common.md`'s ask-vs-log rule) — do not draft a fix for a bug you couldn't confirm.
2. **Find root cause, not symptom.** Trace the actual data/control flow backward from the point of failure. Record the root cause as its own research row, with the actual file:line where the wrong behavior *originates* — not just where it surfaces.
3. **Ambiguous root cause is also blocking.** If two plausible causes remain after tracing, that's a question to ask (or log as `needs-input` in batch mode), not a coin flip.
4. **Load lessons** for the areas the bug lives in (`_plan-common.md` §0) — a bug that matches an existing lesson means that lesson failed; say so in §4 so `lessons-curate` can promote it into a check.
5. **Check for the same bug elsewhere.** Search for copy-pasted or structurally similar code likely to share the flaw. Note any siblings found even if this plan only fixes the reported instance — log the rest as an Open Question or a spun-off plan.
6. **Write the use cases as a Given/When/Then pair** — the buggy behavior (Given the triggering conditions, When the action happens, Then the *current, wrong* outcome) and the fixed behavior for the same Given/When. This pair, not a separate "test concept" invented afterward, is what the regression test proves — the test concept must include a test that fails against the current (buggy) code and would pass once fixed.
7. **Choose a tier, draft options only if there's real design freedom (e.g. fix-at-source vs. guard-at-every-call-site), write steps, hand off to critic** (specifically asked here: does the fix address the root cause or just the symptom? can the regression test actually fail pre-fix?) **and Gate 1** — all per `_plan-common.md`.

## Never
Everything in `_plan-common.md`'s Never list, plus:
- Never propose a fix for a root cause that wasn't confirmed by actual reproduction/tracing.
- Never skip checking for the same bug pattern elsewhere in the codebase.
- Never write "add a test" without naming the specific pre-fix-failing scenario.
