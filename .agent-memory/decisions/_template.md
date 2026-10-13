---
id: <slug>
date: YYYY-MM-DD
source_plan: .agent-plans/<type>-<date>-<slug>.md   # flat filename, no subfolder — matches
                                                      # every plan-creation skill's own naming
superseded_by: null              # set to another decision's id once this one no longer holds —
                                  # never delete a superseded decision file, so history stays honest
---

# <Short decision title>

**Decision:** what was chosen.

**Why:** the actual reasoning — constraints, trade-offs, what was rejected and why. This is the
only part worth keeping; the code itself documents *what*, this file documents *why*.

**Scope:** what this does/doesn't apply to (e.g. "this service only", "applies until multi-region
is needed").
