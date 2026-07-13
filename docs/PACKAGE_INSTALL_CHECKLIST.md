# Package Install Checklist

Install vendors only when the related feature is ready. After each package change, let Unity compile, run the setup check and all EditMode tests, then commit the package manifest, lock file, project settings, imported assets, and `.meta` files together.

## Baseline — complete

- [x] Open with Unity `6000.3.19f1` and install iOS Build Support through Unity Hub.
- [x] Confirm a clean compile and 32 passing EditMode tests.
- [x] Run `Tools → Stim Tycoon → Run Setup Check` with no project-level failures.
- [x] Use the native atomic JSON repository for required local saves.
- [x] Install Yarn Spinner from its official Git repository and isolate it behind the Stim dialogue bridge.
- [x] Run the salary-negotiation vertical slice in the mobile simulator.

## Deferred vendors

- [ ] Add Unity Authentication and Cloud Save together when account-linked backup work begins.
- [ ] Add Apple GameKit when Game Center sign-in, achievements, or leaderboards enter the active slice.
- [ ] Add Unity LevelPlay / Ads Mediation last, after placements and consent behavior are defined.
- [ ] Create an iOS development build after each native SDK integration.

## Guardrails

- Keep vendor SDK types inside `Assets/Scripts/Vendors` or a dedicated adapter assembly.
- Gameplay code depends on Stim-owned interfaces, never directly on vendor SDKs.
- Do not enable `STIM_EASY_SAVE_3` unless Easy Save 3 is deliberately imported for adapter evaluation; it is not required by the current save system.
- Record exact resolved versions in `Packages/packages-lock.json` and test upgrades separately.
