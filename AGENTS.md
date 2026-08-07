# MacBridgeWin Agent Guide

## Project Overview

MacBridgeWin is a native Windows desktop application for users who frequently switch between macOS and Windows. Its goal is to make common interactions feel more consistent across both platforms while preserving important native Windows behavior.

The first planned product capabilities are:

- macOS-style keyboard shortcuts on Windows, using `Alt` similarly to the macOS `Command` key.
- Right-button mouse gestures for configurable actions.
- A lightweight system tray application with a Settings window.
- Local JSON configuration that can later support user-defined mappings, gesture actions, profiles, and auto-start settings.

This repository currently has keyboard remapping, mouse gestures, multi-segment gestures, editable Settings, application profiles, auto-start, and import/export foundations in place. Do not implement installer packaging or broad action systems unless a task explicitly asks for that phase.

## Product Goals

- Provide familiar keyboard shortcuts for macOS users working on Windows.
- Preserve native Windows shortcuts and behavior, including `Ctrl` shortcuts, `Alt+Tab`, and `Alt+F4`.
- Preserve normal right-click behavior when no valid mouse gesture is recognized.
- Keep the application lightweight enough for long-term background execution.
- Build a modular architecture that can support custom mappings, multi-segment gestures, application-specific profiles, and packaging later.

## Technical Stack

- Language: C#
- Framework: .NET 8
- UI framework: WPF
- Target platforms: Windows 10 and Windows 11
- Configuration format: JSON
- Test framework: To be selected during project foundation, preferably a common .NET test framework with minimal dependencies.

## Architecture Principles

- Keep keyboard input handling, mouse input handling, gesture recognition, action execution, configuration, profiles, tray behavior, and Settings UI separate.
- Prefer native .NET and Windows APIs when practical.
- Avoid unnecessary third-party libraries.
- Avoid abstractions that do not serve a clear foreseeable need.
- Keep changes focused on the current task and current phase.
- Preserve native Windows behavior unless a feature explicitly and safely overrides it.
- Design hook-related code to avoid recursive handling of synthetic input.
- Ensure startup and shutdown paths release hooks and background resources cleanly.

## Planned Project Structure

The intended solution structure is:

```text
MacBridgeWin.sln
src/
  MacBridgeWin.App/
  MacBridgeWin.Core/
tests/
  MacBridgeWin.Tests/
```

### MacBridgeWin.App

WPF application project. Responsible for:

- Application startup and shutdown.
- System tray integration.
- Settings window and view models.
- Wiring application services together.
- User-facing status and error presentation.

### MacBridgeWin.Core

Core library with platform and business logic. Responsible for:

- Keyboard shortcut mapping models and services.
- Low-level keyboard input interfaces and implementations.
- Mouse input interfaces and implementations.
- Gesture capture and recognition logic.
- Action execution abstractions.
- Configuration models and JSON persistence.
- Application profile models and resolution rules.
- Logging abstractions or lightweight logging helpers.

### MacBridgeWin.Tests

Unit test project. Responsible for:

- Configuration loading and saving behavior.
- Keyboard mapping decision logic.
- Synthetic input recursion guards.
- Gesture recognition logic.
- Action resolution behavior.
- Profile selection logic when profiles are introduced.

## Main Modules and Responsibilities

### Keyboard Input Handling

Responsible for observing low-level keyboard events, deciding whether an event should be remapped, preserving native shortcuts, and dispatching synthetic replacement input when appropriate.

Current/future candidate types:

- `KeyboardHookService`
- `KeyboardShortcutMapper`
- `KeyboardMapping`
- `KeyboardMappingOptions`
- `SyntheticInputGuard`
- `KeyboardRemappingService`
- `SyntheticKeyboardInputSender`

### Mouse Input Handling

Responsible for observing mouse events, capturing right-button gesture sessions, deciding whether to suppress or preserve right-click behavior, and passing captured movement to the gesture recognizer.

Current/future candidate types:

- `MouseHookService`
- `MouseGestureService`
- `MouseGestureSession`
- `MouseGestureOptions`
- `SyntheticMouseInputSender`

### Gesture Recognition

Responsible for converting captured pointer movement into gesture directions.

Current support recognizes:

- `Left`
- `Right`
- `Up`
- `Down`
- Multi-segment direction sequences such as `Left,Up` and `LeftUp`

Current/future candidate types:

- `BasicDirectionalGestureRecognizer`
- `GestureSequence`
- `GesturePath`
- `GestureDirection`
- `GestureDefinition`

### Action Execution

Responsible for executing actions bound to keyboard mappings or gestures. Initial actions should stay small and explicit. Later phases may add configurable built-in actions.

Future candidate types:

- `IActionExecutor`
- `ActionDefinition`
- `ActionType`
- `BuiltInActionExecutor`

### Configuration Management

Responsible for local JSON configuration persistence, default configuration creation, versioning, and safe fallback behavior if the configuration file is missing or invalid.

Future candidate types:

- `IConfigurationStore`
- `JsonConfigurationStore`
- `AppConfiguration`
- `ConfigurationDefaults`
- `ConfigurationVersion`

### Application-Specific Profiles

Responsible for selecting mappings and gesture actions based on the active foreground application.

Current/future candidate types:

- `ApplicationProfile`
- `IActiveApplicationProvider`
- `ProfileResolver`
- `ActiveApplicationProvider`

### System Tray Functionality

Responsible for running primarily in the notification area, showing status, opening Settings, toggling features, and exiting cleanly.

Future candidate types:

- `TrayIconService`
- `TrayMenuController`
- `ApplicationLifecycleService`

### Settings UI

Responsible for feature toggles, editable keyboard mappings, editable gesture actions, and configuration status. The UI should be practical and focused rather than decorative.

Current/future candidate types:

- `SettingsWindow`
- `SettingsViewModel`
- `KeyboardMappingViewModel`
- `MouseGestureActionViewModel`
- `KeyboardMappingsViewModel`
- `MouseGesturesViewModel`

### Startup and Import/Export

Responsible for current-user Windows auto-start and JSON configuration import/export.

Current/future candidate types:

- `StartupConfiguration`
- `WindowsStartupService`

## Coding Conventions

- Follow established .NET naming conventions.
- Use nullable reference types when projects are created.
- Prefer small, focused classes with clear responsibilities.
- Keep public APIs intentional and documented where behavior is non-obvious.
- Use async APIs only where they provide a real benefit.
- Keep comments concise and limited to non-obvious behavior or platform edge cases.
- Avoid broad refactors during feature tasks unless needed to complete the requested change safely.

## Dependency Policy

- Prefer .NET and Windows APIs.
- Add third-party dependencies only when they provide clear value and reduce risk.
- Do not add dependencies for simple logic that can be implemented clearly in the project.
- Document any new dependency and why it was chosen.

## Build Commands

The expected build command is:

```powershell
dotnet build MacBridgeWin.sln
```

## Test Commands

The expected test command is:

```powershell
dotnet test MacBridgeWin.sln
```

## Testing Requirements

- Add focused unit tests for core behavior whenever implementation changes affect mapping, gesture recognition, configuration, action execution, or profile resolution.
- Test edge cases before relying on manual validation for hook behavior.
- Keep platform-specific hook code thin so most decision logic remains unit-testable.
- Build the solution after code changes.
- Run relevant tests after code changes.
- Fix compilation errors before finishing a task.

## Logging Requirements

- Logging should help diagnose startup, shutdown, configuration loading, hook installation, hook removal, suppressed events, synthetic input, and unexpected failures.
- Logs must avoid recording sensitive typed content.
- Logging should be lightweight and suitable for long-running background execution.

## Windows Compatibility Requirements

- Target Windows 10 and Windows 11.
- Preserve native `Ctrl` shortcuts.
- Preserve `Alt+Tab`, `Alt+F4`, and other important system-level shortcuts.
- Account for elevated applications, where non-elevated hooks may not affect elevated windows.
- Handle multiple monitors and mixed DPI scaling for mouse gesture movement.
- Cleanly install and remove hooks during startup, feature toggles, and shutdown.

## Performance Expectations

- Keep low-level hook callbacks fast.
- Avoid blocking, file I/O, logging-heavy work, or UI work inside hook callbacks.
- Minimize allocations in high-frequency input paths.
- Keep background memory and CPU usage low.

## Important Architectural Constraints

- Do not broaden keyboard remapping behavior without tests for native Windows shortcut preservation.
- Do not broaden mouse gesture behavior without tests for right-click preservation and gesture recognition.
- Validate editable configuration before saving from Settings.
- Keep profile resolution testable in Core and foreground process detection isolated in App.
- Do not implement unrelated features while working on a specific phase.
- Preserve normal Windows behavior by default.
- Keep configuration schemas versionable.
- Keep profile support planned but inactive until explicitly implemented.

## Instructions for Future Codex Sessions

- Read this `AGENTS.md` file before making changes.
- Inspect the existing codebase before implementation.
- Follow the current architecture and roadmap.
- Avoid unnecessary refactoring.
- Avoid implementing unrelated features.
- Keep changes focused on the current task.
- Build the project after code changes.
- Run relevant tests after code changes.
- Fix compilation errors before finishing.
- Update documentation when architectural decisions change.
- Summarize all changes and test results after each task.
