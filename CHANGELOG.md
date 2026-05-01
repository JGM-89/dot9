# Changelog

All notable changes to Dot[9] will be documented in this file.

## Unreleased

### Changed

- README now leads with downloading the GitHub Release ZIP instead of developer build commands.
- Home tuning now includes centre point size, opacity, colour, and shape controls.
- Live preview now shows configured overlay elements, including horizon and vignette, instead of drawing fake decoration.
- Horizon Line now defaults to one centre-gapped line so preview and overlay match.
- README now includes a visual demo preview for the GitHub project homepage.
- README demo preview no longer includes decorative motion-path lines that could be mistaken for an app feature.
- Overlay now watches foreground-window changes and reasserts topmost placement while visible so it can recover after games change display mode.

### Fixed

- Draw overlay cues per physical monitor instead of treating multi-monitor setups as one giant surface.
- Refresh the live preview immediately after settings changes.
- Force dropdown selected values and menu items to use black text on a light field.
- Improve legibility for the primary toggle button.
- Wire all centre point, horizon, and vignette controls to the settings model.
- Remove the non-functional Counter-Motion preset until motion-reactive input is implemented.
- Add fallback hotkeys: `F8` for overlay toggle and `F9` for emergency off.
- Make closing the app quit Dot[9] and clear the overlay; minimizing now keeps it available from the tray.
- Show a warning when global hotkeys cannot be registered or when duplicate shortcuts are selected.
- Re-register global hotkeys only when hotkey settings change.
- Debounce local settings saves while sliders are being dragged.
- Convert monitor bounds from pixels to WPF drawing units for better mixed-DPI overlay placement.
- Update the test plan to use the current `F9` Emergency Off default.
- Add a home control to turn edge dots off while keeping centre point, horizon, or vignette enabled.
- Remove explicit third-party creator/brand references from app and documentation copy.

### Added

- Centre point option.
- Horizon Line with side tick, segmented, and full-line styles.
- Gentle Vignette with strength, radius, and opacity controls.
- Custom Dot[9] app and tray icon.
- GitHub Actions workflow that builds a downloadable Windows artifact.
- GitHub project link and update note on the About screen.
- One-time Windows tray notification the first time the settings window is minimized.

## [0.1.0] - 2026-04-30

Initial MVP prototype.

### Added

- Windows WPF settings app for Dot[9].
- Transparent always-on-top overlay window with click-through behavior.
- Stable edge dot overlay renderer.
- Dot controls for opacity, size, colour palette, shape, edge selection, dots per edge, and distance from screen edge.
- Live monitor preview reflecting current dot settings.
- Presets for Gentle, FPS, Vertigo, and Fast Motion.
- Global hotkeys for overlay toggle and emergency off.
- System tray menu for toggle, emergency off, settings, preset selection, and quit.
- Local JSON settings persistence.
- Product plan, research notes, README, and test plan.

### Notes

- This release focuses on Stable Anchor Mode.
- Centre anchor, horizon line, vignette, motion-reactive modes, profiles, onboarding, and packaging are planned future work.
- Dot[9] is an experimental comfort tool, not a medical treatment.
