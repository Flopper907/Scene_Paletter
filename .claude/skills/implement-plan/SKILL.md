---
name: implement-plan
description: Executes an approved plan from .agent-plans/ step by step — implements, tests, and verifies each step for real, updates the plan file as the single source of truth, and stops at capped retries or material deviations instead of improvising past them. Produces a diff for human review; never commits.
---

## When to use
Use when the user says to implement/work on/continue a specific plan file, or to "implement all approved plans" (batch mode). Refuses to run against a plan that isn't at `status: approved` or `status: in_progress` (resuming).

## Running this as a dispatched subagent (the default in Claude Code — see `PLAN.md` §4.8)
Implementation is usually the most context-heavy phase of all — many file reads, edits, and test
runs. The orchestrating session should dispatch this whole procedure to a subagent whose **only**
input is the plan file's path (not the planning conversation, not the orchestrator's own context)
and whose only output is a short result summary plus the diff — never the full blow-by-blow of
every edit/retry, which stays in the subagent's disposable context, not the orchestrator's.
This works specifically because the plan file is self-contained by design (§4.2) — nothing about
implementation should require anything the orchestrator remembers that isn't already written
into the plan.

**The one thing that changes when dispatched:** this skill can implement steps 1-14 below and
produce a finished diff entirely on its own, but it **cannot present Gate 2 to the human itself**
— a backgrounded subagent has no direct line to the person, only the orchestrator does. So a
dispatched run stops at "diff ready, status still `in_progress`" and returns that to the
orchestrator, which presents Gate 2 and, on approval, flips the plan's status to `done` itself
(a one-line edit, not worth another full dispatch). See step 16.

GitHub Copilot CLI has no equivalent isolation (per `_plan-common.md`'s note on this) — there,
this procedure runs inline in the same conversation that will also present Gate 2, so step 16
happens without a handoff.

## Before starting

1. **Read the plan file fresh from disk — and, if dispatched, *only* the plan file.** Never rely on what you remember about this plan (or about this skill's own rules) from earlier in the conversation — time may have passed, another session may have edited the plan, or a long session's context may have summarized away specifics like the exact retry cap. The plan file is the single source of truth about its own state; this skill file is the single source of truth about the procedure. Re-reading both, every time, is cheap insurance against drift in either — and if you were dispatched specifically to avoid inheriting the planning conversation's context, don't ask the orchestrator for more background than the plan file itself provides; if the plan is missing something you need, that's a finding about the plan's completeness, not a reason to go fetch the orchestrator's context.
2. **Check status.** `draft`/`critiqued` → stop, this hasn't passed Gate 1 yet; report back and route to the matching `plan-*` skill instead of implementing. `needs-input` → a blocking question is still open (see Batch mode below); resolve it before proceeding. `blocked` → read the Execution Log to see why before deciding whether to resume. `done`/`abandoned` → nothing to do, say so.
3. **Check `depends_on`.** Any listed plan not yet `done` → stop and report which dependency is missing, rather than implementing on top of something that doesn't exist yet.
4. **Create a checkpoint.** Before touching any file, create a git branch (`plan/<id>`) or, for higher-risk/parallel work, an isolated worktree. Record the checkpoint name in the Execution Log immediately — if nothing else survives a crash, this line must.

## Step loop (repeat for every unchecked step, in dependency order)

5. **Re-verify the step's assumption before touching anything.** Re-read the actual file the step targets. If reality has drifted from what Research recorded when the plan was written (file moved, function renamed, section already implemented by someone else) — **stop this step**, update the research and this step's description to match reality, note it in the Execution Log, and only continue once the step again makes sense. Never silently improvise past a stale assumption.
6. **If a genuinely blocking question surfaces mid-implementation** (not anticipated at planning time) — apply `_plan-common.md`'s ask-vs-log rule: running inline/interactive → ask directly; dispatched as a subagent → stop, set this plan's `status: needs-input`, log the question, and return that as your result rather than trying to reach the human yourself; batch/unattended → set `needs-input`, log it, move on to the next independent plan rather than stalling all of them.
7. **Make the change.** Smallest concrete edit that satisfies the step's description — not more (no drive-by refactoring of unrelated code, no gold-plating beyond what the plan says; something else worth doing is a new Open Question or follow-up plan, not scope creep on this step).
8. **Read back the real result.** After the edit, re-read the actual file (or re-hash it) — never trust your own "done" claim without confirming the filesystem agrees.
9. **Verify this step now, not later.** Run this step's linked test/lint immediately, for real (never simulate/assume the outcome).
   - **Pass** → check the box, append a one-line Execution Log entry, move to the next step.
   - **Fail** → feed the actual error back and retry, capped at **3 attempts for this step**. Each retry gets its own log line (terse — "attempt 2/3: fixed unused import, re-ran lint").
   - **Still failing after 3** → mark the step `blocked`, stop the whole loop (don't continue to later steps that may depend on this one), roll back to the last good checkpoint if the failed attempts left the tree inconsistent, and report back the specific error and what was tried.
10. **Per-step loop guard, not a raw global tool-call count.** If a single step's own attempts (research re-checks, edits, verification runs — not every read-only lookup) pass roughly 10 without that step's checkbox advancing, treat it the same as a capped retry failure: stop and ask, don't keep spending turns hoping it resolves. A multi-file research-heavy step legitimately taking many *read* actions before its first edit is not, by itself, a stuck loop — don't count those against the cap.

## After all steps pass

11. **Run the full test concept**, not just the per-step tests — same 3-attempt cap, same stop-and-escalate on failure.
12. **Run the `critic` agent against the actual diff vs. this plan** — does the implementation match what the plan said? Is test coverage actually present in the diff, not just described? Anything sloppy? Record findings, tagged Confirmed/Judgment call per `critic.agent.md`, appended (don't overwrite earlier rounds).
13. **Extract durable decisions.** Anything learned that belongs in long-term memory gets written to `.agent-memory/decisions/` (using `.agent-memory/decisions/_template.md`) or `.agent-memory/gotchas.md` as its own entry. If this decision replaces an earlier one, set that earlier file's `superseded_by:` field — don't leave a dead decision looking current.
14. **Fill in the Result Summary** — files changed, tests added, checkpoint branch name, and the efficiency line (steps, retries used, whether any cap was hit). Leave the commit field blank; committing is never automatic. **Status stays `in_progress` here — not `done` yet.** "Done" means Gate 2 was actually approved, not just that the code works; setting it early would let a plan look finished before a human ever saw the diff.
15. **Return/present the diff** — file path, what changed, test results, any critic findings. If dispatched, this is your final report to the orchestrator, kept to the summary, not a replay of every edit. If running inline, this is what you show the human directly.
16. **Gate 2.** The orchestrator (always — whether it did the work inline or is relaying a dispatched subagent's report) shows the diff to the human and waits for explicit approval.
    - **Approved** → the orchestrator sets `status: done` on the plan file (a small, direct edit — no need to re-dispatch for this). Do not run `git commit` under any circumstance in this skill, even if asked to "just finish it up" — approval and committing are the human's own separate action.
    - **Rejected** → ask what's wrong, fix (re-dispatching this skill with the feedback if that's how it ran the first time), re-verify (steps 7-9), re-present. If the same diff is rejected twice for reasons that trace back to the plan itself rather than an implementation slip, stop revising code and ask whether the plan needs to go back through Gate 1 instead.

## Resuming a plan (session restarted mid-implementation)

Re-reading the plan file and this skill file (step 1) already gives everything needed: which steps are checked, the checkpoint branch name from the Execution Log, and any `blocked`/`needs-input` note explaining why it stopped. Resume the step loop at the first unchecked step — do not ask the human to re-explain the plan, and do not restart planning unless the plan itself is stale (step 5 surfaces this naturally as you go).

## Batch mode ("implement all approved plans")

Process plans in `depends_on` order (a plan cannot start before its dependencies reach `done`). Run the full procedure above per plan, including its own Gate 2. Do not silently chain past Gate 2 into the next plan without the human's approval on the current one, unless the human explicitly said "run all of them without stopping between" — and even then, a per-step cap-out (10) always stops that plan, not the whole batch, and a `needs-input` plan is skipped over (per step 6), not blocking. At the end of a batch run, report every plan's final status together — done, blocked, and needs-input — so nothing parked mid-batch is silently forgotten. In Claude Code, each plan in the batch is naturally its own subagent dispatch, one at a time (not in parallel — parallel dispatches implementing different plans could touch overlapping files with no coordination between them).

## Never
- Never implement against a plan not yet at `approved`/`in_progress`/`needs-input` (resuming one that's unblocked).
- Never trust a "this worked" claim without re-reading the actual file/test output.
- Never continue past a capped retry (step 9) or the per-step loop guard (step 10) — stop and ask.
- Never silently adjust the plan's approach mid-implementation without updating the plan file itself (step 5) — the file must stay the accurate record of what's actually being done.
- Never set `status: done` before Gate 2 is actually approved (step 14/16) — and never commit or push regardless; Gate 2 approval is not the same as permission to commit unless the human says so explicitly in that moment.
- Never batch through multiple plans' Gate 2 approvals as one — each plan's diff is reviewed on its own.
- Never fetch the orchestrator's planning-conversation context to fill a gap in the plan file when dispatched — treat the gap as a plan-completeness finding instead.
