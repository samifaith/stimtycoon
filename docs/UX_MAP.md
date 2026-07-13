# Stim Tycoon MVP Interaction Map

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

The first implementation lives in `Assets/UI/StimCozyCorporate.uss`. It is deliberately Stim-owned. A free third-party design-system package was evaluated, but its runtime did not compile against the pinned Unity `6000.3.19f1` API and was not retained.

## Persistent shell

```text
Player header
├── identity, age, and current role
├── cash / net-worth shortcut
└── expandable player overview

Active screen content
└── one dominant action

Bottom navigation
├── Life
├── Money
├── Social
└── Business
```

Career, investing, property, health, education, and activities begin as destinations within these four sections. They become separate tabs only if playtesting shows that the additional navigation is clearer.

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

### Secondary sections

| Destination | Default landing content | Dominant action |
|---|---|---|
| Money | Cash flow, debt, assets, recent transactions | Manage money |
| Social | Important relationships and recent changes | Interact |
| Business | Active ventures, opportunities, staff, and efficiency | Operate or invest |

## Reusable component order

Build the component layer in this order so each addition is exercised by the playable loop:

1. `StimButton`, `StimCard`, and `SectionHeader`
2. `StatMeter`, `MoneyValue`, and signed `ChangeBadge`
3. `EventCard`, `ChoiceButton`, and `OutcomeCard`
4. `TimelineEntry` and bottom navigation
5. `SkillMeter`, `TraitBadge`, and `FamilyCard`
6. Business, asset, and market summary components

All components must support dynamic text, readable focus states, screen-reader labels, reduced motion, and signed text feedback independent of color.
