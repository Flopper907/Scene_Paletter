---
name: clean-arch-feature-scaffold
description: The reference architecture for every Flutter app in this repo (derived from notes_app) — 4 layers presentation → application → domain ← infrastructure, get_it composition root, Cubits with sealed Equatable states, ports + adapters, go_router, ARB l10n. Use when creating a new app, scaffolding a new feature/screen/port/cubit in any app, placing a new file, reviewing or refactoring an app's structure, or auditing an app (chat, notes, pdfs, passwords) against the architecture. Overrides the generic clean-architecture / bloc / get_it / go_router skills where they differ.
---

# Clean-architecture app & feature scaffold (material_apps)

The one architecture every app here follows. Derived from `notes_app` (the reference: 153 files, layer rule
enforced by `notes_app/test/architecture_test.dart`). Verified 2026-09-27 — re-check the cited files if this
skill seems out of date, and fix this file, not just your chat answer.

## Precedence over the generic skills
Where `clean-architecture-and-guidelines`, `apply-bloc-cubit-pattern`, `register-get-it-dependencies` or
`generate-go-router-config` say something different, **this skill wins** in this repo:
- **Layer-first folders** (`lib/domain/…`, `lib/application/…`), *not* feature-first (`features/x/`). Features are
  sub-folders *inside* each layer (`application/note/`, `presentation/pages/note/`).
- The adapter layer is called **`infrastructure/`**, not `data/`. Mapping (prefs keys, JSON) lives in the adapter.
- **No use-case classes.** Cubits call ports and pure domain services directly. Add a use case only when two
  cubits would otherwise duplicate the same multi-port orchestration.
- Cubits are **app-lifetime lazy singletons** when they hold app state (one note, settings, AI queue); **screen-
  scoped** (`BlocProvider(create:)`, not registered in get_it) when they hold per-screen/per-item state.

## 1. The layers
```
presentation ─▶ application ─▶ domain ◀─ infrastructure      lib/injection.dart sees all (composition root)
```
| Layer | Holds | May import | Must not import |
|---|---|---|---|
| `domain/` | `entities/`, `value_objects/`, `ports/` (abstract), `services/` (pure functions/classes) | `dart:` (no `dart:io`/`dart:ui`), `equatable`, pure package APIs (`package:llm_kit/llm_kit.dart`) | Flutter, plugins, any other layer |
| `application/` | `<feature>/<x>_cubit.dart` + `<x>_state.dart`, `AppStartup` | domain, `flutter_bloc`, `equatable` | Flutter widgets/foundation, plugins, infrastructure, presentation |
| `infrastructure/` | `repositories/` (prefs, files), `platform/` (plugins, method channels), `engines/` — each `implements` a port | domain, plugins, Flutter | application, presentation |
| `presentation/` | `pages/<feature>/`, `widgets/`, `adaptive/`, `topbar/`, `router/`, `forms/`, `l10n_labels.dart`, `app.dart` | application, domain, Flutter, non-plugin UI packages | infrastructure, plugins, `injection.dart` (except `app.dart`) |
| root | `main.dart`, `injection.dart`, `l10n/` (ARB + generated) | everything | — |

Why: domain + application run in plain `flutter test` without devices and hit the coverage gates
(domain 95 / application 90 / infrastructure 85 / presentation 70 — `notes_app/tool/coverage_gate.py`).

## 2. Building blocks (copy the reference, don't reinvent)
| Block | Rule | Reference in notes_app |
|---|---|---|
| **Port** | `abstract class` in `domain/ports/`, doc says real + test implementation, methods document "must not throw" where relevant | `domain/ports/note_repository.dart`, `speech_input.dart` |
| **Null-object port impl** | Platform without support gets `Unsupported…`/`No…` impl, never `if (Platform…)` in callers | `NoShareInbox`, `UnsupportedHomeWidgetGateway`, `unsupported_fullscreen.dart` |
| **Adapter** | `infrastructure/`, `implements` the port, `const` if stateless, owns keys/format + migration of old keys | `repositories/prefs_settings_repository.dart` |
| **Platform choice** | A `create…()` factory in `infrastructure/platform/` picks the adapter per platform | `platform/speech_factory.dart` |
| **Value object** | `factory X.parse(raw)` normalizes + validates, private ctor, `error` enum (not exception, not text), `isValid`, `Equatable` | `domain/value_objects/section_title.dart`, `ollama_url.dart` |
| **Entity** | Immutable, `const` defaults, `copyWith`, enums for choices; no JSON/prefs code | `domain/entities/app_settings.dart`, `backup.dart` |
| **Domain service** | Pure; time and ids are **injected** (`DateTime Function() now`, `String Function() newId`) so tests are deterministic | `domain/services/backup_service.dart`, `note_sections.dart` |
| **Failure** | `sealed class XFailure` + `final class` per case, each needing a *different fix*; data, not text | `domain/entities/ai_failure.dart` |
| **Failure → text** | Only presentation turns failures/enums into words, via one `L10nLabels` | `presentation/l10n_labels.dart` |
| **State** | `sealed class XState extends Equatable` with `XLoading / XReady / XFailed`; a settings-like state is one Equatable class | `application/note/note_state.dart`; `SettingsCubit extends Cubit<AppSettings>` |
| **Cubit** | Ports via constructor; `try/catch` → `XFailed`; `if (isClosed) return;` after every `await` before `emit`; close owned sub-cubits in `close()` | `application/note/note_cubit.dart` |
| **Startup** | `AppStartup.init()` memoized (`_ready ??= _init()`), loads settings then housekeeping | `application/app/startup.dart` |
| **Composition root** | `configureDependencies({DependencyOverrides overrides})`: every outside-world port has an override field; `dispose: (c) => c.close()` for cubits; `resetDependencies()` for tests | `lib/injection.dart` |
| **App shell** | `App` waits for `AppStartup`, then `MultiRepositoryProvider`/`MultiBlocProvider` with `.value(value: getIt())`; widgets below only use `context.read/watch` | `presentation/app.dart` |
| **Router** | `AppRoutes` string constants; `AppRouter.create(settingsCubit)`, `refreshListenable` on the settings stream; redirect logic is a **pure function** `redirectFor(...)` with its own test; unknown route → home | `presentation/router/` |
| **l10n** | Every user-visible string in `lib/l10n/app_en.arb` + `app_de.arb`; parity is tested | `test/l10n/l10n_parity_test.dart` |

## 3. Scaffolding a feature — do it in this order
1. **Domain first:** entity / value object / failure types; pure service for the logic. Unit tests now.
2. **Port** in `domain/ports/` for every outside thing (storage, plugin, network, clock-like platform API).
3. **Adapter** in `infrastructure/` implementing it (+ `Unsupported…` impl if some platform lacks it).
4. **State + Cubit** in `application/<feature>/` — `blocTest` for primary flow, empty input, failure path.
5. **Register** in `injection.dart`: port with `overrides.x ?? RealX()`, cubit lazy singleton (app state) —
   or none if screen-scoped. Add the override field to `DependencyOverrides`.
6. **Presentation:** page in `pages/<feature>/`, split widgets into `widgets/` before the file nears 300 lines;
   use `AdaptiveScaffold`, `ContentWidth`, `AppTopBar` + `BarAction` from the shell — don't build a new one.
7. **Route** constant in `AppRoutes`, route in `AppRouter`, guard change in `redirectFor` + its test.
8. **Strings** in both ARB files → `flutter gen-l10n`.
9. **Tests:** fakes in `test/support/` (`InMemory…Repository`, `Fake…`), app-level widget test through
   `DependencyOverrides`, update `docs/test-matrix.md` row. Run `test/architecture_test.dart`.

## 4. Hard rules (enforced by `architecture_test.dart` — copy it into every app)
- Layer rule of §1; plugin imports only in `infrastructure/`.
- `injection.dart` imported only by `main.dart`, `injection.dart`, `presentation/app.dart`.
- Files ≤ 300 lines (excluding `generated/`); no `print(`; no `Navigator.push`/`MaterialPageRoute` in presentation (router only).
- Allow-list `_knownViolations` may only shrink. New app: start it empty.
To reuse in another app: copy `notes_app/test/architecture_test.dart`, replace `notes_app` with the package
name, adjust `_notPlugins`/`_pureLibs` (pure-Dart packages like `uuid` go in `_pureLibs` *or* get injected as
a `newId` function — prefer injection).

## 5. New app checklist
- Copy the skeleton: `main.dart`, `injection.dart` (+ `DependencyOverrides`), `presentation/{app.dart, brand.dart,
  theme_mapping.dart, adaptive/, topbar/, router/, pages/{settings,onboarding,error}/, widgets/common/}`,
  `application/app/{settings_cubit, startup, onboarding_*}.dart`, `domain/{entities/app_settings, ports/settings_repository}.dart`,
  `infrastructure/repositories/prefs_settings_repository.dart`. Take them from `notes_app` (most complete) and
  compare with `ai_chat_app` (cleanest). These copies are the known duplication — see §7.
- `analysis_options.yaml`: `very_good_analysis`, `public_member_api_docs: false`, `lines_longer_than_80_chars: false`, `dart format -l 140`.
- `l10n.yaml` + en/de ARB; `test/architecture_test.dart`, `test/l10n/l10n_parity_test.dart`, `tool/verify.sh` + `coverage_gate.py`.
- Path deps: `packages/llm_kit` (LLM), `packages/markdown_editor_interpreter` (markdown) — never copy their code.
- Data that is a *list of records* (vault entries, PDFs, chats) → a real store (files/sqlite) behind the port;
  SharedPreferences only for settings and single values.

## 6. Audit — status of each app (2026-09-27, rules of §4)
| App | Layer violations | Files > 300 | Other |
|---|---|---|---|
| `ai_chat_app` | 0 | 0 | cleanest; no `architecture_test.dart` yet — add it |
| `notes_app` | 0 | 0 | its own architecture test is green (F1 `notes-architecture-green`, 2026-09-27) |
| `pdfs_app` | 11 — `application/app/startup.dart` → infrastructure; `presentation/pages/viewer/viewer_page.dart` → infrastructure; `pdfrx` in presentation (7); `uuid` in `editor_cubit` | 7 (`library_page` 420, `page_edit_layer` 375, `pdfium_editor` 364, …) | 1 `Navigator.push` |
| `passwords_app` | 52 — Flutter in domain (13), `lib/core/` outside the layers, application → infrastructure, plugins in presentation (14) | 7 (hand-written l10n 1347, `validator_service` 340, …) | `injection.dart` used in 12 files; `flutter_lints`; rebuild target |
Re-run: `dart .github/scripts/arch_check.dart <app_dir> <package>` (read-only, seconds) — update this table.

## 7. Findings to refactor in notes_app itself (the reference must pass its own test)
1. **Export as a port — done:** `domain/ports/note_exporter.dart` + `infrastructure/export/{file_share_note_exporter,html_note_renderer,pdf_note_renderer}.dart`,
   called from `presentation/pages/note/note_export.dart`, registered in `injection.dart` with a `noteExporter` override.
2. **Dictation — done:** `heard` is part of `DictationState`; `application/dictation/` is split into `dictation_state.dart`,
   `dictation_target.dart`, `dictation_cubit.dart`.
3. **The 3 big widgets — done:** `app_sidebar` (→ `sidebar_nav_item.dart`, `sidebar_brand_header.dart`), `note_actions` (export moved out),
   `settings_page` (data-driven `_ChapterTile`).
4. **`AppSettings` fan-in 34:** every settings change rebuilds/touches everything that reads it. Keep one entity, but
   widgets must `buildWhen`/`select` the fields they use (as `app.dart` does) — check this in reviews.
5. **Shared shell:** ~50 shell/settings/onboarding files exist in 2–4 apps and have drifted (8 byte-identical).
   Once the audit above is green, extract them into `packages/app_shell` (plan it via `plan-project`).

## Never
- Never put a plugin, `dart:io`, or Flutter import in `domain/`/`application/` "just this once" — add a port.
- Never read `getIt` in a widget; never `new` a service inside a widget (inject it, see finding 1).
- Never return user-facing text from domain/application — return an enum/failure, translate in `L10nLabels`.
- Never grow `_knownViolations`; never raise the 300-line limit instead of splitting.
