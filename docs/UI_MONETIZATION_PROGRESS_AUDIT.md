# Stim Tycoon UI and Monetization Progress Audit

**Audit basis:** `main` at commit `48419733c8700853189731461d7810b388631fc2`
**Compared against:** Stim Tycoon Final UI, Asset, Mini-Game, Ads, and IAP Specification v2.0
**Status date:** July 18, 2026

## Status legend

- `[x] ~~Crossed out~~` means implemented in the playable repository and connected to runtime state.
- `[ ] PARTIAL` means a working foundation or disabled scaffold exists, but the task is not complete.
- `[ ]` means no production implementation was found.
- `DEFERRED` means implementation is not authorized roadmap work until its named product, economy, legal, privacy, or service gate is approved.
- `SUPERSEDED` means the product decision changed after the original specification.

## Product-decision corrections

- **SUPERSEDED:** `Sparks` is no longer the premium-currency name. The repository's canonical decision is **Legacy Gems**.
- Legacy Gems must remain separate from Cash, savings, debt, accounts, and net worth.
- Legacy Gems may be purchasable and occasionally earnable in bounded amounts, but cannot be required for ordinary progression, recovery, events, achievements, time advancement, saves, or endings.
- The Store remains outside the six-item bottom navigation and opens from the header wallet entry, upsells, settings, or insufficient-balance prompts.
- A Season Pass remains deferred and must not ship in the MVP.
- Current commerce elements are intentionally disabled presentation scaffolds. They do not represent functioning ads, purchases, rewards, entitlements, or premium currency.
- Store, Stim+, rewarded-ad, IAP, and LevelPlay behavior are conditional M18 work, not committed near-term implementation. They remain inactive unless their separate commerce, economy, legal, privacy, age-treatment, and service gates are approved.

## Overall progress

| Status | Count |
|---|---:|
| Implemented | 5 |
| Partial or scaffolded | 17 |
| Not started | 12 |
| Total audited implementation tasks | 34 |

These counts describe implementation evidence, not roadmap priority or product approval. Deferred commerce items may appear in the partial or not-started totals. The gameplay, save, Bank, Education, goal, career, business, relationship, and life-loop foundations are substantially farther along than the final visual, mini-game, and commerce layers.

---

# Phase 1: Foundation

- [x] ~~Audit current UXML, USS, and C# bindings~~
  Protected binding names, a grouped binding manifest, structural tests, the frontend/wiring contract, and the Shell binder are present.
- [x] ~~Create tokens and theme files~~
  `StimTheme.uss`, `Shell.uss`, `Components.uss`, `Destinations.uss`, and the last-loaded `FrontendCanvas.uss` are canonical.
- [x] ~~Build reusable Player Header~~
  Implemented as `Assets/StimTycoon/UI/Components/AppHeader/AppHeader.uxml` and wired through `StimShellBinder`.
- [x] ~~Build reusable Bottom Navigation~~
  Implemented as `Assets/StimTycoon/UI/Components/BottomNavigation/BottomNavigation.uxml` with six exclusive destinations.
- [ ] **PARTIAL:** Fix safe areas, overlap, scrolling, and clipping.
  Safe-area geometry, persistent destination scroll offsets, simulator profiles, responsive rules, and automated containment checks exist. Live visual approval at 320, 390, 430, and 768 points, 130% text, visible scroll affordances, VoiceOver, and physical-device validation remain open.
- [ ] **PARTIAL:** Replace placeholder letter icons.
  Navigation and functional destination icons use imported pictograms, and many content placeholders use emoji. Relationship rows still generate letter avatars, generic placeholder slots remain, and production avatar/object/badge art is unfinished.

# Phase 2: Core components

- [ ] **PARTIAL:** Build `DataRow`.
  Reusable feed, achievement, account, path, relationship, stat, and section row factories exist. They are separate variants rather than one finalized shared row contract, and several lists still build bespoke rows.
- [ ] **PARTIAL:** Build `MetricCard`.
  Net-worth, balance, stat, progress, and summary cards exist in UXML and USS, but there is no finalized reusable `MetricCard` template or factory.
- [x] ~~Build `ActionCard`~~
  `ActionCard.uxml`, `StimActionCardFactory`, state mapping, progress, requirements, previews, and callbacks are implemented.
- [ ] **PARTIAL:** Build progress, chips, banners, and state components.
  Shared presentation states, progress families, requirement chips, segmented tabs, info callouts, signed result chips, and action states exist. Common visual polish, accessibility semantics, timers, cooldown rows, and complete branch coverage remain incomplete.
- [ ] **PARTIAL:** Build feedback modals and toasts.
  Event, Study confirmation, New Life, ending, result, rollback, retry, and return-context behavior exists. A reusable generic modal/toast system and commerce-specific feedback surfaces do not.

# Phase 3: Main screens

- [ ] **PARTIAL:** Refactor Life.
  Compact semantic Life Feed rows, five stat meters, focus actions, time controls, Life Summary, and a four-stage age strip are live. Dedicated `Next Up`, daily-goal preview, feed archive/See All, recently changed stat treatment, and final visual hierarchy remain open.
- [ ] **PARTIAL:** Refactor Study.
  Education progress, path catalog, three disciplines, qualification thresholds, transactional timed sessions, claim states, and edge-state copy exist. Study Match, final path-detail presentation, full interruption QA, and production art remain open.
- [ ] **PARTIAL:** Refactor Work.
  Age-aware path preview, career actions, promotions, retraining, firing/unemployment foundations, manual work, and Local Services business actions exist. The focused career/business dashboards, Shift Match, richer routing, and final visual hierarchy remain open.
- [ ] **PARTIAL, FUNCTIONALLY STRONG:** Refactor Bank.
  Savings, exact and percentage transfers, transaction history, 3.50% APY, projections, cash flow, credit repayment, index investing, contribution/performance reporting, age gates, rollback, and persistent tabs are implemented. Compact quick actions, account detail routes, financial tips, extreme or negative value states, and final presentation remain open.
- [ ] **PARTIAL:** Refactor Social.
  Relationship list, persistent selection, profiles, discovery, warmth, genetics, actions, romance, parenting, children, custody, and safety rules exist. Compact production rows, bounded candidate UX, relationship history, consent/end-state clarity, and a focused family workspace remain open.
- [ ] **PARTIAL:** Refactor Goals.
  Main, Daily, and Life goal models, destination routing, progress, claims, transactional rewards, achievements, and compact row templates exist. Pinned goals, Manage behavior, separate goal boards, final category hierarchy, and complete locked/active/claimable/claimed presentation remain open.

# Phase 4: Mini-game engine

- [ ] Build one reusable match engine.
- [ ] Implement Study Match through configuration.
- [ ] Add Shift Match and Legacy Gems themes through configuration.
- [ ] Add mini-game results, replay, cooldown, reward caps, pause/reload reconciliation, and optional rewarded multiplier flow.

No reusable match-board model, board state, tile configuration, scoring engine, mini-game screen, or persisted match session was found. Timed Study sessions are a separate commitment system and do not satisfy this phase.

# Phase 5: Monetization UI — Deferred Beyond the Disabled Scaffold

- [ ] **DEFERRED, SUPERSEDED NAME:** Build the **Legacy Gem Store**, replacing the earlier Spark Store concept, only after separate commerce approval.
  No store screen, product rows, pack catalog, cosmetic catalog, restore flow, legal area, or purchase-result UI is implemented.
- [ ] **DEFERRED:** Build the Stim+ paywall only if subscriptions are separately approved.
  No plan selector, localized subscription offer, benefit list, entitlement state, legal links, or member state is implemented.
- [ ] **PARTIAL SCAFFOLD; ACTIVE BEHAVIOR DEFERRED:** Add rewarded-ad placements.
  Nine stable disabled commerce slots exist in the header, Study, Work, Bank, Social, and Goals. They are correctly labeled unavailable and cannot mutate game state. A rewarded-ad prompt and active placement flow do not exist.
- [ ] **DEFERRED:** Implement purchase and ad states only after the corresponding product and service approvals.
  Loading, pending, success, cancelled, failure, restore, already-owned, expired, grace-period, duplicate callback, cap, cooldown, consent, ATT, and reward-pending states are not implemented.
- [ ] **DEFERRED:** Connect localized product metadata only after commerce approval.
  No runtime product catalog or store-localized title, description, price, billing period, or eligibility binding exists.

## Existing monetization foundation that must not be mistaken for completion

- Unity IAP `5.4.1` and Unity LevelPlay `9.5.0` are installed in `Packages/manifest.json`.
- `IStimAdsService` exists as an SDK boundary.
- The default runtime uses `NoOpAdsService`.
- No concrete LevelPlay adapter was found.
- No Unity Purchasing code or purchase-service abstraction was found.
- No Legacy Gem balance, ledger, receipt ID, grant, spend, restore, or entitlement model exists in the save schema.

# Phase 6: Art and polish

- [ ] **PARTIAL:** Replace temporary sprites.
  Approved vendor assets, a license/asset manifest, nine-slice mappings, icon bindings, and placeholder contracts exist. Original avatars, rooms, businesses, school art, badges, event art, mini-game tiles, and production illustrations remain unfinished. Emoji and generic placeholders are still active.
- [ ] Add animations, sound, haptics, and reduced-motion support.
  No complete production motion/audio/haptic system or reduced-motion setting was found.
- [ ] **PARTIAL:** Run device and accessibility QA.
  iPhone simulator profiles, responsive classes, text-scale code, structural tests, and PlayMode smoke tests exist. The full width/text matrix, VoiceOver, focus order, contrast, physical iPhones, touch momentum, nested scrolling, and final visual signoff remain open.

## Important visual-state note

`Assets/UI/Styles/FrontendCanvas.uss` is intentionally a neutral presentation baseline loaded after the structural styles. It currently clears inherited colors, borders, radii, and shape-bearing button/progress art while preserving layout and bindings. The final cozy-corporate visual pass should be implemented primarily in this file without renaming protected UXML elements or moving gameplay rules into USS.

# Phase 7: Platform integration — Conditional M18 Work

- [ ] **DEFERRED:** Configure products in App Store Connect and Google Play Console only after commerce approval.
- [ ] **DEFERRED, PARTIAL FOUNDATION ONLY:** Integrate Unity IAP after privacy, validation, restore, failure, and offline rules are approved.
  The package is installed, but no concrete purchasing service, product catalog, callbacks, fulfillment, or entitlement storage was found.
- [ ] **DEFERRED, PARTIAL FOUNDATION ONLY:** Integrate the selected ad provider after consent, privacy, age-treatment, cap, and reward rules are approved.
  LevelPlay is installed and an interface plus no-op adapter exist, but no production adapter, placement IDs, consent flow, callbacks, caps, or rewards are connected.
- [ ] **DEFERRED:** Implement receipt validation and durable fulfillment after commerce approval.
- [ ] **DEFERRED:** Test sandbox purchases, restores, cancellations, failed purchases, refunds, duplicate callbacks, and offline recovery after the approved service objects exist.

---

# Screen-by-screen completion map

| Destination | Live foundation | Major remaining work | Status |
|---|---|---|---|
| Global shell | Reusable header, six-tab nav, modal arbitration, safe-area handling, scroll restoration | Final visuals, device matrix, settings, notifications, accessibility | Partial |
| Life | Feed, stats, focus actions, Life Summary, time controls, age strip | Next Up, daily-goal preview, archive/See All, final density/art | Partial |
| Home | Persistent actions, condition, maintenance, upgrades, content definitions | Bounded inventory/timer contract, room/object workspace, offline reconciliation | Partial; M15 priority |
| Study | Progress, paths, disciplines, timed sessions, claims | Study Match, path-detail polish, production illustration | Partial |
| Work | Career/business rules and actions, manual work, path preview | Focused dashboards, Shift Match, production hierarchy/art | Partial |
| Bank | Savings, history, credit, cash flow, investing, rollback | Quick-action presentation, tips, account details, edge states | Partial, strongest destination |
| Social | Relationships, discovery, profile/actions, saved family systems | Family workspace, candidate list, terminal states, portraits/history | Partial |
| Goals | Goals, achievements, progress, routing, claims | Pinned/manage layer, separate boards, complete visual states | Partial |
| Legacy Gem Store | Disabled header entry only | Entire store, wallet, economy, products, purchase flow | Deferred pending approval |
| Stim+ | Disabled premium slots only | Entire paywall, subscription and entitlement lifecycle | Deferred pending approval |
| Rewarded ads | Disabled placements and no-op boundary | Prompt, provider adapter, callbacks, limits, rewards, consent | Scaffold only; behavior deferred |
| Mini-games | Reference placeholders only | Entire persisted reusable match framework and themes | Not started |

# Acceptance-criteria audit

## Implemented or substantially proven

- [x] ~~One reusable player header is used across the playable shell.~~
- [x] ~~One six-item bottom navigation is used across the playable shell.~~
- [x] ~~Visual tokens and canonical stylesheet ownership are centralized.~~
- [x] ~~Life Feed entries use compact reusable rows.~~
- [x] ~~Goal and achievement items use compact reusable rows.~~
- [x] ~~Baseline progression does not require ads or purchases.~~
- [x] ~~Existing gameplay mutations use transactional save/rollback foundations.~~

## Partial

- [ ] Safe-area and responsive behavior is structurally implemented but not visually approved on the full matrix.
- [ ] Shared row architecture exists but is not fully consolidated.
- [ ] Functional iconography is partly normalized, while placeholders and mixed temporary art remain.
- [ ] Primary-action hierarchy and final page density are not visually complete because the neutral frontend canvas is still active.
- [ ] Touch targets and overflow have automated coverage, but final live-device approval remains.

## Not implemented or approval-gated

- [ ] One reusable match-game engine.
- [ ] Legacy Gem wallet and ledger.
- [ ] Legacy Gem Store — deferred pending commerce approval.
- [ ] Stim+ paywall and entitlements — deferred pending subscription approval.
- [ ] Active rewarded ads — deferred pending product, privacy, and service approval.
- [ ] Localized product metadata — deferred pending commerce approval.
- [ ] Receipt validation and durable commerce fulfillment — deferred pending commerce approval.
- [ ] Purchase, restore, refund, cancellation, duplicate, and offline test matrix — deferred until approved service objects exist.
- [ ] Production animations, haptics, sound hooks, and reduced-motion behavior.

# Recommended next execution order

1. Close the M13 exit gate with live Play Mode approval and retained screenshots at 320, 390, 430, and 768 points at 100% and 130% text; verify safe areas, 44-point targets, scroll affordances, representative reward/claim feedback, and unobscured time controls.
2. Finish the remaining M13 shared visual states, scrollbars, empty/loading/error components, generic confirmation feedback, and navigation restoration details.
3. Complete Life `Next Up`, daily-goal preview, feed archive/See All, recently changed stat treatment, and time-control branch QA.
4. Execute M15 in its required order: bounded inventory/timer save contract, room/object Home workspace, Social discovery/profiles, then the focused family workspace and durable NPC triggers.
5. Execute M16 convergence for Career, Business, Goals, achievements, transitions, and their focused workspace states.
6. Build one persisted reusable match lifecycle, then configure Study Match and Shift Match on it.
7. Execute M17 content, balance, and production-presentation work; design and simulate the Legacy Gem earn/spend economy without activating a wallet, Store, purchases, subscriptions, or ads.
8. Execute M18 Settings, accessibility, pseudo-localization, device/recovery profiling, privacy/licensing, and iOS beta hardening.
9. During M18, add the already approved Legacy Gem foundation behind additive migration: wallet, bounded ledger, reason and transaction IDs, earned-versus-purchased attribution, atomic grant/spend, rollback, duplicate protection, offline behavior, and conflict rules—without activating products or paid rewards.
10. Only after their separate product, economy, legal, privacy, and service approvals, add Store, Stim+, rewarded-ad, IAP, or LevelPlay behavior with verification, restore, caps, offline handling, and sandbox tests.

# Source-of-truth paths

- `STIM_TYCOON_MASTER_README(2).md` — product definition and milestone sequence
- `docs/TASKS.md` — authoritative ordered implementation backlog
- `docs/REFERENCE_UI_GAP_ANALYSIS.md`
- `docs/FRONTEND_WIRING_WORKFLOW.md`
- `Assets/UI/StimVerticalSlice.uxml`
- `Assets/UI/Styles/StimTheme.uss`
- `Assets/UI/Styles/FrontendCanvas.uss`
- `Assets/StimTycoon/UI/Components/`
- `Assets/Scripts/Runtime/StimVerticalSliceController.cs`
- `Assets/Scripts/Runtime/StimUiComponentFactory.cs`
- `Assets/Scripts/Runtime/StimActionCardFactory.cs`
- `Assets/Scripts/Runtime/StimPresentationState.cs`
- `Assets/Scripts/Runtime/StimRuntimeCompositionRoot.cs`
- `Assets/Scripts/Domain/Save/StimSaveSchema.cs`
- `Packages/manifest.json`
