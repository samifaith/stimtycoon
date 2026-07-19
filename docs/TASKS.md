# Stim Tycoon — Master Task List

This is the sole product roadmap and ordered implementation backlog for Stim Tycoon. It incorporates the final UI, asset, mini-game, ads, and IAP specification v2.0 and reconciles that specification with the working Unity project.

The root `README.md` is only the repository guide. Supporting documents under `docs/` may explain a subsystem but must not establish a competing roadmap or product decision.

## Authority and locked decisions

The following decisions supersede older README, mockup, audit, and backlog language.

### Product and navigation

- Target teens and adults; do not position the game as an Apple Kids Category app.
- Retain six persistent destinations: **Life, Study, Work, Bank, Social, and Goals**.
- Retain the monthly life-simulation loop. Do not add action energy, hunger, an action quota, or `End Turn`.
- Store is a routed screen opened from the premium balance, plus button, offers, Settings, or insufficient-currency flow. It is not a seventh navigation destination.
- Mockup names, balances, stats, prices, rewards, timers, and relationship values are illustrative unless adopted below.

### Economy terminology

- **Cash** is the soft currency used by the life simulation.
- **Sparks** are the single premium currency. They use a separate whole-unit wallet and never enter cash, savings, debt, or net-worth calculations.
- **XP** is progression and is not spendable.
- **Legacy Points** are deferred generational progression and are not a launch currency.
- **Legacy Gems** is a Goals-themed match mini-game, not a currency or wallet.
- Migrate all current `Legacy Gem` premium-wallet presentation and task language to `Sparks` before enabling commerce. Do not add a second premium currency.
- Sparks may fund optional cosmetics, rerolls, bonus actions, mini-game continues, or transparent temporary boosts. They may never be required for ordinary time advancement, baseline education/career progress, event resolution, recovery, relationships, earned reward claims, saves, or endings.
- Never sell core stats, age, relationships, or guaranteed career outcomes directly.

### Launch monetization

Approved launch scope:

- optional rewarded ads;
- Spark packs;
- one-time Starter Pack;
- Remove Ads, only if it removes meaningful prompts or provides a clear alternate reward path;
- cosmetic packs;
- one Stim+ subscription.

Do not launch passive banners, forced interstitials, loot boxes, paid core-stat upgrades, multiple subscriptions, or a Season Pass. Season work remains feature-flagged post-launch scope.

No commerce control may activate until its economy, migration, fulfillment, restore, privacy, age-treatment, offline, legal, and sandbox gates pass.

### Mini-games

- Build one reusable deterministic match engine.
- Configure Study Match, Shift Match, and Legacy Gems as themes over the same board, input, timer, scoring, reward, result, reload, and claim lifecycle.
- At least one functional theme ships in MVP.
- Rewards require caps and duplicate-safe transactional fulfillment.

### UI and implementation

- Preserve working gameplay logic and stable named bindings.
- UXML owns structure, USS owns presentation, and C# owns behavior and data.
- Use shared components before screen-specific variants.
- Use a cream header, pale-blue page, light cards, navy text, lavender premium accents, coral/blue primary actions, and normalized flat 2D art.
- Do not bake dynamic text, prices, balances, ownership, or rewards into images or UXML.
- Common portrait widths are 320, 390, 430, and 768 points, at 100% and 130% text.
- Primary touch targets must be at least 44 points.

## Current position — July 19, 2026

- [x] M1–M6: offline architecture, complete-life loop, transactional action foundation, Education foundation, monetary input, and visible skills.
- [x] M7: Advance Year, annual accumulation, Year in Review, and duplicate-safe annual rewards.
- [x] M8: savings, transactions, interest, cash flow, credit repayment, and index investing.
- [x] M9: persistent Home actions, condition, maintenance, content definitions, and upgrades.
- [x] M10: persistent relationships, romance, family planning, children, parenting, custody, and validation.
- [x] M11: three career industries and one complete operational business.
- [x] M12: Main/Daily/Life goals, achievements, transitions, orientation, and alpha-content validation.
- [x] M13 shell foundation: six destinations, shared header/navigation, safe-area seams, disabled commerce slots, presentation states, modal arbitration, and restoration.
- [x] M14 domain foundation: persistent Bank/Education workspaces, timed study claims, qualification progression, and portfolio reporting.
- [x] W0/W1: binding manifest, shell binder, shared state machine, retries, rollback, and reload-safe workflows.
- [x] W2 automation: 100 staged Yarn/catalog definitions with parity, age, follow-up, distribution, copy, and dynamic-reward audits.
- [x] Latest retained automation is recorded in the canonical [QA baseline](QA_BASELINE.md).
- [ ] W2 editorial release: human approval and measured staged-event cohort rollout.
- [ ] Production presentation and genuine 130% typography remain open, but do not block destination functionality work.

## Execution order

Work in this order unless a blocking defect requires a smaller prerequisite fix:

1. M15 Home, inventory, Social, and family convergence.
2. M16 Career, Business, Goals, transitions, and reusable match engine.
3. M14 remaining Bank and Study edge-state convergence.
4. M13/M17 production presentation and asset convergence.
5. M17 content and economy depth.
6. M18 services, accessibility, iOS hardening, and launch preparation.

W2 editorial approval and small staged-event cohorts may proceed in parallel.

## M15 — Home, inventory, Social, and family

### Home workspace

- [x] Turn the existing Home foundation into a focused room/object workspace.
- [x] Surface condition, improvement progress, upgrade level, maintenance state, and household effects.
- [x] Route reading, training, rest, maintenance, and household-object actions through existing transactional services.
- [x] Preserve age gates, affordability, signed previews, cooldowns, Life Feed output, and reload safety.
- [x] Restore the selected room/object and scroll position after actions and overlays.

### Inventory and timers

- [x] Add bounded books and equipment inventory with stable item IDs, quantity, capacity, and acquisition source.
- [x] Add active timers with UTC reconciliation, pause/reload behavior, expiry, and single-claim completion.
- [x] Define consumption, replacement, capacity-full, missing-item, expired, claimable, claimed, and recovery states.
- [x] Do not introduce energy, hunger, pay-to-recover requirements, or countdown pressure.

### Social and family

- [x] Present deterministic persistent relationship candidates in a bounded, refreshable list.
- [x] Build focused profiles with warmth, stage, consent, history, cooldowns, requirements, and available actions.
- [x] Surface partner, child, parenting, dependent-cost, household, separation, divorce, custody, death, and unavailable states.
- [x] Add deterministic NPC event triggers with priority, timing windows, cooldowns, cancellation, and reload coverage.
- [x] Restore selected person, filter, list position, and return context.
- [x] Populate UI only from saved relationship IDs and canonical 0–100 relationship state.

### M15 acceptance

- [ ] Every Home/Social action uses the shared action contract and atomic save flow.
- [ ] Empty, locked, insufficient-resource, active, cooldown, terminal, error, retry, rollback, and restored states are covered.
- [x] Persistent collections are bounded and migration-tested.
- [x] Targeted EditMode and production-scene PlayMode tests pass.

## M16 — Work, Goals, transitions, and match engine

### Career and business

- [x] Build focused career routing for industries, requirements, applications, interviews, performance, promotion, retraining, firing, unemployment, and retirement.
- [x] Build a Local Services Co. dashboard for action points, work, revenue, expenses, staff, payroll, upgrades, locations, disruptions, valuation, failure, and sale.
- [x] Keep manual work economically coherent: one hour pays annual salary divided by 2,080; longer authored shifts expose duration and cooldown.
- [x] Retain the implemented age-18 business gate unless a separately tested rule changes it.

### Goals and transitions

- [x] Present Main, Daily, and Life goals as focused boards with pinning, progress, direct `Go`, expiry/reset, and transactional claim states.
- [x] Keep Goals and Achievements as separate saved objects while sharing compact row components.
- [x] Give birth/new life, graduation, marriage, parenthood, retirement, death, and legacy focused durable presentations.

### Reusable match engine

- [x] Define a persisted match session with stable activity/instance ID, deterministic board seed, theme, duration, timestamps, score, target, and reward preview.
- [x] Implement board generation, matching, cascades, legal-move detection, reshuffle, pause/reload reconciliation, timeout, success/failure, and results.
- [x] Implement duplicate-safe claim, replay, cooldown, cap, and optional rewarded-multiplier state.
- [x] Configure Study Match first, then Shift Match and Legacy Gems without separate game codebases.
- [x] Provide reduced-motion behavior and input/touch accessibility.

### M16 acceptance

- [x] Work and Goals systems are reachable without searching a mixed-purpose scroll.
- [x] Match themes are configuration-driven and deterministic under test.
- [x] Rewards are capped, balanced, transactional, and never require ads or Sparks.
- [x] Save/reload at every match lifecycle state is covered.

## M14 completion — Bank and Study edge states

- [x] Finish Study path detail and session-state consistency.
- [x] Preserve canonical qualification tiers `50/125/250`, session durations `60/120/180` seconds, XP `+10/+20/+35`, and bounded Smarts effects `+1/+1/+2` unless balance tests approve a change.
- [x] Finish Bank extreme/negative values, account details, debt/investment edge states, and contextual tips.
- [x] Preserve 3.50% savings APY, $10 minimum index contribution, and the current investment eligibility gates unless balance tests approve a change.
- [x] Treat Checking as a detail route over the cash wallet unless a separate product is deliberately modeled.
- [x] Keep Sparks separate from every Bank and net-worth calculation.

## M13/M17 — Production UI and assets

- [x] Standardize production screens on UI Toolkit; retain only the Input System EventSystem required for runtime input.
- [x] Consolidate approved copied art under `Assets/StimDesignSystem`, move retained imports under `Assets/ThirdParty`, and remove demos, size variants, example fonts, and unrelated imagery.
- [ ] Replace the stripped presentation canvas with the approved centralized token and component system.
- [ ] Complete shared destination headers, segmented tabs, modal sheets, requirement chips, progress, timer/cooldown rows, selected navigation, feedback, and branch states.
- [ ] Replace placeholder/default scrollbars with a perceivable, accessible scroll-affordance system.
- [ ] Implement actual 130% typography rather than reflow-only class changes.
- [ ] Resolve the generic header XP mismatch by binding it to a real named progression value.
- [ ] Normalize functional icons, custom illustrations, avatars, rooms, objects, rewards, careers, finance, social, goals, monetization, and mini-game assets.
- [ ] Keep text out of production sprites; use SVG for functional icons and PNG/sprite sheets/nine-slice assets where appropriate.
- [ ] Add restrained animation, sound, haptics, and reduced-motion behavior.
- [ ] Re-run and approve the 48-image 320/390/430/768 × 100%/130% matrix.

## M17 — Content and balance depth

- [ ] Complete human editorial review of the 100 staged events.
- [ ] Activate small category/life-stage cohorts behind the deterministic rollout cap and measure pacing before expansion.
- [ ] Expand original health, school, drama, career, relationship, family, business, financial, and world-event chains.
- [ ] Complete age-boundary audits for events, choices, outcomes, rewards, NPC roles, tasks, and visuals.
- [ ] Expand education, career, business, Home, inventory, goal, achievement, and ending breadth.
- [x] Add a playable starter rental portfolio: purchase, mortgage, rent, vacancy, maintenance, appreciation, sale, ledger, and net worth.
- [x] Add save-safe portfolio slots and visible skeletons for two additional businesses; full economics remain post-MVP.
- [ ] Simulate constrained, middle-income, and affluent lives and remove dominant strategies.
- [ ] Run human comprehension and replay tests.

## M18 — Services, monetization, accessibility, and iOS beta

### Settings and accessibility

- [ ] Add Settings for text scale, reduced motion, sound/music, captions/text alternatives, haptics, notifications, and destructive confirmations.
- [ ] Complete VoiceOver labels, focus order, contrast, dynamic text, fallback fonts, readable charts, and pseudo-localization.
- [ ] Validate every scroll surface with touch, mouse/trackpad, keyboard/focus, VoiceOver, reduced motion, nested scrolling, and 130% text.

### Spark economy and durable models

- [ ] Add an additive migration for a whole-unit Spark wallet and bounded append-only ledger.
- [ ] Track reason code, transaction/receipt ID, earned/purchased attribution, balance before/after, timestamp, and duplicate key.
- [ ] Implement atomic grant/spend, insufficient balance, rollback, duplicate receipt/grant rejection, refund/revocation, offline, and cloud-conflict rules.
- [ ] Define and simulate Spark sources, packs, caps, sinks, costs, and progression impact before enabling spending.
- [ ] Rename current disabled Legacy Gem wallet/store presentation to Sparks; retain Legacy Gems only as the Goals match theme.

### Store and Stim+

- [ ] Build a routed Spark Store with localized product metadata, balance, packs, Starter Pack, cosmetics, Remove Ads where meaningful, restore, and legal links.
- [ ] Build a separate Stim+ paywall with monthly/yearly plan metadata, clear recurring benefits, renewal terms, restore, entitlement, expiry, grace period, cancellation, and offline states.
- [ ] Never hardcode displayed platform prices.
- [ ] Configure environment-specific product IDs and keep development builds in sandbox/test mode.
- [ ] Grant consumable Sparks only after verified, durably recorded fulfillment.

### Rewarded ads

- [ ] Enable only optional rewarded placements with clear reward previews, explicit initiation, cooldowns, daily caps, unavailable/failure states, and duplicate-safe grants.
- [ ] Do not show ads on first launch, before page inspection, during mandatory resolution, or as a baseline/recovery requirement.
- [ ] If Remove Ads ships with rewarded-only ads, make its prompt-removal and alternate-reward value explicit.

### Reliability and distribution

- [ ] Expand PlayMode journeys through New Life, navigation, time advancement, events, Bank, Study timer claim, save/reload, and second-life callback teardown.
- [ ] Validate migrations, corruption recovery, backup restore, downgrade behavior, bounded histories, and duplicate protection from representative old saves.
- [ ] Add privacy-safe diagnostics and performance markers without requiring an online SDK.
- [ ] Implement Authentication, Game Center, Cloud Save, and conflict fixtures after beta save semantics freeze.
- [ ] Configure consent/ATT, privacy manifests/disclosures, age treatment, purchase restore/validation, licensing, and legal links.
- [ ] Validate App Store sandbox purchases, restores, pending/failure, refunds/revocations, entitlement expiry, ad failure, and offline behavior.
- [ ] Install on supported physical iPhones and profile save/load, memory, safe areas, touch, suspend/resume, thermal behavior, and complete-life stability.
- [ ] Prove GitHub Actions remotely and require the EditMode and PlayMode quality gates on `main`.
- [ ] Prepare signing, entitlements, build, privacy, licenses, known issues, rollback build, tester instructions, and TestFlight release checklist.
- [ ] Resolve critical/high defects and complete one clean physical-device birth-to-ending run.

## Post-launch

- [ ] Consider Legacy Points and generational upgrades only after launch balance is stable.
- [ ] Consider a feature-flagged Season Pass only after separate product, economy, legal, and retention approval.
- [ ] Do not add forced interstitials, passive banners, loot boxes, multiple subscriptions, or direct paid core-stat upgrades.

## Shared definition of done

Every milestone must satisfy all applicable rules:

- Stored model changes have additive, idempotent migration, validation, old-save fixtures, and rollback coverage.
- Persistent lists have explicit retention limits and tests.
- Cash uses integer minor units; Sparks use whole units in a separate wallet.
- Rates, costs, rewards, caps, and timers are authored, visible, and covered by seeded simulations where they affect balance.
- Actions have stable IDs, availability reasons, signed previews, atomic autosave, duplicate protection, and Life Feed output.
- Timed work survives reload and UTC reconciliation without requiring ads or premium payment.
- Authored content has original copy/assets, localization-safe IDs, eligibility/editorial validation, diagnostic tags, cooldowns, and reachable terminal branches.
- UI passes supported widths, actual text scales, safe areas, focus behavior, touch targets, overflow, and accessibility labels.
- External services remain replaceable, privacy-aware, failure-tolerant, and offline-safe.
- Prices and entitlements come from platform/service metadata, not UXML or hardcoded display copy.
- Core progression remains playable without ads, purchases, Stim+, or Sparks.
