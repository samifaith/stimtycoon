# Stim Tycoon

**Status:** Phase 1 offline life loop verified; Phase 2 gameplay expansion active; shared-action foundation delivered

**Target:** iOS 13+

**Unity:** `6000.3.19f1` (Unity 6.3 LTS)

**Stack:** C#, UI Toolkit, Yarn Spinner, native JSON saves

Stim Tycoon is a mobile life and wealth simulator built around choice-driven life progression, weighted outcomes, careers, relationships, business, investing, and legacy.

## Current Build

The repository now contains:

- a versioned event schema, validator, and risk/reward calculator
- deterministic, save-backed weighted outcome resolution
- a transactional game-session service that applies effects, history, follow-ups, and autosaves
- a reusable candidate-save transaction runner, with Education action rules extracted behind the existing session API
- a migration-safe shared action contract with stable instances, availability states, signed previews, persisted completion, idempotency, and reusable UI Toolkit cards
- a versioned save envelope with validation, SHA-256 integrity checks, atomic replacement, and backup recovery
- an idempotent additive v1 save migrator with structured migration reports
- Yarn Spinner dialogue authoring behind a Stim-owned bridge
- all five representative events—childhood, school, career, health, and money—wired through the C# resolver
- monthly gross pay, tax withholding, living expenses, debt pressure, stat feedback, and career progression, plus annual age rollover, randomized event timing with anti-drought protection, cooldowns, and pending-event persistence
- required school-path decisions, context-sensitive childhood/school/work activities, persistent peers, relationship drift, authored drama, and scheduled consequences
- a coming-of-age identity chain followed by friendship-gated dating, prom, first-kiss, partnership, engagement, marriage, strain, counseling, separation, and divorce branches
- persistent household happiness/cohesion, spouse-derived savings/debt/income, fixed-price family activities, and cash-or-credit payment selection with risk-based APR and monthly interest
- a mobile UI Toolkit vertical slice with choices, outcomes, cash, life feed, autosave feedback, and a collapsible six-stat player overview
- replaceable interfaces for dialogue, saves, accounts, cloud saves, ads, and event catalogs
- a user-verified 220-test EditMode baseline including reusable amount, payment, and UI-control coverage

Not yet implemented:

- Unity Authentication, Apple Game Center, and Cloud Save adapters
- Unity LevelPlay ads
- production navigation, accessibility, localization, art, and audio
- iOS device build and automated play-flow coverage
- meaningful one-time achievement prizes, such as cash, durable resources, unlocks, or cosmetic/status rewards

## Current Focus

Milestones 1–5 are complete with a verified 220-test baseline, including reusable percentage/exact-amount input and authored cash-or-credit validation.

The next implementation sequence is:

1. Finish the shared framework states and controls: in-progress/claimable transitions, timers, cash-or-credit confirmation, and reusable percentage/exact-amount input.
2. Complete the Education vertical slice with study tracks, authored costs, difficulty-based sessions, qualification progress, and career/event unlocks.
3. Add at least two skill paths beyond Learning and connect them to visible prerequisites and later consequences.
4. Validate all current screens at 320, 390, 430, and 768 widths, including 130% text scale.
5. Produce the first iOS development build and profile persistence, memory, safe areas, and touch behavior on a supported iPhone.

See [the active task list](docs/TASKS.md) for acceptance criteria and later Money, Home, Relationship, and Business slices.

## Open and Run

1. Install Unity `6000.3.19f1` with iOS Build Support through Unity Hub.
2. Open this repository as an existing project and allow package import to finish.
3. Confirm the Console has no compilation errors.
4. Open `Assets/Scenes/StimVerticalSlice.unity`.
5. Press Play, resolve events, advance months, and use **View Player Overview** to inspect the current life state.

If the scene ever needs to be rebuilt, use:

`Tools → Stim Tycoon → Create Vertical Slice Scene`

For a setup audit, use:

`Tools → Stim Tycoon → Run Setup Check`

## Run Tests

In Unity:

1. Open `Window → General → Test Runner`.
2. Select **EditMode**.
3. Click **Run All**.

The current user-verified result is **220 passing EditMode tests** as of July 14, 2026, including the seeded birth-to-ending harness. That harness is tagged `SlowSimulation`, so it can be selected or excluded with the Test Runner category filter. A full run should include it; a quick development run may exclude it. Its progress output reports elapsed milliseconds, simulated months, transaction count, maximum serialized-save length, and final Life Feed size.

The full-life test can pause the Test Runner briefly because it performs hundreds of transactional JSON clones and autosaves while the Life Feed grows; this is known test-path work, not a deadlock. Transactional autosaves now use compact rather than pretty-printed JSON to avoid unnecessary formatting allocation and whitespace. If tests do not appear after a code change, run `Assets → Refresh` and reopen Test Runner.

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
- Interactive slices use stable action IDs and persisted instance IDs so completion remains single-award across repeated taps and reloads.
- Every event outcome must include a non-zero numeric stat change, shown with an explicit `+` or `−` on the outcome card.
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
- [x] All five representative events
- [x] Monthly pay, annual rollover, and multi-event progression
- [x] Playable mobile UI vertical slice
- [x] Player overview for stats and secondary career details
- [x] Six finalized core stats in the save model and player overview
- [x] 220-test verified baseline
- [x] Seeded birth-to-ending simulation
- [x] Save migration fixtures
- [ ] Cloud-conflict tests
- [ ] iOS development build

## Save Format Decision

Keep the Stim-owned atomic JSON repository for the current beta path. Its readable versioned envelope, migrations, integrity checks, atomic replacement, and backup recovery are more valuable right now than switching formats.

The full-life Test Runner pause is primarily caused by repeatedly cloning and serializing a growing save in one synchronous simulation, not by the repository's disk-write implementation. The harness is now separately categorized and instrumented, and transactional output is compact JSON. Use its measurements to guide further optimization before evaluating another save package.

Profile save/load on physical iPhones before changing formats. If device profiling later shows unacceptable serialization time or file size, evaluate MessagePack behind `IStimSaveRepository`; do not replace the logical save envelope or migration boundary. Easy Save 3 remains optional and is not expected to solve the simulation-test cloning cost.

## Documentation

- [Master product definition and roadmap](<STIM_TYCOON_MASTER_README(2).md>)
- [Active next task list](docs/TASKS.md)
- [Current implementation audit](docs/IMPLEMENTATION_AUDIT_2026-07-13.md)
- [Package install checklist](docs/PACKAGE_INSTALL_CHECKLIST.md)
- [Phase 0 architecture checklist](docs/PHASE_0_ARCHITECTURE_CHECKLIST.md)
- [Event schema](docs/EVENT_SCHEMA.md)
- [Modifier rules](docs/MODIFIER_RULES.md)
- [Business turn design](docs/BUSINESS_TURNS.md)
- [Unity setup guide](UNITY_6_UPGRADE_GUIDE.md)

## Version Control

Commit Unity source assets, their `.meta` files, `Packages`, and relevant `ProjectSettings`. Do not commit generated folders such as `Library`, `Temp`, `Obj`, `Build`, `Logs`, or `UserSettings`.
