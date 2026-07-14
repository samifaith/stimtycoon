# Stim Tycoon — Next Task List

This is the active queue after the user-verified 242-test EditMode baseline on July 14, 2026. The master README remains the product definition.

## Next Phase — Playable Alpha Expansion (Milestones 7–13)

**Phase objective:** turn the verified life-loop and Education foundation into a cohesive, replayable alpha where time, money, home, relationships, work, goals, and major transitions all offer meaningful player decisions.

### M7 — Time, Year in Review, and annual rewards

- [x] Verify and ship Advance Year as up to twelve ordinary monthly transactions with per-month autosave and immediate stops for required input, failures, events, or endings.
- [x] Add an annual-change accumulator covering money, stats, relationships, Education, skills, career, and major Life Feed outcomes.
- [x] Present a Year in Review event after every completed twelve-month cycle, whether reached month-by-month or through Advance Year.
- [x] Offer meaningful next-year choices and grant one previewed, path-appropriate annual benefit exactly once.
- [x] Persist annual-event and reward claim state so tap spam, reload, interruption, and offline reconciliation cannot duplicate it.
- [x] Add the save-schema migration, validation, rollback, and old-save fixtures required by annual accumulators and reward-claim records.
- [x] Define deterministic annual-summary ordering and bounded history retention so Year in Review remains stable and saves do not grow without limit.

### M8 — Money and banking vertical slice

- [x] Add transactional deposits, withdrawals, exact amounts, percentage amounts, and savings balance/history.
- [x] Show grounded interest, projected returns, monthly cash flow, taxes, expenses, debt, and available credit without exaggerated rates.
- [x] Add revolving-credit repayment and cash-or-credit confirmation using integer minor units and atomic rollback.
- [x] Gate investing behind age, knowledge, qualification, emergency savings, and available-funds requirements; keep gambling/casino risk out until banking is stable.
- [x] Add seeded economy simulations and balance budgets for income, expenses, debt, interest, investing risk, and wealth growth across starting backgrounds.
- [x] Cap or archive transaction history without losing balances, auditability, or recovery correctness.

### M9 — Home and personal-development vertical slice

- [x] Add one persistent home with transactional actions for reading, training, rest, maintenance, and household time, including independent monthly cooldowns and rollback-safe autosave.
- [x] Show each action's cost, reading/equipment stock or capacity, cooldown, improvement progress, affordability, and benefit before commitment.
- [x] Connect low home condition to repair overhead, Happiness, household cohesion, household relationships, and an authored repair-or-defer event.
- [x] Add a three-level cash-and-earned-progress home upgrade path that restores condition and improves home-action benefits without premium currency or artificial countdown pressure.
- [x] Persist ownership, condition, reading stock/capacity, equipment condition, improvement progress, and upgrade state through validation and additive migration fixtures.
- [x] Establish and validate a reusable home/room-object content contract with stable IDs, action mappings, previews, costs, capacity consumption, condition wear, progress, and upgrade scaling; drive runtime and UI metadata from it.

### M10 — Relationships, dating, and family expansion

- [ ] Add compatible-person discovery with persistent identity, history, warmth, stage, and introduction context.
- [ ] Expand friendship, rivalry, dating, partnership, marriage, separation, and recovery into multi-step consequence chains.
- [ ] Add consent-aware family planning, pregnancy/adoption, childbirth, child records, parenting choices, and household consequences.
- [ ] Preserve friendship thresholds, adult-only romantic rules, opt-outs, and authored exceptions.
- [ ] Add editorial and automated safety checks for age, consent, family roles, identity, relationship eligibility, and sensitive-event exclusions.
- [ ] Test household/child relationship history across reload, aging, separation, custody, death, and new-life boundaries.

### M11 — Careers and first complete business

- [ ] Add multiple career industries with Education/skill gates, uncertain interviews, firing, unemployment, retraining, and distinct ladders.
- [ ] Make Professional and other relevant skills affect visible career requirements and outcomes.
- [ ] Build one complete business with work actions, action points, revenue, expenses, staffing, upgrades, locations, risks, valuation, failure, and sale.
- [ ] Validate the economy before adding more industries, business types, property, or portfolio breadth.
- [ ] Add deterministic multi-year career/business simulations covering startup, growth, payroll, debt, failure, sale, unemployment, and re-entry.
- [ ] Define bounded business ledgers and migration fixtures before expanding business count.

### M12 — Goals, achievement rewards, and major transitions

- [ ] Add Main, Daily, and Life goals with visible progress, direct navigation, and claimable non-premium rewards.
- [ ] Add meaningful one-time achievement prizes such as cash, durable resources, unlocks, or cosmetic/status rewards.
- [ ] Give graduation, marriage, parenthood, retirement, death, and new-life transitions focused presentations and persisted consequences.
- [ ] Ensure every reward is once-only and never requires an advertisement as the baseline completion path.
- [ ] Define launch-alpha content minimums by life stage and destination, including event-chain starts, follow-ups, endings, cooldowns, and anti-repetition coverage.
- [ ] Run every new event through schema, risk/reward, editorial-tone, localization-key, eligibility, and unreachable-branch validation.
- [ ] Add a lightweight first-life orientation that explains the Life Feed, time controls, locked requirements, and saving without trapping childhood in a long tutorial.

### M13 — Playable-alpha hardening and iOS gate

- [ ] Complete the 320/390/430/768 layout matrix at 100% and 130% text scale, including overlays and persistent controls.
- [ ] Produce and install the first iOS development build on a supported physical iPhone.
- [ ] Profile save/load latency, save size, memory, safe areas, touch behavior, and full-life stability.
- [ ] Resolve critical/high defects and complete one clean birth-to-ending device playthrough.
- [ ] Evaluate save-format or service integrations only from measured device evidence; keep JSON and offline-first play unless profiling justifies change.
- [ ] Add Settings for text scale, reduced motion, sound/music levels, captions/text alternatives, haptics, and destructive-action confirmation.
- [ ] Complete keyboard/focus-order, contrast, readable-chart, VoiceOver-label, dynamic-text, and reduced-motion checks for all alpha screens.
- [ ] Establish localization readiness with no hard-coded player-facing strings in new milestone content, fallback-font coverage, and a pseudo-localized overflow pass.
- [ ] Audit placeholder logo, avatar, icon, font, animation, sound, and music assets; replace launch-blocking placeholders and record licenses/attribution.
- [ ] Add privacy-safe local diagnostics and performance markers for crashes, save failures, event frequency, economy balance, and funnel testing without requiring an online SDK.
- [ ] Verify content/save versioning, forward migrations, corruption recovery, backup restore, downgrade behavior, and bounded save growth from a Milestone 6 fixture through the current build.
- [ ] Prepare the internal-alpha checklist: build/version numbers, signing, entitlements, privacy manifest/disclosures, third-party licenses, known issues, rollback build, and tester instructions.

### Phase exit criteria

- [ ] Milestones 7–12 are playable through the shared UI and transactional save path, not only through domain tests.
- [ ] Every action previews requirements/tradeoffs, every completion is duplicate-safe, and every outcome reaches the Life Feed.
- [ ] The complete EditMode suite, seeded full-life harness, responsive checks, and physical-device smoke pass are clean.
- [ ] A new player can start a life, make meaningful choices in every major destination, complete annual reviews, and reach a stable ending without developer intervention.
- [ ] Representative old saves migrate safely, corrupt saves recover, history remains bounded, and no milestone can duplicate money, rewards, relationships, businesses, or annual claims.
- [ ] Alpha content meets its life-stage/destination minimums, passes automated/editorial validation, and avoids unreachable or immediately repetitive branches.
- [ ] First-life orientation, Settings, accessibility, pseudo-localization, asset licensing, diagnostics, privacy, and internal-distribution checklists are complete.

### Phase-wide engineering and content rules

- [ ] Every stored model change ships with validation, an additive idempotent migration, old/current round-trip fixtures, rollback coverage, and a documented content/save version decision.
- [ ] Every authored action/event ships with stable IDs, localization keys, analytics/diagnostic tags, eligibility tests, risk/reward validation, Life Feed output, and anti-repetition behavior.
- [ ] Every economy-producing feature has deterministic long-run simulations and explicit balance targets before content breadth is added.
- [ ] Every persistent list has a retention/archive policy and a test proving save growth remains bounded over a complete long life.
- [ ] External accounts, cloud, ads, and analytics SDKs remain optional adapters and cannot block offline play; their consent, privacy, child-directed-treatment, and failure behavior must be documented before integration.

## P0 — Keep the verified loop fast and shippable

- [x] Mark the birth-to-ending harness as `SlowSimulation` coverage so it is separately selectable in the Unity Test Runner.
- [x] Report harness duration, simulated months, transaction count, largest serialized save, and final Life Feed size in test progress output.
- [x] Remove pretty-print allocation and whitespace from transactional autosaves while retaining native JSON and the existing save contract.
- [ ] Reduce redundant serialization in simulation-only coverage without weakening focused atomic-save, rollback, migration, corruption, and recovery tests.
- [ ] Keep the native atomic JSON repository through the first physical-device profiling pass.
- [ ] Validate Life, Social, education, career, achievements, event overlays, new-life setup, and ending summary at 320, 390, 430, and 768 widths.
- [ ] Repeat the layout pass at 100% and 130% text scale; fix clipping, wrapping, scroll reachability, and persistent-action overlap.
  - [x] Add automatic compact layout rules at 360 points and below for the header, navigation, cards, action dock, forms, and overlays.
  - [x] Replace fixed shell heights with scalable minimum heights and preserve 44-point-or-larger primary touch targets.
  - [x] Add an explicit 130% accessibility-text reflow mode for dense cards, time controls, forms, headings, and overlays.
  - [ ] Complete the visual Unity/Game View matrix and record any remaining screen-specific defects.
- [ ] Produce the first iOS development build.
- [ ] Profile save/load latency, save size, memory, safe areas, and touch targets on a physical supported iPhone.
- [ ] Evaluate MessagePack behind `IStimSaveRepository` only if device profiling shows unacceptable JSON latency or file size.

## P1 — Interactive gameplay framework

### P1A — Shared action contract and UI

- [x] Introduce a reusable candidate-save transaction runner that clones, revision-stamps, persists, and exposes state only after a successful commit.
- [x] Extract Education action eligibility and mutation from `StimGameSessionService` while preserving its public compatibility API.
- [x] Add focused transaction success, rejection, persistence-rollback, and Education rollback coverage.
- [x] Define a reusable activity/action model with stable ID, destination, prerequisites, locked reason, costs, payment methods, resource/stat deltas, progress, duration/cooldown, risk, outcome, and feed metadata.
- [x] Persist migration-safe action instances and reject duplicate completion requests across repeated submission and reload.
- [x] Render Education through reusable UI Toolkit action cards with requirement chips, signed previews, accessible tooltips, and 44-point commit controls.
- [x] Support persisted `Ready`, `In Progress`, `Complete`, `Claimable`, and `Locked` states without tying completion to premium currency or advertising.
- [ ] Build reusable UI Toolkit action cards with requirement chips, signed previews, progress, timers/cooldowns, cash-or-credit selection, confirmation, and accessible feedback.
- [x] Build a reusable `5% / 10% / 25% / 50% / 100%` amount selector plus exact-amount validation for transfers, repayments, and later investments.
- [x] Validate authored cash/credit options and available balances in integer minor units before monetary action commitment.
- [ ] Resolve each action as one deterministic transaction: validate, apply, autosave, roll back on failure, and write every completed outcome to the Life Feed.
- [x] Persist in-progress activities, reconcile elapsed UTC time after reload, and enforce single-claim completion.
- [ ] Add focused model, controller, rollback, reload, and UI structure tests before expanding activity content.

### P1B — Interactive vertical slices

- [x] **Education:** choose a study track; show prerequisites and explicitly authored costs; select easy/medium/hard study sessions; trade time/resources for XP; advance visible qualification tiers; unlock careers and events.
  - [x] Choose a persisted General, Academic, or Vocational track at ages 14–17 with visible authored costs, affordability checks, atomic autosave, rollback, and Life Feed output.
  - [x] Add easy, medium, and hard study sessions with explicit resource tradeoffs, monthly cooldowns, and qualification XP previews.
  - [x] Advance through visible Foundation, Certificate, Diploma, and Advanced qualification tiers.
  - [x] Use track/tier prerequisites to unlock careers and authored events while preserving compatibility for legacy saves without a selected track.
- [ ] **Money:** deposit and withdraw exact or percentage amounts; show grounded interest and projected income; add credit repayment; retain cash-or-credit choices for eligible costs; gate investing, property, and casino-risk content.
- [ ] **Home:** interact with room objects to read, train, rest, maintain the home, and perform household activities; show stock/capacity, house progress, costs, cooldowns, and benefits.
- [ ] **Relationships:** discover compatible age-appropriate people, inspect persistent warmth/stage/history, and choose social or romantic actions while preserving friendship-threshold and adult casual-event rules.
- [ ] **Business:** operate one complete business with work actions, action points, revenue/expense previews, staffing, upgrades, locations, timers, risks, valuation, failure, and sale.
- [ ] **Goals:** add Main, Daily, and Life goals with visible progress, direct `Go` navigation, claimable non-premium rewards, and achievement integration; never require watching an ad.
- [ ] **Transitions:** add focused birth, coming-of-age, graduation, marriage, parenthood, retirement, death, and new-life presentations with persisted Life Feed consequences.

### Interactive quality gates

- [ ] Every action displays its requirements and known tradeoffs before commitment; chance-based choices describe risk without exposing hidden rolls.
- [ ] Every monetary action uses integer minor units, explicit authored costs/rates, affordability checks, and atomic cash/credit handling.
- [ ] Every reward and completion can be granted only once across tap spam, reload, interruption, and offline reconciliation.
- [ ] Every slice works at 320, 390, 430, and 768 widths and at 100% and 130% text scale with 44-point-or-larger primary targets.
- [ ] Stim-specific content, pacing, visuals, and economy remain original; reference products supply interaction patterns only.

### Time controls

- [x] Add Advance Year as a safe batch of up to twelve normal Advance Month transactions, stopping for pending events, required school/life decisions, claimable work, failures, endings, or other player input.
- [x] Show a batch summary covering elapsed months and important money, stat, relationship, education, and career changes.
- [x] End every completed twelve-month cycle—whether reached month-by-month or through Advance Year—with an authored Year in Review event that condenses major outcomes and offers meaningful next-year options.
- [x] Grant one clearly previewed, path-appropriate year-completion benefit and persist its claim so it cannot duplicate across taps, reloads, interruption, or offline reconciliation.
- [ ] Keep Advance Month free and always available. If Advance Year is monetized later, treat it only as optional convenience with an earned/free path; never let it bypass consequences or improve hidden odds.

## P2 — Branching life simulation

### P2A — School, work, and persistent paths

- [x] Add an additive persistent life-path/decision record so later content can test choices without inferring them from feed text.
- [x] Replace automatic school transitions with required enrollment/path decisions at primary, middle, and high-school ages.
- [ ] Add stage-specific school paths and consequences: attend, study hard, socialize, skip class, transfer, and eventually graduate or drop out.
- [x] Extend the focus controls into an age/context activity deck:
  - childhood: play, family time, read, explore;
  - enrolled youth: attend school, study, clubs, friends, skip class;
  - working age: job search, work shift, overtime, training, socialize, rest;
  - unemployed/retired: retraining or retirement-specific health, family, hobby, and social actions.
- [ ] Make education, skills, reputation, relationships, health, and prior decisions unlock or block later activities and event choices.
- [x] Add at least two skill paths beyond Learning, with XP thresholds and visible downstream benefits: Fitness reduces overtime strain and Professional increases focused career progress.
- [ ] Add multiple career industries, education requirements, authored interview uncertainty, firing, unemployment, career changes, and distinct promotion ladders.

### P2B — Relationships and family

- [x] Add a supportive coming-of-age identity chain where the player chooses a persistent gender identity at 16 and sexual orientation at 17.
- [x] Continue coming of age into consent-aware prom, dating, and first-kiss branches with friendly and opt-out paths.
  - [x] Require an existing friend or best friend at 60+ strength for romance; reserve threshold bypasses for explicitly authored casual-encounter events.
- [x] Add deterministic school peers with persistent introduction context, friendship/best-friend/rival states, and age-appropriate competition and reconciliation.
- [ ] Add multi-step relationship arcs where neglect, support, arguments, reconciliation, jealousy, and betrayal affect later events.
  - [x] Persist months since interaction and allow neglect, arguments, competition, and reconciliation to change friendship stages.
  - [x] Consume scheduled follow-up records during annual event selection so relationship choices can produce later consequences.
  - [x] Add the first authored trust, betrayal, rivalry, and reconciliation chain with a delayed consequence.
  - [x] Add jealousy between old and new peers with social-group choices that can strengthen or damage both relationships.
  - [ ] Add more relationship-chain endings and carry adolescent friendship history into adult relationships.
- [ ] Add adult dating, partnership, breakup, engagement, and marriage only after friendship arcs and age gates are stable.
  - [x] Add age-gated dating, committed partnership, and breakup transitions that grow from strong existing friendships.
  - [x] Add strength-gated engagement and marriage with postpone, breakup, and call-off branches.
  - [x] Make prom, proposal, wedding, postponement, and cancellation costs respond to salary, career progress, cash, debt capacity, and relationship strength.
    - Financial effects are explicitly authored per choice; unrelated events and relationship outcomes never receive automatic costs.
  - [x] Add neglect-driven partnership decay plus counseling, recommitment, separation, and divorce consequences.
  - [x] Merge spouse savings/debt once at marriage and combine persistent NPC-derived income into monthly household cash flow.
  - [x] Add household happiness/cohesion and group activities whose relationship and fixed authored cost effects scale with attendees.
    - [x] Use fixed authored ticket prices; require cash or an adult household credit line based on income, career progress, cohesion, and existing debt.
    - [x] Assign risk-based 8.00%–29.99% APR, preserve a weighted rate across purchases, and accrue visible monthly revolving-credit interest.
    - [x] Route every explicitly costed event through cash-or-credit payment selection; never attach payment choices to free or rewarding outcomes.
    - [x] Persist emotional credit feedback: approval grants Happiness +1; denial applies Happiness −2 without applying the attempted purchase.
  - [ ] Add jealousy, infidelity, shared-property disputes, and post-divorce recovery arcs.
- [ ] Add consent-aware family-planning choices, pregnancy/adoption event chains, childbirth, and child relationship records for adults.
- [ ] Let children age alongside the player and create parenting, custody, sibling, inheritance, and intergenerational consequences.

### P2C — Drama, health, death, and endings

- [ ] Add chained drama stories with prerequisites and follow-ups instead of isolated random events: school conflict, family secrets, workplace rivalry, financial crisis, infidelity, estrangement, and reconciliation.
- [ ] Expand health gameplay with illness, injury, checkups, treatment, recovery, chronic statuses, addiction safeguards, and player choices before late-life decline.
- [ ] Add cause-specific death, relationship grief, funeral, estate/debt settlement, inheritance, and surviving-family consequences.
- [ ] Add branch-aware life summaries and alternate endings based on education, career, wealth, relationships, family, health, and major decisions.
- [ ] Expand achievements across education, relationships, careers, wealth, health, family, drama arcs, and alternate endings.
  - [ ] Give every achievement a meaningful one-time prize: cash, debt relief, durable resources, content/action unlocks, or cosmetic/status value proportional to difficulty.
  - [ ] Preview achievement prizes and claim them transactionally with persisted duplicate-award protection; never require an advertisement or premium currency.

### Branching-content quality gates

- [ ] Every major decision records durable state, has at least one later consequence, and appears in the final life summary when significant.
- [ ] Every event chain defines entry requirements, age gates, mutually exclusive branches, cooldowns, follow-ups, and terminal outcomes.
- [ ] Seeded tests cover at least three materially different complete lives: education/career success, relationship/family focus, and unstable or adverse path.
- [ ] Sensitive content remains age-appropriate, non-exploitative, and configurable for intensity before authored drama is expanded.

## P3 — Money destination

- [x] Implement the first playable Money destination using the existing four-tab shell.
- [x] Add a transactional manual-work tap that pays one hour at annual salary ÷ 2,080, rounded to the nearest cent.
- [x] Run and verify the 7 new Money/manual-work cases, establishing the earlier 147-test intermediate baseline.
- [ ] Show monthly gross income, taxes, expenses, debt pressure, net cash flow, net worth, and persisted transaction history.
- [ ] Add fatigue/cooldown balance and accessibility feedback after the base tap economy is playtested.
- [ ] Add debt repayment and emergency-expense decisions before investing.
- [ ] Add stock/index investing only after cash-flow and transaction-history behavior is stable.

## P4 — First business slice

- [ ] Implement one complete business type before expanding to three.
- [ ] Add the business state model, shorter operating turns, action budget, revenue, expenses, and annual settlement.
- [ ] Add staffing, pricing, marketing, upgrades, business events, valuation, failure, and sale incrementally.
- [ ] Add property ownership only after the first business and investing loops are stable.

## Production and service gates

- [ ] Replace placeholder glyphs with the approved logo, avatar treatment, and licensed icon set.
- [ ] Add the rounded production font and verify fallback/localization coverage.
- [ ] Add accessibility settings for text scale and reduced motion.
- [ ] Freeze beta save semantics before account-enabled distribution.
- [ ] Add Unity Authentication, Game Center, Cloud Save, and conflict fixtures after the offline save contract is frozen.
- [ ] Add LevelPlay only after placement, consent, and child-directed treatment are validated.

## Verified baseline

- [x] Phase 0 offline architecture and representative content
- [x] 220 passing EditMode tests, including Milestone 5 amount, payment, and UI-control coverage
- [x] 226 passing EditMode tests, including Milestone 6 Study Track transaction, persistence, rollback, and UI coverage
- [x] 229 passing EditMode tests, including difficulty-based study sessions, visible qualification tiers, and cooldown coverage
- [x] 232 passing EditMode tests, including career qualification gates, visible locked reasons, and authored Education event requirements
- [x] 235 passing EditMode tests, including visible Fitness/Professional progress and their gameplay consequences
- [x] 242 passing EditMode tests, including responsive accessibility reflow and safe Advance Year batching/stops
- [x] Deterministic seeded birth-to-ending simulation
- [x] Transactional local saves, migration, integrity checks, backup recovery, and rollback safety
- [x] Playable Life, Social, education, career, achievement, retirement/death, and final-summary flows
- [x] Run the expanded EditMode suite in Unity and record the new verified test count before additional high-risk feature work (220 passing on July 14, 2026).
