# Stim Tycoon — Completion Plan

This is the operational roadmap after the July 17, 2026 code/documentation audit. Milestones 1–12 and the M14 Bank/Education implementation are in place. The M13 disabled premium/paid-reward presentation scaffold is implemented and awaits a clean Unity verification run; shell view state, modal arbitration, and physical-layout gates are next, and M15 begins after those gates. The master README remains the product definition. `REFERENCE_UI_GAP_ANALYSIS.md` is the detailed, deduplicated screen/component/behavior/branch-state comparison behind the task IDs used here.

## Current position

- [x] M1–M6 — offline architecture, complete-life loop, transactional action foundation, Education foundation, reusable monetary input, and visible skill paths
- [x] M7 — Advance Year, annual accumulator, Year in Review, and duplicate-safe annual rewards
- [x] M8 — savings, transaction history, grounded interest, cash flow, credit repayment, and gated index investing
- [x] M9 — persistent home actions, condition, maintenance consequences, content definitions, and upgrades
- [x] M10 — relationship discovery, adult romance, family planning, children, parenting, custody, and safety validation
- [x] M11 — three career industries and one complete operational business
- [x] M12 — Main/Daily/Life goals, achievement rewards, transition records, orientation, and alpha content validation
- [x] M14 implementation — persistent Bank/Education workspaces, timed study claims, progression standards, discipline consequences, and portfolio contribution/performance reporting
- [x] Clean headless result: 729 / 729 EditMode and 5 / 5 production-scene PlayMode smoke test cases passed on July 18, 2026, with retained NUnit and coverage artifacts
- [ ] M13 exit verification — 320/390/430/768 widths, 100%/130% text, safe areas, touch targets, and live Play Mode visual approval

**July 18 verification note:** a fresh local headless run passed 729/729 EditMode and 5/5 PlayMode smoke cases after adding validated definitions for all 100 staged nodes plus bounded, migration-safe Life Feed/event-history archival. The responsive smoke gate covers the 320/390/430/768 × 100%/130% matrix; retained screenshots, human visual approval, staged-content pacing/editorial approval, controller/session decomposition, physical-device validation, and production services remain incomplete.

## Parallel ownership and immediate wiring track

The frontend owner controls UXML/USS/art/responsive presentation and runtime screenshots. The wiring owner controls stable named-element contracts, binders/view state, callbacks, domain/application services, age and requirement behavior, persistence/rollback, Yarn/catalog parity, accessibility semantics, and automated tests. Follow `FRONTEND_WIRING_WORKFLOW.md`; do not move gameplay rules into UXML/USS or rename bound elements without a coordinated contract change.

> **Current UI handoff:** keep the repository UI as a stripped structural shell. Do not add, replace, or restyle production sprites as wiring/automation work. The frontend owner will load and place sprites directly in Unity UI Builder; code-side work should preserve named bindings, layout seams, behavior, accessibility, and testability unless a UI change is explicitly requested.

- [x] W0 — use one canonical launch catalog in the playable controller; compact large header money values while preserving exact accessible values.
- [x] W0 — grouped binding manifest plus header/navigation/safe-area/time-control Shell binder are live; destination rendering and global modal arbitration now sit behind the binder, shell actions are blocked behind modals, and behavior coverage protects the lifecycle seam.
- [x] W1 — implement the shared visual state machine, modal arbitration, confirmation/error/retry behavior, and reload-safe multi-step workflows.
- [ ] W2 — validated data-driven definitions and exact Yarn-to-catalog event/choice parity tests are complete for all 100 staged Childhood/School/Career/Health/Money nodes. Automated audits now cover event and non-event age boundaries, follow-up contracts, and deterministic weighted selection across childhood, school, teen, adult, and senior cohorts. Finish editorial review and gradual measured registration before random selection.
- [ ] W3 — wire destination slices in this order: Shell → Life → Home/Social → Work → Goals → reusable mini-games → Settings/services.

## Phase 5 — Experience Convergence

**Objective:** turn the broad, card-heavy vertical slice into focused mobile destinations with the clarity, interaction density, and progression visibility demonstrated by the references, while retaining original Stim Tycoon visuals, content, economy, resources, and writing.

### M13 — Navigation shell and destination framework

> **P0 UI priority override:** the disabled premium/paid-reward presentation scaffold shown by the approved six-destination mockups is now implemented and pending a clean Unity verification run. Presentation scaffolding does not activate purchases, ads, premium currency, subscriptions, reward multipliers, or entitlements.

> **Approved premium-currency decision:** **Legacy Gems** are Stim Tycoon's single premium currency. They will be purchasable and occasionally earnable in small, bounded gameplay awards. They are whole units, remain separate from cash/net worth/debt, and may never be required for ordinary time advancement, baseline progression, event resolution, recovery, earned achievement claims, or endings. Stim Coins are retired as a reference concept; do not create a second premium currency.

> **Approved component direction:** the supplied colored mobile references are the visual authority and Stim-owned UI Toolkit is the responsive component system. Free Casual GUI contributes calibrated nine-sliced SVG controls plus palette/type/progress accents, Space Exploration GUI Kit contributes pictograms, and Jelly UI Pack contributes reward marks. Vendor folders stay untouched. See `Assets/UI/Art/ASSET_MANIFEST.md`.

**Required implementation workflow:** UXML owns layout, USS owns presentation and sprite references, and C# owns binding, data, and behavior. For the current handoff, the repository provides a stripped shell and the frontend owner loads and places sprites in Unity UI Builder. Wiring work must not perform that sprite or final-presentation pass unless the frontend owner explicitly requests it; UI Builder changes still save back to the source-controlled UXML/USS assets.

#### Immediate execution checklist

**Course-corrected visual target:** compact card-based mobile UI matching the approved reference hierarchy: an 88–96 point player/cash header, 24–28 point wrapped page titles, dense 44–64 point action rows, restrained white surfaces on a pale-blue canvas, and six icon-over-label navigation items. Large display typography, oversized dashboard cards, stretched source sprites, and horizontal content overflow are out of scope for the live shell.

Specification execution phases:

- [x] **P0 premium presentation scaffold** — add Stim-owned premium/paid-reward icons and visibly disabled, explicitly labeled sections in the live shell: header money/store entry affordance; Study premium module; Work sponsored/rewarded module; Bank premium tools and optional reward module; Social premium module; Goals sponsored challenge, premium/season preview, and paid-reward/bonus-game preview. Nine stable COM slots now share an unavailable state and expose no price, entitlement, multiplier, transaction, or working reward.
- [x] Add structural and PlayMode tests proving every required premium/paid-reward slot is present, disabled, non-focus-trapping, accessibility-labeled as unavailable, omitted from baseline progression/tasks, and unable to mutate save state or invoke a vendor SDK.

Required initial slot IDs:

| Slot ID | Live placement | Disabled presentation role |
|---|---|---|
| `com.header.money_entry` | Header cash/net-worth cluster | Legacy Gem wallet/store entry icon with unavailable label |
| `com.study.premium_module` | Study | Premium study module |
| `com.work.rewarded_module` | Work | Sponsored/rewarded work section |
| `com.bank.premium_tools` | Bank | Premium budgeting/tools section |
| `com.bank.rewarded_module` | Bank | Optional paid/reward section |
| `com.social.premium_module` | Social | Premium social section |
| `com.goals.sponsored_challenge` | Goals | Sponsored challenge section |
| `com.goals.season_preview` | Goals | Premium/season preview section |
| `com.goals.bonus_game_preview` | Goals | Paid-reward/bonus-game preview section |
- [x] Phase 1A — centralize the supplied color, spacing, radius, and compatibility tokens in `StimTheme.uss`.
- [x] Phase 1B — establish the reusable compact player/cash header without changing its controller bindings.
- [x] Phase 1C — rebuild six-destination navigation with licensed Lucide SVGs, icon-over-label composition, active capsules, and 44-point targets.
- [ ] Phase 1D — complete Play Mode overlap, truncation, safe-area, and 320/390/430/768-width verification.
- [x] Add Unity Device Simulator profiles for iPhone 17, iPhone 17 Pro, and iPhone 17 Pro Max using native pixel dimensions and conservative Dynamic Island/home-indicator safe areas.
- [ ] Phase 2 — finish the custom responsive component system. `AppHeader` and `BottomNavigation` are live templates; static page composition stays in `StimVerticalSlice.uxml`; changing collections use the runtime factories. The other extracted UXML files are reviewable structure contracts until explicitly instantiated by a production screen.
- [x] Replace temporary content-art glyphs with emoji imagery across live feed, life actions, education, career previews, goals, avatars, and reusable visual slots; retain Lucide SVGs for functional navigation.
- [x] Add the compact four-stage age progression strip required by the Life wireframe and bind its active/completed/locked state to player age.
- [ ] Phase 3 — after the P0 scaffold exists, add normalized original illustrations and one reusable mini-game framework with Study Match; retain the premium/sponsored slots as disabled presentation until separately activated.
- [ ] Phase 4 — add Shift Match, mature the Legacy Gem presentation after its wallet/economy gates, and complete animation, haptic/audio hooks, and final presentation polish.

**Phase 2 wireframe contract:** the approved six-screen grayscale wireframe defines composition and density. Life uses an age strip, 4–6 compact timeline feed rows, compact stat tiles, and one aging-action row. Study and Work use progress/path modules plus one mini-game slot. Bank uses balance, quick actions, and compact accounts. Social uses compact relationship rows. Goals uses pinned goals and 48–58 point achievement rows. Monetization and mini-game areas remain labeled placeholders until their systems are implemented; they must not imply working purchases, ads, or rewards.

**Mockup value contract:** names, ages, balances, relationship scores, stat values, XP totals, rewards, prices, and timers pictured in the references are illustrative unless a task below explicitly adopts them. Runtime values must come from the saved simulation and canonical progression standards. Do not convert the header's career-progress binding into unlabeled global player XP, use a `$120` example as a one-hour wage, grant the pictured `+10 Smarts` mini-game reward, or adopt inconsistent example Bank balances without an authored rule and seeded balance proof.

**Commerce wireframe boundary:** the placement, hierarchy, iconography roles, and disabled sections from the Store, Stim+, rewarded-ad, and season-pass wireframes are now the highest-priority M13 presentation scaffold. Legacy Gems are the approved single premium-currency identity, but their wallet transactions, purchases, grants, and spending remain disabled until the versioned economy/service work below is complete. Do not add Stim Coins, energy, subscriptions, ad rewards, prices, or enabled purchase buttons before their gates. Every inactive slot must be explicitly labeled unavailable, backed by a stable ID, and unable to affect baseline progression/recovery.

**Reference-only concepts:** the alternate Home/School/Activities/City/Menu shell, per-turn action quota, `End Turn`, Stim Coins, season XP, and premium reward lanes do not replace the approved six-destination monthly loop. Legacy Gems supersede Stim Coins; season and premium-lane behavior still require their own approval and implementation gates.

### Unity Device Simulator targets

The editor installs these definitions into `Assets/DeviceSimulatorDevices` after script reload. They then appear in the Device Simulator device dropdown; if the window was already open, close and reopen it. The manual command is `Tools → Stim Tycoon → Install iPhone 17 Simulator Profiles`.

| Profile | Native portrait pixels | Logical points at 3× | Safe-area baseline |
|---|---:|---:|---:|
| Apple iPhone 17 | 1206 × 2622 | 402 × 874 | 62 pt top, 34 pt bottom |
| Apple iPhone 17 Pro | 1206 × 2622 | 402 × 874 | 62 pt top, 34 pt bottom |
| Apple iPhone 17 Pro Max | 1320 × 2868 | 440 × 956 | 62 pt top, 34 pt bottom |

The native display sizes come from Apple’s published technical specifications. Safe-area values are conservative simulator baselines for layout testing, not a substitute for validation on physical hardware and the current iOS SDK.

- [x] Confirm the live scene uses `Assets/UI/StimVerticalSlice.uxml` and preserve its stable controller bindings.
- [x] Treat `StimTheme.uss`, `Shell.uss`, `Components.uss`, and `Destinations.uss` as the four exclusive stylesheets referenced directly by the playable root.
- [x] Remove the retained legacy layout imports after assigning shell, component, and destination rules to their canonical owners.
- [x] Apply bounded Stim-owned theme-adapter surfaces to representative primary/secondary actions and reward/claim feedback without stretching large source sprites onto layout containers.
- [x] Add structural tests that reject direct legacy stylesheet references and require the representative live vendor classes.
- [ ] Verify the live scene in Play Mode at 320/390/430/768 widths and record screenshots/results. An eight-case production-scene smoke test now provides an early containment and persistent-navigation target-size gate, but retained visual approval remains required.
- [ ] Repeat the width checks at 130% text scale and correct any clipping, overlap, or sub-44-point primary target. Automated matrix coverage now verifies responsive state selection plus persistent navigation, time-control, Study-confirmation, event-continuation, and new-life target seams; destination and overlay screenshots remain open.
- [x] Replace the retained legacy layout import with shell-, destination-, and component-owned production rules.

1. Keep the imported vendor packs in their existing vendor-owned folders; place only Stim-owned derivatives, mappings, and manifest records in `Assets/UI/Art` and `Assets/UI/Icons`.
2. Build destination screens as `.uxml` files under `Assets/UI/Screens` and reusable UI elements under the designated Stim-owned component directory.
3. Use `Assets/UI/Styles/StimTheme.uss` as the canonical shared theme and `Components.uss` for reusable component rules; eliminate or explicitly map overlapping legacy theme files so the live scene cannot silently use the wrong stylesheet.
4. Use `scale-to-fit` for icons/progress and complete calibrated USS nine-slicing for approved Skyden SVG controls. Never stretch an undivided component image or place dynamic copy over an uncalibrated asset. A responsive component is complete when its hierarchy, density, states, text containment, and behavior match the reference contract.
5. Preview and adjust the UXML with Unity UI Builder as useful, then verify the actual `StimVerticalSlice` scene in Play Mode.
6. Give interactive UXML elements stable names and connect them to C# controllers/binders without moving gameplay rules out of the existing domain/runtime services.

```text
Assets/UI/
├── Art/                   # Stim-owned derivatives, mappings, and asset manifest
├── Icons/                 # Stim-owned icon selections/derivatives
├── Screens/               # HomeScreen.uxml, BankScreen.uxml, and other destinations
├── Components/            # Reusable headers, cards, tabs, sheets, and navigation
├── Styles/
│   ├── StimTheme.uss      # Canonical colors, spacing, typography, buttons, and cards
│   └── Components.uss     # Reusable component and vendor-sprite styling
└── Scripts/               # UI-only controllers/binders where needed
```

- [x] Implement a safe-area-aware persistent status header for age/calendar, cash/net worth, and Stim's actual stats/resources.
- [ ] Resolve the header XP semantic mismatch: label and bind the meter to a real contextual progression such as career or qualification progress, or define a genuine global player/legacy level before presenting it as generic XP.
- [x] Establish six destinations with exclusive active states: Life/Home, Education, Career/Business, Bank, Social/Family, and Goals/Legacy.
- [x] Establish the approved three-pack UI direction through a replaceable Stim-owned USS adapter and an asset/license manifest, without reorganizing vendor imports.
- [x] Apply the imported GUI packs visibly to the live `StimVerticalSlice` UI through Stim-owned UXML/USS: Free Casual GUI supplies calibrated nine-sliced SVG controls, the responsive palette/type language, and aspect-contained progress; Space Exploration GUI Kit owns navigation/destination/information identity; Jelly UI Pack supplies aspect-contained achievement/outcome marks plus the palette for claims, qualification, and inputs. Tests reject unsliced stretching and incomplete or unapproved nine-slicing.
- [ ] Replace the current plain/default/placeholder presentation across the six-destination shell with the selected vendor sprites and Stim-owned themed derivatives; importing assets, listing them in the manifest, or defining unused USS tokens does not satisfy this task.
- [x] Audit the overlapping `Assets/UI` and `Assets/StimTycoon/UI` UXML/USS trees, retain `Assets/UI/StimVerticalSlice.uxml` plus the four `Assets/UI/Styles` owners as production paths, and keep extracted templates under `Assets/StimTycoon/UI/Components`; structural tests protect these boundaries.
- [ ] Complete and document a live Play Mode visual verification at 320/390/430/768 widths showing that approved aspect-contained assets and kit-matched responsive surfaces render correctly in the status header, navigation, representative destination panels, primary/secondary actions, and at least one reward/claim flow.
- [ ] Add reusable destination headers, segmented tabs, modal sheets, requirement chips, action states, progress bars, timer/cooldown rows, and selected-navigation styling.
- [ ] Replace default/placeholder scrollbars throughout the application with one polished Stim scrollbar and scroll-affordance system for page, list, sheet, tab, and nested-scroll contexts; do not solve visual quality by hiding required position feedback.
- [ ] Complete the shared UI-detail pass for spacing, dividers, shadows, borders, pressed/hover/focus/disabled states, empty/loading/locked states, truncation/wrapping, and consistent icon/text alignment.
- [ ] Implement and test the shared branch-state vocabulary from `REFERENCE_UI_GAP_ANALYSIS.md`: age-absent, relevant-locked, available/selected, insufficient resources, confirming, active, cooldown, claimable, claimed, empty/exhausted, loading/offline/error/retry, saving/rollback, terminal, and restored navigation state.
- [ ] Restore destination, tab, scroll position, and selected object/person after sheets and action resolution. Per-destination scroll offsets, Life Summary return state, selected Social profiles, and Bank tab state are implemented; broader tab and post-action restoration remain.
- [x] Omit options that are not yet age-appropriate from destination UI; once relevant by age, show other unmet requirements as explicit locks rather than leaking future-life actions early.
- [ ] Keep Advance Month/Year, pending decisions, transition presentations, and endings reachable and unobscured.
- [ ] Add structural and interaction coverage for navigation, overlays, back/close behavior, focus order, and state restoration.
- [x] Render Life Feed updates as a deterministic semantic ordered list with age/month/revision ordering, category context, numbered accessible item context, and no in-place save reordering.
- [x] Add a reusable Stim-owned visual-placeholder definition/factory with stable IDs, roles, aspect ratios, accessibility/decorative metadata, fallbacks, theme tokens, and development labeling.
- [ ] Place the reusable visual slots into destination heroes, event art, avatars, icons, objects, badges, and backgrounds. Bounded Life Feed archival behavior is complete.
- [ ] Complete reference tasks LIFE-01–LIFE-07: compact signed feed rows with archive/`See all`, canonical age-strip placement, recently changed stat semantics, common action states, time-control scenario QA, `Next Up`, and a compact goal preview. LIFE-08 Settings/notification entry points remain M18 work; no action-energy quota is approved.
- [ ] Keep age as calendar/identity state rather than a sixth mutable core stat, and retain the canonical `0–12`, `13–18`, `19–26`, and `27+` life-stage boundaries unless a separately tested progression change is approved.

**Exit gate:** the live playable shell—not only mockups, manifests, imported folders, or unused style definitions—visibly renders the approved three-pack asset direction across its header, navigation, destination surfaces, controls, and reward feedback. It also passes 320/390/430/768 widths at 100% and 130% text, maintains 44-point primary targets, respects safe areas, has no navigation dead ends, and uses the approved scrollbar/scroll-affordance and shared UI-detail system in every implemented destination and overlay.

### M14 — Bank and Education convergence

**Status:** implementation complete and automation-verified. The human comprehension portion of the exit gate remains a playtest item; it does not block starting M15.

- [x] Build a Bank workspace with persistent Savings, Credit/Cash Flow, and Investing tabs; age-inappropriate adult tabs remain absent before age 18.
- [x] Preserve exact/percentage transfers, transparent 3.50% APY, annual projections, bounded transaction history, credit repayment, and atomic rollback.
- [x] Add readable portfolio contributions/performance without promised returns; keep casino content deferred.
- [x] Build an Education catalog with study disciplines, qualification badges, progress, visible requirements, and `Go` links for resolvable locks. Applied Finance, Community Health, and Sustainable Trades map migration-safely onto the established tracks, with earned/current/locked tier badges and actionable requirements.
- [x] Build a focused study sheet with easy/medium/hard sessions, clear benefits/costs, duration/cooldown, progress, and single-claim completion. Persisted 60/120/180-second sessions expose numeric previews, reserve the monthly action, show in-progress/claimable state in Education, withhold rewards until completion, and grant them through one transactional claim.
- [x] Add at least three original disciplines with distinct career/event consequences using reusable content definitions. Applied Finance, Community Health, and Sustainable Trades now map to distinct career outcomes and track-gated annual education challenges.
- [x] Apply the documented stat, skill, qualification, wealth, task-reward, and locked-requirement thresholds; add reachability and pacing tests. `StimProgressionStandards` now owns the core-stat bands, cumulative skill curve through Level 7, `50/125/250` qualification tiers, investing age/Smarts/emergency-savings boundary, `25/50/75` Finance ladder, level-scaled business progress, and Daily/Main/Life reward bands. Exact-boundary tests cover the shared contract and business locks; seeded pacing proves the easy-study route reaches Advanced within the teen window and the Finance ladder reaches Manager within two years of monthly actions.
- [ ] Complete the remaining reference presentation/edge-state tasks without reopening the finished domain foundation: STUDY-02/03 path detail and session-state consistency; graduated, left-school/no-path, and missing-state error presentations are now explicit. BANK-01/03/05/06/07 still needs extreme/negative values, account-detail routing, debt/investment edge states, and contextual financial tips. STUDY-04 is the shared mini-game work scheduled with M16.
- [ ] Preserve the canonical Bank and Study values during presentation convergence: 3.50% savings APY; $10 minimum index contribution; age 18, Smarts 40, education, and emergency-savings investment gates; `50/125/250` qualification tiers; and `60/120/180`-second study sessions with `+10/+20/+35` qualification XP and bounded `+1/+1/+2` Smarts tradeoffs.
- [ ] Treat the mockup's Checking Account as a presentation/detail route over Stim's existing cash wallet unless a separate checking product is deliberately added; Legacy Gems use a separate wallet and never enter cash, account balances, debt, or net-worth calculations.

**Exit gate:** players can explain where money went, what interest/risk means, why a financial or education action is locked, and what each commitment changes.

### M15 — Home, inventory, Social, and family convergence

**Execution order:** add the bounded inventory/timer save contract first; build the room/object Home workspace on that state; then converge discovery and relationship profiles; finish with the family workspace, NPC-trigger scheduler, reload/duplicate tests, and Play Mode validation.

- [ ] Converge the existing Home foundation into a room/object workspace with visible condition, improvement progress, upgrade level, and interactive reading, training, rest, maintenance, and household objects.
- [ ] Add bounded books/equipment inventory with stock/capacity, active timers, offline reconciliation, consumption, and single-claim completion.
- [ ] Do not add energy, hunger, a second premium currency, or countdown pressure. Legacy Gems are the sole approved premium currency and cannot replace Home, relationship, inventory, or recovery requirements.
- [ ] Converge existing compatible-person discovery into a bounded list with deterministic persistent candidates and explicit refresh/cap behavior.
- [ ] Converge existing relationship state/actions into focused profiles with warmth, stage, history, cooldowns, requirements, consent state, and available actions.
- [ ] Surface the implemented partner, child, parenting, dependent-cost, and custody state in a focused family workspace.
- [ ] Complete SOCIAL-01–SOCIAL-05 terminal states: compact/filterable rows, no candidates/no match/cap/refresh, no available actions, deceased/unavailable NPC, consent/relationship-end handling, cooldown presentation, and state restoration.
- [ ] Treat pictured people, roles, avatars, and relationship scores as display examples; populate rows only from persistent relationship IDs and saved 0–100 warmth/stage/history state.
- [ ] Add persistent NPC event triggers with deterministic priority, timing windows, cooldowns, cancellation rules, and death/consent/role/reload coverage.

**Exit gate:** every visible object/person has durable state, every action previews consequences, and reload/interruption cannot duplicate progress, inventory, relationships, or child outcomes.

### M16 — Career, Business, Goals, and transitions convergence

- [ ] Converge the implemented career foundation into a focused workspace for industries, requirements, applications/interviews, performance, promotion, retraining, firing, unemployment, and retirement.
- [ ] Converge the existing operational business into a Local Services Co. dashboard for action points, work, revenue/expenses, staff, payroll, upgrades, locations, disruptions, valuation, failure, and sale.
- [ ] Converge implemented Main/Daily/Life goals and transactional claims into focused boards with visible progress and `Go` navigation.
- [ ] Complete WORK-01–WORK-06 and GOAL-01–GOAL-03: persistent work-path routing, manual-work states, reusable Study Match/Shift Match lifecycle, pinned-goal management, compact categorized achievements, and locked/active/claimable/claimed presentation.
- [ ] Add one persisted reusable mini-game contract for Study Match and Shift Match with stable activity/instance ID, deterministic board seed, theme/content set, duration, start/end timestamps, score/target, reward preview, pause/reload reconciliation, timeout, success/failure, claim, replay, and cooldown state.
- [ ] Keep manual work economically coherent: `WORK 1 HOUR` continues to pay annual salary divided by 2,080; any mockup-style `$120` action must be authored as a longer shift with explicit duration/cooldown. Standardize business ownership presentation on the implemented age-18 gate rather than the references' conflicting 18/21 labels.
- [ ] Preserve separate Goal and Achievement objects while presenting the mockup's compact hierarchy: pinned/manageable Main/Daily/Life goals, categorized achievement rows, direct `Go`, claimable/claimed states, reset/expiry copy, and once-only transactional rewards.
- [ ] Give birth/new life, graduation, marriage, parenthood, retirement, death, and legacy focused original presentations with concise durable consequences.
- [ ] Ensure all Phase 2–4 systems are reachable without searching a long mixed-purpose scroll.
- [ ] Populate age-appropriate Main/Daily/Life tasks, events, rewards, and recoverable outcomes to the per-stage minimums in the content standards.
- [ ] Replace the M13 disabled Work/Goals paid-reward placeholders with configured behavior only after COM approval; never make ads a task, gate, baseline reward, or recovery requirement.

**Exit gate:** all major destinations are coherent playable workspaces and all completion/reward paths remain duplicate-safe and Life Feed backed.

## Phase 6 — Content, Economy, and Presentation Depth

### M17 — Replayable content and production presentation

- [ ] Complete a full age-appropriateness audit of every authored event, choice, outcome, reward, NPC role/trigger, follow-up, task, and visual; correct invalid age ranges and add boundary tests at every life-stage transition.
- [ ] Expand original health, school, drama, career, relationship, family, business, financial, and world-event chains with prerequisites, delayed consequences, and terminal outcomes.
- [ ] Add property and a small diversified portfolio only after existing Bank balance simulations and playtests remain stable.
- [ ] Add a second business only after the first business is understandable, balanced, and recoverable from failure.
- [ ] Expand books, equipment, home upgrades, education disciplines, career roles, goals, achievement rewards, and branch-aware ending summaries.
- [ ] Replace launch-blocking placeholder avatar, room, object, icon, font, motion, sound, and music assets; record licenses and attribution.
- [ ] Tune constrained, middle-income, and affluent lives so no dominant strategy erases health, education, relationship, or career tradeoffs.
- [ ] Balance every adopted mockup-like reward, wage, timer, threshold, and price against canonical progression; the pictured `$75–$250` rewards, `$1,635` balances, relationship scores, and stat totals are test fixtures/examples rather than target economy values.
- [ ] Define and simulate the Legacy Gem earn/spend economy: bounded gameplay sources, purchase packs, transparent sinks, grant cadence, wallet caps if needed, refund/restore rules, duplicate protection, and constrained/middle-income/affluent play profiles. No source or sink may create a dominant paid progression path.
- [ ] Run multiple seeded complete-life profiles and human comprehension/replay tests.

**Exit gate:** the complete authored catalog passes automated age-boundary checks and human age-appropriateness review; lives are visibly varied, outcomes are understandable, setbacks are recoverable, content repetition is bounded, and the art/audio direction is original and cohesive.

## Phase 7 — Production Hardening and iOS Beta

### M18 — Accessibility, reliability, services, and distribution

- [x] Add repository CI configuration for Unity `6000.3.19f1` EditMode and PlayMode checks, coverage, NUnit/log artifacts, and scheduled deterministic simulation; add matching local headless entry points and the production scene to Build Settings.
- [ ] Validate the populated Unity personal-license GitHub Actions secrets by completing remote activation; keep all values out of source control and replace the current rejected credentials/license payload.
- [ ] Prove the GitHub jobs on the remote runner and require `EditMode quality gate` plus `PlayMode smoke gate` on `main`; retain event-catalog/pending-event recovery, age-boundary, save-migration, UXML-binding, stylesheet-ownership, and supported-layout contracts in the blocking suite.
- [x] Establish the first production-scene PlayMode smoke layer for UIDocument/Panel Settings, the single Input System EventSystem, the six-destination/overlay binding contract, and controller disable/enable lifecycle.
- [ ] Expand PlayMode smoke coverage into user-driven New Life, navigation, Advance Month/Year interruption and resume, event resolution, Bank transfer, timed Study claim, save/reload, and second-life callback-leak journeys.
- [x] Add Yarn authoring-contract tests for unique node/event ownership, localization-safe resolved IDs, and exact `#choice:`/`stim_resolve_choice` parity.
- [ ] Add Settings for text scale, reduced motion, sound/music, captions/text alternatives, haptics, and destructive-action confirmation.
- [ ] Complete VoiceOver labels, focus order, contrast, readable charts, dynamic text, fallback fonts, and pseudo-localization.
- [ ] Validate every scroll container with touch drag, momentum, mouse/trackpad wheel, keyboard/focus scrolling, VoiceOver, reduced motion, nested containers, and 130% text; confirm position/overflow remains perceivable without obstructing content.
- [ ] Run every screen and overlay at 320/390/430/768 widths and 100%/130% text scale.
- [ ] Validate migrations, corruption recovery, backup restore, downgrade behavior, bounded histories, and duplicate protection from representative old saves.
- [x] Add explicit Life Feed and event-history retention/archive policies with migration-safe tests: Life Feed retains 256 entries, event history retains 128 entries, and both contribute to a bounded 32-item major-summary archive.
- [ ] Add privacy-safe diagnostics and performance markers for save failures, event pacing, economy balance, memory, and funnels without requiring an online SDK.
- [ ] Install on supported physical iPhones; profile save/load latency, save size, memory, safe areas, touch, thermal behavior, and complete-life stability.
- [ ] Freeze beta save semantics, then implement Authentication, Game Center, Cloud Save and conflict fixtures before account-enabled TestFlight.
- [ ] Configure the enabled Unity LevelPlay `9.5.0` and Unity IAP `5.4.1` packages behind Stim-owned adapters: use environment-specific app/ad-unit IDs and store product IDs, keep development builds in test mode, and define consent/ATT, privacy, age treatment, purchase restore/validation, cancellation, and offline/failure behavior. Ads and purchases must remain optional and must never gate baseline progression or recovery.
- [ ] Implement the approved Legacy Gem foundation behind additive migration: whole-unit wallet balance, append-only bounded ledger, source/sink reason codes, transaction/receipt IDs, earned-versus-purchased attribution, atomic grant/spend APIs, insufficient-balance handling, rollback, duplicate receipt/grant rejection, offline behavior, and cloud-conflict rules.
- [ ] Keep Store products, Stim+, rewarded ads, and season/event behavior inactive until their separate product/economy/legal gates. If approved, implement localized product loading, restore, cancellation, failure, entitlement, expiry, offline, duplicate-callback, cooldown/cap, and age-treatment states using Legacy Gems as the only premium currency.
- [ ] Introduce explicit versioned models for product catalog/localized price, entitlement and restore state, purchase transaction state, ad placement/reward/cap state, subscription expiry/grace period, season definition/XP/tracks, and duplicate-safe reward claims before enabling the corresponding commerce controls.
- [ ] Prepare build/signing, entitlements, privacy manifest/disclosures, licenses, known issues, rollback build, tester instructions, and TestFlight checklist.
- [ ] Resolve all critical/high defects and complete one clean physical-device birth-to-ending run.

**Exit gate:** an approved beta candidate has a clean automated suite, accessibility/device matrix, safe migration/recovery, complete privacy/licensing documentation, and a stable physical-device life playthrough.

## Shared definition of done

Every milestone must satisfy all applicable rules:

- Stored model changes have additive idempotent migration, validation, old-save fixtures, and rollback coverage.
- Persistent lists have explicit retention limits and tests.
- Monetary values use integer minor units; rates and costs are authored and visible; long-run economy changes have seeded simulations.
- Actions have stable IDs, explicit availability/locked reasons, signed previews, atomic autosave, single-award completion, and Life Feed output.
- Timed work survives reload and UTC reconciliation without ads or premium payment being required.
- Authored content has original copy/assets, localization-safe IDs, eligibility/risk/editorial validation, diagnostic tags, cooldowns, and reachable terminal branches.
- UI passes supported widths, text scales, safe areas, keyboard/focus behavior, touch targets, and accessibility labels.
- External services remain optional offline-safe adapters with documented privacy and failure behavior.
