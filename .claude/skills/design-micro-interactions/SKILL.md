---
name: design-micro-interactions
description: Adds implicit animations, smooth state transitions, and tactile feedback to enhance interactive UI responsiveness.
---

# Skill: Design Micro-Interactions

## Description
Adds subtle animations, feedback effects, and visual transitions to improve perceived software quality.

## Instructions
When creating interactive UI components:

1. **Animation Guidelines**:
   - Duration: Keep transitions between **150ms and 300ms** for crisp feedback.
   - Curves: Prefer `Curves.easeInOutCubic` or `Curves.fastOutSlowIn`.
   - Prefer implicit animations (`AnimatedContainer`, `AnimatedOpacity`, `AnimatedCrossFade`) for simplicity.

2. **Feedback Types**:
   - Button presses: Scale or elevation feedback (`InkWell`, `ElevatedButton`).
   - State shifts: Smooth transitions between loading, success, and content states.
   - Haptics: Add subtle haptic feedback (`HapticFeedback.lightImpact()`) on key user actions.

## Output Requirements
- Flutter widget code incorporating smooth state animations and interactive feedback.