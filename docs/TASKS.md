# Stim Tycoon — Next Task List

This is the active queue after the user-verified 196-test EditMode baseline on July 14, 2026. The master README remains the product definition.

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
  - [ ] Complete the visual Unity/Game View matrix and record any remaining screen-specific defects.
- [ ] Produce the first iOS development build.
- [ ] Profile save/load latency, save size, memory, safe areas, and touch targets on a physical supported iPhone.
- [ ] Evaluate MessagePack behind `IStimSaveRepository` only if device profiling shows unacceptable JSON latency or file size.

## P1 — Interactive gameplay framework

### P1A — Shared action contract and UI

- [ ] Define a reusable activity/action model with stable ID, destination, prerequisites, locked reason, costs, payment methods, resource/stat deltas, progress, duration/cooldown, risk, outcome, and feed metadata.
- [ ] Support `Ready`, `In Progress`, `Complete`, `Claimable`, and `Locked` states without tying completion to premium currency or advertising.
- [ ] Build reusable UI Toolkit action cards with requirement chips, signed previews, progress, timers/cooldowns, cash-or-credit selection, confirmation, and accessible feedback.
- [ ] Build a reusable `5% / 10% / 25% / 50% / 100%` amount selector plus exact-amount validation for transfers, repayments, and later investments.
- [ ] Resolve each action as one deterministic transaction: validate, apply, autosave, roll back on failure, and write every completed outcome to the Life Feed.
- [ ] Persist in-progress activities and reconcile elapsed offline time without granting duplicate completion or rewards.
- [ ] Add focused model, controller, rollback, reload, and UI structure tests before expanding activity content.

### P1B — Interactive vertical slices

- [ ] **Education:** choose a study track; show prerequisites and explicitly authored costs; select easy/medium/hard study sessions; trade time/resources for XP; advance visible qualification tiers; unlock careers and events.
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
- [ ] Add at least two skill paths beyond Learning, with XP thresholds and visible action/event unlocks.
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
- [x] 196 passing EditMode tests, including the expanded Phase 2 coverage
- [x] Deterministic seeded birth-to-ending simulation
- [x] Transactional local saves, migration, integrity checks, backup recovery, and rollback safety
- [x] Playable Life, Social, education, career, achievement, retirement/death, and final-summary flows
- [x] Run the expanded EditMode suite in Unity and record the new verified test count before additional high-risk feature work (196 passing on July 14, 2026).
