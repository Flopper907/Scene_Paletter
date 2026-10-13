# Shared plan-creation steps

Referenced by `plan-feature.md`, `plan-bugfix.md`, and `plan-project.md` — those files cover
what's actually different between the three (research approach, type-specific questions); this
file covers what's identical, so it only has to change in one place instead of three (plus their
`.claude/` mirrors). Not a standalone skill — nothing invokes this by itself.

## Running this as a dispatched subagent (the default in Claude Code — see `PLAN.md` §4.8)
Research is often the most context-heavy part of drafting a plan (many file reads, grep results,
memory lookups) — running it inline in the main conversation fills that session's context with
noise it doesn't need once the plan file is written. In Claude Code, the orchestrating session
should dispatch this whole procedure to a subagent (the Agent/Task tool) whose only output is the
finished plan file plus a short summary — the plan file itself is the handoff artifact back to the
orchestrator, not a transcript of how it was produced. This is exactly why the plan template is
written to be self-contained ("implement from cold, without re-explaining anything") — that
property is what makes handing off to a fresh context safe.

**The one thing that changes when running this way:** a dispatched subagent generally cannot ask
the human a clarifying question directly (no interactive turn — see the Ask vs. log section
below, "dispatched subagent" branch). Everything else in this procedure is identical whether it
runs inline or dispatched.

GitHub Copilot CLI has no equivalent isolation mechanism — its `@agent-name` personas run in the
same conversation context, not an isolated one (per `.DATA/EXAMPLE_HARNESSES/general_purpose_harness/`'s
own documented constraint: "no mechanism for one agent to hand off to another mid-session"). On
that side, this procedure always runs inline, and a long planning session's context growth is a
real, currently-unmitigated cost — see `PLAN.md` §7.

## Choosing a tier (before anything else, `plan-feature`/`plan-bugfix` only)
- Estimate blast radius from what you already know: one file, no new abstraction, no
  auth/secrets/external-data surface, easily reversible → `tier: quick`, use
  `.github/templates/_template-quick.md` (`.claude/templates/_template-quick.md` — identical
  mirror, whichever is local to the tool you're running in).
- Anything else (new files, multiple files, touches security-relevant surface, a non-trivial
  design choice) → `tier: full`, use `.github/templates/_template.md`
  (`.claude/templates/_template.md`).
- State which tier was chosen and why in the plan's Summary — a visible decision, not a silent
  shortcut. If it turns out mid-drafting that a `quick` plan actually needs Solution Options or a
  real Security Assessment, stop and switch it to `full` rather than bolting sections onto the
  quick template.
- `plan-project` always uses the full template — there is no "quick" project plan.

## Research discipline
No placeholders, ever. Every claim in Research & Findings (or, on the quick template, "The
change") is backed by an actual file read performed while drafting — never an assumption about
what "probably" exists. Use the two-stage retrieval from `PLAN.md` §4.4: search
`.agent-memory/` and the code-map for relevant terms first, then read only the top few matches in
full — don't dump the entirety of memory into context on every plan. If this project has more
than one app/package sharing the folder, `.agent-memory/code-map/units.md` (see the `code-map`
skill) says which unit's own subfolder to search — don't read another unit's graph for a change
that doesn't touch it.

## Use cases — write them before options, steps, or tests
Every plan's §3 (full tier) / §3 (quick tier) is a set of concrete Given/When/Then scenarios
extracted from the idea — not a restatement of the Goal, and not invented after the fact to
justify implementation steps already decided. Draft these **before** Solution Options and
Implementation Steps: a use case is what surfaces a genuinely blocking question in the first
place (writing "Given a user who is already rate-limited, When they retry, Then..." is what
makes you realize you don't know if limiting is per-IP or per-account — better to hit that while
writing the use case than after implementation starts). At minimum: one primary flow, and every
edge/failure case a competent engineer would ask about (boundaries, empty/missing input,
concurrent access, a dependency failing). For a bugfix, the primary use case *is* the bug
(current wrong behavior) paired with a second use case stating the fixed behavior for the same
Given/When — the delta between them is what the regression test has to prove.

Every use case must end up traceable to at least one Implementation Step ("serves use case(s)")
and at least one Test Concept entry ("covers use case(s)") — a use case nothing implements or
nothing tests is a sign of an incomplete plan, and an implementation step or test that serves no
use case is scope creep or padding respectively. The critic checks this traceability explicitly
(`critic.agent.md`).

## Ask vs. log (blocking vs. non-blocking ambiguity)
- **Genuinely blocking** (would change which files get touched or how the thing behaves) — ask.
  - **Single-shot / interactive session, running inline (not dispatched):** ask directly (max 1-2
    sharp questions), record the Q&A, then continue.
  - **Dispatched as a subagent** (§ above): don't attempt to ask the human — stop drafting, set
    `status: needs-input`, record the exact question, and return that as your result to the
    orchestrator instead of a finished plan. The orchestrator (which does have a direct line to
    the human) asks the question, gets the answer, and either re-dispatches you with the answer
    included or finishes the one remaining piece itself.
  - **Batch mode** (this plan is one of several being drafted or implemented unattended, dispatched
    or not): do not stall the whole batch waiting for an answer nobody's there to give. Set
    `status: needs-input`, record the exact question, and move on to the next independent plan.
    Surface every `needs-input` plan together at the end of the batch run rather than one at a
    time.
- **Not blocking, just unresolved** — log it in Open Questions with the assumption you're
  proceeding on. Don't stall for this.

## Solution options (full tier only)
At least two, unless there is genuinely only one reasonable approach — say why if so. Each option
needs actual pros/cons/effort/risk grounded in what research actually found, not a token
alternative invented to look thorough — a critic worth trusting checks specifically for this
(see `critic.agent.md`'s pretextual-options check).

## Implementation steps
Every step names one real file path. A new file is marked **(NEW FILE)** explicitly. A step whose
file couldn't be pinned down by research isn't ready — go back to research, don't write "TBD".
Every step names which use case(s) it serves — a step serving none is scope creep.

## Test concept
Written before implementation starts, tied to specific steps *and* to specific use cases (not
independently invented) — "add tests" is not sufficient. The scenarios in a well-written use-case
section already are the edge cases (boundary values, empty/null, concurrency, dependency
failure); the test concept's job is turning each one into a named, executable check, not
re-discovering them from scratch.

## Critic handoff
Hand the drafted plan to the `critic` agent (`.github/agents/critic.agent.md` /
`.claude/agents/critic.md` — identical duplicates, whichever is local to the tool you're running
in). If it raises real
issues, revise the plan file itself (not just your chat response) and re-run the critic — capped
at 2 rounds. After round 2, let the plan through at `status: critiqued` with unresolved findings
stated plainly — never present it as clean when it isn't.

## Gate 1 handoff and its outcomes
Persist the plan, then hand it to the human with: file path, one-line summary, any open
questions. Do not implement in the same turn unless explicitly told to. Record the outcome in the
plan's "Gate 1 outcome" checklist:
- **Approved as-is** → proceed to `implement-plan`.
- **Approved with specific step(s) flagged for rework** → revise only those steps, re-critique
  only those steps (not the whole plan), then proceed.
- **Rejected** → ask what's wrong (an open question, not a leading one), revise, re-critique —
  same 2-round cap as the critic loop above. **Two full rejections in a row** is a signal to stop
  proposing incremental fixes and ask directly whether the underlying approach — not just this
  plan's details — is right at all.

## Never (every plan-creation skill)
- Never leave a research row, option, or implementation step as an unbacked placeholder.
- Never silently resolve blocking ambiguity by guessing.
- Never mark a plan `approved` yourself — only the human does that, at Gate 1.
- Never skip the critic pass regardless of how small the change looks.
- Never reference a skill that hasn't been authored yet as if it will actually run — say
  explicitly what you did instead (e.g. the `security-audit` situation handled inline per
  `.github/templates/_template.md`'s §8 until that skill exists).
