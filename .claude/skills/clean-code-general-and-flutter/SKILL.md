---
name: clean-code-general-and-flutter
description: Combines universal software engineering clean code standards with Flutter- and Dart-specific performance, readability, and widget optimizations.
---

# Skill: Clean Code (General & Flutter-Specific)

## Description
An all-in-one Clean Code skill combining universal software engineering best practices with Flutter and Dart-specific performance and readability standards.

## Instructions
When writing or refactoring code, enforce both general and Flutter-specific clean code standards:

### Part 1: Universal Clean Code Principles
1. **Naming Standards**:
   - Use meaningful, searchable, and pronounceable names.
   - Functions should be verbs (`getUserData`, `calculateTotal`); classes should be nouns (`UserRepository`, `OrderProcessor`).
   - Avoid acronyms or arbitrary abbreviations.
2. **Functions & Methods**:
   - Keep functions short and focused on a single task (aim for < 20 lines).
   - Limit function arguments to 3 or fewer (use parameter objects or named arguments for more).
   - Avoid side effects and prefer pure functions where possible.
3. **Comments & Self-Documenting Code**:
   - Code should explain *what* it does through clear naming; comments should only explain *why* non-obvious decisions were made.
   - Delete dead/commented-out code immediately.

### Part 2: Flutter & Dart Specific Standards
1. **Widget Tree Optimization**:
   - Prefer `const` constructors everywhere possible to allow Flutter to skip unnecessary widget rebuilds.
   - Split large `build()` methods (> 50 lines) into separate, reusable `StatelessWidget` classes (avoid helper methods returning `Widget`).
   - Keep business and UI logic separate: never execute network calls, database queries, or raw state mutation directly inside widget lifecycle hooks or callbacks.
2. **Dart Language Idioms**:
   - Use null-safety features effectively (avoid `!` operator overuses; use `?`, `??`, and `if-case` pattern matching).
   - Prefer tear-offs (`onPressed: _handleSubmit`) over redundant closures (`onPressed: () => _handleSubmit()`).
   - Use final/immutable fields by default across models and states (`@immutable` or `freezed`).

## Output Requirements
- High-quality, self-documenting, performant Dart/Flutter code adhering strictly to both sets of guidelines.