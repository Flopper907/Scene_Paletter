---
name: plan-project
description: Turns a broad project or product idea into a persisted scouting plan directly in .agent-plans/ (prefixed project-) covering architecture and tech stack, typically spinning off individual plan-feature plans afterward rather than listing file-level implementation steps itself.
---

## When to use
Use for an idea bigger than one feature — a new service, a major restructure, "should we build X at all." Not for a single bounded feature (use `plan-feature`) or a bug (use `plan-bugfix`).

## Output
A single file: `.agent-plans/project-<YYYY-MM-DD>-<slug>.md`, flat in `.agent-plans/` like every other plan, always `tier: full` (there is no quick project plan), `type: project`. §10 Implementation Steps is replaced by a **Feature Breakdown** list — each item becomes its own `plan-feature` plan later, linked via `depends_on`, not implemented directly from this file.

## What's specific to a project plan (shared steps — research discipline, ask-vs-log, critic handoff, Gate 1 outcomes — are in `_plan-common.md`; this file only covers what differs)

1. **Capture the idea and the actual problem it solves** — for whom, why now. Push back here if the "why" is thin; a senior developer asks this before architecture, not after.
2. **Survey existing architecture and decisions** — `.agent-memory/architecture/`, `.agent-memory/decisions/`, and the current codebase structure — so the proposal is grounded in what already exists. This step decides whether this is **modify-existing-architecture** or **greenfield** — say which explicitly, since it changes how much of the existing system the plan must respect.
3. **Propose architecture options** (per `_plan-common.md`'s options rule) — monolith module vs. separate service, sync vs. event-driven, etc. — pros/cons/effort/risk/migration-cost.
4. **Propose tech stack** only where not already fixed by the existing project (check the root instructions file first).
5. **Ask the open strategic questions** — scope, timeline, must-have vs. nice-to-have — before breaking the project into features; getting this wrong wastes the whole breakdown.
6. **Break the project into a feature list** — each entry short (name + one-line goal + rough size + the primary user journey it serves in one Given/When/Then line), ordered by dependency. This §3 is deliberately lighter than a feature plan's full use-case set — that detail (edge cases, failure paths) belongs in each spun-off `plan-feature` plan, written later, one at a time or in batch.
7. **Critic pass** — focused on architecture soundness and scope realism, not code-level edge cases (there's no code yet).
8. **Gate 1.** Once approved, offer to immediately draft `plan-feature` plans for the first N items in the breakdown (batch mode) or stop here and let the human decide when to proceed.

## Never
Everything in `_plan-common.md`'s Never list, plus:
- Never write file-level implementation steps in a project plan — that belongs in the feature plans it spins off.
- Never lock in a tech stack or architecture choice without checking the root instructions file and `.agent-memory/architecture/` first for an existing decision.
