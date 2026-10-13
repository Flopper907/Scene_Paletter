---
name: register-get-it-dependencies
description: Sets up and configures Dependency Injection using GetIt with appropriate singleton and factory registration lifecycles.
---

# Skill: Register GetIt Dependencies

## Description
Configures Dependency Injection using `get_it` (and optionally `injectable`) following project standards.

## Instructions
When wiring up services, repositories, or Cubits:

1. **Registration Lifecycles**:
   - `registerLazySingleton`: Use for stateless services, API clients, databases, and repositories.
   - `registerFactory`: Use for Cubits/BLocs and transient objects that must be re-instantiated on screen creation.
   - `registerSingleton`: Use only for critical startup instances that must initialize immediately.

2. **Structure**:
   - Create a centralized service locator setup function (`setupLocator()` or `configureDependencies()`).
   - Group registrations logically: Data Sources -> Repositories -> Use Cases -> Cubits/BLocs.

3. **Rules**:
   - Always register against abstract interfaces for Repositories and Data Sources to facilitate unit testing.
   - Avoid direct global calls to `GetIt.I` inside leaf widgets; pass dependencies via constructors or use `BlocProvider` with `GetIt`.

## Output Requirements
- Provide the registration code block along with constructor injection signatures for the classes.