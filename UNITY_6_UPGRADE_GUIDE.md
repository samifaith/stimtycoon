# Unity 6.3 LTS Setup Guide

**Status:** Upgrade complete; the project is pinned to Unity `6000.3.19f1`.

## Install the Editor

1. Open Unity Hub and select **Installs → Install Editor**.
2. Install Unity `6000.3.19f1`.
3. Include **iOS Build Support** for Stim Tycoon's target platform.
4. Game Center support is a separate project dependency and can remain deferred.

## Open the Project

1. In Unity Hub, select **Projects → Add → Add project from disk**.
2. Choose this repository root.
3. Let the first package import and script compilation finish.

The project may live on an external drive. Use APFS on macOS, keep the drive mounted at a stable path, and ensure the current user has read/write access.

## Verify the Project

1. Confirm the title bar or **Unity → About Unity** reports `6000.3.19f1`.
2. Run `Tools → Stim Tycoon → Run Setup Check`.
3. Open `Window → General → Test Runner`, select **EditMode**, and run the complete suite. The repository currently contains 340 EditMode test methods; include the `SlowSimulation` birth-to-ending harness in a full verification run.
4. Open `Assets/StimTycoon/Scenes/StimVerticalSlice.unity` and press Play.

## Current Dependencies

- UI Toolkit and UI Builder
- Input System
- Unity Test Framework
- Unity LevelPlay `9.5.0` (installed; configuration and production adapter deferred)
- Unity IAP `5.4.1` (installed; catalog, restore/validation flows, and adapter deferred)
- Yarn Spinner
- native Stim atomic JSON saves

Authentication, Cloud Save, and Apple GameKit remain uninstalled and deferred. LevelPlay and Unity IAP are installed but not integrated into production gameplay. Dialogue System for Unity and Easy Save 3 are not required.

## Troubleshooting

### Project does not open

- Confirm Unity `6000.3.19f1` is installed, not merely queued in Hub.
- Confirm the external drive is mounted and writable.
- In Hub, remove only the project-list entry and add the repository again; this does not delete project files.
- Check `~/Library/Logs/Unity/Editor.log` for the first concrete error.

### UI scene is empty

- Exit Play Mode.
- Run `Tools → Stim Tycoon → Create Vertical Slice Scene`.
- Wait for compilation, clear the Console, and press Play again.

### Tests do not appear

- Run `Assets → Refresh`.
- Close and reopen Test Runner.
- Avoid context-clicking its test list if Unity emits an editor-only layout exception.

### Compilation errors after a package change

- Wait for package resolution to finish.
- Inspect the first Console error rather than later cascading errors.
- Revert or isolate the package upgrade if the clean baseline no longer compiles.
