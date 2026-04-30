# Dot[9] Product And Architecture Plan

## Product Plan

Dot[9] is a calm Windows comfort utility for motion-heavy games. The MVP focuses on a polished settings window, a transparent click-through overlay, safe stable edge dots, friendly presets, tray controls, hotkeys, local settings, and careful safety language.

The Day[9] connection is affectionate inspiration only. The app should not imply official Day[9]TV ownership, endorsement, or branding unless that changes later.

## Science-Informed Rationale

Cybersickness is often connected to sensory conflict: the eyes see movement while the body and vestibular system feel still. Apple Vehicle Motion Cues address the opposite travel mismatch, where the body feels motion while the eyes may be looking at a static screen.

For games, the safest first hypothesis is a stable visual rest frame. Low-opacity edge dots may give the visual system a quiet physical-screen reference during camera movement. Effects will vary, so Dot[9] must be customisable and cautious.

## Technical Architecture Recommendation

The recommended MVP stack is C#/.NET WPF:

- Reliable Windows-native transparent overlay behavior.
- Direct access to click-through and always-on-top window styles.
- Normal OS-level global hotkeys through `RegisterHotKey`.
- System tray support through Windows Forms interop.
- Low runtime overhead compared with Electron.
- Simple packaging with standard .NET tooling.

WinUI 3 remains a future option for a more modern control set, but WPF is safer for the overlay MVP. Electron, Tauri, and Python add more risk around transparent overlay behavior, packaging, or dependency weight.

## MVP Implementation Plan

1. Create WPF project structure and design tokens.
2. Build strongly typed settings and JSON persistence.
3. Implement transparent click-through overlay window.
4. Draw stable edge dots with configurable shape, size, colour, opacity, edge distance, and edge selection.
5. Build a polished settings GUI with live preview.
6. Add presets, tray menu, toggle hotkey, and emergency-off hotkey.
7. Add README, research notes, and test plan.
8. Commit logically, push the feature branch, and open a PR when local Git/GitHub CLI access permits.

## Assumptions

- Stable Anchor Mode is the only fully implemented visual behavior in the MVP.
- The overlay covers the Windows virtual screen as a first pass for multi-monitor support.
- Some exclusive fullscreen games may render above normal desktop overlays.
- No game injection, graphics API hooking, game memory reads, input automation, or game file modification will be used.
- Settings stay local and telemetry is off by default.
