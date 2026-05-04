# Dot[9] Test Plan

## Desktop Overlay

- Launch Dot[9] on a normal Windows desktop.
- Toggle the overlay from the main window.
- Confirm dots appear over the desktop and do not block mouse clicks.
- Confirm `Ctrl+Alt+D` toggles the overlay.
- Confirm `F9` immediately hides the overlay.
- Change both hotkeys and confirm duplicate shortcut choices are rejected.
- Choose a shortcut already owned by another app, if available, and confirm Dot[9] shows a warning.
- Confirm the tray menu can toggle, emergency-off, open settings, choose presets, and quit.

## Game Modes

- Test with a windowed game.
- Test with a borderless fullscreen game.
- Test with an exclusive fullscreen game and note whether the game renders above the overlay.
- Launch a game after the overlay is already visible and confirm Dot[9] reasserts topmost placement without taking focus.
- Alt-Tab between the game and another app, then confirm Dot[9] remains visible when the game regains focus.
- If a game hides the overlay, switch the game to borderless or windowed fullscreen and retest.
- Confirm Dot[9] does not inject into, attach to, read memory from, or modify the game.

## Multi-Monitor And DPI

- Test one monitor at 100 percent scaling.
- Test one monitor at 125 or 150 percent scaling.
- Test two monitors with matching scaling.
- Test two monitors with mixed scaling.
- Confirm the overlay covers the expected virtual desktop area and remains click-through.

## Settings

- Change dot opacity, size, distance from edge, colour, shape, and edge selection.
- Confirm the live preview updates immediately.
- Confirm the real overlay updates after each setting change.
- Restart the app and confirm settings are restored.
- Reset to the Gentle preset and confirm safe defaults return.

## Safety And Accessibility

- Confirm there is no default flashing, pulsing, or sudden movement.
- Confirm all 1.0 controls are keyboard reachable.
- Confirm text remains readable at common window sizes.
- Confirm emergency disable works while a game or another app is focused.
- Confirm reduced-motion preference disables optional animation settings when detected.

## Performance

- Watch CPU and GPU usage with the overlay visible but static.
- Confirm the overlay does not visibly affect desktop interaction.
- Test on a low-end machine if available.

## Removal

- Quit from the tray.
- Confirm no background process remains.
- Uninstall or delete the app folder.
- Confirm there are no hooks, drivers, services, or game files left behind.
