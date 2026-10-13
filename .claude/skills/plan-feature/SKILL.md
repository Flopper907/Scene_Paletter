---
name: plan-feature
description: Turns a feature idea into a persisted, fully-researched plan file directly in .agent-plans/ (prefixed feature-) before any code is written. Never writes code directly — produces the plan that implement-plan later executes.
---

## When to use
Use when the user describes a new feature, capability, or enhancement they want built — before writing any code. If the request is actually a bug (something that used to work, or should work, and doesn't), use `plan-bugfix` instead. If it's a broader idea without a clear single feature yet (a new project, a major restructure), use `plan-project` instead.

## Output
A single file: `.agent-plans/feature-<YYYY-MM-DD>-<slug>.md`, using `.github/templates/_template.md` (or `_template-quick.md` — see tier selection below; `.claude/templates/` mirror is identical). All plan files live flat, directly in `.agent-plans/`, distinguished by their `feature-`/`bug-`/`project-` filename prefix — no subfolders. The plan is the deliverable of this skill — do not describe the plan in chat instead of writing it, and do not skip writing it because the feature "seems simple" (that's what the quick tier is for).

## What's specific to a feature plan (shared steps are in `_plan-common.md` — read it, this file doesn't repeat it)

1. **Restate the idea.** One or two sentences, in your own words, before research starts.
2. **Research.** Find every file this feature will plausibly touch (actual path + line range + what's there), read `.agent-memory/architecture/` and `.agent-memory/decisions/` for anything relevant, and check whether this should `depends_on` another not-yet-`done` plan. Follow `_plan-common.md`'s research discipline — no placeholders.
3. **Surface conflicts early.** If the idea conflicts with an existing architecture decision or a `.agent-memory/decisions/` entry, say so now — don't silently plan around a contradiction.
4. **Choose a tier, write use cases, ask/log ambiguity, draft options, write steps and test concept, hand off to critic, hand off to Gate 1** — all per `_plan-common.md`, in that order. Use cases come before options and steps, not after — see `_plan-common.md`'s "Use cases" section for why.

## Never
Everything in `_plan-common.md`'s Never list, plus:
- Never write implementation code while this skill is active — its only output is the plan file.
- Never write a use case that restates the Goal instead of describing an actual scenario.
