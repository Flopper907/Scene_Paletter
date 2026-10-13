---
id: <slug>                       # short kebab-case identifier, stable for the life of this plan
type: feature                    # feature | bugfix | project
status: draft                    # draft -> [needs-input] -> critiqued -> approved -> in_progress -> blocked | done | abandoned
tier: full                       # full (this template) | quick (see _template-quick.md) — chosen per .github/skills/_plan-common.md
created: YYYY-MM-DD
updated: YYYY-MM-DD
depends_on: []                   # ids of other plans that must reach `done` first
related_memory: []               # .agent-memory/ files read or written for this plan
---

# <Title>

## Summary
One to three sentences — enough for someone scanning `.agent-plans/` to know what this is without opening it.

## 1. Idea / Problem
What is the current situation? What is wrong, missing, or requested? Quote the user's own words where useful — don't paraphrase away specifics.

## 2. Goal / Definition of Done
Concrete, checkable outcomes. Not "improve performance" — "P95 latency on `/login` drops below 200ms under 50 concurrent users." Not "fix the bug" — "the reproduction case in §4 no longer fails; regression test in §11 passes."

## 3. Use Cases
The bridge between the idea and both implementation and tests — written as concrete scenarios, not
restated goals. Every use case here must be traceable to at least one §10 Implementation Step and
at least one §11 Test Concept entry; a use case nothing implements or nothing tests is a sign the
plan is incomplete, not a nice-to-have left for later.

Each one, in Given/When/Then form:

### UC1 — <short name> (primary flow)
- **Given** <preconditions/state>
- **When** <the actual action/trigger>
- **Then** <observable expected outcome>

### UC2 — <short name> (edge case)
- **Given** ...
- **When** ...
- **Then** ...

### UC3 — <short name> (error/failure path)
- **Given** ...
- **When** ...
- **Then** ...

At minimum: one primary flow, and every edge/failure case that came up during research or that a
competent engineer would ask about (boundary values, empty/missing input, concurrent access,
a dependency failing) — the same rigor §11's Test Concept demands, because these scenarios are
what §11 is written *from*, not independently invented at test-writing time.

For a bugfix plan, UC1 is the bug itself in this form (Given the conditions that trigger it, When
the buggy action happens, Then the *current, wrong* behavior) plus a second use case stating the
*fixed* behavior for the same Given/When — the delta between them is exactly what the regression
test in §11 has to prove.

## 4. Research & Findings
No placeholders. Every row is backed by an actual read performed while drafting this plan, never an assumption about what "probably" exists.

| File | Lines | What's there | Why it matters here |
|---|---|---|---|
| `src/auth/session.ts` | 12-40 | `SessionStore` class, in-memory `Map` | Where the fix/feature must plug in |

**Code-map notes:** callers/callees relevant to this change, pulled from `.agent-memory/code-map/<unit>/` (check `.agent-memory/code-map/units.md` first if this repo has more than one app/package — see the `code-map` skill).

**Memory consulted:** which `.agent-memory/architecture/*.md` and `.agent-memory/decisions/*.md` files were read, and what they said — write "none found relevant" explicitly if that's the honest answer, don't just omit this.

## 5. Constraints
Technical, security, backward-compatibility, time/scope boundaries the solution must respect.

## 6. Clarifying Questions (asked before drafting)
Only genuinely blocking questions belong here — asked via `AskUserQuestion` before the plan's shape was decided. Softer ambiguity goes to §13 instead, as an explicit assumption, not a stalled plan.

| Question | Answer | How resolved |
|---|---|---|
| ... | ... | asked user / inferred from `.agent-memory/...`, confirm if wrong |

## 7. Solution Options
At least two, unless there is genuinely only one reasonable approach — say why if so.

### Option A — <name>
Pros / Cons / Effort / Risk

### Option B — <name>
Pros / Cons / Effort / Risk

**Chosen:** Option ? — reasoning, grounded in §4's findings and §5's constraints, not first-principles preference.

## 8. Risk & Security Assessment
Explicit even when nothing applies — write "None applicable: <reasoning>" rather than leaving this blank. If the change touches auth, input handling, secrets, or external/untrusted data: the dedicated `security-audit` skill doesn't exist yet (see `PLAN.md` §6 Phase 5), so apply this checklist inline (same categories `critic.agent.md` checks against) and say so here — don't reference a skill that isn't real:
- **Injection** — untrusted input reaching a query, shell command, template, or eval without escaping/parameterization.
- **Broken access control** — an operation that should check permission/ownership and doesn't, or checks it in a way that can be bypassed.
- **Hardcoded secrets** — credentials, keys, or tokens in source instead of config/secrets storage.
- **Insecure dependencies** — a new/updated package with known CVEs or from an unverified source.
- **Missing validation at boundaries** — user input, external API responses, or file contents trusted without validation where they cross into this system.

## 9. Architecture Fit Check
Pass / Fail / N/A — against `.agent-memory/architecture/`. A Fail must be resolved, or explicitly accepted with reasoning, before this plan can reach `approved`.

## 10. Implementation Steps
Every step names one real path. A new file is marked **(NEW FILE)** explicitly. A step whose file couldn't be pinned down by research isn't ready yet — that's a sign to go back to §4, not to write "TBD" here.

### [ ] Step 1 — `path/to/file.ts`
- **Action:** modify | create | delete (delete always needs explicit human confirmation at execution time, regardless of plan approval)
- **Symbols touched:** `functionName`, `ClassName`
- **Change:** what changes, and why — specific enough that someone else could make the same edit from this description alone
- **Serves use case(s):** which §3 UC(s) this step exists for — a step serving no use case is scope creep
- **Depends on:** — (or "Step N", if order matters)
- **Verified by:** which test(s) in §11 cover this step
- **Execution notes:** *(left empty at planning time — filled in during implementation, see `implement-plan` skill)*

### [ ] Step 2 — `path/to/other.ts` (NEW FILE)
...

## 11. Test Concept
Written before implementation starts. Every test traces to a §3 use case — name the scenario, not just "add tests."

| Test | Type | Covers use case(s) | Covers step(s) |
|---|---|---|---|
| Session expires exactly at TTL boundary | unit | UC2 | 1 |
| Concurrent requests both refresh the same expiring session | integration | UC3 | 1, 2 |

## 12. Critic Review Log
Append each round — never overwrite an earlier round's entry. Each finding is tagged **Confirmed**
(the critic actually re-read the cited file/ran a real check and it held up) or **Judgment call**
(a plausible concern, not independently verified) — see `.github/agents/critic.agent.md`. Don't
collapse this distinction; a self-review's Judgment calls are opinions, not facts.

### Round 1 — <date>
Findings: ...
Resolution: ...

### Round 2 — <date> (only if Round 1 found real issues)
...

### Gate 1 outcome
- [ ] Approved as-is
- [ ] Approved with step(s) reworked: — (list which; only those steps get re-critiqued, not the whole plan)
- [ ] Rejected, round 1 — reason: ... → plan revised, re-critiqued above
- [ ] Rejected, round 2 — reason: ... → if this happens, the next step is not another quick revision: ask directly whether the underlying approach (not just this plan's details) is right at all

## 13. Open Questions (non-blocking)
Flagged during creation or implementation, not resolved, plan proceeds anyway on a stated assumption.

- [ ] Question — **Assumption made:** ...

## 14. Decisions Log
Durable "why" decisions worth keeping past this plan's lifetime. Copied into `.agent-memory/decisions/<slug>.md` (using `.agent-memory/decisions/_template.md`) when the plan reaches `done` — this section is the source, not a duplicate kept in sync by hand. If this plan's decision replaces an earlier one, set that earlier file's `superseded_by:` field — don't leave a dead decision looking current for the next plan's research step to cite by mistake.

- YYYY-MM-DD: chose X over Y because Z

## 15. Execution Log
Append-only, terse, one line per event, written during implementation. Never raw tool output or pasted stack traces here — a one-line summary, always.

- YYYY-MM-DD HH:MM — checkpoint branch `plan/<id>` created
- YYYY-MM-DD HH:MM — Step 1 done, tests pass
- YYYY-MM-DD HH:MM — Step 2 failed lint (unused import), retry 1/3 — fixed

## 16. Result Summary (filled at completion)
- Files changed: ...
- Tests added: ...
- Checkpoint branch: ...
- Efficiency: N steps total, M retries used across all steps, loop cap hit: yes/no — visibility into whether this run was cheap or a struggle, so that trend is noticeable across plans, not just per-plan correctness
- Commit: *(left blank — committing is always a manual, human action)*
- Follow-ups spun off: `.agent-plans/feature-<new-plan>.md`
