---
name: code-map
description: Generates or regenerates this project's code map/code graph — detects every independently-buildable app/package in the repo first, then picks the right dependency/AST-graph tool per unit's language (graphify where it has coverage, an explicit per-language fallback table where it doesn't, e.g. React/JS-JSX or Flutter/Dart), and writes one separate, unconnected graph per unit. Use when code-map tooling needs setting up for the first time, when graphify comes back empty or low-quality for a language in this repo, or when the repo holds more than one app/package (monorepo, workspace, or just two unrelated folders sharing a root — e.g. a React frontend next to a Flutter backend).
---

## When to use
- Setting up code-map tooling for a project for the first time (`HARNESS.md`/README's "One-time
  setup after installing", step 3 — still optional, not required to draft the first plan).
- `graphify` returns nothing, or clearly wrong output, for a language present in this repo — its
  coverage has real gaps (confirmed: it doesn't parse Dart/Flutter or React/JSX natively as of
  this writing).
- The project folder contains more than one independently-buildable app/package and the code-map
  needs generating or regenerating per unit.
- A plan's research step (`_plan-common.md`'s two-stage retrieval) needs to know which unit's
  code-map is relevant to the files being touched, before reading any graph file in full.

## Step 1 — Detect units first, before picking any tool
One project folder can hold more than one independently-buildable app/package that share nothing
at the code level — a React frontend next to a Flutter backend in the same repo is the case that
motivated this skill. Treat each one as its own **unit** with its own graph. Never merge two
units into one combined graph, and never let one unit's tool run over the whole repo root and
silently swallow the other (most JS/Dart graph tools will happily walk right past a sibling
folder's `node_modules`/`.dart_tool` and produce a nonsense combined graph if pointed at the repo
root instead of the unit root).

Detect units by real markers, not assumption:
- **A workspace manifest that already enumerates packages** — `package.json` `"workspaces"`,
  `pnpm-workspace.yaml`, `lerna.json`, `nx.json`, `turbo.json`, a Cargo `[workspace]` table, a
  Gradle `settings.gradle(.kts)` with `include(...)`, a `melos.yaml` for a Dart/Flutter
  multi-package repo. If one exists, its own package list *is* the unit list — use it verbatim,
  don't re-derive it by scanning directories.
- **No workspace manifest, but clearly separate roots** — any directory (top-level, and one level
  into `apps/*`, `packages/*`, `services/*` if those exist) with its own language-defining
  manifest is its own unit even with zero tooling tying them together: `package.json`,
  `pubspec.yaml`, `go.mod`, `Cargo.toml`, `pyproject.toml`/`setup.py`, `pom.xml`/`build.gradle`, a
  `.csproj`/`.sln`. This is the React-frontend-next-to-Flutter-backend case: two folders, two
  manifests, no monorepo tool at all — still two units.
- **One manifest at the repo root and nothing else** → one unit, the whole repo.

Record each detected unit's name, root path, and language/ecosystem — this list drives Step 2 and
gets written into the index in Step 3.

## Step 2 — Pick a tool per unit, by language/ecosystem
Try `graphify` first for a unit's language — it's this harness's default because it already
covers most backend-style languages in one tool. Fall back to the table below only where
`graphify` has no or poor coverage for that unit. Re-verify coverage before assuming a row below
is still needed — tool coverage changes over time, and a stale "graphify doesn't do X" belief is
itself worth re-checking occasionally rather than trusted forever.

| Ecosystem | Fallback tool(s) | Notes |
|---|---|---|
| React / JS / TS (any framework) | [dependency-cruiser](https://github.com/sverweij/dependency-cruiser) (preferred — validates rules, flags cycles, exports DOT/Mermaid); [madge](https://github.com/pahen/madge) (faster, less configurable) | Point either at the unit's own root/`tsconfig.json` so a workspace unit doesn't pull in sibling packages |
| Flutter / Dart — **this repo** | `dart .github/scripts/code_map.dart` (no deps, ≈2 s, all units at once) | Writes `units.md` + a compact per-unit `map.md` (public types, internal imports, fan-in, `[T]` = has test). Preferred here over the tools below: text an agent can grep, not a DOT graph |
| Flutter / Dart | [lakos](https://pub.dev/packages/lakos) (DOT file dependency graph, most mature CLI); [dart_source_graph](https://pub.dev/packages/dart_source_graph) (Widget/Bloc/Notifier-aware, built for AI-assisted codebase mapping); [dcdg](https://pub.dev/packages/graphx) (UML-style class diagrams) | Prefer `dart_source_graph` when the graph needs to reflect Flutter's widget/state layer, not just raw file imports |
| Python | pyan, pydeps, code2flow | |
| Go | go-callvis, godepgraph | |
| Rust | cargo-modules, cargo-depgraph | |
| Java / Kotlin | `jdeps` (built into the JDK), depgraph-maven-plugin / depgraph-gradle-plugin | |
| C / C++ | Doxygen + Graphviz (call-graph output), cinclude2dot | |
| Anything with no purpose-built tool yet | Don't force a graph — log the gap as a `.agent-memory/gotchas.md` entry and fall back to hand-written notes in `.agent-memory/architecture/` for that unit instead | Keeps research honest: a missing graph is visible, not silently faked |

This table is a starting point, not exhaustive. When research turns up a better-fitting tool for
a language not listed here, use it and add the row — keep it current the same way `gotchas.md`
stays current: whoever hits the gap fixes it for the next session, in this file, not just in that
session's own chat.

## Step 3 — Write output per unit, never merged
Each unit's graph goes under its own subfolder: `.agent-memory/code-map/<unit-slug>/` (slug =
the unit's own package/app name, lowercase-hyphenated — take it from the workspace manifest's own
package name when one exists, otherwise from the unit's directory name). Never write two units'
output into the same file or directory, and never add edges between units — a frontend importing
nothing from a sibling backend's Dart code is not a bug in the graph, it's the correct picture of
two codebases that happen to share a folder.

Also write (or update) `.agent-memory/code-map/units.md` — one row per detected unit: name, root
path, language/ecosystem, tool used, last-generated date. This is the index a plan's research step
reads first (`_plan-common.md`'s two-stage retrieval) to find which subfolder is relevant to the
files being planned, before reading any graph file in full — for a single-unit repo this index is
still worth writing (one row), so research never has to guess whether more units exist.

`.agent-memory/code-map/` is gitignored end-to-end (regenerated, never hand-edited, never
diff-reviewed) — this applies equally to `units.md` and every per-unit subfolder.

## Never
- Never combine two units' dependency graphs into one file or one tool run — a monorepo with a
  React app and a Flutter app produces two separate graphs, never one that merges them or one that
  silently only covers whichever unit the tool defaulted to.
- Never point a unit's tool at the repo root when the repo has more than one unit — scope every
  run to that unit's own root so it can't wander into a sibling package.
- Never assume `graphify` (or any tool in the fallback table) still lacks coverage for a language
  without checking current output first.
- Never treat this skill as a gate — per README/`HARNESS.md`'s "Code-graph tooling is optional"
  note, a missing or stale code-map degrades research quality, it never blocks drafting a plan.
