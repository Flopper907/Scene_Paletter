---
id: <slug>
type: feature | bugfix
status: draft
tier: quick                      # marks this as the lightweight template — see .github/skills/_plan-common.md "Choosing a tier"
created: YYYY-MM-DD
updated: YYYY-MM-DD
depends_on: []
related_memory: []
---

# <Title>

## Summary
One sentence.

## 1. Idea / Problem
What's wrong or wanted, in a sentence or two.

## 2. Goal / Definition of Done
Concrete and checkable, even for a small change.

## 3. Use Case
Even a small change gets one concrete scenario — this is what §4 implements and §5 tests, not a
restated goal.
- **Given** <precondition/state>
- **When** <the action/trigger>
- **Then** <observable expected outcome>

## 4. The change
- **File:** `path/to/file.ts` — modify | create | delete
- **What changes and why:** ...
- **Confirmed by reading the actual file** (not assumed): yes — cite the line(s) that matter.

## 5. Test
The concrete test/check that proves the §3 use case holds, named specifically (not "add a test").

## 6. Risk check
One line: does this touch auth/secrets/input-handling/external data? If yes, this plan should
not be `quick` — stop and switch to the full template (`_template.md`). If no, say so briefly.

## 7. Critic Review Log
Even a `quick` plan gets one critic pass before Gate 1 — just a lighter one, focused on "is this
actually as small/safe as it claims" rather than the full 5-category review.

- Round 1 — <date>: ...

## 8. Open Questions (non-blocking)
- [ ] ... — **Assumption made:** ...

## 9. Execution Log
- YYYY-MM-DD HH:MM — ...

## 10. Result Summary (filled at completion)
- Files changed: ...
- Steps: 1 total, N retries used, cap hit: yes/no
- Commit: *(left blank — always manual)*
