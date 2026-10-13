# Example "Project facts" blocks

Worked examples of `copilot-instructions.md`'s "Project facts" section, filled in for real —
referenced from there so "fill this in" has an actual model to copy instead of just a TODO.
Not itself read automatically by either tool; it's a reference doc, not a skill.

## Example 1 — single-stack web app

```markdown
## Project facts

- **Tech stack:** Next.js 14 (App Router) + TypeScript, Postgres via Prisma, deployed on Vercel.
- **Code style:** ESLint (`airbnb-typescript` base) + Prettier, enforced pre-commit. Functional
  components only, no class components. Server actions preferred over API routes for new code.
- **Definition of Done:** `npm run lint` and `npm test` both pass, and a preview deploy exists
  for anything touching a page or API route.
```

## Example 2 — single-stack service

```markdown
## Project facts

- **Tech stack:** Python 3.12, FastAPI, SQLAlchemy against Postgres, packaged with `uv`.
- **Code style:** `ruff` for lint + format (config in `pyproject.toml`), type hints required on
  every public function, no bare `except:`.
- **Definition of Done:** `ruff check .` and `pytest -q` both pass. New endpoints need an
  OpenAPI-visible docstring.
```

## Example 3 — multi-unit repo (the case `code-map` exists for)

One folder, two independently-buildable apps sharing nothing at the code level. This is exactly
the shape `.github/skills/code-map.md` detects and generates separate graphs for — listing units
explicitly here is what that skill reads before it has to re-derive the same thing by scanning
directories.

```markdown
## Project facts

This repo holds two independent units — no workspace tooling ties them together, they just share
a root folder. Treat them as fully separate: separate code-maps, separate lint/test commands,
separate research when drafting a plan that only touches one of them.

- **Units:**
  - `app/` — React + TypeScript frontend (Vite). Tech stack: React 18, TS, Tailwind. Code style:
    ESLint + Prettier. DoD: `npm run lint && npm test` in `app/`.
  - `server/` — Flutter/Dart backend (yes, Dart on the server, via `dart_frog`). Tech stack: Dart
    3, `dart_frog` for routing. Code style: `dart format`, `dart analyze` clean. DoD:
    `dart test` in `server/`.
- **Definition of Done (repo-wide):** both units' own DoD above, run independently — a change to
  one unit never requires the other unit's tests to pass.
- Once code-map is set up, expect `.agent-memory/code-map/units.md` to list these same two units
  with their own subfolders — if it doesn't match this list, the code-map is stale, regenerate it.
```
