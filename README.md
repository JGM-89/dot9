# Dot[9]

Dot[9] is a Windows comfort overlay for motion-heavy games.

It adds subtle visual anchors over the screen so your eyes have somewhere still to land while a game camera turns, falls, sprints, flies, shakes, or rolls. Some players may find that a quiet, screen-fixed reference frame makes long play sessions more comfortable.

Dot[9] is a fan-made gift prototype inspired by a love of thoughtful gaming tools and Day[9]'s warmth around games. It is not an official Day[9]TV product unless adopted or approved by Day[9].

## What It Tries To Solve

Video-game-induced motion sickness and cybersickness can happen when your eyes see strong virtual motion while your body and inner ear feel still. Dot[9] explores a gentle accessibility idea: stable, configurable visual cues may help some players separate game-camera motion from the physical screen.

This is experimental. Dot[9] is a comfort tool, not a medical treatment.

## MVP Features

- Transparent, always-on-top Windows overlay
- Click-through by default, so games still receive mouse and keyboard input
- Stable edge dots
- Dot size, opacity, colour palette, shape, edge selection, and distance controls
- Live settings preview
- Friendly presets: Gentle, FPS, Vertigo, Fast Motion, Experimental
- Global hotkeys
- System tray toggle
- Local settings persistence
- Safety and privacy copy in the app

## Build

Install the .NET 8 SDK for Windows desktop development, then run:

```powershell
dotnet build .\Dot9.sln
```

This machine currently has the .NET desktop runtime installed but not the .NET SDK, so building here requires installing the SDK first.

## Run

```powershell
dotnet run --project .\src\Dot9\Dot9.csproj
```

Or build in Visual Studio 2022 and start the `Dot9` project.

## Use

1. Launch Dot[9].
2. Start with the Gentle preset.
3. Use the live preview to tune dot visibility.
4. Turn the overlay on before launching or focusing a game.
5. If anything feels worse, use Emergency Off and stop playing if symptoms are strong.

## Default Hotkeys

- Toggle overlay: `Ctrl+Alt+D`
- Emergency off: `Ctrl+Alt+Backspace`

## Safety Note

Dot[9] may help some players, but it does not cure, prevent, diagnose, or treat motion sickness, vertigo, migraine, vestibular disorders, or any medical condition. Avoid high contrast and rapid animation. Stop playing if you feel strong nausea, dizziness, headache, or disorientation.

## Privacy

Dot[9] works offline. The MVP collects no telemetry and sends no game names, usage data, health-related settings, profile data, or hardware data anywhere by default. Settings are stored locally in the user's application data folder.

## Limitations

- The MVP focuses on stable edge dots only.
- Some exclusive fullscreen games may render above normal desktop overlays.
- Multi-monitor coverage uses the Windows virtual screen and may need refinement for unusual DPI layouts.
- Counter-motion, horizon line, centre anchor, vignette, profiles, and onboarding are planned but not fully implemented in this MVP.

## Packaging

For a local self-contained Windows build:

```powershell
dotnet publish .\src\Dot9\Dot9.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish\Dot9
```

Do not commit the `publish/`, `bin/`, or `obj/` folders.

## Roadmap

- Centre Point anchor
- Horizon Line
- Gentle Vignette
- Counter-Motion and Motion Echo modes using normal OS-level mouse delta only
- Adaptive Comfort mode
- First-run onboarding
- Comfort Profiles with import/export
- Better multi-monitor and DPI refinement
- Installer packaging
