---
name: responsive-layout-builder
description: Creates adaptive screen layouts that dynamically adjust across mobile, tablet, and desktop breakpoints.
---

# Skill: Responsive Layout Builder

## Description
Creates responsive layouts that adapt dynamically across mobile, tablet, and desktop viewports.

## Instructions
When building adaptive screens:

1. **Breakpoints**:
   - Compact (Mobile): `< 600dp`
   - Medium (Tablet): `600dp - 839dp`
   - Expanded (Desktop/Web): `≥ 840dp`

2. **Structural Adaptation**:
   - Mobile: Single column, bottom navigation bar, modal bottom sheets.
   - Tablet/Desktop: Navigation rail/drawer, multi-column master-detail layouts, side dialogs.
   - Use `LayoutBuilder`, `MediaQuery.sizeOf(context)`, or packages like `responsive_framework`.

## Output Requirements
- Responsive screen implementation switching cleanly between mobile, tablet, and desktop views.