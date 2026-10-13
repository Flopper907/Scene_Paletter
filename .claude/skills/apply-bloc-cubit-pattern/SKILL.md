---
name: apply-bloc-cubit-pattern
description: Generates BLoC/Cubit state management structures using flutter_bloc with immutable sealed states and clean UI event handling.
---

# Skill: Apply BLoC / Cubit Pattern

## Description
Guidelines and rules for generating Flutter state management code strictly using `flutter_bloc` / `cubit`.

## Instructions
When asked to create or refactor state management logic:

1. **State Definition**:
   - Use `sealed class` for base states (Dart 3+).
   - Use `freezed` or value equality (`equatable`) if payload comparisons are needed.
   - Separate state into granular states: `Initial`, `Loading`, `Success`, `Failure`.

2. **Cubit / BLoC Implementation**:
   - Prefer `Cubit` for simple state transitions and `Bloc` when handling complex event streams/transformers.
   - Emit updated immutable states using `copyWith` or specific state subclasses.
   - Do not perform direct UI manipulations or platform calls within the Cubit; delegate to injected repositories.

3. **UI Integration**:
   - Use `BlocBuilder` for rebuilding widgets based on state.
   - Use `BlocListener` for one-time side effects (snackbars, navigation).
   - Use `BlocConsumer` when both rebuilding and side-effects are required.
   - Provide Cubits using `BlocProvider` high enough in the widget tree or via `get_it`.

## Output Requirements
- Clean, production-ready Dart code.
- Explicit type declarations and null-safety.
- Proper error handling with descriptive failure states.