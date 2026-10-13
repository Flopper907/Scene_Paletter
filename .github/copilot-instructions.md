# Dev Harness — Workspace Rules

Loaded on every message (Copilot CLI directly, Claude Code via `CLAUDE.md`). Rules only — the why
behind them is in `.github/HARNESS-NOTES.md` (read on demand, never needed to operate).

## Project facts

`material_apps` — a family of small, local-first Material 3 Flutter apps (Android, Windows,
macOS, Linux, iOS, web) plus shared packages. No workspace tool (no melos/pub workspace): each
unit is built and tested on its own; apps pull packages in via `path:` dependencies.

- **Units** (details + per-unit code map: `.agent-memory/code-map/units.md`):
  - `ai_chat_app/` — chat with Ollama / on-device GGUF / Android system model. Uses both packages.
  - `notes_app/` — markdown notes, dictation, local-AI clean-up, Android homescreen widget. Uses both packages. The reference app: most complete tests (`test/architecture_test.dart`, `tool/verify.sh` coverage gates).
  - `passwords_app/` — password vault. **Older structure** (`lib/core/`, `flutter_lints`, no shared layout yet) — TODO.md plans a rebuild onto the shared structure.
  - `pdfs_app/` — PDF library/toolbox (view, merge, split, sign).
  - `packages/llm_kit/` — shared LLM engines + model file library (Flutter plugin).
  - `packages/markdown_editor_interpreter/` — markdown editor + renderer.
  - `packages/app_ui/` — shared Material 3 design system (tokens, theme, components, shell) plus an `example/` gallery; pure Flutter; used by: none yet (F3+).
- **Tech stack:** Flutter 3.47 / Dart ≥3.10, `flutter_bloc` (Cubits + `equatable` states),
  `get_it` (composition root = `lib/injection.dart`, `DependencyOverrides` for tests), `go_router`,
  `shared_preferences` repositories, `flutter gen-l10n` (en + de ARB, generated files checked in).
- **Architecture:** four layers per app, `presentation → application → domain ← infrastructure`;
  `domain/` is pure Dart (entities, value objects, abstract ports, pure services). Enforced by
  `notes_app/test/architecture_test.dart`. See `.agent-memory/architecture/overview.md`.
- **Code style:** `very_good_analysis` (chat/notes/pdfs; `passwords_app` + packages still on
  `flutter_lints`), `dart format -l 140`, `public_member_api_docs` off, comments only for the
  non-obvious "why". User-facing strings go through ARB files (en + de), never hard-coded.
- **Definition of Done** (per touched unit, run inside that unit's folder):
  `flutter analyze` introduces no new issues and `flutter test` passes; for `notes_app` also
  `python3 tool/coverage_gate.py` after `flutter test --coverage`. New behavior has a test
  (cubit → `bloc_test`, widget → `testWidgets`). A change to a package also runs the tests of
  every app that depends on it (see `units.md` path deps) — `dart tool/test_all.dart packages/<pkg>` from the repo root runs
  the package and every dependent unit, sequentially, stopping at the first red.
- **Code map:** regenerate with `dart .github/scripts/code_map.dart` (≈2 s) after adding/moving
  files; output is gitignored.
- Post-edit hook: `.claude/hooks/project-commands.sh` runs `dart analyze` on the edited `.dart`
  file only; tests are not run per edit.

## Session start
1. `.github/scripts/list-plans.sh` (`.ps1` on Windows) — resume `in_progress`/`blocked` plans first
   unless the user asks for something else.
2. Read `.agent-memory/lessons/general.md` and `process.md`. The other lessons files are loaded per
   plan by its `areas:` field (see `_plan-common.md` → Lessons).
3. Check `.agent-memory/architecture/`, `decisions/` (skip `superseded_by`), `code-map/units.md` for
   anything relevant.

## Gates (process discipline — no hook enforces these)
- **Gate 1:** a plan is persisted in `.agent-plans/`, critic-reviewed, and approved by the human
  before any code is written.
- **Gate 2:** the diff is shown to the human before anything is committed. Commit/push only when
  the human says so in that moment — never inferred from "tests pass".
- The only other interruption is a genuinely blocking question (`_plan-common.md` → Ask vs. log).
  Never ask approval for single edits, retries or test runs.

## Skills
| Situation | Skill |
|---|---|
| New feature | `plan-feature` |
| Something doesn't work | `plan-bugfix` |
| Bigger than one feature | `plan-project` |
| Approved plan → build it | `implement-plan` |
| Status of all plans | `list-plans` |
| Code map missing/stale | `code-map` (here: `dart .github/scripts/code_map.dart`) |
| A lessons file > 30 entries, or 5 plans reached `done` since the last run | `lessons-curate` |

Project skills are auto-selected by their `description`: `clean-arch-feature-scaffold`, `flutter-ui`,
`flutter-testing`, `security`, `refactor`. Shared plan steps: `.github/skills/_plan-common.md`.
Plan templates: `.github/templates/_template.md` (full) / `_template-quick.md`.

## Claude Code: run plan-*/implement-plan/lessons-curate as subagents
Dispatch via the Agent tool with only the plan path (or the task) as input; get back a short summary,
not a transcript. A subagent can't ask the human: it returns `status: needs-input` with the question;
the main session asks, re-dispatches, and always presents Gate 2 itself. Copilot CLI: run inline.

## Rules
- Persist first — a plan or decision that exists only in chat does not exist.
- Max 3 retries on the same failure; stop a step after ~10 attempts without its checkbox advancing.
- The filesystem is the truth — re-read after editing, run tests for real, never assume an outcome.
- Checkpoint (git branch/worktree) before implementing. No git → stop and say so.
- `.agent-plans/` and `.agent-memory/` are git-tracked (except `code-map/`, regenerated).
- Document only what code and git can't tell: decisions (why), lessons (what failed → rule),
  cross-file architecture. No summaries of code, no "went well" entries, no restating the plan.

## Adding a skill
Canonical file `.github/skills/<name>.md` (stack packs: `.github/skills/lang/<name>.md`) with a
specific `description` (that is what auto-selects it), copy to `.claude/skills/<name>/SKILL.md`, run
`.github/scripts/check-mirrors.sh`. A fact (stack, style, DoD) goes in Project facts, not a skill.

## Enforcement and mirrors
- Claude Code hooks hard-block destructive git (`reset --hard`, `push -f`, `clean -f`, `branch -D`),
  recursive force-delete, edits to `.env`/secrets/`.git`, and any change to the hooks or
  `.claude/settings.json`. If one of those is really needed, the human does it outside the session.
  Copilot CLI: policy only.
- `.github/` is canonical; `.claude/` holds byte-identical copies of skills, agents, templates,
  examples and scripts. Edit `.github/`, copy, run `.github/scripts/check-mirrors.sh`.
