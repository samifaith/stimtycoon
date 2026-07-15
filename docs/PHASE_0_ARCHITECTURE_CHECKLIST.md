# Phase 0 Architecture Checklist

## Confirmed in the repository

- [x] Unity `6000.3.19f1` project and assembly boundaries
- [x] Versioned event schema and validator
- [x] Risk/reward calculator
- [x] Deterministic, save-backed weighted outcome resolver
- [x] Versioned save envelope and validator
- [x] Native atomic JSON writes, SHA-256 integrity checks, and backup recovery
- [x] Transactional session service with effects, history, follow-ups, revision, RNG step, and autosave
- [x] Stim-owned interfaces for dialogue, saves, accounts, cloud saves, ads, and event catalogs
- [x] Yarn Spinner package and isolated bridge
- [x] Salary-negotiation Yarn script and representative event data
- [x] Playable UI Toolkit scene for the first vertical slice
- [x] 340 passing EditMode tests, including seeded birth-to-ending, transactional systems, UI structure, and long-run simulation coverage (user-verified July 15, 2026)

## Remaining Phase 0 work

- [x] Childhood representative event
- [x] School representative event
- [x] Health representative event
- [x] Money representative event
- [x] All five events validated and runnable through Yarn into the C# resolver
- [x] Save migration fixtures and tests
- [x] Weighted-distribution coverage
- [x] Save/reload vertical-slice flow coverage
- [x] Complete seeded birth-to-ending play test
- [ ] First iOS development build

## Deferred service integrations

- [ ] Unity Authentication
- [ ] Apple GameKit / Game Center
- [ ] Unity Cloud Save and conflict handling
- [ ] Unity LevelPlay production adapter and placement/consent configuration (package `9.5.0` installed)
- [ ] Unity IAP adapter, product catalog, restoration, and validation (package `5.4.1` installed)

These service implementations are intentionally deferred until their production gates. Installed packages do not count as gameplay integration; implementations must stay behind Stim-owned interfaces.

## Package status

Present now:

- UI Toolkit and UI Builder
- Unity Input System
- Unity Test Framework
- Unity LevelPlay `9.5.0` package (unconfigured)
- Unity IAP `5.4.1` package (unconfigured)
- Yarn Spinner
- native Stim save repository

Optional or deferred:

- MessagePack benchmark only if physical-device JSON profiling fails the performance target
- Easy Save 3 adapter evaluation only for convenience features, not as the current performance fix
- Apple GameKit
- Unity Authentication and Cloud Save
- LevelPlay and Unity IAP production configuration/adapters
