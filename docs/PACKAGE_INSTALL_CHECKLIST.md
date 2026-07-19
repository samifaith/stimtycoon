# Package Install Checklist

Install vendors only when the related feature is ready. After each package change, let Unity compile, run the setup check and all EditMode tests, then commit the package manifest, lock file, project settings, imported assets, and `.meta` files together.

## Baseline — complete

- [x] Open with Unity `6000.3.19f1` and install iOS Build Support through Unity Hub.
- [x] Record a clean July 17, 2026 baseline of 679 EditMode and 3 production-scene PlayMode smoke cases (superseding the July 15 340-case baseline).
- [ ] Reproduce the recorded baseline locally or in CI and retain NUnit/coverage evidence after resolving Unity licensing/package entitlement recovery.
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

### External configuration required

The native commerce slice is blocked until these non-repository values exist:

- Apple bundle identifier and App Store Connect app record.
- LevelPlay iOS app key plus rewarded ad-unit IDs (test and production).
- App Store Connect product IDs for Spark packs, Starter Pack, Remove Ads, cosmetics, and Stim+.
- Apple agreements/tax/banking completion and purchase-validation endpoint/keys.

Do not invent or commit these values. Keep ads and purchases disabled until production values, consent behavior, receipt validation, and sandbox tests pass.

## Deferred vendors

- [ ] Add Unity Authentication and Cloud Save together when account-linked backup work begins.
- [ ] Add Apple GameKit when Game Center sign-in, achievements, or leaderboards enter the active slice.

## Guardrails

- Keep vendor SDK types inside `Assets/StimTycoon/Integrations` or a dedicated adapter assembly.
- Gameplay code depends on Stim-owned interfaces, never directly on vendor SDKs.
- Do not enable `STIM_EASY_SAVE_3` unless Easy Save 3 is deliberately imported for adapter evaluation; it is not required by the current save system.
- Keep atomic JSON through the first device-profiling pass. The slow full-life test is dominated by repeated growing-save cloning and serialization, so changing repositories is not the first optimization.
- If physical-device profiling proves that JSON serialization or file size is unacceptable, benchmark MessagePack behind `ISaveRepository` while preserving the versioned logical envelope, migrations, integrity validation, and recovery fixtures.
- Record exact resolved versions in `Packages/packages-lock.json` and test upgrades separately.
