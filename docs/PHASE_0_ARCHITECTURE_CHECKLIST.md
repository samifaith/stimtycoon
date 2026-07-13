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
- [x] 32 passing EditMode tests

## Remaining Phase 0 work

- [ ] Childhood representative event
- [ ] School representative event
- [ ] Health representative event
- [ ] Money representative event
- [ ] All five events validated and runnable through Yarn into the C# resolver
- [ ] Save migration fixtures and tests
- [ ] Weighted-distribution coverage
- [ ] Complete vertical-slice play tests
- [ ] First iOS development build

## Deferred service integrations

- [ ] Unity Authentication
- [ ] Apple GameKit / Game Center
- [ ] Unity Cloud Save and conflict handling
- [ ] Unity LevelPlay / Ads Mediation

These services are intentionally deferred until the offline life loop is stable. Their implementations must stay behind the existing Stim-owned interfaces.

## Package status

Present now:

- UI Toolkit and UI Builder
- Unity Input System
- Unity Test Framework
- Yarn Spinner
- native Stim save repository

Optional or deferred:

- Easy Save 3 adapter evaluation
- Apple GameKit
- Unity Authentication and Cloud Save
- Unity LevelPlay / Ads Mediation
