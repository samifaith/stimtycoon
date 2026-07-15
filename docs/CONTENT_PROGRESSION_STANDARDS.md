# Stim Tycoon — Content and Progression Standards

**Status:** baseline specification for M13–M17
**Scope:** progression thresholds, age-appropriate content, NPC triggers, ad placeholders, Life Feed ordering, and visual placeholders

These values are the first authoring baseline. Existing runtime values remain authoritative until the matching implementation task migrates them. Balance changes must update this document, deterministic simulations, previews, and tests in the same change.

## 1. Progression thresholds

### Core stats

Core stats remain within `0–100`. Content uses bands rather than testing many arbitrary values.

| Band | Range | Meaning | Authoring use |
|---|---:|---|---|
| Critical | 0–19 | immediate vulnerability | recovery actions and high-priority support events |
| Low | 20–39 | meaningful disadvantage | common locked choices or negative modifiers |
| Stable | 40–59 | ordinary capability | default access; no special modifier |
| Strong | 60–79 | demonstrated strength | advanced choices and moderate positive modifiers |
| Exceptional | 80–94 | rare strength | elite choices and strong positive modifiers |
| Peak | 95–100 | life-defining strength | capstone events; never required for the main path |

Requirements should normally use `20`, `40`, `60`, or `80`. A main-life path cannot require more than `60`; thresholds above `60` are optional mastery routes.

### Skills

Skill levels use the implemented cumulative formula `25 × (level − 1) × level` XP.

| Level | XP required | Expected role |
|---:|---:|---|
| 1 | 0 | novice actions |
| 2 | 50 | basic unlocks |
| 3 | 150 | competent work |
| 4 | 300 | advanced actions |
| 5 | 500 | specialist choices |
| 6 | 750 | expert choices |
| 7 | 1,050 | mastery content |

Routine actions should award `5–15 XP`, committed sessions `15–35 XP`, and major authored outcomes `25–75 XP`. No repeatable action should grant more than 25% of the next level in one use without a meaningful cost, cooldown, or risk.

### Education qualifications

These match the implemented Education thresholds.

| Tier | Qualification XP | Unlock intent |
|---|---:|---|
| Foundation | 0 | entry study and general work |
| Certificate | 50 | first trained roles and adult investing alternative |
| Diploma | 125 | mid-tier careers and advanced study |
| Advanced | 250 | specialist careers and capstone events |

Easy/medium/hard sessions must show XP, stat tradeoffs, duration/cooldown, and any cost before commitment. Hard sessions accelerate progress but cannot be the only route to a tier.

### Relationships

| Warmth/strength | Stage intent |
|---:|---|
| 0–19 | hostile/estranged |
| 20–39 | distant/rival |
| 40–59 | acquaintance/familiar |
| 60–79 | friend/close connection |
| 80–100 | best friend/committed bond |

Romance remains adult-only. Dating requires age `18+` and an established friendship at `60+`; commitment normally requires age `21+` and `75+`; engagement requires age `24+` and `80+`; marriage requires age `25+` and an active engagement. Consent/opt-out state always overrides numeric eligibility.

### Career, business, home, and wealth

- Career promotion steps use visible cumulative progress targets of `25`, `50`, and `75` for the first ladder; later catalogs may scale to `100` but must show the target.
- Business upgrades require `level × 25` operating progress. Each level must add a visible operational benefit and expense/risk consequence.
- Home upgrades use three alpha levels. Each requires both the authored cash price and earned improvement progress; condition below `40` triggers repair pressure and below `20` blocks non-recovery upgrades.
- Investing requires age `18+`, Smarts/financial knowledge `40+`, secondary completion or Certificate qualification, and emergency savings of at least one current month of living expenses (minimum `$500`).
- Goal rewards should target roughly 5–20% of the cost or effort needed for the next ordinary unlock. Rewards cannot be large enough to skip an entire tier.

## 2. Age and life-stage content matrix

Age checks use exact ages; life-stage labels group authoring and safety rules.

| Stage | Ages | Appropriate tasks | Event families | Reward emphasis | Exclusions |
|---|---:|---|---|---|---|
| Infant | 0–2 | observe/advance, caregiver bond | birth, health, family stability | relationship, small stat changes | no independent spending, work, romance, or risk actions |
| Early childhood | 3–5 | play, family time, read together, explore | first memories, preschool, minor illness | Happiness, Learning, family warmth | no unsupervised adult systems |
| Primary school | 6–11 | attend, study, play, clubs from 10, ask for help | classmates, school effort, hobbies, family | Learning XP, friendship, small durable traits | no romance, work, credit, investing, or business |
| Teen | 12–17 | study, clubs, socialize, exercise from 13, age-safe part-time exposure | identity, peers, conflict, exams, school paths | qualifications, skills, reputation, relationships | no adult dating discovery, credit, investing, or ownership |
| Emerging adult | 18–24 | career entry, training, saving, investing gates, adult dating | independence, interviews, debt, early partnership | cash, qualifications, career/relationship progress | no marriage before authored age gates |
| Adult | 25–49 | career/business, household, parenting, portfolio, health maintenance | marriage, children, career risk, business, housing | wealth, family, mastery, durable unlocks | sensitive content remains consent- and context-gated |
| Midlife | 50–64 | leadership, caregiving, checkups, succession planning | chronic health, career plateau/change, family transitions | resilience, legacy prep, debt reduction | avoid automatic decline as the only story |
| Older adult | 65+ | retirement, hobbies, family, mentorship, estate planning | retirement, grief, health, legacy | relationships, legacy, quality of life | no forced passivity; capable characters retain agency |

Every stage needs at least three routine tasks, two beneficial events, two adverse/recovery events, one NPC-driven event, and one milestone or transition before alpha content exit. Outcomes must always offer an age-appropriate recovery route; childhood failures cannot create permanent financial ruin.

## 3. Tasks, rewards, and outcomes

Each task definition must include:

1. stable `taskId`, category (`Main`, `Daily`, or `Life`), destination, and life-stage range;
2. measurable progress source and target;
3. requirements and a player-facing locked reason;
4. reward preview and once-only claim ID;
5. expiry/refresh rule—Main and Life do not silently expire; Daily tasks expire on the next monthly turn;
6. direct `Go` navigation when the required destination is currently available;
7. safe fallback when a life-stage change makes completion impossible.

Reward bands:

| Task scale | Examples | Baseline reward |
|---|---|---|
| Routine | one study, social, home, or work action | `$10–$50` equivalent or `5–10 XP` |
| Main step | start a career, complete a qualification | `$250–$1,000` equivalent or durable unlock |
| Life goal | `$100K` net worth, raise a child, sell a business | `$1,000–$5,000` equivalent, status/cosmetic, or legacy value |
| Capstone | retirement plan, mastery, major recovery | unique badge/presentation plus bounded durable benefit |

Every event choice needs at least one numeric effect. Positive outcomes should not always mean cash; use skills, relationships, health, reputation, household state, time, or unlocked content. Negative outcomes must be bounded, explained, and recoverable. Neutral outcomes should still record a meaningful tradeoff or durable decision.

## 4. NPC events and triggers

NPC-driven content must reference a persistent `relationshipId`; never select a person by display name. A trigger definition contains:

- `triggerId`, source event/action, eligible relationship types/stages, age range, and location;
- minimum/maximum warmth, months since interaction, and required prior decisions/statuses;
- exclusion and cancellation rules for death, estrangement, incompatible family role, consent withdrawal, or life ending;
- timing window in months/years, probability, priority, cooldown, and once/repeat policy;
- the target event ID and fallback behavior if no eligible NPC exists.

Trigger priority is:

1. safety/terminal state and required transition;
2. already scheduled NPC consequence whose window is closing;
3. fixed-age or fixed-month milestone;
4. relationship neglect/support response;
5. ordinary weighted NPC event.

At most one required NPC event is presented per monthly turn. Additional eligible triggers remain scheduled, preserving their deterministic order. The same NPC/event pair cannot repeat within 12 months unless explicitly authored as a short chain. NPC events must test age, consent, role, death, separation/custody, reload, and cancellation boundaries.

## 5. Advertisement placeholders

Ads remain optional adapters. M13–M17 may render disabled development placeholders so layout and consent flows can be tested without an SDK.

Allowed placeholder slots:

| Slot ID | Location | Allowed future use |
|---|---|---|
| `rewarded_optional_speedup` | completed/in-progress timer sheet | optional time reduction; ordinary completion remains available |
| `rewarded_optional_refresh` | exhausted discovery/content refresh | one optional refresh; normal monthly refresh remains available |
| `rewarded_optional_bonus` | after a completed action | bounded bonus that does not alter hidden outcome odds |

Rules:

- Placeholder copy must say `Optional reward placeholder — unavailable in this build` and remain disabled unless a fake-development adapter is explicitly enabled.
- No ad is a Main, Daily, or Life task; no baseline reward, recovery path, ending, or progression gate requires an ad.
- Ads cannot reroll a resolved outcome, guarantee success, bypass age/consent/financial requirements, create debt relief beyond an authored cap, or target child-stage play.
- Each slot reserves a stable ID, disclosure text key, reward preview, cooldown, daily cap, consent requirement, offline behavior, and analytics tag before SDK integration.

## 6. Ordered Life Feed

The UI presents the Life Feed as a semantic ordered list (`1…n`) with newest entries first. Persisted entries retain their chronological facts; rendering must not reorder the stored save in place.

Sort key, descending:

1. `age`;
2. `monthOfYear`;
3. `revision`;
4. parsed `timestampUtc`;
5. `entryId` ordinal as the deterministic final tie-breaker.

Entries from one transaction share the committed revision. Within that revision, category priority is `transition`, `event outcome`, `reward/achievement`, `relationship/family`, `career/business/money`, `education/home/activity`, then `time summary`. If multiple entries still tie, preserve insertion order via a future explicit `sequenceInRevision`; until that field is migrated, use `entryId`.

Each rendered list item includes age/month context, category, localized summary, signed effects when applicable, and optional related destination/entity IDs. Accessibility must announce position, category, time context, and summary. Histories remain bounded; archival summaries preserve major milestones before old routine entries are removed.

## 7. Imagery and visual placeholders

Every destination, event, NPC, task, transition, home object, education discipline, career, and business may declare a visual slot without requiring final art.

Required placeholder metadata:

- stable `visualId` such as `event.school.exam_result`;
- `visualRole`: `hero`, `thumbnail`, `avatar`, `icon`, `background`, `object`, or `badge`;
- aspect ratio and safe focal region;
- localization-independent accessibility label key, or `decorative: true`;
- fallback initials/glyph and theme color token;
- source status: `placeholder`, `concept`, `licensed`, or `production`;
- license/attribution record for any non-original production asset.

Default slot sizes are `1:1` avatar/icon, `4:3` card art, `16:9` event/transition hero, and full-bleed portrait destination background. UI layouts must remain usable when imagery is missing, delayed, hidden for reduced data, or replaced by 30% longer localized text. Placeholder visuals use simple Stim-owned shapes/color tokens and must be visibly marked in development builds; they may not imitate the supplied reference art.

## 8. Verification gates

- Before M17 exits, generate a complete catalog report listing every event, choice, outcome, task, NPC trigger, follow-up, reward, and visual against its minimum/maximum age and life stage. Human editorial review must approve every row; automated tests must exercise the exact minimum age, exact maximum age, and the adjacent rejected ages.
- Content coverage tests count tasks/events/NPC triggers by life stage and destination.
- Validator tests reject invalid ages, unreachable thresholds, missing recovery outcomes, unsafe NPC roles, missing cancellation rules, required ads, duplicate IDs, and incomplete visual metadata.
- Seeded simulations report progression pace, event frequency, NPC trigger drought/repetition, reward value, and Life Feed size/order.
- UI structure tests verify ordered-list semantics, placeholder alt/decorative behavior, disabled ad slots, locked reasons, and signed previews.
- UI review verifies consistent custom scrollbars/overflow affordances, scroll-position visibility, nested-scroll behavior, interaction states, spacing, alignment, wrapping, and empty/loading/locked presentation across every destination and overlay.
- Human review confirms tone, age appropriateness, consent, cultural context for USA/Jamaica, visual originality, and outcome clarity.
