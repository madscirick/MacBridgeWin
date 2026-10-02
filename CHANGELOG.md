# Changelog

All notable changes to MacBridgeWin are documented in this file.

## 0.1.2 - 2026-10-02

### Added

- Chrome navigation defaults: hold the right mouse button, move down then left to go back, or down then right to go forward.
- Existing Chrome profiles receive missing navigation bindings when loading configuration; customized or disabled bindings are preserved.

### Fixed

- Restore normal right-clicks when pointer movement starts gesture feedback but does not reach the recognition threshold.
- Restore right-clicks for gestures without an enabled action, including bindings set to `None`.
- Capture the resolved configuration for each delayed gesture action instead of letting later configuration updates change it.

### Changed

- Run the low-level mouse hook on a dedicated message-loop thread to avoid delays caused by WPF UI work.
- Skip process lookup for ordinary clicks, sub-threshold movement, and configurations without application profiles.
- Skip native payload marshaling for unrelated mouse events, reuse synthetic right-click buffers, and avoid duplicate trail notifications.
- Add regression coverage for right-click preservation, gesture action selection, feedback, threshold boundaries, and hook restart.

## 0.1.1 - 2026-08-07

### Changed

- Windows releases now use self-contained single-file publishing. After extraction, users only need to run `MacBridgeWin.App.exe`.
- Added an automated release check that rejects unexpected extra files in the publish output.
- Made the ready-to-run download link more prominent in the English and Chinese README files.

## 0.1.0 - 2026-08-05

### Added

- System tray application and Settings window.
- Configurable Alt-to-Ctrl keyboard mappings with safeguards for native Windows shortcuts.
- Right-button directional and multi-segment mouse gestures, including normal right-click replay when a gesture is not recognized.
- JSON configuration, validation, import/export, application profiles, and current-user auto-start support.
- Focused unit tests and a Windows GitHub Actions build-and-test workflow.

### Not included

- Installer packaging or signed release binaries.
- A broad built-in action system or detailed profile-specific editors.
