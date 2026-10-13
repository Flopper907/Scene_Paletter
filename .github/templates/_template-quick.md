---
id: <slug>
type: feature                    # feature | bugfix
status: draft
tier: quick                      # one file, no new abstraction, no security surface — see _plan-common.md "Tier"
areas: []                        # lessons to load: ui | state-arch | data-platform | testing | build-ci
created: YYYY-MM-DD
updated: YYYY-MM-DD
depends_on: []
related_memory: []
---

# <Title>

## Summary
One sentence.

## 1. Problem / Goal
What's wrong or wanted, and the checkable outcome.

## 2. Use Case
- **Given** … **When** … **Then** …

## 3. The change
- **File:** `path` — modify | create · **Lines read:** … · **What and why:** …

## 4. Test
The named check that proves §2.

## 5. Risk
One line: no auth/secrets/input/external data touched (otherwise switch to the full template).

## 6. Critic Log
- Round 1 — YYYY-MM-DD: …
- Gate 1: [ ] approved · [ ] rejected — reason:

## 7. Execution Log
- YYYY-MM-DD HH:MM — …

## 8. Result
- Files: … · Retries/cap hit: … · Lessons written: … · Commit: *(manual)*
