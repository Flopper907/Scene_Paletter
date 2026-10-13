---
name: apply-typography-hierarchy
description: Establishes and applies Material 3 TextTheme scales to ensure semantic text usage across all app screens.
---

# Skill: Apply Typography Hierarchy

## Description
Establishes and applies a consistent, scalable `TextTheme` across the app.

## Instructions
When implementing text in UI components:

1. **Material 3 Typography Roles**:
   - `displayLarge`, `displayMedium`, `displaySmall` (Hero sections, prominent numbers)
   - `headlineLarge`, `headlineMedium`, `headlineSmall` (Screen titles, section headers)
   - `titleLarge`, `titleMedium`, `titleSmall` (Card titles, list item headers)
   - `bodyLarge`, `bodyMedium`, `bodySmall` (Long-form text, standard content)
   - `labelLarge`, `labelMedium`, `labelSmall` (Buttons, captions, tags)

2. **Rules**:
   - Use `Theme.of(context).textTheme.<role>` instead of inline `TextStyle(fontSize: ...)`.
   - Apply local modifications using `.copyWith(...)` sparingly (e.g., changing color or font weight only when necessary).

## Output Requirements
- Centralized `TextTheme` declaration for `ThemeData` and examples of referencing it in widgets.