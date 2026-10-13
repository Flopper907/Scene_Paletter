---
name: enforce-spacing-grid
description: Enforces a strict 8pt layout grid system for paddings, margins, and gaps to eliminate magic spacing numbers.
---

# Skill: Enforce Spacing Grid

## Description
Enforces a strict 8pt grid system for paddings, margins, and gaps.

## Instructions
When layout out widgets:

1. **Spacing Scale**:
   - `xxs`: `2.0`
   - `xs`: `4.0`
   - `sm`: `8.0`
   - `md`: `16.0`
   - `lg`: `24.0`
   - `xl`: `32.0`
   - `xxl`: `48.0`

2. **Implementation Rules**:
   - Avoid random "magic numbers" like `Padding(padding: EdgeInsets.all(13.5))`.
   - Define an `AppSpacing` class containing static constants.
   - Use `SizedBox(height: AppSpacing.md)` or `Gap(AppSpacing.md)` (if using `gap` package) for layout spacing.

## Output Requirements
- `AppSpacing` utility class and refactored UI code demonstrating standardized layout gaps.