---
name: list-plans
description: Renders every plan in .agent-plans/ as a status table (id, type, tier, status, updated) so batch mode has a way to see everything queued at a glance instead of opening every file.
---

## When to use
Use whenever the user asks what's queued/in-progress/done across plans, before starting a batch run ("implement all approved plans"), or at the end of one to summarize outcomes together (per `implement-plan`'s batch mode step).

## Procedure
1. Run the list-plans script from the repo root — `.github/scripts/list-plans.sh` and
   `.claude/scripts/list-plans.sh` are identical duplicates, use whichever is local to the tool
   you're running in (`.ps1` version of either on Windows).
2. Present its table output directly — don't re-derive the same information by opening each plan file individually; the script already reads every frontmatter block.
3. If the user asks to act on specific plans from the table (e.g. "implement all `approved` ones"), hand off to `implement-plan`'s batch mode with that filtered list — this skill only reports, it never implements or changes plan status itself.

## Never
- Never edit a plan file from this skill — it's read-only reporting.
- Never hand-summarize plan statuses by memory instead of running the script — the script reads the actual current frontmatter, which is the same "filesystem is the only truth" discipline the rest of this harness follows.
