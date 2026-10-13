---
name: clean-architecture-and-guidelines
description: Enforces Clean Architecture principles, domain boundary separation, layer dependencies, and core software architectural rules across system modules.
---

# Skill: Clean Architecture & System Guidelines

## Description
A comprehensive, unified guide for structuring software applications using Clean Architecture, domain-driven boundaries, and robust system architectural guidelines.

## Instructions
When designing, structuring, or refactoring application modules:

1. **Layer Separation & Dependency Rule**:
   - **Domain Layer (Core)**: Contains Business Entities, Value Objects, and Repository Interfaces. Must have ZERO external framework dependencies (no Flutter, HTTP libraries, UI logic, etc.). Dependencies always point INWARD toward Domain.
   - **Data Layer (Adapters)**: Implements Domain repository interfaces, manages JSON mapping, local caching (SQLite/Hive/Isar), and remote REST/GraphQL clients. Converts raw DTOs into Domain Entities.
   - **Presentation Layer (UI/UX)**: State management (BLoC/Cubit), screens, and widgets. Consumes Domain entities directly via Use Cases / Repositories.

2. **Architectural Principles**:
   - **Single Responsibility Principle (SRP)**: Each class/module must have only one reason to change.
   - **Inversion of Control (IoC)**: Always depend on abstractions (interfaces/abstract classes), never concrete implementations.
   - **Feature-First / Domain-Driven Structure**: Organize files by feature domain (`features/auth`, `features/checkout`) rather than by technical layer type (`controllers/`, `views/`).

3. **Data Flow & Boundaries**:
   - Data flows from UI -> Use Case / Repository Interface -> Data Source -> API/Database.
   - Response flows back: Data Source DTO -> Domain Entity -> Presentation State -> UI Widget.
   - Use strict Result types or Exception-to-Failure mappings to prevent raw data errors from crashing higher layers.

## Output Requirements
- Clear module boundaries, explicit interface definitions, and architectural layout plans.