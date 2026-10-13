---
name: component-abstraction-check
description: Identifies bloated widget build methods and decomposes them into modular, const-optimized StatelessWidget components.
---

# Skill: Component Abstraction Check

## Description
Analyzes large `build()` methods and refactors them into small, modular, performant `StatelessWidget` classes.

## Instructions
When writing or reviewing Flutter widget code:

1. **Refactoring Rules**:
   - If a `build()` method exceeds **80 lines**, decompose it into smaller private or dedicated public widgets.
   - Prefer extracting separate `StatelessWidget` classes over helper methods returning `Widget` (e.g., `_buildHeader()`), to allow Flutter to optimize widget rebuild trees and const constructors.
   - Mark constructors as `const` wherever possible to prevent unnecessary redraws.

2. **Reusability**:
   - Extract domain-agnostic UI patterns into an `app_ui` package or `shared/widgets` directory.

## Output Requirements
- Refactored Flutter code splitting monolithic screens into clean, modular sub-widgets.