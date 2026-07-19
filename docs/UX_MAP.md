# Stim Tycoon MVP Interaction Map

This map defines the approved navigation and interaction model. The exhaustive comparison against the supplied screen references—including component status, branched states, deferred commerce, and stable task IDs—is maintained in `REFERENCE_UI_GAP_ANALYSIS.md`.

The frontend/wiring boundary is defined in `FRONTEND_WIRING_WORKFLOW.md`: UXML/USS owns composition and presentation; binders and application/domain services own dynamic state and behavior. A bound UXML `name` is an API and must not be renamed independently.

## Visual contract

The locked direction is **cozy corporate**: structured and financially literate, but warm enough to support childhood, relationships, health, failure, and recovery.

- Cyan establishes the energetic game world.
- Cream and paper surfaces keep dense information comfortable to read.
- Deep navy provides outlines and text without the harshness of pure black.
- Magenta marks the dominant action or active destination.
- Yellow marks progress, opportunity, and secondary emphasis.
- Meaning is always repeated with text, shape, or an icon; color alone is never the only signal.
- Cards use thick outlines, rounded corners, clear headings, and short blocks of copy.
- Normal tap targets are at least 44 points tall; primary actions target 48 points or more.

The live implementation is rooted at `Assets/StimTycoon/UI/StimVerticalSlice.uxml` and references only the canonical Stim-owned `Assets/StimTycoon/UI/Styles/StimTheme.uss` and `Assets/StimTycoon/UI/Styles/Components.uss` entry points. `Components.uss` temporarily maps retained legacy layout rules while they are extracted screen by screen. Imported GUI packs remain vendor-owned and are consumed only through Stim-owned composition and style adapters.

The approved presentation is compact and wireframe-led: restrained white cards on a pale-blue canvas, an 88–96 point player/cash header, wrapped 24–28 point page headings, dense 44–64 point rows, and six icon-over-label navigation targets. Lucide SVGs identify functional navigation and controls. Emoji are the temporary imagery system for avatars, feed categories, destination illustrations, objects, and badges until original production art replaces them.

## Persistent shell

```text
Player header
├── identity, age, and current role
├── cash / net-worth shortcut
└── expandable player overview

Active screen content
├── compact destination-specific modules
└── one clearly prioritized action group

Bottom navigation
├── Life / Home
├── Education
├── Career / Business
├── Bank
├── Social / Family
└── Goals / Legacy
```

The six destinations use exclusive active states and remain the Phase 5 navigation contract. Investing belongs within Bank; career and business share a destination; health and ordinary activities remain reachable from Life/Home unless playtesting justifies a separately approved navigation change. Life follows the approved age-strip → timeline feed → stat tiles → aging actions hierarchy. Study and Work use progress/path modules with reserved mini-game slots; Bank, Social, and Goals use compact scannable rows rather than oversized dashboard cards.

## Primary flows

### Monthly life loop

```text
Life screen
→ Advance month
→ Apply income, expenses, tax, and monthly stat movement
→ Roll eligible timed and ordinary events
→ Show event when selected, otherwise show monthly summary
→ Present outcome with explicit +/− changes
→ Autosave
→ Return to Life screen
```

### Event decision

```text
Event card
→ Read situation
→ Choose from unlabeled options
→ C# validates and resolves outcome
→ Outcome card lists every changed stat and system value with +/− notation
→ Life feed records the consequence
```

Risk labels remain hidden during ordinary play. Choice wording, character preparation, and context provide the information needed to make the guess. A later optional premium fourth choice may disclose and guarantee a narrowly defined positive stat boost; it must never disguise its exact benefit.

### Destination responsibilities

| Destination | Default landing content | Dominant action |
|---|---|---|
| Life | Feed, stats, current focus, upcoming decision, time controls | Choose an activity or advance time |
| Study | Education progress, disciplines, study sessions, path requirements | Learn and qualify |
| Work | Career paths, current role, work actions, business operations | Work, apply, or operate |
| Bank | Net worth, savings, cash flow/credit, investing, transaction history | Manage money |
| Social | Relationships, discovery, profiles, family state | Interact |
| Goals | Pinned goals, Main/Daily/Life boards, achievements | Navigate or claim |

The alternate five-tab navigation, action quota/`End Turn`, Stim Coins, and season progression shown in exploratory references are not part of this contract. Sparks are the single approved premium currency; they remain separate from cash/net worth and cannot gate the monthly loop. Legacy Gems is reserved for the Goals match-game theme. Store, subscription, and rewarded-ad behavior follow the gated launch scope in `docs/TASKS.md`; a Season Pass remains post-launch only.

## Reusable component order

Build the component layer in this order so each addition is exercised by the playable loop:

1. `StimButton`, `StimCard`, and `SectionHeader`
2. `StatMeter`, `MoneyValue`, and signed `ChangeBadge`
3. `EventCard`, `ChoiceButton`, and `OutcomeCard`
4. `TimelineEntry` and bottom navigation
5. `SkillMeter`, `TraitBadge`, and `FamilyCard`
6. Business, asset, and market summary components

All components must support dynamic text, readable focus states, screen-reader labels, reduced motion, and signed text feedback independent of color.
