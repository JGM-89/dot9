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
- Optional centre point
- Live settings preview
- Friendly presets: Gentle, FPS, Vertigo, Fast Motion, Experimental
- Global hotkeys
- System tray toggle
- Local settings persistence
- Safety and privacy copy in the app

## Build

Install the .NET 10 SDK for Windows desktop development, then run:

```powershell
dotnet build .\Dot9.sln
```

This project targets `net10.0-windows` because the local machine has .NET SDK 10.0.203 installed.

GitHub Actions also builds Dot[9] on Windows and uploads a `Dot9-win-x64` artifact for each pull request and push to the MVP branch.

## Run

```powershell
dotnet run --project .\src\Dot9\Dot9.csproj
```

Or build in Visual Studio 2022 and start the `Dot9` project.

For a clickable local build, run:

```powershell
dotnet publish .\src\Dot9\Dot9.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false -o .\artifacts\Dot9-win-x64 --configfile .\NuGet.Config
```

Then open:

```powershell
.\artifacts\Dot9-win-x64\Dot9.exe
```

## Use

1. Launch Dot[9].
2. Start with the Gentle preset.
3. Use the live preview to tune dot visibility.
4. Turn the overlay on before launching or focusing a game.
5. If anything feels worse, use Emergency Off and stop playing if symptoms are strong.

## Default Hotkeys

- Toggle overlay: `Ctrl+Alt+D`
- Emergency off: `Ctrl+Alt+Backspace`
- Fallback toggle: `F8`
- Fallback emergency off: `F9`

## Safety Note

Dot[9] may help some players, but it does not cure, prevent, diagnose, or treat motion sickness, vertigo, migraine, vestibular disorders, or any medical condition. Avoid high contrast and rapid animation. Stop playing if you feel strong nausea, dizziness, headache, or disorientation.

## Privacy

Dot[9] works offline. The MVP collects no telemetry and sends no game names, usage data, health-related settings, profile data, or hardware data anywhere by default. Settings are stored locally in the user's application data folder.

## Limitations

- The MVP focuses on stable edge dots only.
- Some exclusive fullscreen games may render above normal desktop overlays.
- Multi-monitor coverage now draws per physical monitor, but unusual mixed-DPI layouts may still need refinement.
- Counter-motion, horizon line, vignette, profiles, onboarding, installer packaging, and automatic updates are planned but not fully implemented in this MVP.

## Roadmap

- Horizon Line
- Gentle Vignette
- Counter-Motion and Motion Echo modes using normal OS-level mouse delta only
- Adaptive Comfort mode
- First-run onboarding
- Comfort Profiles with import/export
- Better multi-monitor and DPI refinement
- Signed installer and automatic updates
