# Changelog

All notable changes to Dot[9] will be documented in this file.

## Unreleased

### Fixed

- Draw overlay cues per physical monitor instead of treating multi-monitor setups as one giant surface.
- Refresh the live preview immediately after settings changes.
- Improve legibility for the primary toggle button and dropdown controls.
- Add fallback hotkeys: `F8` for overlay toggle and `F9` for emergency off.
- Make closing the app quit Dot[9] and clear the overlay; minimizing now keeps it available from the tray.

### Added

- Basic centre point option.
- Custom Dot[9] app and tray icon.
- GitHub Actions workflow that builds a downloadable Windows artifact.
- GitHub project link and update note on the About screen.

## [0.1.0] - 2026-04-30

Initial MVP prototype.

### Added

- Windows WPF settings app for Dot[9].
- Transparent always-on-top overlay window with click-through behavior.
- Stable edge dot overlay renderer.
- Dot controls for opacity, size, colour palette, shape, edge selection, dots per edge, and distance from screen edge.
- Live monitor preview reflecting current dot settings.
- Presets for Gentle, FPS, Vertigo, Fast Motion, and Experimental.
- Global hotkeys for overlay toggle and emergency off.
- System tray menu for toggle, emergency off, settings, preset selection, and quit.
- Local JSON settings persistence.
- Product plan, research notes, README, and test plan.

### Notes

- This release focuses on Stable Anchor Mode.
- Centre anchor, horizon line, vignette, motion-reactive modes, profiles, onboarding, and packaging are planned future work.
- Dot[9] is an experimental comfort tool, not a medical treatment.
