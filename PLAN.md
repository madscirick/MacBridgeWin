# MacBridgeWin Plan

## Product Scope

MacBridgeWin is a lightweight native Windows desktop application that helps macOS users feel more at home on Windows.

The product will initially combine two core features:

- macOS-style keyboard shortcuts, using the Windows `Alt` key similarly to macOS `Command`.
- Right-button mouse gestures for common configurable actions.

The application should run primarily from the system tray, expose a Settings window, store user configuration in JSON, and remain suitable for long-term background use on Windows 10 and Windows 11.

This plan intentionally separates foundation work from hook and gesture implementation. The project should be built incrementally.

## Intended Architecture

The planned solution contains three projects:

```text
MacBridgeWin.sln
src/
  MacBridgeWin.App/
  MacBridgeWin.Core/
tests/
  MacBridgeWin.Tests/
```

`MacBridgeWin.App` is the WPF application. It owns startup, shutdown, system tray behavior, Settings UI, view models, and service wiring.

`MacBridgeWin.Core` contains the testable application logic and platform-facing services. It owns keyboard mapping rules, mouse gesture capture models, gesture recognition, action execution, configuration persistence, and future profile resolution.

`MacBridgeWin.Tests` contains focused unit tests for core behavior.

## Main Modules

### Keyboard Input Handling

Handles low-level keyboard events, macOS-style shortcut remapping, native shortcut preservation, and synthetic input recursion prevention.

Initial mappings:

- `Alt+C` -> `Ctrl+C`
- `Alt+V` -> `Ctrl+V`
- `Alt+X` -> `Ctrl+X`
- `Alt+A` -> `Ctrl+A`
- `Alt+Z` -> `Ctrl+Z`
- `Alt+Shift+Z` -> `Ctrl+Y`
- `Alt+S` -> `Ctrl+S`
- `Alt+F` -> `Ctrl+F`
- `Alt+P` -> `Ctrl+P`
- `Alt+N` -> `Ctrl+N`
- `Alt+O` -> `Ctrl+O`
- `Alt+W` -> `Ctrl+W`
- `Alt+T` -> `Ctrl+T`
- `Alt+R` -> `Ctrl+R`

### Mouse Input Handling

Handles low-level mouse events, right-button gesture session capture, and right-click preservation when no valid gesture is detected.

### Gesture Recognition

Converts captured mouse movement into directional gestures. Initial support is limited to `Left`, `Right`, `Up`, and `Down`, with a movement threshold.

### Action Execution

Executes configured actions for keyboard mappings and gestures. The action model should start small and expand only as needed.

### Configuration Management

Loads and saves local JSON configuration, provides defaults, supports enable/disable flags, and leaves room for versioned schema changes.

### Application-Specific Profiles

Eventually supports per-application mappings and gesture actions based on the active foreground process or window.

### System Tray Functionality

Provides tray status, opens Settings, toggles features, and exits cleanly.

### Settings UI

Provides controls for enabling and disabling features, editing keyboard mappings, editing gesture actions, and later managing profiles.

## Development Phases

### Phase 1: Project Foundation

Milestones:

- Create `MacBridgeWin.sln`.
- Create `MacBridgeWin.App` WPF project.
- Create `MacBridgeWin.Core` class library.
- Create `MacBridgeWin.Tests`.
- Add initial system tray shell.
- Add Settings window shell.
- Add lightweight logging approach.
- Add JSON configuration models and persistence.
- Add unit test infrastructure.

Acceptance criteria:

- The solution builds on Windows with .NET 8.
- The WPF app starts and exits cleanly.
- The app can run from the system tray.
- The Settings window can be opened from the tray.
- A default JSON configuration can be created, loaded, and saved.
- Unit tests run successfully.

### Phase 2: Keyboard Shortcut Remapping

Milestones:

- Add low-level keyboard hook service. Completed.
- Add default macOS-style `Alt` mappings. Completed.
- Preserve native `Ctrl` shortcuts. Completed in mapping decision logic.
- Preserve important Windows shortcuts such as `Alt+Tab` and `Alt+F4`. Completed in mapping decision logic.
- Add synthetic input dispatch. Completed.
- Add recursion guard for synthetic keyboard events. Completed with tagged synthetic input.
- Add enable/disable support from configuration. Completed.

Acceptance criteria:

- Initial `Alt` mappings behave like their configured `Ctrl` equivalents.
- Native `Ctrl` shortcuts still work.
- `Alt+Tab` and `Alt+F4` still work.
- Synthetic input does not recursively trigger remapping.
- Keyboard mapping logic is covered by unit tests where practical.

### Phase 3: Basic Mouse Gesture Engine

Milestones:

- Add low-level mouse hook service. Completed.
- Capture right-button gesture sessions. Completed.
- Recognize `Left`, `Right`, `Up`, and `Down`. Completed.
- Execute configurable actions for recognized gestures. Completed for `None` and `KeyboardShortcut:<shortcut>`.
- Preserve normal right-click when no valid gesture is recognized. Completed by replaying a synthetic right-click.
- Add configurable movement threshold. Completed through JSON configuration.

Acceptance criteria:

- Holding the right mouse button and moving beyond the threshold can trigger a basic gesture.
- Short or ambiguous movement preserves normal right-click behavior.
- Gesture recognition logic is unit-tested.
- Mouse gesture feature can be enabled or disabled through configuration.

### Phase 4: Settings UI for Mappings and Gestures

Milestones:

- Add UI for enabling and disabling keyboard mappings. Completed.
- Add UI for enabling and disabling mouse gestures. Completed.
- Add UI for editing keyboard mappings. Completed.
- Add UI for editing gesture actions. Completed.
- Persist settings changes to JSON. Completed.
- Validate editable settings before saving. Completed.

Acceptance criteria:

- Users can toggle keyboard remapping and mouse gestures.
- Users can view and edit mappings.
- Changes persist after app restart.
- Invalid settings are prevented or safely handled.

### Phase 5: Advanced Gestures and Profiles

Milestones:

- Add multi-segment gesture recognition. Completed for direction sequences.
- Add application-specific profiles. Completed for profile data and process matching.
- Add profile resolution based on active application. Completed using foreground process name.
- Add more configurable action types. Partially completed with keyboard shortcut actions.

Acceptance criteria:

- Multi-segment gestures can be configured and recognized.
- Different applications can use different mappings.
- Profile fallback behavior is predictable and tested.

### Phase 6: Polish, Packaging, and Distribution

Milestones:

- Add Windows auto-start support. Completed for current-user Run key.
- Add configuration import/export. Completed in Settings.
- Improve stability and diagnostics. Partially completed through validation and logging.
- Optimize performance in hook paths. Ongoing.
- Add packaging and distribution workflow. Publish command documented; installer packaging not yet implemented.

Acceptance criteria:

- Auto-start can be enabled and disabled.
- Configuration can be exported and restored.
- Long-running background usage remains stable.
- The application can be packaged for installation or distribution.

## Technical Risks and Edge Cases

### Low-Level Keyboard Hooks

Hook callbacks run on a sensitive path and must stay fast. Blocking work, UI calls, disk I/O, or heavy logging inside callbacks can cause input lag or instability.

### Low-Level Mouse Hooks

Mouse input can be high frequency. Gesture capture must avoid excessive allocation and should defer heavier processing outside the hook callback when possible.

### Synthetic Input Recursion

Generated `Ctrl` shortcuts can re-enter the keyboard hook. The architecture needs a clear guard to identify and ignore synthetic events created by MacBridgeWin.

### Preserving Native Alt Behavior

Windows uses `Alt` for menus and system shortcuts. The remapping layer must only intercept explicit configured mappings and allow other `Alt` behavior to pass through.

### Preserving Alt+Tab and Alt+F4

These system shortcuts must remain untouched. Tests and manual validation should specifically cover them.

### Preserving Normal Right-Click Behavior

Right-click should still work when movement is below the gesture threshold or no gesture is recognized. This is a core usability requirement.

### Elevated Applications

A non-elevated app may not affect elevated windows. This should be documented clearly and revisited during implementation.

### Multiple Monitors

Gesture movement should use screen coordinates that behave predictably across monitor boundaries and negative coordinates.

### DPI Scaling

Gesture thresholds and movement calculations should account for different DPI scaling settings and mixed-DPI monitor setups.

### Long-Term Background Performance

The app should keep CPU, memory, logging, and allocations low while idle and during input-heavy use.

### Clean Startup and Shutdown

Hooks, tray resources, background services, and configuration saves must be cleaned up reliably when the app exits or features are toggled.

## Current Development Status

Status: Phase 6 polish foundation implemented.

Completed:

- Product requirements analyzed.
- Intended architecture documented.
- Development phases defined.
- Technical risks identified.
- .NET 8 solution and projects created.
- WPF app shell created.
- System tray shell created with Settings and Exit commands.
- Settings window shell created with feature toggles.
- JSON configuration defaults and persistence added.
- Lightweight file logging added.
- MSTest infrastructure added.
- Configuration default and persistence tests added.
- Testable keyboard shortcut mapping decision layer added.
- Low-level keyboard hook service added.
- Synthetic keyboard input sender added.
- Keyboard remapping service wired to startup, Settings save, and shutdown.
- Unit tests added for `Alt` mappings, native `Ctrl` pass-through, `Alt+Tab`/`Alt+F4` pass-through, disabled mappings, and synthetic input pass-through.
- Testable basic directional gesture recognizer added.
- Low-level mouse hook service added.
- Right-button gesture capture added.
- Normal right-click replay added when no valid gesture is recognized.
- Mouse gesture service wired to startup, Settings save, and shutdown.
- Simple gesture action executor added for `None` and `KeyboardShortcut:<shortcut>` actions.
- Unit tests added for `Left`, `Right`, `Up`, `Down`, below-threshold movement, and ambiguous diagonal movement.
- Settings UI expanded with General, Keyboard, and Mouse tabs.
- Keyboard mappings can be edited in Settings.
- Mouse gesture actions and movement threshold can be edited in Settings.
- Configuration validation added before saving editable settings.
- Unit tests added for configuration validation.
- Multi-segment gesture recognition added.
- Gesture sequence parsing added for `Left,Up` and compact `LeftUp` formats.
- Application profile resolver added.
- Foreground application process detection added in the WPF app.
- Keyboard and mouse services now resolve active profile configuration at input time.
- Profiles tab added to Settings for basic profile name and process editing.
- Unit tests added for multi-segment gestures and profile resolution.
- Windows auto-start toggle added.
- Configuration import/export added to Settings.
- Publish command documented.

Not yet implemented:

- Rich built-in action types beyond keyboard shortcut actions.
- Per-profile detailed mapping editor.
- Installer packaging and distribution automation.
- Diagnostics UI for logs.

## Recommended Next Task

Recommended next task:

- Manually test the running tray application.
- Verify keyboard remapping in common apps.
- Verify right-click preservation and mouse gestures.
- Add a diagnostics/log viewer in Settings if manual testing shows issues.
- Add installer packaging once behavior is stable.
