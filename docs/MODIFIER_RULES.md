# Modifier Rules for Event Outcome Resolution

**Document:** MODIFIER_RULES.md  
**Version:** 0.1  
**Phase:** 0 (Design)  
**Status:** Phase 0 exit criteria

---

## Purpose

Event outcome probabilities are calculated by applying named modifier rules to a base success chance. This document defines the modifier set for MVP and the evaluation strategy.

Modifier rules are **configurable, data-driven, and evaluated in C#**—not hard-coded into Yarn dialogue scripts.

---

## Modifier Categories

### 1. Skill Modifiers

Skills directly increase or decrease the probability of success in events.

#### Format

```
{
  "id": "skill_SKILL_NAME_LEVEL_N",
  "type": "skill_based",
  "skillName": "negotiation",
  "minLevel": 3,
  "probabilityDelta": +8
}
```

#### MVP Skill Modifiers

| ID                      | Trigger                 | Effect | Use Cases                               |
| ----------------------- | ----------------------- | ------ | --------------------------------------- |
| `skill_negotiation_2`   | Negotiation ≥ level 2   | +5%    | Salary, supplier, relationship disputes |
| `skill_negotiation_4`   | Negotiation ≥ level 4   | +12%   | Major contract, high-stakes deal        |
| `skill_finance_2`       | Finance ≥ level 2       | +4%    | Investment, loan approval               |
| `skill_finance_4`       | Finance ≥ level 4       | +10%   | Complex investment, tax event           |
| `skill_coding_3`        | Coding ≥ level 3        | +7%    | Tech career, freelance pitch            |
| `skill_sales_3`         | Sales ≥ level 3         | +6%    | Business growth, vendor relations       |
| `skill_leadership_3`    | Leadership ≥ level 3    | +5%    | Promotion, team conflict                |
| `skill_communication_2` | Communication ≥ level 2 | +3%    | Any social event                        |
| `skill_fitness_3`       | Fitness ≥ level 3       | +8%    | Health recovery, physical challenge     |
| `skill_medicine_4`      | Medicine ≥ level 4      | +12%   | Medical outcome, diagnosis              |

---

### 2. Stat Modifiers

Core stats influence event probabilities based on their current values.

#### Format

```
{
  "id": "stat_STAT_NAME_RANGE",
  "type": "stat_based",
  "statName": "health",
  "condition": "below_25",
  "probabilityDelta": -10
}
```

#### Condition Types

- `equals_VALUE` – exact match
- `above_VALUE` – strictly greater than
- `below_VALUE` – strictly less than
- `between_MIN_MAX` – inclusive range

#### MVP Stat Modifiers

| ID                        | Trigger         | Effect | Context                                 |
| ------------------------- | --------------- | ------ | --------------------------------------- |
| `stat_health_below_25`    | Health < 25%    | -8%    | Any event; -15% for positive outcomes   |
| `stat_health_above_75`    | Health > 75%    | +4%    | Physical and work events                |
| `stat_smarts_above_75`    | Smarts > 75%    | +6%    | Education, finance, coding events       |
| `stat_smarts_below_25`    | Smarts < 25%    | -5%    | School, learning, analysis events       |
| `stat_happiness_below_25` | Happiness < 25% | -4%    | Relationship, career progression events |
| `stat_happiness_above_75` | Happiness > 75% | +3%    | Social, opportunity events              |
| `stat_luck_above_75`      | Luck > 75%      | +5%    | Rare outcomes, critical events          |
| `stat_luck_below_25`      | Luck < 25%      | -6%    | Any event                               |

---

### 3. Trait Modifiers

Traits can increase, decrease, or unlock alternate outcomes.

#### Format

```
{
  "id": "trait_TRAIT_NAME_EFFECT",
  "type": "trait_based",
  "traitName": "ambitious",
  "probabilityDeltaForOutcomeClass": {
    "positive": +6,
    "neutral": 0,
    "negative": -3
  }
}
```

#### MVP Trait Modifiers

| ID                                      | Trait       | Effect                                                         |
| --------------------------------------- | ----------- | -------------------------------------------------------------- |
| `trait_ambitious_boosts_career`         | ambitious   | +6% for positive career outcomes, -4% for safety-first choices |
| `trait_cautious_boosts_safety`          | cautious    | +8% for safe choices, -6% for extreme outcomes                 |
| `trait_resilient_boosts_recovery`       | resilient   | +7% for recovery from negative events                          |
| `trait_disciplined_boosts_learning`     | disciplined | +5% for skill XP gain, education                               |
| `trait_creative_unlocks_outcomes`       | creative    | Unlocks alternate positive outcomes in business/career         |
| `trait_empathetic_boosts_relationships` | empathetic  | +4% for relationship improvement events                        |
| `trait_competitive_boosts_risky`        | competitive | +5% for risky outcomes vs. safe alternatives                   |
| `trait_anxious_reduces_extreme`         | anxious     | -6% for extreme outcomes, +4% for safe choices                 |

---

### 4. History and Relationship Modifiers

Events can be influenced by prior history and relationship strength.

#### Format

```
{
  "id": "history_EVENT_ID_CONSEQUENCE",
  "type": "history_based",
  "originatingEventId": "childhood_broken_promise_001",
  "yearsAgo": { "min": 1, "max": 20 },
  "probabilityDelta": -8,
  "appliesTo": ["outcome_trust_repair"]
}
```

#### MVP History Modifiers

| ID                            | Trigger                         | Effect                                            | Purpose                             |
| ----------------------------- | ------------------------------- | ------------------------------------------------- | ----------------------------------- |
| `history_prior_job_loss`      | Job loss in previous 5 years    | -5% for job security, +8% for risk-taking         | Reflects instability or emboldening |
| `history_bankruptcy`          | Bankruptcy in previous 10 years | -12% for credit access, +6% for rebuilding events | Long-term credit shadow             |
| `history_successful_business` | Business survived 5+ years      | +6% for new business, +4% for investment          | Confidence and skill transfer       |
| `relationship_strong_trust`   | Relationship trust > 75%        | +7% for shared ventures, loans                    | Relationship strength matters       |
| `relationship_broken_trust`   | Relationship trust < 25%        | -10% for cooperation, -8% for loans               | Broken trust lingers                |

---

### 5. Resource Modifiers

Available cash and assets influence event probabilities.

#### Format

```
{
  "id": "resource_RESOURCE_TYPE_THRESHOLD",
  "type": "resource_based",
  "resourceType": "cash",
  "condition": "below_50000",
  "probabilityDelta": -4
}
```

#### MVP Resource Modifiers

| ID                            | Trigger                 | Effect                                        | Context                  |
| ----------------------------- | ----------------------- | --------------------------------------------- | ------------------------ |
| `resource_cash_abundant`      | Cash > net_worth × 0.4  | +3% for investment, -2% for risk-averse       | Liquidity confidence     |
| `resource_cash_scarce`        | Cash < net_worth × 0.1  | -6% for investment, +4% for desperate choices | Scarcity bias            |
| `resource_debt_high`          | Debt > net_worth × 1.5  | -8% for credit events, +6% for extreme income | Desperation and pressure |
| `resource_assets_diversified` | Assets in 3+ categories | +4% for financial events                      | Portfolio confidence     |
| `resource_business_owned`     | Active business exists  | +5% for business-related events               | Ownership mindset        |

---

### 6. Age and Life-Stage Modifiers

Age unlocks or restricts certain paths and influences probabilities.

#### Format

```
{
  "id": "age_STAGE_EFFECT",
  "type": "age_based",
  "ageRange": { "min": 40, "max": 60 },
  "probabilityDelta": +5,
  "appliesTo": ["outcome_promotion_senior"]
}
```

#### MVP Age Modifiers

| ID                       | Trigger   | Effect                                             | Purpose                        |
| ------------------------ | --------- | -------------------------------------------------- | ------------------------------ |
| `age_childhood_2_7`      | Age 2–7   | -20% for all outcomes; events are narrative-driven | Limited agency                 |
| `age_teenager_13_19`     | Age 13–19 | +4% for social/risk events, -3% for career         | Typical teen behavior          |
| `age_young_adult_20_30`  | Age 20–30 | +5% for career growth, +3% for investment          | Peak opportunity               |
| `age_midcareer_35_55`    | Age 35–55 | +6% for promotion, +4% for business ownership      | Experience and capital         |
| `age_late_career_55_65`  | Age 55–65 | -8% for job security, -6% for new ventures         | Fewer doors open               |
| `age_retirement_65_plus` | Age 65+   | +6% for investment events, -15% for career         | Fixed income, investment focus |

---

### 7. World State Modifiers

Dynamic world conditions (economy, market, health) affect local probabilities.

#### Format

```
{
  "id": "world_CONDITION_EFFECT",
  "type": "world_based",
  "conditionType": "economy",
  "conditionState": "recession",
  "probabilityDelta": -8,
  "appliesTo": ["outcome_job_promotion", "outcome_business_growth"]
}
```

#### MVP World Modifiers

| ID                        | Trigger                     | Effect                                                 | Applies To         |
| ------------------------- | --------------------------- | ------------------------------------------------------ | ------------------ |
| `world_strong_economy`    | Strong economy              | +5% job/career, +4% investment                         | Broad positive     |
| `world_recession`         | Recession                   | -8% job security, -6% investment, +8% extreme outcomes | Broad negative     |
| `world_high_inflation`    | High inflation (>5% annual) | -4% purchasing power, +3% wage growth pressure         | Financial events   |
| `world_housing_boom`      | Housing market boom         | +6% real estate returns, +4% property access           | Real estate events |
| `world_housing_decline`   | Housing market decline      | -5% real estate returns, -3% property access           | Real estate events |
| `world_tech_boom`         | Tech sector booming         | +8% tech career, +6% startup success                   | Tech events        |
| `world_unemployment_high` | Unemployment > 5%           | -10% job search, -4% negotiation                       | Career events      |

---

## Evaluation Strategy

### Modifier Evaluation Engine

```csharp
public class ModifierEvaluator
{
    public float CalculateFinalSuccessChance(
        float baseSuccessChance,
        Character character,
        IEnumerable<ModifierRule> applicableRules)
    {
        float totalDelta = 0f;

        foreach (var rule in applicableRules)
        {
            if (rule.Evaluate(character))
            {
                totalDelta += rule.ProbabilityDelta;
            }
        }

        float finalChance = Mathf.Clamp01(baseSuccessChance + (totalDelta / 100f));

        // Hard clamp: no outcome is guaranteed or impossible
        return Mathf.Clamp(finalChance, 0.05f, 0.95f);
    }
}
```

### Modifier Stacking Rules

1. **All applicable modifiers stack additively.** If three rules apply (+5%, +3%, -2%), total delta is +6%.
2. **No modifier is applied twice.** Each rule ID evaluates once per choice resolution.
3. **Outcome class is considered.** Some traits modify positive vs. negative outcomes differently (e.g., `trait_cautious_boosts_safety` adds more to safe outcomes than negative ones).
4. **Hard clamp range:** [5%, 95%]. No outcome is 100% guaranteed or impossible.

---

## Adding New Modifiers (Phase 1+)

When adding modifiers during Phase 1+ content:

1. **Propose the modifier** with ID, type, trigger, and delta.
2. **Verify no duplicate IDs** against this list.
3. **Test in isolation** – confirm the rule evaluates correctly on test data.
4. **Test in context** – run at least 100 seeded lives to check outcome distribution doesn't skew unexpectedly.
5. **Document in this file** before merging.

---

## MVP Modifier Validation (Phase 0 Exit)

All five representative events must pass:

1. Each event choice specifies its `baseSuccessChance`.
2. For each choice, the set of applicable modifiers is correctly identified.
3. Final success chance falls into the correct risk band after modifiers.
4. Risk label (Safe/Moderate/Risky/Extreme) matches final chance.

Example test:

```
Event: "career_salary_negotiation_001"
Choice: "Make the case with results"
Base chance: 55%
Player state:
  - Negotiation skill: 4
  - Trait: ambitious
  - Economy: strong
Applicable modifiers:
  + skill_negotiation_4: +12%
  + trait_ambitious_boosts_career: +6%
  + world_strong_economy: +5%
Total delta: +23%
Final chance: 55% + 23% = 78%
Expected risk band: Moderate (50–69%) ❌
Actual: 78% → Upper Moderate boundary or Moderate+

---

This needs review. Either:
1. Reduce trait_ambitious_boosts_career to +3%
2. Or lower base chance to 50%
3. Or clarify this is Moderate-to-Safe boundary
```

---

## Implementation Checklist (Phase 1)

**Current-code reconciliation:** deterministic eligibility, saved RNG, risk/reward calculation, weighted outcomes, requirements, Luck-influenced selection, and representative-event distribution tests exist. The generalized polymorphic modifier architecture proposed below has not been implemented as specified and should be treated as a future refactor/design option, not a missing prerequisite for the current resolver.

- [ ] Implement `ModifierRule` interface with Evaluate() method
- [ ] Create rule subclasses: SkillRule, StatRule, TraitRule, HistoryRule, ResourceRule, AgeRule, WorldRule
- [ ] Create `ModifierEvaluator` with stacking and clamping logic
- [ ] Create unit tests for each rule type
- [ ] Create integration test: resolve all 5 representative events 100+ times; verify internal risk bands match
- [ ] Wrap modifiers in ScriptableObjects or data files for remote configuration

---

## Notes

- Modifiers are **not** replacement values. They adjust probability, not hard rules.
- If a modifier is too powerful (moves outcome by >20%), it should be split into two smaller modifiers or reconsidered.
- Removed modifiers should be archived (never deleted) to preserve save migration compatibility.
