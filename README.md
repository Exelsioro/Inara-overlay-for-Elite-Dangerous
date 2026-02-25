# Elite Dangerous Inara Overlay

A .NET 8 WPF overlay for Elite Dangerous that displays trade routes sourced from INARA.

## Requirements

- Windows 10 (1607+) or Windows 11
- Elite Dangerous (`EliteDangerous64.exe`)

## Features

- Overlay window attached to the game window
- Focus-aware visibility (hide/show with game focus)
- Trade route search via INARA
- Results overlay with route cards
- Pinned route overlay
- Global hotkey: `Ctrl+5`
- Theme system with import/export

## Repository Layout

```text
ED_Inara_Overlay/
|- ED_Inara_Overlay/                # Main WPF app
|- InaraTools/                      # INARA communication and parsing
|- Logger/                          # Shared logging library
|- Testing/                         # Test harnesses and scripts
|- Documentation/                   # Project documentation
|- build_installer.ps1              # Build + installer automation
|- installer.iss                    # Inno Setup script
```

## Build

```bash
dotnet build ED_Inara_Overlay/ED_Inara_Overlay.sln
```

## Run

```bash
dotnet run --project ED_Inara_Overlay/ED_Inara_Overlay.csproj
```

## Test Utilities

```bash
# Build mock target app
dotnet build Testing/MockTargetApp/MockTargetApp.csproj

# Run quick regression script
powershell -ExecutionPolicy Bypass -File Testing/QuickRegressionTest.ps1
```

## Installer

```powershell
# Build app + installer
.\build_installer.ps1

# Build app only
.\build_installer.ps1 -SkipInstaller

# Build installer only (if binaries already built)
.\build_installer.ps1 -SkipBuild
```

Installer output is written to `Installer/`.

## Logging

Runtime logs are written to `...\Elite Dangerous Inara Overlay\logs/`.

## License

MIT
