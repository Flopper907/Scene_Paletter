---
id: <slug>
type: feature                    # feature | bugfix | project
status: draft                    # draft -> [needs-input] -> critiqued -> approved -> in_progress -> blocked | done | abandoned
tier: full                       # full | quick — see _plan-common.md "Tier"
areas: []                        # lessons to load: ui | state-arch | data-platform | testing | build-ci (general + process always load)
created: YYYY-MM-DD
updated: YYYY-MM-DD
depends_on: []
related_memory: []
---

# <Title>

## Summary
1–3 sentences, incl. tier and why.

## 1. Problem
Current situation; quote the user where it matters.

## 2. Definition of Done
Checkable outcomes.

## 3. Use Cases
### UC1 — <name> (primary)
- **Given** … **When** … **Then** …
### UC2 — <name> (edge)
### UC3 — <name> (failure)

## 4. Research
| File | Lines | What's there | Why it matters |
|---|---|---|---|

**Consulted:** code map / architecture / decisions / lessons files read — or "none relevant".

## 5. Constraints

## 6. Clarifying Questions
| Question | Answer | How resolved |
|---|---|---|

## 7. Options
### A — <name>
Pros / Cons / Effort / Risk
### B — <name>
Pros / Cons / Effort / Risk

**Chosen:** <option> because <§4/§5 evidence>.

## 8. Risk & Security
"None applicable: <why>" or findings against the `security` skill checklist.

## 9. Architecture Fit
Pass / Fail vs `clean-arch-feature-scaffold` and `.agent-memory/architecture/`.

## 10. Steps
### [ ] Step 1 — `path/to/file.dart` (NEW FILE)
- **Change:** … · **Symbols:** … · **Serves:** UC1 · **Verified by:** T1 · **Depends on:** —
- **Notes:** *(filled during implementation)*

## 11. Tests
| # | Test | Type | UC | Step |
|---|---|---|---|---|

## 12. Critic Log
### Round 1 — YYYY-MM-DD
### Gate 1
- [ ] Approved · [ ] Approved, rework steps: — · [ ] Rejected — reason:

## 13. Open Questions
- [ ] … — **Assumption:** …

## 14. Decisions
- YYYY-MM-DD: X over Y because Z

## 15. Execution Log
- YYYY-MM-DD HH:MM — <one line>

## 16. Result
- Files: … · Tests: … · Checkpoint: … · Steps/retries/cap hit: … · Lessons written: … · Commit: *(manual)*
