---
name: build-empty-and-error-states
description: Implements standardized loading shimmers, friendly empty views, and actionable error states for data views.
---

# Skill: Build Empty and Error States

## Description
Ensures every screen gracefully handles loading, error, and empty state conditions.

## Instructions
When implementing data-driven screens:

1. **State Requirements**:
   - **Loading**: Show shimmer placeholders or progress indicators (avoid plain blank screens).
   - **Empty**: Display a clear illustration/icon, friendly explanation, and a primary CTA (e.g., "Add New Item").
   - **Error**: Display a user-friendly error message, an error icon, and a retry button that triggers a BLoC/Cubit event.

2. **Reusability**:
   - Use generic widgets like `EmptyStateWidget` and `ErrorStateWidget`.

## Output Requirements
- Complete BLoC/Cubit state switching code handling `Loading`, `Success`, `Empty`, and `Error` UI representation.