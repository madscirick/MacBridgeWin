# MacBridgeWin

<p align="center">
  <a href="https://github.com/madscirick/MacBridgeWin/releases/download/v0.1.2/MacBridgeWin-v0.1.2-win-x64.zip">
    <img alt="Download MacBridgeWin v0.1.2 for Windows" src="https://img.shields.io/badge/普通用户下载-MacBridgeWin%20v0.1.2-1677FF?style=for-the-badge&logo=windows11&logoColor=white">
  </a>
</p>

<p align="center"><strong>Windows 10/11 · Version v0.1.2 · Released 2026-10-02</strong></p>

MacBridgeWin is a lightweight Windows 10/11 tray application for people who move between macOS and Windows. It brings selected macOS-style interactions to Windows while deliberately preserving native Windows behavior.

[简体中文](README.zh-CN.md) · [Privacy](PRIVACY.md) · [Security](SECURITY.md) · [Contributing](CONTRIBUTING.md)

> **Status:** early, source-first release. The project is functional but does not yet provide an installer or signed binaries.

## Download for Windows

**Most users should download the ready-to-run release, not the repository source code.**

### [Download MacBridgeWin v0.1.2 for Windows (ZIP)](https://github.com/madscirick/MacBridgeWin/releases/download/v0.1.2/MacBridgeWin-v0.1.2-win-x64.zip)

1. Download the ZIP above and extract the entire archive.
2. Run `MacBridgeWin.App.exe` from the extracted folder.
3. MacBridgeWin starts in the notification area; open **Settings** from its tray menu.

> [!WARNING]
> Do **not** use GitHub's green **Code → Download ZIP** button or the automatically generated **Source code (zip)** asset unless you intend to build the application yourself. Those downloads contain source code, not a runnable release.

[View the v0.1.2 release page](https://github.com/madscirick/MacBridgeWin/releases/tag/v0.1.2) · [SHA-256 checksum](https://github.com/madscirick/MacBridgeWin/releases/download/v0.1.2/MacBridgeWin-v0.1.2-win-x64.zip.sha256)

**Release date:** 2026-10-02

## Version history

Use the latest version for the right-click fixes and performance improvements. Earlier versions remain available if you need to roll back.

| Version | Release date | Main changes | Windows x64 download | Release notes |
| --- | --- | --- | --- | --- |
| **v0.1.2 (latest)** | 2026-10-02 | Right-click preservation fixes and reduced mouse hook overhead | [ZIP](https://github.com/madscirick/MacBridgeWin/releases/download/v0.1.2/MacBridgeWin-v0.1.2-win-x64.zip) | [Details](https://github.com/madscirick/MacBridgeWin/releases/tag/v0.1.2) |
| v0.1.1 | 2026-08-07 | Self-contained single-file publishing; no separate .NET installation required | [ZIP](https://github.com/madscirick/MacBridgeWin/releases/download/v0.1.1/MacBridgeWin-v0.1.1-win-x64.zip) | [Details](https://github.com/madscirick/MacBridgeWin/releases/tag/v0.1.1) |
| v0.1.0 | 2026-08-07 | Initial release with keyboard mappings, mouse gestures, and tray settings | [ZIP](https://github.com/madscirick/MacBridgeWin/releases/download/v0.1.0/MacBridgeWin-v0.1.0-win-x64.zip) | [Details](https://github.com/madscirick/MacBridgeWin/releases/tag/v0.1.0) |

[View all releases](https://github.com/madscirick/MacBridgeWin/releases). Each release page also includes a SHA-256 checksum file. Exit the running application before switching versions.

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
4. Run the single `MacBridgeWin.App.exe` file; the app starts in the notification area.

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
