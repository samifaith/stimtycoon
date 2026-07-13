# Stim Tycoon

**Status:** Phase 0 foundation with a playable career-event vertical slice

**Target:** iOS 13+

**Unity:** `6000.3.19f1` (Unity 6.3 LTS)

**Stack:** C#, UI Toolkit, Yarn Spinner, native JSON saves

Stim Tycoon is a mobile life and wealth simulator built around choice-driven life progression, weighted outcomes, careers, relationships, business, investing, and legacy.

## Current Build

The repository now contains:

- a versioned event schema, validator, and risk/reward calculator
- deterministic, save-backed weighted outcome resolution
- a transactional game-session service that applies effects, history, follow-ups, and autosaves
- a versioned save envelope with validation, SHA-256 integrity checks, atomic replacement, and backup recovery
- Yarn Spinner dialogue authoring behind a Stim-owned bridge
- a salary-negotiation event wired from Yarn choices into the C# resolver
- a mobile UI Toolkit vertical slice with choice, outcome, cash, life-feed, and autosave feedback
- replaceable interfaces for dialogue, saves, accounts, cloud saves, ads, and event catalogs
- 32 passing EditMode tests covering events, runtime behavior, and saves

Not yet implemented:

- age/year progression and complete-life simulation
- the other four Phase 0 representative events
- Unity Authentication, Apple Game Center, and Cloud Save adapters
- Unity LevelPlay ads
- production navigation, accessibility, localization, art, and audio
- iOS device build and automated play-flow coverage

## Open and Run

1. Install Unity `6000.3.19f1` with iOS Build Support through Unity Hub.
2. Open this repository as an existing project and allow package import to finish.
3. Confirm the Console has no compilation errors.
4. Open `Assets/Scenes/StimVerticalSlice.unity`.
5. Press Play and choose an outcome in the annual-review event.

If the scene ever needs to be rebuilt, use:

`Tools → Stim Tycoon → Create Vertical Slice Scene`

For a setup audit, use:

`Tools → Stim Tycoon → Run Setup Check`

## Run Tests

In Unity:

1. Open `Window → General → Test Runner`.
2. Select **EditMode**.
3. Click **Run All**.

The current expected result is **32 passing tests**. If tests do not appear after a code change, run `Assets → Refresh` and reopen Test Runner. Unity 6.3 can emit editor-only Test Runner layout errors when its list is context-clicked; these are not game test failures.

## Packages

Already installed or built in:

- UI Toolkit and UI Builder
- Unity Input System
- Unity Test Framework
- Yarn Spinner from its official Git repository
- native Stim save repository; Easy Save 3 is optional and not required

Still deferred until the related gameplay needs them:

- Unity Authentication and Cloud Save
- Apple GameKit / Game Center
- Unity LevelPlay / Ads Mediation

See [the package checklist](docs/PACKAGE_INSTALL_CHECKLIST.md) before adding a vendor dependency. Keep all vendor SDK types behind the interfaces in `Assets/Scripts/Domain/Abstractions`.

## Project Structure

```text
Assets/
├── Dialogue/Events/       # Yarn-authored event dialogue
├── Scenes/                # Playable Unity scenes
├── Scripts/
│   ├── Domain/            # Schemas, validation, resolution, interfaces
│   ├── Runtime/           # Sessions, persistence, composition, UI binding
│   ├── Editor/            # Setup checks and scene tooling
│   ├── Vendors/           # Isolated vendor integrations
│   └── Tests/             # EditMode test suite
└── UI/                    # UXML, USS, and panel settings

Packages/                  # Pinned Unity dependencies
ProjectSettings/           # Unity project configuration
docs/                      # Architecture and gameplay specifications
```

## Architecture Rules

- C# domain code owns eligibility, probability, effects, scheduling, and save validation.
- Yarn owns dialogue copy and choice flow; it does not mutate gameplay state directly.
- Every resolved action is applied to a candidate save and committed atomically before becoming active state.
- RNG seed and step live in the save so outcomes are reproducible.
- Currency is stored in integer minor units.
- Vendor integrations remain replaceable behind Stim-owned interfaces.

## Phase 0 Progress

- [x] Event schema and validator
- [x] Risk/reward calculator
- [x] Deterministic outcome resolver
- [x] Save schema, validation, atomic write, and corruption recovery
- [x] Runtime composition and event catalog
- [x] Yarn Spinner bridge
- [x] First representative career event
- [x] Playable mobile UI vertical slice
- [x] 32-test baseline
- [ ] Four remaining representative events
- [ ] Save migration fixtures and cloud-conflict tests
- [ ] iOS development build

## Documentation

- [Master product definition and roadmap](<STIM_TYCOON_MASTER_README(2).md>)
- [Package install checklist](docs/PACKAGE_INSTALL_CHECKLIST.md)
- [Phase 0 architecture checklist](docs/PHASE_0_ARCHITECTURE_CHECKLIST.md)
- [Event schema](docs/EVENT_SCHEMA.md)
- [Modifier rules](docs/MODIFIER_RULES.md)
- [Business turn design](docs/BUSINESS_TURNS.md)
- [Unity setup guide](UNITY_6_UPGRADE_GUIDE.md)

## Version Control

Commit Unity source assets, their `.meta` files, `Packages`, and relevant `ProjectSettings`. Do not commit generated folders such as `Library`, `Temp`, `Obj`, `Build`, `Logs`, or `UserSettings`.
