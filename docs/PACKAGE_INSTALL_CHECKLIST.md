# Package Install Checklist

Install vendors only when the related feature is ready. After each package change, let Unity compile, run the setup check and all EditMode tests, then commit the package manifest, lock file, project settings, imported assets, and `.meta` files together.

## Baseline — complete

- [x] Open with Unity `6000.3.19f1` and install iOS Build Support through Unity Hub.
- [x] Confirm a clean compile and 340 passing EditMode tests (user-verified July 15, 2026).
- [x] Run `Tools → Stim Tycoon → Run Setup Check` with no project-level failures.
- [x] Use the native atomic JSON repository for required local saves.
- [x] Install Yarn Spinner from its official Git repository and isolate it behind the Stim dialogue bridge.
- [x] Run the salary-negotiation vertical slice in the mobile simulator.

## Installed but awaiting integration

- [x] Install Unity LevelPlay / Ads Mediation `9.5.0`.
- [x] Install Unity IAP `5.4.1`.
- [ ] Configure LevelPlay placements, test mode, consent/ATT, privacy and age treatment, offline failure behavior, and a Stim-owned production adapter.
- [ ] Configure the Unity IAP product catalog, store IDs, purchase restoration and validation, cancellation/failure behavior, and a Stim-owned adapter.
- [ ] Create an iOS development build after each native SDK integration is configured.

## Deferred vendors

- [ ] Add Unity Authentication and Cloud Save together when account-linked backup work begins.
- [ ] Add Apple GameKit when Game Center sign-in, achievements, or leaderboards enter the active slice.

## Guardrails

- Keep vendor SDK types inside `Assets/Scripts/Vendors` or a dedicated adapter assembly.
- Gameplay code depends on Stim-owned interfaces, never directly on vendor SDKs.
- Do not enable `STIM_EASY_SAVE_3` unless Easy Save 3 is deliberately imported for adapter evaluation; it is not required by the current save system.
- Keep atomic JSON through the first device-profiling pass. The slow full-life test is dominated by repeated growing-save cloning and serialization, so changing repositories is not the first optimization.
- If physical-device profiling proves that JSON serialization or file size is unacceptable, benchmark MessagePack behind `IStimSaveRepository` while preserving the versioned logical envelope, migrations, integrity validation, and recovery fixtures.
- Record exact resolved versions in `Packages/packages-lock.json` and test upgrades separately.
