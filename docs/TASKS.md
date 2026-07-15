# Stim Tycoon — Completion Plan

This is the operational roadmap after the July 15, 2026 code/documentation audit. The repository contains 340 EditMode test methods and implemented Milestones 7–12. The master README remains the product definition.

## Current position

- [x] M1–M6 — offline architecture, complete-life loop, transactional action foundation, Education foundation, reusable monetary input, and visible skill paths
- [x] M7 — Advance Year, annual accumulator, Year in Review, and duplicate-safe annual rewards
- [x] M8 — savings, transaction history, grounded interest, cash flow, credit repayment, and gated index investing
- [x] M9 — persistent home actions, condition, maintenance consequences, content definitions, and upgrades
- [x] M10 — relationship discovery, adult romance, family planning, children, parenting, custody, and safety validation
- [x] M11 — three career industries and one complete operational business
- [x] M12 — Main/Daily/Life goals, achievement rewards, transition records, orientation, and alpha content validation
- [x] Clean Unity Run All result recorded for the current 340-test source suite on July 15, 2026

## Phase 5 — Experience Convergence

**Objective:** turn the broad, card-heavy vertical slice into focused mobile destinations with the clarity, interaction density, and progression visibility demonstrated by the references, while retaining original Stim Tycoon visuals, content, economy, resources, and writing.

### M13 — Navigation shell and destination framework

> **Approved asset direction:** Free Casual GUI is the visual foundation, Space Exploration GUI Kit guides layout and information hierarchy, and Jelly UI Pack supplies reward interaction accents. Vendor folders stay untouched; Stim-owned UXML and the `StimTheme.uss`/`Components.uss` adapter layer own composition and styling. See `Assets/UI/Art/ASSET_MANIFEST.md`.

**Required implementation workflow:** GUI work is source-controlled UI Toolkit work and should be completed primarily in VS Code. UXML owns layout, USS owns presentation and vendor-sprite references, and C# owns binding, data, and behavior. Unity UI Builder is an optional preview/layout-adjustment tool for the same UXML/USS assets, not a separate source of truth.

#### Immediate execution checklist

**Course-corrected visual target:** compact card-based mobile UI matching the approved reference hierarchy: an 88–96 point player/cash header, 24–28 point wrapped page titles, dense 44–64 point action rows, restrained white surfaces on a pale-blue canvas, and six icon-over-label navigation items. Large display typography, oversized dashboard cards, stretched source sprites, and horizontal content overflow are out of scope for the live shell.

Specification execution phases:

- [x] Phase 1A — centralize the supplied color, spacing, radius, and compatibility tokens in `StimTheme.uss`.
- [x] Phase 1B — establish the reusable compact player/cash header without changing its controller bindings.
- [x] Phase 1C — rebuild six-destination navigation with licensed Lucide SVGs, icon-over-label composition, active capsules, and 44-point targets.
- [ ] Phase 1D — complete Play Mode overlap, truncation, safe-area, and 320/390/430/768-width verification.
- [x] Add Unity Device Simulator profiles for iPhone 17, iPhone 17 Pro, and iPhone 17 Pro Max using native pixel dimensions and conservative Dynamic Island/home-indicator safe areas.
- [ ] Phase 2 — extract and adopt reusable `SectionHeader`, `FeedRow`, `StatTile`, `AchievementRow`, `ActionCard`, and `InfoBanner` components.
- [x] Replace temporary content-art glyphs with emoji imagery across live feed, life actions, education, career previews, goals, avatars, and reusable visual slots; retain Lucide SVGs for functional navigation.
- [x] Add the compact four-stage age progression strip required by the Life wireframe and bind its active/completed/locked state to player age.
- [ ] Phase 3 — add normalized original illustrations, one reusable mini-game framework with Study Match, and disabled/configurable Stim+ and sponsored placeholders.
- [ ] Phase 4 — add Shift Match, Legacy Gems, animation, haptic/audio hooks, and final presentation polish.

**Phase 2 wireframe contract:** the approved six-screen grayscale wireframe defines composition and density. Life uses an age strip, 4–6 compact timeline feed rows, compact stat tiles, and one aging-action row. Study and Work use progress/path modules plus one mini-game slot. Bank uses balance, quick actions, and compact accounts. Social uses compact relationship rows. Goals uses pinned goals and 48–58 point achievement rows. Monetization and mini-game areas remain labeled placeholders until their systems are implemented; they must not imply working purchases, ads, or rewards.

**Commerce wireframe boundary:** Store, Stim+, rewarded-ad, and season-pass wireframes are approved as future composition references only. Do not add Stim Coins, energy, purchasable progression, subscriptions, ad rewards, prices, or purchase buttons until M18 service/product configuration and a separate economy approval define their real behavior. Before then, only disabled, explicitly labeled placeholders with stable slot IDs are allowed, and baseline progression/recovery must remain unaffected.

### Unity Device Simulator targets

The editor installs these definitions into `Assets/DeviceSimulatorDevices` after script reload. They then appear in the Device Simulator device dropdown; if the window was already open, close and reopen it. The manual command is `Tools → Stim Tycoon → Install iPhone 17 Simulator Profiles`.

| Profile | Native portrait pixels | Logical points at 3× | Safe-area baseline |
|---|---:|---:|---:|
| Apple iPhone 17 | 1206 × 2622 | 402 × 874 | 62 pt top, 34 pt bottom |
| Apple iPhone 17 Pro | 1206 × 2622 | 402 × 874 | 62 pt top, 34 pt bottom |
| Apple iPhone 17 Pro Max | 1320 × 2868 | 440 × 956 | 62 pt top, 34 pt bottom |

The native display sizes come from Apple’s published technical specifications. Safe-area values are conservative simulator baselines for layout testing, not a substitute for validation on physical hardware and the current iOS SDK.

- [x] Confirm the live scene uses `Assets/UI/StimVerticalSlice.uxml` and preserve its stable controller bindings.
- [x] Treat `Assets/UI/Styles/StimTheme.uss` and `Assets/UI/Styles/Components.uss` as the only stylesheets referenced directly by the playable root.
- [x] Explicitly map the retained legacy layout rules through `Components.uss` until they are migrated component by component.
- [x] Apply bounded Stim-owned theme-adapter surfaces to representative primary/secondary actions and reward/claim feedback without stretching large source sprites onto layout containers.
- [x] Add structural tests that reject direct legacy stylesheet references and require the representative live vendor classes.
- [ ] Verify the live scene in Play Mode at 320/390/430/768 widths and record results.
- [ ] Repeat the width checks at 130% text scale and correct any clipping, overlap, or sub-44-point primary target.
- [ ] Replace the retained legacy layout import with destination/component-owned rules as each screen is extracted under `Assets/UI/Screens`.

1. Keep the imported vendor packs in their existing vendor-owned folders; place only Stim-owned derivatives, mappings, and manifest records in `Assets/UI/Art` and `Assets/UI/Icons`.
2. Build destination screens as `.uxml` files under `Assets/UI/Screens` and reusable UI elements under the designated Stim-owned component directory.
3. Use `Assets/UI/Styles/StimTheme.uss` as the canonical shared theme and `Components.uss` for reusable component rules; eliminate or explicitly map overlapping legacy theme files so the live scene cannot silently use the wrong stylesheet.
4. Reference the approved pack sprites from USS backgrounds and component classes. A sprite being imported but never referenced by a live UXML/USS asset is not implementation.
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
- [x] Establish six destinations with exclusive active states: Life/Home, Education, Career/Business, Bank, Social/Family, and Goals/Legacy.
- [x] Establish the approved three-pack UI direction through a replaceable Stim-owned USS adapter and an asset/license manifest, without reorganizing vendor imports.
- [ ] Apply the imported GUI packs visibly to the live `StimVerticalSlice` UI through Stim-owned UXML/USS: use Free Casual GUI for the primary panel/button/control surfaces, Space Exploration GUI Kit to reshape headers, status clusters, navigation, and information-dense layouts, and Jelly UI Pack for reward, claim, achievement, and celebratory interaction accents.
- [ ] Replace the current plain/default/placeholder presentation across the six-destination shell with the selected vendor sprites and Stim-owned themed derivatives; importing assets, listing them in the manifest, or defining unused USS tokens does not satisfy this task.
- [ ] Audit the overlapping `Assets/UI` and `Assets/StimTycoon/UI` UXML/USS trees, choose and document the canonical live paths, and consolidate or explicitly map the remaining files without breaking Unity `.meta` references. The scene, UI tests, and future destination work must all consume the same canonical theme and component set.
- [ ] Complete and document a live Play Mode visual verification at 320/390/430/768 widths showing that the approved assets are actually rendered in the status header, navigation, representative destination panels, primary/secondary actions, and at least one reward/claim flow.
- [ ] Add reusable destination headers, segmented tabs, modal sheets, requirement chips, action states, progress bars, timer/cooldown rows, and selected-navigation styling.
- [ ] Replace default/placeholder scrollbars throughout the application with one polished Stim scrollbar and scroll-affordance system for page, list, sheet, tab, and nested-scroll contexts; do not solve visual quality by hiding required position feedback.
- [ ] Complete the shared UI-detail pass for spacing, dividers, shadows, borders, pressed/hover/focus/disabled states, empty/loading/locked states, truncation/wrapping, and consistent icon/text alignment.
- [ ] Restore destination, tab, scroll position, and selected object/person after sheets and action resolution.
- [ ] Keep Advance Month/Year, pending decisions, transition presentations, and endings reachable and unobscured.
- [ ] Add structural and interaction coverage for navigation, overlays, back/close behavior, focus order, and state restoration.
- [x] Render Life Feed updates as a deterministic semantic ordered list with age/month/revision ordering, category context, numbered accessible item context, and no in-place save reordering.
- [x] Add a reusable Stim-owned visual-placeholder definition/factory with stable IDs, roles, aspect ratios, accessibility/decorative metadata, fallbacks, theme tokens, and development labeling.
- [ ] Place the reusable visual slots into destination heroes, event art, avatars, icons, objects, badges, and backgrounds; add bounded Life Feed archival behavior.

**Exit gate:** the live playable shell—not only mockups, manifests, imported folders, or unused style definitions—visibly renders the approved three-pack asset direction across its header, navigation, destination surfaces, controls, and reward feedback. It also passes 320/390/430/768 widths at 100% and 130% text, maintains 44-point primary targets, respects safe areas, has no navigation dead ends, and uses the approved scrollbar/scroll-affordance and shared UI-detail system in every implemented destination and overlay.

### M14 — Bank and Education convergence

- [ ] Build a Bank workspace with Savings, Credit/Cash Flow, and Investing tabs.
- [ ] Preserve exact/percentage transfers, transparent 3.50% APY, annual projections, bounded transaction history, credit repayment, and atomic rollback.
- [ ] Add readable portfolio contributions/performance without promised returns; keep casino content deferred.
- [ ] Build an Education catalog with study disciplines, qualification badges, progress, visible requirements, and `Go` links for resolvable locks.
- [ ] Build a focused study sheet with easy/medium/hard sessions, clear benefits/costs, duration/cooldown, progress, and single-claim completion.
- [ ] Add at least three original disciplines with distinct career/event consequences using reusable content definitions.
- [ ] Apply the documented stat, skill, qualification, wealth, task-reward, and locked-requirement thresholds; add reachability and pacing tests.

**Exit gate:** players can explain where money went, what interest/risk means, why a financial or education action is locked, and what each commitment changes.

### M15 — Home, inventory, Social, and family convergence

- [ ] Build a room/object Home workspace with visible condition, improvement progress, upgrade level, and interactive reading, training, rest, maintenance, and household objects.
- [ ] Add bounded books/equipment inventory with stock/capacity, active timers, offline reconciliation, consumption, and single-claim completion.
- [ ] Do not add energy, hunger, premium currency, or countdown pressure unless separately approved as real simulation systems.
- [ ] Build compatible-person discovery as a bounded list with deterministic persistent candidates and explicit refresh/cap behavior.
- [ ] Build relationship profiles with warmth, stage, history, cooldowns, requirements, consent state, and available actions.
- [ ] Surface partner, child, parenting, dependent-cost, and custody state in a family workspace.
- [ ] Add persistent NPC event triggers with deterministic priority, timing windows, cooldowns, cancellation rules, and death/consent/role/reload coverage.

**Exit gate:** every visible object/person has durable state, every action previews consequences, and reload/interruption cannot duplicate progress, inventory, relationships, or child outcomes.

### M16 — Career, Business, Goals, and transitions convergence

- [ ] Build a career workspace for industries, requirements, applications/interviews, performance, promotion, retraining, firing, unemployment, and retirement.
- [ ] Build the Local Services Co. dashboard for action points, work, revenue/expenses, staff, payroll, upgrades, locations, disruptions, valuation, failure, and sale.
- [ ] Build Main/Daily/Life goal boards with visible progress, `Go` navigation, and transactionally claimed non-premium rewards.
- [ ] Give birth/new life, graduation, marriage, parenthood, retirement, death, and legacy focused original presentations with concise durable consequences.
- [ ] Ensure all Phase 2–4 systems are reachable without searching a long mixed-purpose scroll.
- [ ] Populate age-appropriate Main/Daily/Life tasks, events, rewards, and recoverable outcomes to the per-stage minimums in the content standards.
- [ ] Add disabled optional-ad placeholders with stable slot IDs and previews; never make ads a task, gate, baseline reward, or recovery requirement.

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
- [ ] Run multiple seeded complete-life profiles and human comprehension/replay tests.

**Exit gate:** the complete authored catalog passes automated age-boundary checks and human age-appropriateness review; lives are visibly varied, outcomes are understandable, setbacks are recoverable, content repetition is bounded, and the art/audio direction is original and cohesive.

## Phase 7 — Production Hardening and iOS Beta

### M18 — Accessibility, reliability, services, and distribution

- [ ] Add Settings for text scale, reduced motion, sound/music, captions/text alternatives, haptics, and destructive-action confirmation.
- [ ] Complete VoiceOver labels, focus order, contrast, readable charts, dynamic text, fallback fonts, and pseudo-localization.
- [ ] Validate every scroll container with touch drag, momentum, mouse/trackpad wheel, keyboard/focus scrolling, VoiceOver, reduced motion, nested containers, and 130% text; confirm position/overflow remains perceivable without obstructing content.
- [ ] Run every screen and overlay at 320/390/430/768 widths and 100%/130% text scale.
- [ ] Validate migrations, corruption recovery, backup restore, downgrade behavior, bounded histories, and duplicate protection from representative old saves.
- [ ] Add privacy-safe diagnostics and performance markers for save failures, event pacing, economy balance, memory, and funnels without requiring an online SDK.
- [ ] Install on supported physical iPhones; profile save/load latency, save size, memory, safe areas, touch, thermal behavior, and complete-life stability.
- [ ] Freeze beta save semantics, then implement Authentication, Game Center, Cloud Save and conflict fixtures before account-enabled TestFlight.
- [ ] Configure the enabled Unity LevelPlay `9.5.0` and Unity IAP `5.4.1` packages behind Stim-owned adapters: use environment-specific app/ad-unit IDs and store product IDs, keep development builds in test mode, and define consent/ATT, privacy, age treatment, purchase restore/validation, cancellation, and offline/failure behavior. Ads and purchases must remain optional and must never gate baseline progression or recovery.
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
