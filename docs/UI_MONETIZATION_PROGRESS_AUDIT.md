# Stim Tycoon UI and Monetization Progress Audit

**Audit basis:** `main` at commit `2210bc604c07d8f8a0af75a6cf8969317ac1f169`  
**Compared against:** Stim Tycoon Final UI, Asset, Mini-Game, Ads, and IAP Specification v2.0  
**Status date:** July 18, 2026

## Status legend

- `[x] ~~Crossed out~~` means implemented in the playable repository and connected to runtime state.
- `[ ] PARTIAL` means a working foundation or disabled scaffold exists, but the task is not complete.
- `[ ]` means no production implementation was found.
- `SUPERSEDED` means the product decision changed after the original specification.

## Product-decision corrections

- **SUPERSEDED:** `Sparks` is no longer the premium-currency name. The repository's canonical decision is **Legacy Gems**.
- Legacy Gems must remain separate from Cash, savings, debt, accounts, and net worth.
- Legacy Gems may be purchasable and occasionally earnable in bounded amounts, but cannot be required for ordinary progression, recovery, events, achievements, time advancement, saves, or endings.
- The Store remains outside the six-item bottom navigation and opens from the header wallet entry, upsells, settings, or insufficient-balance prompts.
- A Season Pass remains deferred and must not ship in the MVP.
- Current commerce elements are intentionally disabled presentation scaffolds. They do not represent functioning ads, purchases, rewards, entitlements, or premium currency.

## Overall progress

| Status | Count |
|---|---:|
| Implemented | 5 |
| Partial or scaffolded | 17 |
| Not started | 12 |
| Total audited implementation tasks | 34 |

The gameplay, save, Bank, Education, goal, career, business, relationship, and life-loop foundations are substantially farther along than the final visual, mini-game, and commerce layers.

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

# Phase 5: Monetization UI

- [ ] **SUPERSEDED NAME:** Build the **Legacy Gem Store**, replacing the earlier Spark Store concept.  
  No store screen, product rows, pack catalog, cosmetic catalog, restore flow, legal area, or purchase-result UI is implemented.
- [ ] Build the Stim+ Paywall.  
  No plan selector, localized subscription offer, benefit list, entitlement state, legal links, or member state is implemented.
- [ ] **PARTIAL:** Add rewarded-ad placements.  
  Nine stable disabled commerce slots exist in the header, Study, Work, Bank, Social, and Goals. They are correctly labeled unavailable and cannot mutate game state. A rewarded-ad prompt and active placement flow do not exist.
- [ ] Implement all purchase and ad states.  
  Loading, pending, success, cancelled, failure, restore, already-owned, expired, grace-period, duplicate callback, cap, cooldown, consent, ATT, and reward-pending states are not implemented.
- [ ] Connect localized product metadata.  
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

# Phase 7: Platform integration

- [ ] Configure products in App Store Connect and Google Play Console.
- [ ] **PARTIAL FOUNDATION ONLY:** Integrate Unity IAP.  
  The package is installed, but no concrete purchasing service, product catalog, callbacks, fulfillment, or entitlement storage was found.
- [ ] **PARTIAL FOUNDATION ONLY:** Integrate the selected ad provider.  
  LevelPlay is installed and an interface plus no-op adapter exist, but no production adapter, placement IDs, consent flow, callbacks, caps, or rewards are connected.
- [ ] Implement receipt validation and durable fulfillment.
- [ ] Test sandbox purchases, restores, cancellations, failed purchases, refunds, duplicate callbacks, and offline recovery.

---

# Screen-by-screen completion map

| Destination | Live foundation | Major remaining work | Status |
|---|---|---|---|
| Global shell | Reusable header, six-tab nav, modal arbitration, safe-area handling, scroll restoration | Final visuals, device matrix, settings, notifications, accessibility | Partial |
| Life | Feed, stats, focus actions, Life Summary, time controls, age strip | Next Up, daily-goal preview, archive/See All, final density/art | Partial |
| Study | Progress, paths, disciplines, timed sessions, claims | Study Match, path-detail polish, production illustration | Partial |
| Work | Career/business rules and actions, manual work, path preview | Focused dashboards, Shift Match, production hierarchy/art | Partial |
| Bank | Savings, history, credit, cash flow, investing, rollback | Quick-action presentation, tips, account details, edge states | Partial, strongest destination |
| Social | Relationships, discovery, profile/actions, saved family systems | Family workspace, candidate list, terminal states, portraits/history | Partial |
| Goals | Goals, achievements, progress, routing, claims | Pinned/manage layer, separate boards, complete visual states | Partial |
| Legacy Gem Store | Disabled header entry only | Entire store, wallet, economy, products, purchase flow | Not started |
| Stim+ | Disabled premium slots only | Entire paywall, subscription and entitlement lifecycle | Not started |
| Rewarded ads | Disabled placements and no-op boundary | Prompt, provider adapter, callbacks, limits, rewards, consent | Not started beyond scaffold |
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

## Not implemented

- [ ] One reusable match-game engine.
- [ ] Legacy Gem wallet and ledger.
- [ ] Legacy Gem Store.
- [ ] Stim+ paywall and entitlements.
- [ ] Active rewarded ads.
- [ ] Localized product metadata.
- [ ] Receipt validation and durable commerce fulfillment.
- [ ] Purchase, restore, refund, cancellation, duplicate, and offline test matrix.
- [ ] Production animations, haptics, sound hooks, and reduced-motion behavior.

# Recommended next execution order

1. Complete the cozy-corporate frontend layer in `FrontendCanvas.uss` and capture the required width/text screenshots.
2. Finish shared visual states, scrollbars, empty/loading/error components, and generic confirmation feedback.
3. Add Life `Next Up`, daily-goal preview, feed archive/See All, and recently changed stat treatment.
4. Converge Social/family, Work/business, and Goals into focused workspaces.
5. Build one persisted reusable match engine, then configure Study Match and Shift Match.
6. Design and simulate the Legacy Gem earn/spend economy and add the additive save-schema wallet/ledger.
7. Build the Legacy Gem Store and Stim+ paywall as disabled-data or sandbox-backed UI before enabling transactions.
8. Add concrete IAP and LevelPlay adapters, legal/privacy gates, verification, restore, caps, and sandbox tests.

# Source-of-truth paths

- `docs/TASKS.md`
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
