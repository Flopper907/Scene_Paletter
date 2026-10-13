---
name: implement-plan
description: Executes an approved plan from .agent-plans/ step by step — implements, tests, and verifies each step for real, updates the plan file as the single source of truth, records lessons from what failed, and stops at capped retries or material deviations instead of improvising past them. Produces a diff for human review; never commits.
---

## When to use
The user says to implement/continue a plan, or "implement all approved plans" (batch). Only runs on
`approved`, `in_progress`, or an unblocked `needs-input` plan.

**Claude Code:** runs as a dispatched subagent whose only input is the plan path and whose output is
a short summary + the diff — not a replay of every edit. It stops at "diff ready, `in_progress`";
the main session presents Gate 2 (step 16). Copilot CLI: runs inline, step 16 without handoff.

## Before starting
1. **Read fresh from disk:** the plan file (and, if dispatched, *only* the plan file — a gap in it is
   a plan-completeness finding, not a reason to fetch the orchestrator's context), this skill, and
   the lessons: `.agent-memory/lessons/general.md`, `process.md`, plus every file named in the plan's
   `areas:`. Never rely on memory of an earlier read.
2. **Check status.** `draft`/`critiqued` → stop, not through Gate 1. `needs-input` → resolve the open
   question first. `blocked` → read the Execution Log before resuming. `done`/`abandoned` → nothing to do.
3. **Check `depends_on`.** Any dependency not `done` → stop and report it.
4. **Checkpoint.** Create branch `plan/<id>` (or a worktree for risky/parallel work) and log it first
   thing in §15. Git unavailable → stop and report; don't implement without a way back.

## Step loop (every unchecked step, dependency order)
5. **Re-verify the step's assumption** by re-reading its target file. Reality drifted (moved, renamed,
   already done) → stop the step, update §4 and the step, log it, continue only once it makes sense.
6. **Blocking question mid-implementation** → `_plan-common.md` Ask vs. log (dispatched: set
   `needs-input` and return; batch: park and move on).
7. **Make the smallest edit that satisfies the step** — no drive-by refactors or gold-plating; extra
   ideas become §13 entries or follow-up plans. Follow the project skills that match the files.
8. **Read back the real result** from disk.
9. **Verify now:** run the step's test/lint for real.
   - Pass → check the box, one §15 line, next step.
   - Fail → retry with the actual error, max **3 attempts**, one terse §15 line each
     ("attempt 2/3: missing override in DependencyOverrides").
   - Still failing → step `blocked`, stop the loop, roll back to the checkpoint if the tree is
     inconsistent, report the error and what was tried.
10. **Per-step guard:** ~10 attempts on one step (edits, verification runs — not read-only lookups)
    without its checkbox advancing counts as a capped failure: stop and ask.

## After all steps pass
11. **Full test concept** (§11) + the unit's Definition of Done — same 3-attempt cap.
12. **Critic on the diff vs. the plan** — findings tagged Confirmed/Judgment call, appended to §12.
    Fix Confirmed findings through the step loop.
13. **Lessons.** For each of these in this run, write one line per `_plan-common.md` → Lessons format,
    tag `[impl]` (`[fix]` for a bugfix plan), into the matching area file:
    a retry in §15 whose cause could recur · a Confirmed critic finding · a step-5 stale assumption ·
    a Gate 2 rejection (added in step 16). Bugfix plans also add the prevention rule for the root
    cause. One-off typos and clean runs → nothing. List what was written in §16.
14. **Durable decisions** from §14 → `.agent-memory/decisions/` (template `_template.md`); set
    `superseded_by:` on any decision this replaces.
15. **Result** (§16): files, tests, checkpoint, efficiency line, lessons written. Commit left blank.
    Status stays `in_progress` — `done` means Gate 2 approved, not "code works".
16. **Gate 2** (always the main session): show the diff, test results, critic findings; wait.
    - Approved → set `status: done`. Never `git commit`/push here, even if asked to "finish up" —
      committing is the human's separate action unless they explicitly ask for it right then.
    - Rejected → ask what's wrong, fix (re-dispatch if dispatched), re-verify (7–9), write the
      Gate 2 lesson (step 13), re-present. Rejected twice for plan-level reasons → back to Gate 1.

## Resuming
Re-reading the plan (step 1) gives checked steps, checkpoint and why it stopped. Continue at the first
unchecked step; don't ask the human to re-explain.

## Batch ("implement all approved plans")
`depends_on` order, one plan at a time (never parallel — overlapping files). Each plan gets its own
Gate 2 unless the human said "run all without stopping"; even then a capped step stops that plan and
`needs-input` plans are skipped. Finish with every plan's final status together (via `list-plans`).

## Never
- Never implement a plan that isn't `approved`/`in_progress`/unblocked `needs-input`.
- Never trust a "this worked" claim without re-reading the file or test output.
- Never continue past the retry cap or the per-step guard.
- Never change the approach without updating the plan file (step 5).
- Never set `done` before Gate 2, never commit or push, never batch several Gate 2s into one.
