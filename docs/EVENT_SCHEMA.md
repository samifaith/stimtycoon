# Event Schema and Validator

**File:** `Assets/StimTycoon/Domain/Events/`
**Status:** Schema v1 foundation complete; offline event foundation delivered; staged catalog wiring and online validation deferred
**Version:** 1.0 (locked)

---

## Overview

All Stim Tycoon events conform to a single **locked schema v1.0**. This document explains the schema, how to author events, and how validation works.

Age bands, progression requirements, NPC-trigger priority, reward budgets, Life Feed ordering, ad-slot restrictions, and visual placeholder metadata are defined in [Content and Progression Standards](CONTENT_PROGRESSION_STANDARDS.md). New events must satisfy both documents.

### Key Principles

1. **Strict validation** – Events that don't conform to the schema are rejected before gameplay
2. **Versioned contract** – Schema version bumps only for breaking changes; minor additions go into existing fields
3. **Localization-first** – All user-facing text uses localization keys, not hard-coded strings
4. **Data-driven** – Events are pure data; C# logic is separate

---

## Schema Structure

### Event (Root)

```csharp
Event {
  schemaVersion: int = 1
  id: string (e.g., "career_salary_negotiation_001")
  category: EventCategory (Childhood, School, Career, Health, Money, Relationship, Business, World, Legacy)
  titleKey: string (localization key for event title)
  bodyKey: string (localization key for event body/description)
  toneTags: string[] (editorial guidance: "grounded", "tense", "warm", "funny", "direct")

  ageRange: AgeRange { minAge, maxAge }
  locations: string[] (must include "USA" and/or "Jamaica")

  requirementsJson: string (JSON object defining eligibility conditions)
  exclusionsJson: string (JSON object defining when event cannot run)

  choices: Choice[] (must have ≥2)
  cooldownYears: int (minimum years before event can repeat)
  repeatPolicy: RepeatPolicy (Never, OncePerLifeStage, Repeatable)
  analyticsTags: string[] (telemetry tags for balancing)
}
```

### Choice

```csharp
Choice {
  id: string (e.g., "negotiate")
  labelKey: string (localization key for button text)

  riskPreview: RiskLevel (Safe, Moderate, Risky, Extreme, or Calculated)
  rewardPreview: RewardLevel (Low, Medium, High, Exceptional)

  baseSuccessChance: float [0, 1] (before modifiers)
  modifierRuleIds: string[] (which modifiers apply, e.g., "skill_negotiation_2")

  requirements: string (JSON; choice-specific eligibility)

  outcomes: Outcome[] (must have ≥1)
}
```

### Outcome

```csharp
Outcome {
  id: string (e.g., "negotiation_success")
  classification: OutcomeClassification (Positive, Neutral, Negative)

  resultTextKey: string (localization key for result copy)
  feedEntryKey: string (life-feed summary localization)
  telemetryCode: string (analytics identifier)

  weightWithinResultGroup: float (relative probability)
  effects: Effect[] (state mutations)
  followUps: ScheduledEventRef[] (events triggered later)
}
```

An outcome's `feedEntryKey` is required for production content. Runtime rendering orders resolved entries using the deterministic Life Feed rules in the content standards; event authors must not encode ordering into localized copy.

### Effect

```csharp
Effect {
  type: EffectType (StatDelta, SkillXp, CashDelta, TraitAdd, etc.)
  targetId: string (what to modify, e.g., "health", "negotiation")
  value: float (magnitude)
  metadata: string (JSON for complex effects)
}
```

### NPC trigger and visual metadata

Schema v1 continues to express delayed NPC consequences through `followUps` plus `requirementsJson`, `exclusionsJson`, and `ScheduledEventRef.cancellationRule`. New NPC chains must also reserve stable trigger and relationship IDs in their content definition.

Visuals are optional to event resolution but required as content metadata before production-art exit. Reserve a stable `visualId`, role, aspect ratio, accessibility-label/decorative state, fallback, and source/license status. These are additive content fields; introducing them to serialized runtime events must remain backward compatible with schema v1 or use a separately reviewed schema version.

---

## Validation Rules

### Required Fields (Hard Errors)

- `schemaVersion` must equal `1`
- `id` must not be empty and must be globally unique
- `category` must be a valid EventCategory
- `titleKey`, `bodyKey` must not be empty
- `ageRange` must be valid (min ≤ max, both ≥ 0)
- `locations` must include at least one of: "USA", "Jamaica"
- `choices` must have at least 2 items
- Each choice must have at least 1 outcome
- Each choice must have `baseSuccessChance` in range [0, 1]
- Each outcome must have a unique `id` within its choice
- Each effect must have a non-empty `targetId`
- Event age ranges and choice thresholds must be reachable under the progression standards
- NPC follow-ups must include a cancellation rule and must not target an incompatible age, consent state, or family role

### Optional Fields (Warnings)

- `toneTags` – If empty, warning (should include editorial guidance)
- `analyticsTags` – If empty, warning (should include telemetry tags)
- `modifierRuleIds` – Warning if risky/extreme choice has no modifiers
- Effects – Warning if outcome has no effects
- `feedEntryKey` – Warning in prototype content; hard error at production-content validation
- `Effect.valueRuleId` – Optional registered balance key. When present, runtime resolution and previews use the configured value instead of the authored fallback `value`; unknown keys are hard errors. Cash, XP, stats, and future reward adapters must remain separate typed effects. Spark rules may not be activated until the versioned wallet and ledger gate is complete.
- Visual metadata – Warning until M17; hard error at production-art exit

---

## Authoring an Event (Workflow)

### Step 1: Define in Code or JSON

Create a `Event` object (in C# or as JSON):

```json
{
	"schemaVersion": 1,
	"id": "career_salary_negotiation_001",
	"category": "Career",
	"titleKey": "event.career.salary_negotiation.title",
	"bodyKey": "event.career.salary_negotiation.body",
	"toneTags": ["grounded", "direct"],
	"ageRange": {
		"minAge": 18,
		"maxAge": 75
	},
	"locations": ["USA", "Jamaica"],
	"requirementsJson": "{\"minAge\": 18, \"hasJob\": true}",
	"cooldownYears": 2,
	"repeatPolicy": "Repeatable",
	"analyticsTags": ["career", "negotiation"],
	"choices": [
		{
			"id": "make_the_case",
			"labelKey": "choice.make_the_case",
			"riskPreview": "Moderate",
			"rewardPreview": "High",
			"baseSuccessChance": 0.55,
			"modifierRuleIds": [
				"skill_negotiation_4",
				"trait_ambitious_boosts_career"
			],
			"outcomes": [
				{
					"id": "success",
					"classification": "Positive",
					"resultTextKey": "outcome.salary_increase.success",
					"feedEntryKey": "feed.got_raise",
					"telemetryCode": "salary_raise_success",
					"weightWithinResultGroup": 1.0,
					"effects": [
						{
							"type": "CashDelta",
							"targetId": "salary",
							"value": 5000
						},
						{
							"type": "SkillXp",
							"targetId": "negotiation",
							"value": 12
						}
					]
				}
			]
		},
		{
			"id": "let_it_pass",
			"labelKey": "choice.let_it_pass",
			"riskPreview": "Safe",
			"rewardPreview": "Low",
			"baseSuccessChance": 0.95,
			"modifierRuleIds": [],
			"outcomes": [
				{
					"id": "stable",
					"classification": "Neutral",
					"resultTextKey": "outcome.let_it_pass.stable",
					"feedEntryKey": "feed.no_raise",
					"telemetryCode": "no_raise",
					"weightWithinResultGroup": 1.0,
					"effects": []
				}
			]
		}
	]
}
```

### Step 2: Validate

In C#:

```csharp
var evt = LoadOrDeserializeEvent(json);
var result = EventValidator.ValidateEvent(evt);

if (result.isValid)
{
    Debug.Log("✓ Event is valid");
}
else
{
    Debug.LogError(EventValidator.GetValidationSummary(result, evt.id));
}
```

### Step 3: If Valid, Commit to Content

Store validated events in ScriptableObjects or a centralized data file.

---

## Modifier Rules Reference

See [docs/MODIFIER_RULES.md](../../docs/MODIFIER_RULES.md) for the complete list of modifiers that can be referenced in `modifierRuleIds`.

Example:

```csharp
modifierRuleIds: ["skill_negotiation_4", "stat_smarts_above_75", "trait_ambitious_boosts_career"]
```

---

## Using the Validator in Phase 1+

### Unit Test Validator

In `Assets/StimTycoon/Tests/Domain/Events/EventValidatorTests.cs`:

```csharp
[Test]
public void MyCustomEvent_PassesValidation()
{
    var evt = CreateMyEvent();
    var result = EventValidator.ValidateEvent(evt);
    Assert.IsTrue(result.isValid);
}
```

### Editor Batch Validation

Create an Editor script that validates all events at startup:

```csharp
[InitializeOnLoadMethod]
private static void ValidateAllEvents()
{
    var events = Resources.LoadAll<EventScriptableObject>("Events");
    foreach (var evt in events)
    {
        var result = EventValidator.ValidateEvent(evt.data);
        if (!result.isValid)
        {
            Debug.LogError($"Event {evt.name} failed validation:\n{EventValidator.GetValidationSummary(result, evt.name)}");
        }
    }
}
```

---

## Representative Events (Phase 0)

Five events serve as examples of the schema and branching patterns:

1. **Childhood: The Grown-Folks Table** – Ages 7–11, family conversation event
2. **School: Group Project Politics** – Ages 12–18, collaboration challenge
3. **Career: Say the Number** – Ages 18–75, salary negotiation
4. **Health: Your Body Is Asking for a Pause** – Ages 16–80, fatigue event
5. **Money: The Fast Return** – Ages 18–90, investment pitch

The representative event implementations and their Yarn nodes under `Assets/StimTycoon/Dialogue/Events` are the executable examples for this schema.

---

## Common Mistakes

| Mistake                                     | Fix                                                                               |
| ------------------------------------------- | --------------------------------------------------------------------------------- |
| Outcome weights don't sum to 1.0            | Weights don't need to sum to 1; they're relative probabilities                    |
| Hard-coded text instead of localization key | All user-facing text must use `*Key` fields                                       |
| Single outcome for a choice                 | Provide at least one outcome; use probability weighting if it's mostly one result |
| Using `Calculated` risk without modifiers   | Either define modifiers or use a fixed RiskLevel                                  |
| Forgetting to set `cooldownYears`           | Set to 0 for events that can repeat frequently; > 0 for events that need spacing  |
| Missing `telemetryCode` on outcomes         | Always include; used for analytics and balancing                                  |

---

## Phase 1+: Yarn Spinner Integration

Yarn Spinner owns player-facing dialogue and choice flow. The versioned Stim event remains the canonical source for eligibility, probability, outcomes, effects, and follow-ups.

Mapping:

- Yarn node → dialogue flow for a Stim event
- Yarn option → player-facing choice
- stable command arguments → Stim event ID and choice ID
- `stim_resolve_choice` → validated C# session transaction

C# validates the event and selected choice again before resolving or mutating state.

---

## Extending the Schema (Phase 2+)

To add a new field **without** breaking the schema version:

1. Add the field as optional in the class
2. Provide a sensible default value
3. Add a warning in the validator if it's important
4. Update this document
5. Do NOT bump `schemaVersion`

To make breaking changes (e.g., removing a field or changing a type):

1. Create a migration function: `MigrateEventV1toV2()`
2. Bump `schemaVersion` to 2
3. Update the validator to support both versions during a transition period
4. Retire old version support after a grace period

---

## Test Coverage

Current tests in `Assets/StimTycoon/Tests/Domain/Events/EventValidatorTests.cs`:

- ✓ Rejects null event
- ✓ Rejects wrong schema version
- ✓ Rejects missing required fields
- ✓ Rejects invalid age range
- ✓ Rejects too few choices
- ✓ Rejects duplicate choice IDs
- ✓ Rejects invalid success chance
- ✓ Rejects zero-weight outcomes
- ✓ Warns unknown locations
- ✓ Warns risky choices without modifiers
- ✓ Passes valid event

Add more tests as Phase 1 proceeds.
