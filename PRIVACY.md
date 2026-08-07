# Privacy

MacBridgeWin is designed to operate locally and without a backend service.

## Data processed

MacBridgeWin observes registered shortcut activations, mouse-button events, pointer movement during a gesture, and foreground process names needed to select an application profile. This information is processed in memory to provide the configured behavior.

MacBridgeWin does not collect typed text, clipboard contents, document contents, browser history, account credentials, or precise pointer history outside an active gesture.

## Data stored locally

- Settings are stored under `%LocalAppData%\MacBridgeWin` as JSON.
- Diagnostic logs are stored under `%LocalAppData%\MacBridgeWin\Logs`.
- Settings may contain process names and application paths explicitly selected by the user.
- Logs contain timestamps, application lifecycle events, configured shortcut or gesture identifiers, and exception details. They do not intentionally record typed text, foreground process names, window titles, or configured application paths.

Exception details can contain local file paths supplied by Windows or .NET. Review logs before sharing them publicly.

## Network access and third parties

MacBridgeWin has no telemetry, analytics, advertising, update checker, account system, or network service. The application does not transmit settings, logs, or input events to the project maintainers or third parties.

## Exporting and deleting data

Configuration export is user initiated. Review an exported file before sharing it because it may include application names or paths.

To delete local data, exit MacBridgeWin and remove `%LocalAppData%\MacBridgeWin`. Disable **Start with Windows** before uninstalling so the current-user startup entry is removed.

## Changes

Any future feature that transmits data must be documented here before release and must provide clear user control.
