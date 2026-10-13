---
name: apply-shape-system
description: Configures consistent border radii, card shapes, and theme-level shape tokens across Flutter widgets.
---

# Skill: Apply Shape System

## Description
Defines and enforces a uniform corner radius and shape system across all Flutter UI controls.

## Instructions
When styling components, cards, sheets, or dialogs:

1. **Shape Scale**:
   - **Extra Small**: `4.0` (Tooltips, small badges)
   - **Small**: `8.0` (Text fields, chip buttons, small cards)
   - **Medium**: `12.0` / `16.0` (Cards, dialogs, menus)
   - **Large**: `28.0` (Bottom sheets, floating action buttons)
   - **Full / Pill**: `999.0` or `StadiumBorder` (Pill buttons, avatars)

2. **Theme-Level Config**:
   - Define shape tokens globally in `ThemeData` (e.g., `cardTheme`, `dialogTheme`, `inputDecorationTheme`).
   - Use `RoundedRectangleBorder(borderRadius: BorderRadius.circular(...))`.

## Output Requirements
- Code snippets configuring global component shapes within `ThemeData`.