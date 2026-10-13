---
name: lessons-curate
description: Keeps .agent-memory/lessons/*.md short and useful — merges duplicates, deletes lessons whose code no longer exists, and promotes recurring lessons into mechanical checks (lint rules, architecture tests, hooks) or project-skill lines so they stop costing tokens. Use when a lessons file passes 30 entries, after 5 plans reached done since the last run, when a bug matched an existing lesson, or when the user asks to clean up / optimize the error-solution lists.
---

## When to use
- Any `.agent-memory/lessons/<area>.md` has more than 30 entries.
- 5 plans reached `done` since the last run (last run date: `.agent-memory/lessons/general.md`
  bottom line `<!-- curated: YYYY-MM-DD -->`).
- A `plan-bugfix` §4 says a bug matched an existing lesson (the lesson failed → promote it).
- The user asks for it. Claude Code: run as a dispatched subagent; return the summary + diff.

## Goal
A lesson that has to be read every time is the weakest form of "never again". Order of preference:
1. **Enforced by a check** — lint rule (`analysis_options.yaml`), a test (like
   `notes_app/test/architecture_test.dart`), a script check (`arch_check.dart`, `check-mirrors`), or a
   hook (the human adds hooks; this skill proposes them).
2. **A line in a project skill** (`clean-arch-feature-scaffold`, `flutter-ui`, `flutter-testing`,
   `security`, `refactor`, or a harness skill for `process` lessons) — read only when relevant.
3. **A lessons line** — only if neither of the above fits.

## Procedure (per file, starting with the longest)
1. **Read the file fully.** Note each entry's rule, area, source plan.
2. **Stale?** grep the code for every symbol/path the entry names. Nothing found and the rule is about
   that symbol only → delete. Rule is general (e.g. "never run two `flutter test` at once") → keep.
3. **Duplicates / near-duplicates** → merge into one line keeping the sharpest rule and all plan ids.
4. **Wrong area** → move (e.g. a cubit lesson in `ui.md` → `state-arch.md`).
5. **Promote** each entry that is (a) mechanically checkable or (b) a stable convention:
   - Checkable → write the check (edit the lint config / add a test case / extend `arch_check.dart`),
     run it to prove it passes on current code (or lists the real violations), then delete the line.
     A check that needs a hook → write the proposed hook in the summary for the human.
   - Convention → add one line to the matching skill (`.github/` source + mirror + `check-mirrors`),
     then delete the lessons line.
6. **Cap check:** still > 30 → keep the entries that were violated most recently (plan ids), merge the
   rest harder. Never delete a lesson just to hit the cap without merging or promoting it.
7. Update the `<!-- curated: YYYY-MM-DD -->` line in `general.md`.

## Output
Summary for the human (this is Gate 2 — memory and skill changes are reviewed like code):
`<file>: N → M entries · merged X · deleted stale Y · promoted Z (to: <check/skill>)` per file,
plus any proposed hook, then the diff. Nothing is committed.

## Never
- Never delete a lesson without a reason from step 2–6 recorded in the summary.
- Never promote into a check you didn't run; never weaken an existing check to make it pass.
- Never write new lessons here — this skill only compacts what exists.
- Never edit `.claude/hooks/*` or `.claude/settings.json` (guarded) — propose, the human applies.
