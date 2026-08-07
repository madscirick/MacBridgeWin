# MacBridgeWin

MacBridgeWin is a lightweight Windows 10/11 tray application for people who move between macOS and Windows. It brings selected macOS-style interactions to Windows while deliberately preserving native Windows behavior.

[简体中文](README.zh-CN.md) · [Privacy](PRIVACY.md) · [Security](SECURITY.md) · [Contributing](CONTRIBUTING.md)

> **Status:** early, source-first release. The project is functional but does not yet provide an installer or signed binaries.

## What it does

- Maps selected `Alt` shortcuts to familiar `Ctrl` shortcuts, while preserving native `Ctrl` shortcuts, `Alt+Tab`, and `Alt+F4`.
- Recognizes right-button mouse gestures, including multi-segment paths such as `Left,Up`.
- Replays a normal right-click when no valid gesture is recognized.
- Runs from the system tray and provides a focused Settings window.
- Stores configuration locally as JSON, with import/export support.
- Supports per-application profiles matched by foreground process name.
- Offers an optional current-user Windows auto-start setting.

## Requirements

- Windows 10 or Windows 11
- .NET SDK 8.0.422 or a compatible later feature band (for building from source)

## Run from source

Clone the repository, then build and start the WPF application:

```powershell
dotnet build MacBridgeWin.sln -c Release
dotnet run --project src\MacBridgeWin.App\MacBridgeWin.App.csproj -c Release
```

The app starts in the notification area. Open **Settings** from its tray menu to enable keyboard remapping or mouse gestures.

## Build and test

```powershell
dotnet build MacBridgeWin.sln
dotnet test MacBridgeWin.sln
```

## Publish a local build

```powershell
dotnet publish src\MacBridgeWin.App\MacBridgeWin.App.csproj -c Release -r win-x64 --self-contained false
```

Published output is intentionally excluded from Git. Build it locally or obtain it from a future GitHub Release.

## Install a release build

1. Download the `MacBridgeWin-<version>-win-x64.zip` file from GitHub Releases.
2. Optionally verify its SHA-256 value against the adjacent `.sha256` file.
3. Extract the entire archive to a folder you control.
4. Run `MacBridgeWin.App.exe`; the app starts in the notification area.

MacBridgeWin does not currently ship an installer or a code-signed executable. Windows SmartScreen may therefore show a warning. Review the source and release checksum before deciding whether to run it. Never disable Windows security software to install MacBridgeWin.

To uninstall, disable **Start with Windows**, exit MacBridgeWin from the tray, and delete the extracted folder. User configuration and logs remain under `%LocalAppData%\MacBridgeWin` and can be removed separately.

## Privacy and security

- MacBridgeWin processes shortcut and mouse-gesture events locally. It has no telemetry, account system, analytics, advertising, or network service.
- It does not store typed text. Diagnostic logs contain lifecycle events, configured shortcut/gesture names, and exception details, but avoid foreground process names and configured application paths.
- Configuration and logs are stored under `%LocalAppData%\MacBridgeWin`. Exported configuration may contain application names or paths selected by the user.
- Keyboard remapping and mouse gestures are disabled by default.

See [PRIVACY.md](PRIVACY.md) for the complete data boundary and [SECURITY.md](SECURITY.md) for reporting vulnerabilities.

## Important behavior and limitations

- Keyboard remapping and mouse gestures are disabled by default.
- Global hooks may not affect applications running with higher privileges unless MacBridgeWin is run at a matching elevation level.
- The application does not record typed content. Local diagnostic logs are stored under `%LocalAppData%\MacBridgeWin\Logs`.
- This release does not include installer packaging, signed executables, or a broad built-in action system.

## Project layout

```text
src/MacBridgeWin.App/   WPF application, tray UI, and Windows integrations
src/MacBridgeWin.Core/  configuration, gesture, keyboard, and profile logic
tests/MacBridgeWin.Tests/ focused unit tests
```

## Contributing and security

Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request. Security reports are covered by [SECURITY.md](SECURITY.md). The project is licensed under the [MIT License](LICENSE).

See [CHANGELOG.md](CHANGELOG.md) for release notes and [PLAN.md](PLAN.md) for the product roadmap.
