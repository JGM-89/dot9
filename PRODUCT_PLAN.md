# Dot[9] Product And Architecture Plan

## Product Plan

Dot[9] is a calm Windows comfort utility for motion-heavy games. Version 1.0 focuses on a polished settings window, a transparent click-through overlay, stable edge dots, centre point, horizon line, gentle vignette, friendly presets, tray controls, hotkeys, local settings, and careful safety language.

Dot[9] should be presented as an independent fan-made comfort tool. It should not imply official ownership, endorsement, or branding by any third party.

## Science-Informed Rationale

Cybersickness is often connected to sensory conflict: the eyes see movement while the body and vestibular system feel still. Apple Vehicle Motion Cues address the opposite travel mismatch, where the body feels motion while the eyes may be looking at a static screen.

For games, the safest first hypothesis is a stable visual rest frame. Low-opacity edge dots may give the visual system a quiet physical-screen reference during camera movement. Effects will vary, so Dot[9] must be customisable and cautious.

## Technical Architecture Recommendation

The recommended 1.0 stack is C#/.NET WPF:

- Reliable Windows-native transparent overlay behavior.
- Direct access to click-through and always-on-top window styles.
- Normal OS-level global hotkeys through `RegisterHotKey`.
- System tray support through Windows Forms interop.
- Low runtime overhead compared with Electron.
- Simple packaging with standard .NET tooling.

WinUI 3 remains a future option for a more modern control set, but WPF is safer for reliable overlay behavior. Electron, Tauri, and Python add more risk around transparent overlay behavior, packaging, or dependency weight.

## 1.0 Implementation Summary

1. WPF project structure and design tokens.
2. Strongly typed settings and JSON persistence.
3. Transparent click-through overlay window.
4. Stable edge dots with configurable shape, size, colour, opacity, edge distance, and edge selection.
5. Centre point, horizon line, and gentle vignette controls.
6. Polished settings GUI with live preview.
7. Presets, tray menu, toggle hotkey, and emergency-off hotkey.
8. README, research notes, changelog, and test plan.

## Assumptions

- Stable Anchor Mode is the fully implemented 1.0 visual behavior.
- The overlay supports all monitors, the primary monitor, or one selected display.
- Some exclusive fullscreen games may render above normal desktop overlays.
- No game injection, graphics API hooking, game memory reads, input automation, or game file modification will be used.
- Settings stay local and telemetry is off by default.
