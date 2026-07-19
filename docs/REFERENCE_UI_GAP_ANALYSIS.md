# Reference UI Feature, Behavior, and State Gap Analysis

This document converts the supplied reference images into an implementation contract and compares that contract with the playable Unity UI as of July 16, 2026. It is the detailed source for reference-derived work; `docs/TASKS.md` remains the ordered milestone backlog.

Visual composition is owned by the frontend track and behavior/persistence/testing by the wiring track described in `FRONTEND_WIRING_WORKFLOW.md`. Bound UXML names form the integration API; the status labels below describe the combined playable result, not visual completion alone.

## Reading the references

The images are product specifications for hierarchy, density, feedback, and interaction patterns. They are not authorization to copy art, introduce a second game loop, or ship monetization without approval.

Status labels:

- **Live** — present in the playable `VerticalSlice` and connected to saved game state.
- **Partial** — a working foundation exists, but the pictured component, behavior, or state coverage is incomplete.
- **Missing** — approved for Stim Tycoon but not implemented in the playable destination.
- **Deferred** — intentionally gated behind a later product, economy, service, privacy, or platform decision.
- **Reference only** — useful interaction or presentation guidance that conflicts with the approved Stim product contract and is not backlog work unless separately approved.

The approved shell has six destinations: **Life, Study, Work, Bank, Social, and Goals**. The alternate Home/School/Activities/City/Menu navigation, action-energy quotas, `End Turn`, Stim Coins, and season XP shown in some references are not current Stim systems. **Sparks** are the one approved premium currency: purchasable and occasionally earnable in bounded amounts, but never part of cash/net worth or required baseline progression. **Legacy Gems** is the Goals match-game theme, not a currency.

**Priority clarification:** premium/paid-reward products remain deferred, but their disabled visual scaffold is the first unfinished UI requirement. Before other UI convergence, the live six-destination shell must reserve the pictured premium/sponsored/reward sections with original Stim-owned icons, stable slot IDs, explicit unavailable copy, accessibility labels, and no enabled reward or purchase behavior.

## Cross-screen contract

| Area | Components and UX | Required behavior and branches | Current status | Backlog owner |
|---|---|---|---|---|
| Status header | Avatar, identity, age/role, XP/progress, cash, net worth, money shortcut | Compact and safe-area aware; values update after every transaction; long names and large values contain/wrap; shortcut preserves navigation state | **Partial** — live header and bindings exist; runtime width/text/safe-area approval and production imagery remain | M13 |
| Destination navigation | Six icon-over-label targets with one active capsule | Exclusive selection; age-appropriate destinations remain useful even when actions are unavailable; back/sheet close restores destination, tab, selection, and scroll | **Partial** — six live destinations and per-destination offsets exist; broader restoration/focus coverage remains | M13 |
| Destination heading | Icon/illustration, optional kicker, title, concise purpose statement | Wrap without overlapping; decorative art is aspect-contained; screen reader receives one meaningful heading | **Partial** — live on five focused destinations; imagery and accessibility pass remain | M13/M17 |
| Card and row system | Section header, compact card, account/path/relationship/feed/achievement/action rows, info banner, chips, signed badges | Consistent density, dividers, alignment, selected/pressed/focus/disabled treatment, 44-point targets, no text escape | **Partial** — factories and canonical UXML contracts exist; visual/state detail is incomplete | M13 |
| Progress | Stat, XP, qualification, relationship, goal, age-stage, timer/cooldown, and finance progress | Clamp values; pair color with text/icon; distinguish current, completed, locked, claimable, and failed | **Partial** — several live meter families; common state vocabulary and accessibility are incomplete | M13–M16 |
| Scrolling | Page, list, tab panel, modal sheet, and nested content affordances | Touch/mouse/keyboard/VoiceOver operable; position remains perceivable; no hidden unreachable content; restore offsets | **Partial** — scrolling works, but the common visible affordance and full input matrix are missing | M13/M18 |
| Feedback | Preview, confirmation, signed result, Life Feed entry, autosave, error/retry | Explain costs and gains before commit; commit atomically; prevent double awards; return to useful context | **Partial** — transaction and result foundations are strong; destructive confirms, failure/retry presentation, and uniform return paths remain | M13–M18 |
| Empty/loading/error | Empty illustration/copy/CTA, skeleton or progress state, offline/error banner, retry | Never show a blank card; retain last safe state during failure; do not imply success before save commit | **Missing** as a shared production system; isolated empty/locked copy exists | M13/M18 |
| Responsive/accessibility | Safe areas, dynamic text, focus order, labels, contrast, reduced motion, readable charts | 320/390/430/768 widths at 100%/130%; no overlap, clipping, stretched art, or color-only meaning | **Partial** — structural reflow rules/tests exist; Play Mode, device, VoiceOver, and physical-device gates remain | M13/M18 |

## Life destination

### Components and features

| Reference requirement | Behavior and branched states | Current status | Task |
|---|---|---|---|
| Compact chronological Life Feed | Group by age/month; icon, title, supporting copy, relative time, and signed reward/consequence; empty, populated, archived/See All | **Partial** — deterministic semantic feed is live; compact reward chips, archive/See All, and bounded presentation remain | LIFE-01 |
| Four-stage age progression | Childhood, Teen, Young Adult, Adult with completed/current/future states; current-stage explanation | **Live** in Life Summary; placement/comprehension in the main Life hierarchy still needs validation | LIFE-02 |
| Core stat summary | Five Stim stats with icon, value, meter, See All/detail path; min/max and recently changed treatment | **Live/Partial** — five live meters and detail view; change emphasis and accessibility remain | LIFE-03 |
| Monthly focus/action cards | Age-appropriate actions with clear gain/cost, ready/locked/cooldown/completed states | **Partial** — focus/context actions are live and age-filtered; common state treatment and richer object/action routing remain | LIFE-04 |
| Time controls | New Life, Advance Month, Advance Year; pending-event, paused-year, ending, save-failure, and confirmation branches | **Live/Partial** — resumable year/month flow exists; obstruction, destructive confirmation, and end-state device QA remain | LIFE-05 |
| Next Up and recent events | Surface a scheduled decision and recent meaningful outcomes without duplicating the Life Feed | **Missing** as dedicated modules; pending events and feed data already exist | LIFE-06 |
| Daily goals preview | Compact progress/checklist and direct route to Goals | **Missing** from Life; goal state exists | LIFE-07 |
| Settings/notification shortcuts and date/turn card | Only add settings/notifications if they represent real systems | **Deferred** to M18; the pictured action quota/turn counter is **Reference only** | LIFE-08 |

### Life task slice

- **LIFE-01:** add bounded compact-feed presentation, signed chips, `See all`, and empty/archive states without creating a second event history.
- **LIFE-02:** validate whether the age strip belongs on the Life landing screen or remains in Life Summary; keep one canonical component.
- **LIFE-03:** add recently-changed semantics and accessible meter labels to the existing stat components.
- **LIFE-04:** route focus cards into the shared ready/locked/in-progress/cooldown/completed component states.
- **LIFE-05:** QA time controls through pending event, deferred annual review, resume, failure, death, and new-life confirmation branches.
- **LIFE-06/LIFE-07:** add compact Next Up and goal-preview modules backed by existing scheduled-event and goal state.
- **LIFE-08:** add Settings/notification entry points only when those destinations and unread-state behavior exist; do not add an energy/action quota.

## Study destination

| Reference requirement | Behavior and branched states | Current status | Task |
|---|---|---|---|
| Education progress | Stage, learning/qualification level, XP, next threshold, selected discipline | **Live** | STUDY-01 |
| School/discipline path | Browse path, view requirements and career consequences, select/confirm; omit pre-school and show relevant non-age locks | **Live/Partial** — three disciplines and consequences exist; focused path-detail/navigation polish remains | STUDY-02 |
| Study commitment sheet | Easy/medium/hard previews with cost, benefit, duration, cooldown; ready, unaffordable, locked, active, claimable, claimed | **Live/Partial** — transactional timed sessions exist; visual branch consistency and interruption QA remain | STUDY-03 |
| Study Match mini-game | Reusable timed matching board, instructions, timer, score/reward preview, pause/timeout/success/failure/replay | **Missing** | STUDY-04 |
| Premium study module | Disabled, labeled placeholder only until a separately approved product exists | **Missing/P0 presentation** — product behavior remains deferred and no premium progression may be implied | M13/COM-02 |
| Education empty state | Before formal school, after graduation, no eligible path, and data failure states | **Partial** — pre-school copy exists; graduation/no-path/error states need completion | STUDY-05 |

## Work destination

| Reference requirement | Behavior and branched states | Current status | Task |
|---|---|---|---|
| Age/context banner | Explain why career/business is absent before adulthood and what childhood choices influence | **Live** | WORK-01 |
| Path preview | Part-time work, full-time career, and business rows with reward/requirements and direct navigation | **Partial** — age-aware preview rows exist; persistent selected path and richer requirement routing remain | WORK-02 |
| Career workspace | Industries, qualification gates, applications/interviews, current role/pay, performance, promotion, retraining, firing, unemployment, retirement | **Partial** — domain systems/actions exist, but the destination is not yet a coherent workspace | WORK-03 |
| Business workspace | Action points, work, revenue/expenses, staff/payroll, upgrades, locations, risk/disruption, valuation, failure, sale | **Partial** — Local Services Co. domain exists; focused dashboard is missing | WORK-04 |
| Manual work | Pay preview, one-hour action, cooldown/availability, result and cash update | **Live/Partial** — behavior exists; destination hierarchy and common states need convergence | WORK-05 |
| Shift Match mini-game | Reusable timed matching framework with job-themed content and cash reward | **Missing**, dependent on Study Match framework | WORK-06 |
| Sponsored/rewarded slot | Stable disabled placeholder before service approval; unavailable/loading/declined/completed branches after approval | **Missing/P0 presentation**; behavior deferred | M13/COM-03 |

## Bank destination

| Reference requirement | Behavior and branched states | Current status | Task |
|---|---|---|---|
| Net worth/current cash summary | Signed/large-value-safe formatting; tap header money shortcut; negative-net-worth treatment | **Live/Partial** — summary is live; extreme-value and negative-state visual QA remain | BANK-01 |
| Quick actions | Deposit, withdraw, transfer, history with selected mode and clear available amount | **Live** through Savings tabs/controls; compact quick-action presentation is partial | BANK-02 |
| Accounts list | Cash wallet, savings, revolving credit and investments when relevant; balance/rate/status and detail route | **Live/Partial** — rows exist; chevron/detail destinations and full account state treatment remain | BANK-03 |
| Savings | Exact and 5/10/25/50/100% input, APY/projection, deposit/withdraw, insufficient funds, invalid amount, success/rollback | **Live** | BANK-04 |
| Credit and cash flow | Income/expense detail, debt/APR, repayment amount/method, zero-debt and delinquency states | **Live/Partial** — functionality exists; focused comprehension and edge-state polish remain | BANK-05 |
| Investing | Age/Smarts/emergency-fund gates, contribution, allocation/performance, risk copy, empty/positive/negative history | **Live/Partial** — index path and reporting exist; richer portfolio/history is deferred until balance approval | BANK-06 |
| Financial tip | Contextual, dismissible educational guidance tied to actual state | **Missing** | BANK-07 |
| Spark wallet | Whole-unit premium balance and store shortcut, separate from accounts/net worth | **Approved identity; current disabled presentation requires rename; wallet/economy implementation gated** | M13/COM-01 |
| Premium tools/optional reward sections | Disabled labeled Bank placements with original iconography and stable IDs | **Missing/P0 presentation**; products, prices, and rewards deferred | M13/COM-02/03 |

## Social destination

| Reference requirement | Behavior and branched states | Current status | Task |
|---|---|---|---|
| Relationship list | Avatar, name, role/stage, warmth meter, chevron; compact rows, See All | **Live/Partial** — list and persistent selection exist; row polish/filtering/See All remain | SOCIAL-01 |
| Discover compatible person | Adult-only romance discovery; deterministic bounded candidates; empty/cap/refresh/no-match states | **Partial** — action is live and age-appropriate; bounded candidate-list UX and all terminal states are missing | SOCIAL-02 |
| Relationship profile | Identity/role, warmth, stage, history, genetics where relevant, cooldowns and actions | **Partial** — profile and actions exist; history, consent/status clarity, and complete cooldown presentation remain | SOCIAL-03 |
| Family workspace | Partner, children, parenting, dependent costs, custody and household state | **Missing** as a focused destination; underlying state/actions exist | SOCIAL-04 |
| Social premium module | Disabled labeled placeholder; no match boost or relationship benefit active before approval | **Missing/P0 presentation**; behavior deferred | M13/COM-02 |
| Safety branches | Omit inappropriate people/actions by age/role; handle deceased/unavailable NPC, consent, relationship end, no actions, and reload | **Partial** — core domain guards exist; destination-state coverage and editorial QA remain | SOCIAL-05 |

## Goals destination

| Reference requirement | Behavior and branched states | Current status | Task |
|---|---|---|---|
| Pinned goals | Compact Main/Daily/Life cards, progress, Manage, and direct `Go` | **Partial** — goals render with destination routing; pinned/manage hierarchy is missing | GOAL-01 |
| Achievement rows | Icon, name, category, progress, reward preview, `Go`/claim; locked, active, claimable, claimed | **Partial** — achievement rows and rewards exist; compact categorization and complete visual states remain | GOAL-02 |
| Goal boards | Separate Main, Daily, and Life views with reset/expiry language and once-only rewards | **Missing** as a coherent board; model exists | GOAL-03 |
| Sponsored challenge | Stable disabled placeholder before ads approval; never required for baseline goals | **Missing/P0 presentation**; behavior deferred | M13/COM-03 |
| Season pass | Disabled preview section now; season identity/timer/XP, free/premium lanes, reward claim, and expiry only after approval | **Missing/P0 presentation**; system deferred | M13/COM-04 |
| Legacy Gems match preview | Disabled Goals mini-game/reward section now; future capped rewards may include small Sparks | **Engine missing; theme approved** | M16/COM-05 |

## Commerce and live-service references

These screens are documented so later work does not need to reverse-engineer the images again. They are not approved for production implementation yet.

| Screen | Pictured component contract | Required branches before release | Status/task |
|---|---|---|---|
| Spark Store | Spark packs, Starter Pack, remove-ads entitlement where meaningful, cosmetics, restore purchases, legal/disclosure | Loading, localized pricing, unavailable product, pending, success, cancel, failure, already owned, restore none/success/failure, age treatment, offline | **Approved launch scope; implementation gated — COM-01** |
| Stim+ paywall | Benefit list, monthly/yearly plans, best-value disclosure, subscribe, maybe later, legal/restore | Eligibility, trial/no trial, plan selection, pending, purchase/restore outcomes, cancellation, expiration, grace period, offline and existing entitlement | **Deferred — COM-02** |
| Rewarded-ad prompt | Explicit selectable reward, Watch Ad, decline, available boosts and quotas | Consent/ATT, unavailable, loading, started, skipped, completed, reward grant, duplicate callback, cooldown/cap, offline, age treatment | **Deferred — COM-03** |
| Season/event rewards | Season timer/level/XP, free/premium tracks, claim cells, pass upsell, challenge, mini-game teaser | Active/expired season, locked/unlocked lane, claimable/claimed, missed reward policy, migration, offline, purchase loss/restore | **Deferred — COM-04/05** |

Before any of these tasks becomes active, product must approve the economy and player value, legal/privacy must approve disclosures and age treatment, and engineering must retain optional offline-safe `IAdsService`/IAP boundaries. Baseline progression, recovery, earned achievements, Advance Month, saves, and endings cannot require an ad or purchase.

## Shared branch-state checklist

Every interactive component must declare which of these states apply and test every applicable transition:

1. **Eligibility:** age-inappropriate/absent, relevant-but-locked, available, selected.
2. **Resources:** affordable, insufficient cash, available credit option, invalid/zero/excess amount.
3. **Lifecycle:** ready, confirming, active/in progress, paused/reconciled after reload, cooldown, claimable, completed/claimed, expired/cancelled.
4. **Persistence:** saving, committed, save failed/rolled back, restored, duplicate tap/callback rejected.
5. **Content:** populated, empty, exhausted/no match, archived, unavailable/deceased, terminal outcome.
6. **System:** loading, offline, recoverable error/retry, unavailable service, reduced motion, large text, screen-reader focus.
7. **Navigation:** initial entry, deep-link/`Go`, detail, modal, back/cancel, outcome return, destination/tab/selection/scroll restoration.

## Reconciled differences from the previous backlog

The existing roadmap already covered the six destinations, reusable sheets/tabs, Home inventory, relationship profiles, career/business, goals, mini-games, optional service boundaries, accessibility, and device QA. The image analysis adds or sharpens these missing requirements:

- a single cross-screen branch-state contract covering empty/loading/error/offline, save rollback, claim, cooldown, and restoration states;
- Life `Next Up`, compact goal preview, feed archive/See All, and explicit time-control scenario QA;
- focused pre-school/graduated/no-path Study states and full mini-game pause/timeout/result branches;
- contextual Bank tips, negative/extreme-value presentation, account detail routing, and explicit finance error states;
- bounded Social candidate-list terminal states plus deceased/unavailable/consent branches;
- pinned-goal management and a complete locked/active/claimable/claimed vocabulary;
- a preserved future commerce screen/state inventory without treating currencies, subscriptions, ads, passes, or action quotas as approved features;
- an explicit rule that the alternate five-tab shell, action quota/`End Turn`, and Stim Coins are reference-only; Sparks are the approved premium currency without changing the six-destination monthly loop, while Legacy Gems names the Goals match theme.

## Delivery order

1. **M13 P0:** first add and test the disabled premium/paid-reward icon and section scaffold across the live six-destination shell; then finish shared component states, text containment, scrolling, restoration, Life compact modules, and the device/text/accessibility visual gate.
2. **M15:** implement Home/inventory and Social/family convergence using the shared states.
3. **M16:** implement Work and Goals workspaces, then Study Match and Shift Match on one reusable mini-game lifecycle.
4. **M17:** deepen content/art, run the catalog age audit, and balance the resulting paths.
5. **M18:** complete Settings/accessibility/device hardening; activate commerce/live-service work only after separate approvals.
