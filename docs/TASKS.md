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

- [ ] Implement a safe-area-aware persistent status header for age/calendar, cash/net worth, and Stim's actual stats/resources.
- [x] Establish six destinations with exclusive active states: Life/Home, Education, Career/Business, Bank, Social/Family, and Goals/Legacy.
- [x] Establish the approved three-pack UI direction through a replaceable Stim-owned USS adapter and an asset/license manifest, without reorganizing vendor imports.
- [ ] Add reusable destination headers, segmented tabs, modal sheets, requirement chips, action states, progress bars, timer/cooldown rows, and selected-navigation styling.
- [ ] Replace default/placeholder scrollbars throughout the application with one polished Stim scrollbar and scroll-affordance system for page, list, sheet, tab, and nested-scroll contexts; do not solve visual quality by hiding required position feedback.
- [ ] Complete the shared UI-detail pass for spacing, dividers, shadows, borders, pressed/hover/focus/disabled states, empty/loading/locked states, truncation/wrapping, and consistent icon/text alignment.
- [ ] Restore destination, tab, scroll position, and selected object/person after sheets and action resolution.
- [ ] Keep Advance Month/Year, pending decisions, transition presentations, and endings reachable and unobscured.
- [ ] Add structural and interaction coverage for navigation, overlays, back/close behavior, focus order, and state restoration.
- [x] Render Life Feed updates as a deterministic semantic ordered list with age/month/revision ordering, category context, numbered accessible item context, and no in-place save reordering.
- [x] Add a reusable Stim-owned visual-placeholder definition/factory with stable IDs, roles, aspect ratios, accessibility/decorative metadata, fallbacks, theme tokens, and development labeling.
- [ ] Place the reusable visual slots into destination heroes, event art, avatars, icons, objects, badges, and backgrounds; add bounded Life Feed archival behavior.

**Exit gate:** the shell passes 320/390/430/768 widths at 100% and 130% text, maintains 44-point primary targets, respects safe areas, has no navigation dead ends, and uses the approved scrollbar/scroll-affordance and shared UI-detail system in every implemented destination and overlay.

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
- [ ] Add LevelPlay only after placement, consent, privacy, age treatment, offline failure, and non-required reward paths are approved.
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
