---
name: generate-go-router-config
description: Builds type-safe GoRouter configurations with nested routes, shell routes, auth redirection guards, and parameter mapping.
---

# Skill: Generate GoRouter Configuration

## Description
Generates type-safe, maintainable routing configurations using `go_router`.

## Instructions
When creating routes or navigation logic:

1. **Route Structure**:
   - Use named routes and paths as constants (`AppRoutes.home`, `AppRoutes.details`).
   - Implement `ShellRoute` / `StatefulShellRoute` for persistent bottom navigation bars.

2. **Type Safety & Parameters**:
   - Pass path parameters and query parameters using strongly typed extensions or GoRoute parameters.
   - For complex objects, pass IDs and fetch data in the target screen via Cubit/GetIt rather than passing heavy objects through route extra parameters.

3. **Redirection & Guards**:
   - Implement a central `redirect` callback handling auth checks, onboarding status, and deep links.
   - Ensure redirection listens to state changes (e.g., using `Listenable.merge` or `RefreshListenable` with BLoC/Cubit states).

## Output Requirements
- Complete `GoRouter` configuration setup.
- Helper extension methods or navigation methods for clean UI invocation (`context.goNamed(...)` or `context.pushNamed(...)`).