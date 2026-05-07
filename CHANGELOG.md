# Changelog

All notable changes to Dot[9] will be documented in this file.

## Unreleased

No unreleased changes yet.

## [1.0.1] - 2026-05-07

### Changed

- GitHub Actions now publishes a rolling `latest` release for main builds without rewriting immutable versioned releases.
- Versioned releases such as `v1.0.0` and `v1.0.1` are now treated as stable snapshots.
- README now explains the difference between the rolling latest build and stable versioned releases.
- Changelog now separates shipped changes from unreleased work.

### Fixed

- Enabled per-monitor DPI awareness and improved per-monitor overlay coordinate conversion.
- Reduced overlay topmost compatibility polling to short retry windows after overlay enablement or foreground-window changes.
- Made the GitHub project link in the About screen clickable.

## [1.0.0] - 2026-05-04

First stable release.

### Changed

- Promoted Dot[9] to version 1.0.0 and removed MVP language from user-facing docs and app copy.
- Replaced the original tagline with "Visual anchors for motion-heavy games."
- Softened broad "experimental" wording to clearer science-informed comfort-tool language while keeping medical safety disclaimers.
- README leads with downloading the GitHub Release ZIP instead of developer build commands.
- Home tuning includes centre point size, opacity, colour, and shape controls.
- Live preview shows configured overlay elements, including horizon and vignette, instead of drawing fake decoration.
- Horizon Line defaults to one centre-gapped line so preview and overlay match.
- README includes screenshots for the GitHub project homepage.
- About screen and README include clearer science-informed rationale and cautious comfort-tool language.
- README has a more discoverable opening section with badges and search-friendly project positioning.
- Removed early planning and test-note documents from the public repo; the README now carries the public-facing product and science context.

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
- First-run onboarding flow.
- Keystroke capture for custom hotkeys.

### Notes

- Dot[9] is a comfort tool, not a medical treatment.
- Motion-reactive modes, profiles, installer packaging, and in-app updates are roadmap items.

## [0.1.0] - 2026-04-30

Initial prototype.

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
- README and safety documentation.

### Notes

- This release focuses on Stable Anchor Mode.
- Dot[9] is a comfort tool, not a medical treatment.
