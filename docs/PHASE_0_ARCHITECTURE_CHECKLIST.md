# Phase 0 Architecture Checklist

## Repo Audit Snapshot

### Verified in repo

- Locked event schema, validator, and risk/reward calculator in C#
- Locked save envelope and save validator in C#
- Runtime composition boundary with no-op vendor adapters
- Catalog-backed event runtime service
- NUnit coverage for event, save, and runtime validation
- Stim-owned abstractions for dialogue, save, account, cloud save, ads, and event catalog

### Still missing from implementation

- Dialogue System import and authored prototype conversations
- Easy Save 3 import and repository implementation
- Unity Authentication implementation
- Apple GameKit / Game Center implementation
- Unity Cloud Save implementation
- Unity LevelPlay implementation
- UI Toolkit screens and scene/UXML assets
- Representative events authored in Dialogue System

### Package status

The repository currently has only core Unity packages in `Packages/manifest.json`. The third-party packages above are still needed before the vendor adapters can be implemented.

## Implementation Plan

1. Establish a first-party runtime composition root that depends only on Stim-owned abstractions.
2. Keep vendor systems behind replaceable adapters so Dialogue System, Easy Save, auth, cloud save, and ads can be swapped without touching domain code.
3. Add the first concrete gameplay bridge for event resolution and local save transactions.
4. Import the missing packages, then replace the no-op adapters with real integrations.
5. Author the five representative Dialogue System events and wire them through the bridge.

## Confirmed in repo

- [x] Locked event schema and validator in C#
- [x] Risk/reward calculator in C#
- [x] Locked save envelope and validator in C#
- [x] Unity test coverage for schema validators
- [x] Assembly boundary for runtime code
- [x] Assembly boundary for tests
- [x] Stim-owned wrapper interfaces for third-party systems
- [x] Runtime composition boundary started
- [x] Catalog-backed event runtime service

## Still missing from implementation

- [ ] Dialogue System import and authored prototype conversations
- [ ] Easy Save 3 import (adapter source is wired behind `STIM_EASY_SAVE_3`)
- [ ] Unity Authentication implementation
- [ ] Apple GameKit / Game Center implementation
- [ ] Unity Cloud Save implementation
- [ ] Unity LevelPlay implementation
- [ ] UI Toolkit screens and scene/UXML assets
- [ ] Representative events authored in Dialogue System

## Package status

### Present in `Packages/manifest.json`

- `com.unity.ui`
- `com.unity.ui.builder`
- `com.unity.test-framework`
- `com.unity.textmeshpro`
- `com.unity.timeline`
- `com.unity.visualscripting`

### Still needed for Phase 0 / Phase 1 bridge work

- Dialogue System for Unity from the Asset Store
- Easy Save 3 from the Asset Store
- Apple GameKit / Apple GameKit Plugins
- Unity Authentication package
- Unity Cloud Save package
- Unity LevelPlay / Ads Mediation package

## Notes

- The repo is currently stronger on domain rules than on integration scaffolding.
- The new interfaces exist so vendor-specific code can stay behind Stim-owned boundaries.
- Once the third-party packages are installed, their adapters should live behind these interfaces rather than in gameplay code.
