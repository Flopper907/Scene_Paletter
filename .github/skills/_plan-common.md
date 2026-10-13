# Shared plan-creation steps

Used by `plan-feature`, `plan-bugfix`, `plan-project` (they cover only what differs). Not a skill
on its own. In Claude Code the whole procedure runs as a dispatched subagent whose output is the
plan file + a short summary; in Copilot CLI it runs inline. Only difference: a subagent can't ask
the human (see Ask vs. log).

## 0. Lessons — load first
Read `.agent-memory/lessons/general.md` and `process.md` before anything else. Once research shows
which areas the plan touches, set `areas:` in the frontmatter and read those files too:
`ui` (widgets, layout, theme, l10n) · `state-arch` (cubits, DI, router, layers) · `data-platform`
(repositories, plugins, native, files, llm_kit, network) · `testing` · `build-ci` (build, CI,
platforms, tooling). A step or option that repeats a loaded lesson is a bug in the plan. Name the
files read under §4 "Consulted". Re-load on every invocation — a chat that plans, then implements,
then fixes a bug loads the matching set each time.

## Tier (`plan-feature`/`plan-bugfix`; `plan-project` is always full)
One file, no new abstraction, no auth/secrets/input/external-data surface, easily reversible →
`tier: quick`, `.github/templates/_template-quick.md`. Anything else → `tier: full`,
`.github/templates/_template.md` (`.claude/templates/` is identical). State the choice in Summary. A
quick plan that turns out to need options or a security assessment is switched to full, not patched.

## Research discipline
No placeholders: every research row is backed by a file read made while drafting. Two-stage
retrieval: search `.agent-memory/` and the unit's `code-map/<unit>/map.md` first (`units.md` says
which unit), then read only the top matches in full. Don't read another unit's map for a change
that doesn't touch it.

## Use cases — before options, steps, tests
Concrete Given/When/Then scenarios, not a restated goal: one primary flow plus every edge/failure
case a competent engineer would ask about (boundaries, empty input, concurrency, a dependency
failing). Writing them is what surfaces blocking questions early. Bugfix: UC1 is the current wrong
behavior, UC2 the fixed behavior for the same Given/When — the regression test proves the delta.
Traceability: every UC → ≥1 step and ≥1 test; a step serving no UC is scope creep, a test serving
none is padding. The critic checks this.

## Ask vs. log
- **Blocking** (changes which files are touched or how it behaves) → ask.
  - Inline, interactive: ask directly (max 1–2 sharp questions), record Q&A in §6, continue.
  - Dispatched subagent: stop, `status: needs-input`, record the question, return it. The main
    session asks and re-dispatches with the answer.
  - Batch: `needs-input`, record, move on to the next independent plan; report all at the end.
- **Not blocking** → §13 Open Questions with the assumption you proceed on.

## Options (full tier)
≥2 real options with pros/cons/effort/risk grounded in research — or say why only one is
reasonable. No straw men (the critic checks for pretextual options).

## Steps
Each step names one real path; new files marked **(NEW FILE)**; each names the UC(s) it serves and
the test that verifies it. A file research couldn't pin down means back to research, not "TBD".
A delete step always needs explicit human confirmation at execution time. Project plans replace
steps with a feature breakdown (see `plan-project`).

## Tests
Written before implementation, each tied to a UC and a step, named as a scenario ("expires exactly
at TTL"), never "add tests". Follow the `flutter-testing` skill for where/how.

## Critic handoff
Hand the plan to the `critic` agent. Real issues → revise the plan file (not just the chat) and
re-run. Cap: 2 rounds for full, 1 for quick. Unresolved after the cap → `status: critiqued` with the
findings stated plainly in §12, never presented as clean.

## Gate 1
Persist, then give the human: path, one-line summary, open questions. Don't implement in the same
turn unless told to. Record the outcome in §12:
- Approved → `implement-plan`.
- Approved with steps flagged → revise and re-critique only those steps.
- Rejected → ask what's wrong (open question), revise, re-critique (same cap). Two rejections in a
  row → ask whether the approach itself is right before revising again.

## Lessons — capture on the planning side
Write a lesson (format below) when planning itself went wrong: a Gate 1 rejection, a **Confirmed**
critic finding about research or scope, or a research assumption that turned out false. Tag `[plan]`,
area usually `process`. Nothing went wrong → write nothing.

**Format** (one line, ≤ 200 chars, into `.agent-memory/lessons/<area>.md`):
`- [plan|impl|fix] <imperative rule> — <what went wrong / why>. (<plan-id>)`
Rule first, generalizable beyond this plan, grounded in the Execution Log or critic log — never
written from memory, never "X went well". Before adding, search the file: extend a similar line
instead of duplicating. A file over 30 lines → run `lessons-curate`.

## Records kept in the plan file
- §14 Decisions: durable "why" decisions; copied to `.agent-memory/decisions/` at `done`.
- §15 Execution Log: append-only, one line per event, no raw tool output or stack traces.
- §16 Result: includes the efficiency line (steps, retries, cap hit) and lessons written.

## Never
- Never leave a research row, option or step as an unbacked placeholder.
- Never silently resolve blocking ambiguity by guessing.
- Never mark a plan `approved` yourself — only the human does, at Gate 1.
- Never skip the critic pass, however small the change.
- Never reference an unwritten agent/skill as if it ran (see `.github/HARNESS-NOTES.md`).
