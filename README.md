# Stim Tycoon

**Status:** Broad offline gameplay foundation implemented; playable vertical slice active; Phase 5 experience convergence and production hardening incomplete

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
- a mobile UI Toolkit vertical slice with a compact player/cash header, six persistent destinations, age progression, timeline-style life feed, stat tiles, choices, outcomes, and autosave feedback
- age-appropriate destination presentation that omits school, adult career/business, investing, dating-discovery, and retirement options until their relevant life stage, while retaining actionable non-age lock reasons once an option is relevant
- Stim-owned theme/component boundaries in `StimTheme.uss` and `Components.uss`, licensed Lucide functional navigation icons, and emoji fallbacks for unfinished content imagery
- canonical reusable UXML contracts for section headers, feed rows, stat tiles, achievement rows, action cards, and information banners, plus a non-production brand component gallery
- Unity Device Simulator definitions for iPhone 17, iPhone 17 Pro, and iPhone 17 Pro Max, including conservative Dynamic Island and home-indicator safe-area baselines
- replaceable interfaces for dialogue, saves, accounts, cloud saves, ads, and event catalogs
- age-gated General, Academic, and Vocational study-track selection with authored material costs, affordability previews, transactional persistence, and Life Feed outcomes
- an age-appropriate Education catalog presenting Applied Finance, Community Health, and Sustainable Trades over the migration-safe study tracks, with qualification status, material requirements, career-direction consequences, and focused study-session confirmation
- a tabbed Bank workspace separating Savings, Credit/Cash Flow, and Investing while preserving exact/percentage transfers, transparent rates, transaction history, repayment, and index-fund eligibility
- easy, medium, and hard study sessions with explicit Smarts/Happiness tradeoffs, monthly cooldowns, qualification XP, and visible tier progress
- qualification-aware career application gates and authored event requirements that support Study Track and minimum Qualification XP
- visible Fitness and Professional skill paths with level/XP progress and downstream overtime and career-work benefits
- a safe Advance Year control that reuses ordinary monthly transactions, autosaves every month, summarizes progress, and stops for events, decisions, failures, or endings
- automated compact-width and 130% accessibility-text reflow rules with 44-point primary targets
- a recorded July 18, 2026 baseline of 729 passing EditMode test cases and 5 passing production-scene PlayMode smoke tests, covering the shared action contract, annual pacing, money, home, relationships and family, careers, business, goals, transitions, save safety, the 100-node staged Yarn/catalog contract, bounded history migration, UI structure, UI Builder template integration, responsive layout, and seeded long-run simulations
- a repository-owned QA foundation with production-scene PlayMode smoke tests, Yarn authoring-contract checks, Unity Code Coverage, local headless runners, and PR/nightly GitHub Actions configuration
- an explicit grouped UI binding manifest and disposable Shell binder that own the shared header, navigation, safe-area geometry, time controls, and enable/disable callback teardown without taking presentation ownership away from UI Builder

Not yet complete:

- Unity Authentication, Apple Game Center, and Cloud Save adapters
- configured Unity LevelPlay ads and Unity IAP purchasing flows (the packages are installed, but production adapters and store/placement configuration are not implemented)
- production navigation, accessibility, localization, art, and audio
- iOS device build and automated play-flow coverage
- reference-level destination UX: Bank and Education have functional first workspaces but still need presentation/edge-state convergence; Home, Social, Career/Business, and Goals need focused workspace convergence
- pacing, distribution, and human editorial approval for the 100 staged Childhood, School, Career, Health, and Money Yarn nodes; their validated definitions intentionally remain outside random selection
- broader controller/session decomposition
- broader portfolios and property, deeper books/equipment/inventory, additional businesses, and more authored relationship/career/event content
- production Settings, accessibility validation, pseudo-localization, licensed art/audio, and physical-device verification

## Current Focus

Milestones 1–12 and the M14 Bank/Education domain and initial-workspace implementation are complete. A fresh July 18, 2026 local headless run passed 729/729 EditMode plus 5/5 PlayMode smoke cases with retained NUnit and coverage artifacts. The active phase is **Phase 5 — Experience Convergence**. The M13 premium/paid-reward presentation scaffold is present as nine disabled, explicitly unavailable slots across the six-destination shell. It does not activate purchases, subscriptions, ads, premium-currency transactions, or reward behavior, and its structural and PlayMode safety coverage is green. W0 and W1 are complete: shared presentation states, modal arbitration, duplicate-safe retry and rollback behavior, exact modal return context, reload-safe workflows, and persisted destination/tab/entity/scroll navigation are implemented. M13 has an automated production-scene gate for the 320/390/430/768 × 100%/130% matrix; retained screenshots and human visual approval remain required. M14 still needs presentation edge states and a human comprehension check.

The path to completion is:

1. **M13 — Premium scaffold, navigation shell, and destination framework:** verify the disabled premium/paid-reward slots in Study, Work, Bank, Social, Goals, and the header money entry; then continue shared shell, state, restoration, safe-area, and device work.
2. **M14 — Bank and Education convergence:** finish Bank/Study presentation and edge states while preserving canonical APY, investment gates, qualification tiers, session durations, and stat tradeoffs instead of copying illustrative mockup values.
3. **M15 — Home, Social, and inventory convergence:** room-object home interactions, bounded inventory/timers, persistent relationship rows/profiles, complete terminal states, and focused household/family progress.
4. **M16 — Career, Business, Goals, and mini-games:** focused career/business workspaces, salary-consistent work actions, pinned Main/Daily/Life goals, and one persisted lifecycle shared by Study Match and Shift Match.
5. **M17 — Content, balance, and presentation:** more original content and breadth, transition scenes, economy tuning of every adopted mockup-like reward/wage/timer, and production art/audio.
6. **M18 — Production hardening and iOS beta:** Settings, accessibility, pseudo-localization, device/recovery profiling, and—only after separate approval—versioned commerce/service objects, privacy/licensing, and TestFlight readiness.

Mockup names, ages, balances, stat/relationship values, XP totals, prices, rewards, and timers are illustrative unless `docs/TASKS.md` explicitly adopts them. The six-destination monthly loop is authoritative. **Legacy Gems** are the approved single premium currency: purchasable and occasionally earnable in small bounded amounts, separate from cash/net worth, and never required for baseline progression or recovery. Their disabled wallet/reward presentation is required immediately; transactions, Store products, subscriptions, rewarded-ad behavior, and season systems remain gated. Stim Coins, the alternate five-tab shell, and action quotas/`End Turn` are retired/reference-only.

Every milestone also carries shared gates for save migration and rollback, bounded history growth, deterministic economy balance, authored-content validation, localization keys, accessibility, and offline-safe service boundaries. Money, business, annual-review, relationship, transition, Life Feed, and event histories now have explicit bounds; Life Feed and event archives retain compact major-outcome summaries.

### Frontend and wiring workflow

UI production is intentionally split. The frontend owner controls UXML composition, USS, art, responsive presentation, and runtime visual approval. The wiring owner controls named-element binding, view states, callbacks, domain/application services, age and requirement rules, persistence, rollback, content registration, accessibility semantics, and automated tests. Bound UXML `name` values are the API between those tracks; visual containers and classes may evolve without moving game rules into UXML or USS.

The playable scene now exposes its Panel Settings and Source Asset directly on `UIDocument`, uses one `EventSystem` with `InputSystemUIInputModule`, and treats the UI Builder-authored Feed Row, Achievement Row, and Action Card templates as the runtime hierarchy source. Sprite support is a direct package dependency, and runtime visual states are expressed through USS classes instead of inline colors.

The full handoff format, clean-branch workflow, controller-extraction direction, and immediate W0–W3 wiring backlog are in [the frontend/wiring collaboration contract](docs/FRONTEND_WIRING_WORKFLOW.md).

See [the active task list](docs/TASKS.md) for milestone acceptance criteria and shared completion gates.

The supplied screen references have also been decomposed into a screen-by-screen inventory of components, behaviors, branch states, current implementation status, and stable task IDs. See [the reference UI gap analysis](docs/REFERENCE_UI_GAP_ANALYSIS.md). That analysis adopts Legacy Gems as the single premium-currency identity while preserving the remaining commerce concepts without approving Stim Coins, action-energy quotas, alternate five-tab navigation, subscriptions, ads, or season passes as current gameplay.

The approved M13 UI direction is a compact, light mobile interface based on the supplied wireframes: an 88–96 point status header, dense list rows, and six icon-over-label navigation items. The imported kits are integrated through Stim-owned UXML/USS without editing vendor folders: Free Casual GUI supplies calibrated nine-sliced SVG controls, the palette, Baloo display type, and aspect-contained progress accents; Space Exploration GUI Kit supplies navigation, destination, section, and information identity; Jelly UI Pack supplies aspect-contained achievement and outcome marks while its palette informs claim, qualification, and input surfaces. `StimPanelSettings.asset` provides UI Toolkit's screen-size scaling equivalent to the Skyden uGUI demo. Production tests require `scale-to-fit` for unsliced art and complete approved slice values for responsive Skyden controls. Complex native-ratio panels, baked-copy panels, and fixed HUD decoration remain quarantined from responsive layouts. Independent destination scroll restoration, Bank tab restoration, and the review gallery support the migration. M13 still requires runtime width/text-scale visual approval and interaction-detail completion. The selected dependencies and release checks are recorded in [the UI asset manifest](Assets/UI/Art/ASSET_MANIFEST.md).

## Open and Run

1. Install Unity `6000.3.19f1` with iOS Build Support through Unity Hub.
2. Open this repository as an existing project and allow package import to finish.
3. Confirm the Console has no compilation errors.
4. Open `Assets/Scenes/StimVerticalSlice.unity`.
5. Press Play, resolve events, advance months, and use **View Player Overview** to inspect the current life state.

For mobile layout checks, open Device Simulator and select **Apple iPhone 17**, **Apple iPhone 17 Pro**, or **Apple iPhone 17 Pro Max**. If those profiles are missing after import, run `Tools → Stim Tycoon → Install iPhone 17 Simulator Profiles`, then reopen Device Simulator.

If the scene ever needs to be rebuilt, use:

`Tools → Stim Tycoon → Create Vertical Slice Scene`

For a setup audit, use:

`Tools → Stim Tycoon → Run Setup Check`

## Run Tests

In Unity:

1. Open `Window → General → Test Runner`.
2. Select **EditMode**.
3. Click **Run All**.

The repository has a clean **729 / 729 EditMode test-case** baseline and **5 / 5 PlayMode smoke** baseline recorded on July 18, 2026. EditMode includes the seeded birth-to-ending harness, pending-event recovery, resumable Advance Year, age/financial-agency guards, all 100 staged Yarn/catalog definitions, bounded history migration, and M13/M14 coverage. PlayMode boots the production scene and verifies its UIDocument, Input System EventSystem, navigation/overlay contract, controller lifecycle, disabled commerce slots, and responsive width/text-scale matrix. The full-life harness is tagged `SlowSimulation`, so it can be selected or excluded with the Test Runner category filter. A full verification run should include it; a quick development run may exclude it. Its progress output reports elapsed milliseconds, simulated months, transaction count, maximum serialized-save length, and final Life Feed size.

The full-life test can pause the Test Runner briefly because it performs hundreds of transactional JSON clones and autosaves while the Life Feed grows; this is known test-path work, not a deadlock. Transactional autosaves now use compact rather than pretty-printed JSON to avoid unnecessary formatting allocation and whitespace. If tests do not appear after a code change, run `Assets → Refresh` and reopen Test Runner.

The QA runner supports quick EditMode, production-scene PlayMode smoke, combined, full, and simulation-only runs:

```sh
scripts/qa/run-unity-tests.sh all
```

Results, logs, and coverage reports are written under ignored `Artifacts/` paths. GitHub Actions is configured in `.github/workflows/qa.yml`, and the three expected Unity secret names are present. Remote license activation and both test jobs must pass before merge; branch protection still needs to require `EditMode quality gate` and `PlayMode smoke gate` on `main`. See [the QA strategy](docs/QA_STRATEGY.md) for test tiers, evidence requirements, and rollout gates.

Odin Validator may be used as an optional licensed local serialized-asset check. Odin itself, its scripting defines, and its generated assets are not repository or CI dependencies and must not be committed to this public project.

## Packages

Already installed or built in:

- UI Toolkit and UI Builder
- Unity Input System
- Unity Test Framework
- Unity LevelPlay `9.5.0`
- Unity IAP `5.4.1`
- Yarn Spinner from its official Git repository
- native Stim save repository; Easy Save 3 is optional and not required

Still deferred until the related gameplay or production gate needs them:

- Unity Authentication and Cloud Save
- Apple GameKit / Game Center
- LevelPlay placement/consent configuration and its production Stim-owned adapter
- Unity IAP product catalog, restore/validation flows, and its Stim-owned adapter

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
│   └── Tests/             # EditMode contracts/simulations and PlayMode smoke suite
├── DeviceSimulatorDevices/ # Stim-owned iPhone 17 simulation definitions
└── UI/                    # Canonical UXML, USS, icons, and panel settings

Packages/                  # Pinned Unity dependencies
ProjectSettings/           # Unity project configuration
docs/                      # Architecture and gameplay specifications
scripts/qa/                # Headless local QA entry points
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
- [x] 729 / 729 EditMode and 5 / 5 PlayMode smoke test cases with clean headless baselines recorded July 18, 2026
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
- [Reference UI feature/state gap analysis](docs/REFERENCE_UI_GAP_ANALYSIS.md)
- [MVP interaction and navigation map](docs/UX_MAP.md)
- [Frontend/wiring collaboration contract](docs/FRONTEND_WIRING_WORKFLOW.md)
- [QA strategy and merge gates](docs/QA_STRATEGY.md)
- [Current implementation audit](docs/IMPLEMENTATION_AUDIT_2026-07-13.md)
- [Package install checklist](docs/PACKAGE_INSTALL_CHECKLIST.md)
- [Phase 0 architecture checklist](docs/PHASE_0_ARCHITECTURE_CHECKLIST.md)
- [Event schema](docs/EVENT_SCHEMA.md)
- [Content and progression standards](docs/CONTENT_PROGRESSION_STANDARDS.md)
- [Modifier rules](docs/MODIFIER_RULES.md)
- [Business turn design](docs/BUSINESS_TURNS.md)
- [Unity setup guide](UNITY_6_UPGRADE_GUIDE.md)

## Version Control

Commit Unity source assets, their `.meta` files, `Packages`, and relevant `ProjectSettings`. Do not commit generated folders such as `Library`, `Temp`, `Obj`, `Build`, `Logs`, or `UserSettings`.
