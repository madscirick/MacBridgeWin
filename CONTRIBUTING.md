# Contributing to MacBridgeWin

Thanks for helping improve MacBridgeWin.

By participating, you agree to follow [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

## Before opening an issue

- Check existing issues and the [roadmap](PLAN.md).
- State your Windows version, the MacBridgeWin version or commit, and clear steps to reproduce the behavior.
- For keyboard or mouse behavior, include the affected app and whether it is running elevated.
- Do not include credentials or unreviewed log files.
- Report security vulnerabilities privately as described in [SECURITY.md](SECURITY.md).

## Pull requests

- Keep each pull request small and focused.
- Preserve native Windows behavior, especially `Ctrl` shortcuts, `Alt+Tab`, `Alt+F4`, and normal right-click behavior.
- Keep low-level hook callbacks fast; do not add blocking I/O or heavy work to them.
- Add or update focused tests for changes to Core behavior.
- Run the following before opening a pull request:

```powershell
dotnet build MacBridgeWin.sln -c Release
dotnet test MacBridgeWin.sln -c Release
```

## Development setup

Use the .NET SDK requested by `global.json`. No third-party runtime dependencies are needed beyond the NuGet packages restored by the solution.

```powershell
git clone <repository-url>
cd MacBridgeWin
dotnet restore MacBridgeWin.sln
dotnet test MacBridgeWin.sln -c Release
```

Keep generated output (`bin`, `obj`, `publish`, archives, logs, and local shortcuts) out of commits. Do not add telemetry or network access without a documented product decision and a corresponding update to [PRIVACY.md](PRIVACY.md).

## Commit and review expectations

- Use a concise, imperative commit subject.
- Explain user-visible behavior and test coverage in the pull request.
- Update README, privacy, security, or changelog documentation when behavior changes.
- Expect review to focus closely on hook safety, native shortcut preservation, right-click preservation, privacy, and cleanup paths.
