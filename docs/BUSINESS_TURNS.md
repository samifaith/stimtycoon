# Business Turn System Design

**Document:** BUSINESS_TURNS.md  
**Version:** 0.1  
**Phase:** 0 (Design)  
**Status:** Phase 0 exit criteria

---

## Purpose

Businesses progress through two timescales:

1. **Annual cycle** – Triggered when the player presses "Age" / "Advance Year". Includes revenue settlement, expense accrual, taxes, and event rolls.
2. **Shorter turns** – Optional in-year actions the player can take _without advancing the character's age_. Includes hiring, marketing, pricing, operations, and tap-to-earn-style active engagement.

This document defines the turn frequency, action economy, UI patterns, and lifecycle.

---

## Timescale Overview

```
┌─────────────────────────────────────────────────────┐
│ Player Age = 25, Owns: Retail Storefront           │
├─────────────────────────────────────────────────────┤
│  SHORT TURNS (same year, same age)                  │
│  ├─ Turn 1: Hire employee                           │
│  ├─ Turn 2: Launch marketing campaign               │
│  ├─ Turn 3: Adjust pricing                          │
│  ├─ Turn 4: Tap-to-earn (active work)              │
│  └─ ...up to 10 turns per year (configurable)       │
├─────────────────────────────────────────────────────┤
│  ANNUAL CYCLE (on "Age" press)                      │
│  ├─ Settle annual revenue                           │
│  ├─ Deduct annual expenses                          │
│  ├─ Roll business events                            │
│  ├─ Recalculate valuation                           │
│  └─ Age character → Age becomes 26                  │
└─────────────────────────────────────────────────────┘
```

---

## Short Turns: Frequency and Limits

### Turn Budget per Year

- **MVP default:** 10 short turns per year, per business
- **Replenishment:** 1 new turn every ~3–4 real days (configurable via Remote Config)
- **Overflow:** Players can bank up to 3 unused turns (e.g., if they take 7 turns in week 1, they have 3 turns left plus incoming replenishment)
- **Display:** UI shows "X turns left / 10 available" in the business detail screen

### Why Turns Instead of Cooldown?

Turns provide:

- **Budget consciousness** – Players spend limited turns strategically
- **Remote balance** – Easy to tune via Remote Config
- **Clear feedback** – "X turns left" is immediately understandable
- **Growth trajectory** – Future phases can increase turn budget as business scales

---

## Short Turn Actions

### Category 1: Staffing

| Action               | Cost          | Effect                                              | Cooldown or Limit                     |
| -------------------- | ------------- | --------------------------------------------------- | ------------------------------------- |
| Hire employee        | 1 turn + cash | +1 staff count; monthly salary deducted from profit | None; multiple hires allowed per year |
| Fire employee        | 1 turn        | -1 staff count; potential morale event              | None                                  |
| Give raise           | 1 turn        | Employee morale +10%; salary cost increases         | 1 per employee per year recommended   |
| Train staff          | 1 turn        | Staff quality +5%; next turn delayed 2 days         | 1 per year per business               |
| Negotiate with staff | 1 turn        | Resolve labor unrest or prevent strike              | Only if morale < 30%                  |

### Category 2: Marketing and Sales

| Action          | Cost          | Effect                                              | Cooldown or Limit                               |
| --------------- | ------------- | --------------------------------------------------- | ----------------------------------------------- |
| Launch campaign | 1 turn + cash | +10–25% customer acquisition; cost varies by budget | 1–2 per year recommended                        |
| Discount prices | 1 turn        | +15% sales volume; margin reduced 5–10%             | Temporary (30 days in-sim)                      |
| Raise prices    | 1 turn        | -10% volume; +5–15% margin                          | Temporary; triggers customer satisfaction check |
| Run promotion   | 1 turn + cash | +20% foot traffic; cost based on reach              | 1 per season recommended                        |
| Active outreach | 1 turn        | +5–8% new customers; low cost                       | Unlimited; high effort, low reward              |

### Category 3: Operations and Upgrades

| Action             | Cost          | Effect                                                 | Cooldown or Limit                                    |
| ------------------ | ------------- | ------------------------------------------------------ | ---------------------------------------------------- |
| Buy equipment      | 1 turn + cash | +10% capacity or efficiency; large capital cost        | 1–2 per year                                         |
| Refurbish location | 1 turn + cash | Customer satisfaction +8%; medium cost                 | 1 per year                                           |
| Expand capacity    | 1 turn + cash | +25% max revenue potential; large cost; requires space | 1 per business lifetime                              |
| Cut costs          | 1 turn        | Operating expense -5% annually; risk: quality suffers  | 1 per year; -2% customer satisfaction if done twice+ |
| Improve process    | 1 turn        | Efficiency +5%; staff productivity +3%                 | 1 per year; stack multiple improvements              |

### Category 4: Active Earning

| Action           | Cost   | Effect                                                     | Cooldown or Limit                   |
| ---------------- | ------ | ---------------------------------------------------------- | ----------------------------------- |
| Work a shift     | 1 turn | +$50–200 depending on business type; +1 Smarts or skill XP | Unlimited; 1 per short turn session |
| Personally sell  | 1 turn | +$100–300 depending on sales skill and business type       | Unlimited                           |
| Manage paperwork | 1 turn | +$25–75; no skill effect; "boring but necessary"           | Unlimited                           |

### Category 5: Strategic

| Action               | Cost   | Effect                                                     | Cooldown or Limit                               |
| -------------------- | ------ | ---------------------------------------------------------- | ----------------------------------------------- |
| Seek loan            | 1 turn | Unlock business loan option; +cash; +debt                  | 1 per business; escalating interest if repeated |
| Court investor       | 1 turn | Attract venture capital; negotiation event; give up equity | 1 per business; major consequence               |
| Sell business        | 1 turn | Initiate sale; valuation + auction or direct sale          | 1 per business lifetime                         |
| Open second location | 1 turn | Expand; major capital requirement; +management complexity  | 1 per business family; requires high profit     |

---

## Short Turn Flow (UI/UX)

### Starting Business Detail Screen

```
┌─────────────────────────────────────────────────┐
│ My Retail Store (Age 25, Year 8)               │
├─────────────────────────────────────────────────┤
│                                                 │
│ TURNS REMAINING: 7 / 10                         │
│ [━━━━━━━ ━━]  (visual bar)                     │
│ Next turn in 3d 14h                             │
│                                                 │
│ Revenue (annual): $45,000                       │
│ Expenses (monthly): $3,200                      │
│ Staff: 4                                        │
│ Morale: 82%                                     │
│ Customer Satisfaction: 76%                      │
│                                                 │
├─────────────────────────────────────────────────┤
│ ACTIONS AVAILABLE:                              │
│                                                 │
│ [💼 Staffing (4)]                               │
│ [📢 Marketing (3)]                              │
│ [⚙️ Operations (5)]                              │
│ [💪 Active Earning]                             │
│ [🎯 Strategic (2)]                              │
│                                                 │
│ [← Back to Life] [Age & Settle Financials →]  │
└─────────────────────────────────────────────────┘
```

### Staffing Menu Example

```
┌─────────────────────────────────────────────────┐
│ STAFFING ACTIONS                                │
├─────────────────────────────────────────────────┤
│                                                 │
│ [+] Hire employee                               │
│     Cost: $1,200/month                          │
│     Effect: +1 staff, improves capacity         │
│     ← Confirm [✓] [✗]                           │
│                                                 │
│ [↑] Give raise to highest performer             │
│     Cost: +$400/month                           │
│     Effect: Morale +10%                         │
│     (You have 4 employees)                      │
│                                                 │
│ [📚] Train staff                                │
│     Cost: 1 turn                                │
│     Effect: Quality +5%; next turn in 2 days    │
│     Estimate: +$2,000–5,000 annual              │
│                                                 │
│ [← Back]                                        │
└─────────────────────────────────────────────────┘
```

After choosing "Hire employee" and confirming:

```
┌─────────────────────────────────────────────────┐
│ CONFIRM: Hire 1 Employee                        │
├─────────────────────────────────────────────────┤
│                                                 │
│ You will hire a new employee for your retail    │
│ store. Their base salary: $1,200/month.         │
│                                                 │
│ This will:                                      │
│ • Use 1 short turn                              │
│ • Increase monthly expenses by $1,200           │
│ • Boost your store's service capacity           │
│                                                 │
│ Continue?                                       │
│                                                 │
│ [✓ Yes, hire] [✗ Cancel]                        │
└─────────────────────────────────────────────────┘
```

After confirmation:

```
┌─────────────────────────────────────────────────┐
│ ✓ New Hire                                      │
├─────────────────────────────────────────────────┤
│                                                 │
│ You've hired Alex. They start next week with   │
│ enthusiasm and a clean slate.                   │
│                                                 │
│ Your store's capacity increased. You're        │
│ confident you can serve more customers.         │
│                                                 │
│ TURNS REMAINING: 6 / 10                         │
│                                                 │
│ [Continue to Business Menu]                    │
└─────────────────────────────────────────────────┘
```

---

## Annual Settlement Cycle

### Trigger

Player presses **"Age" / "Advance Year"** on main life screen.

### Process

```
1. Calculate annual revenue
   ├─ Base revenue = (daily rate) × 365
   ├─ Modifier: Customer satisfaction
   ├─ Modifier: Pricing tier
   ├─ Modifier: World economy state
   └─ Result: Adjusted annual revenue

2. Deduct annual expenses
   ├─ Payroll (staff count × monthly salary × 12)
   ├─ Rent or mortgage (if applicable)
   ├─ Utilities, insurance, etc.
   ├─ Equipment maintenance
   └─ Result: Total annual expense

3. Apply taxes
   ├─ Calculate taxable income (revenue - expenses)
   ├─ Apply business tax rate (configurable by location)
   └─ Result: After-tax profit

4. Roll business events (optional)
   ├─ Supplier issue
   ├─ Viral demand
   ├─ Employee scandal
   ├─ Equipment failure
   ├─ Recession impact
   └─ Result: Profit modifier (positive or negative)

5. Add profit to business cash
   └─ business.cashReserves += annual_profit

6. Recalculate business valuation
   ├─ Valuation = (annual profit × multiplier) + (asset value)
   ├─ Multiplier varies by business health and growth
   └─ Used for sale/acquisition offers

7. Reset short turn budget
   └─ Return to 10 turns (or configured value)
```

### Example Settlement

```
Annual Settlement: My Retail Store, Year 8

Revenue Calculation:
  Base daily rate:           $120
  × 365 days:                $43,800
  × Customer satisfaction:   +8% ($3,504)
  × Pricing strategy:        +5% ($2,340)
  × World economy (strong):  +3% ($1,440)
  ────────────────────────
  Annual revenue:            $51,084

Expenses:
  Payroll (4 staff):         -$57,600
  Rent/Location:             -$24,000
  Utilities/Insurance:       -$6,000
  Maintenance:               -$2,400
  ────────────────────────
  Total expenses:            -$90,000

Gross profit:                -$38,916

Business Events:
  "Supplier negotiation"     +$2,500 (positive outcome)
  "Customer satisfaction"    No change

Final profit:                -$36,416
  (You lost money this year. Cash reserves down.)

Valuation: $234,000
  (Down from $290,000 last year due to negative profit)

Next Year: Turns reset to 10/10
```

---

## Short Turn Frequency (Remote Config)

All timing is remotely tunable:

```
{
  "BusinessTurnsPerYear": 10,
  "TurnReplenishmentMinutes": 5760,        // 4 days
  "MaxTurnBank": 3,
  "TurnRefundOnAgeButton": false,          // Don't give turns when aging
  "BusinessEventChancePerYear": 0.35,      // 35% chance of 1+ event
  "BusinessLoanInterestRate": 0.12         // 12% APR
}
```

---

## Three Business Types: Turn Economy

### Type 1: Retail Storefront

- **Revenue model:** Daily foot traffic + purchases
- **Main expenses:** Payroll, rent, inventory, utilities
- **Margin:** 40–60% after all costs
- **Turn strategy:** Staffing and marketing matter most
- **Example short turns:** Hire, market, discount, buy equipment
- **Annual event frequency:** High (0.5 events/year average)

### Type 2: Digital Service Company

- **Revenue model:** Subscriptions or project fees; passive scale potential
- **Main expenses:** Payroll, software, server costs
- **Margin:** 60–80% after all costs
- **Turn strategy:** Product improvements and sales outreach
- **Example short turns:** Improve process, seek investor, active selling
- **Annual event frequency:** Medium (0.3 events/year average)

### Type 3: Property Rental Business

- **Revenue model:** Monthly rent collection; property appreciation
- **Main expenses:** Mortgage, property tax, maintenance, vacancy
- **Margin:** 20–40% after costs (low, but equity builds)
- **Turn strategy:** Maintenance and tenant relations matter most
- **Example short turns:** Refurbish, negotiate with tenants, buy equipment
- **Annual event frequency:** Medium (0.25 events/year average)

---

## State Machine: Business Lifecycle

```
┌─────────────────────────────────────────────────┐
│ [Startup]                                       │
│ └─ Capital required: Yes                        │
│    Skill check: Entrepreneurship or relevant    │
│    Duration: Player chooses start; Year 0       │
└────────────┬────────────────────────────────────┘
             │
             ↓
┌─────────────────────────────────────────────────┐
│ [Operating]                                     │
│ └─ Short turns available                        │
│    Annual settlement on Age                     │
│    Business events roll annually               │
│    Can sell, expand, or close                  │
└────────────┬────────────────────────────────────┘
             │
        ┌────┴────┬──────────┬─────────┐
        ↓         ↓          ↓         ↓
    [Thriving] [Stable] [Struggling] [Bankruptcy]
       │         │          │           │
       │         └──────────┴───────────┘
       │                    │
       ↓                    ↓
    [Sold]           [Closed/Failed]
```

---

## Debugging and Testing (Phase 1)

### Test Scenario 1: Basic Turn Spending

```gherkin
Scenario: Player spends turns, advances year

Given: Business in operating state with 10 turns
When:  Player hires 2 employees (2 turns)
       Player launches marketing (1 turn)
       Player works a shift (1 turn)
Then:  Turns remaining: 6/10

When:  Player presses "Age"
Then:  Annual settlement runs
       Turns reset to 10/10
       Character age increments
       Business profit/loss recorded
```

### Test Scenario 2: Annual Settlement Calculation

```gherkin
Scenario: Retail storefront annual settlement

Given:  Base revenue: $50k
        4 staff @ $1200/mo = $57.6k/year
        Rent: $2k/mo = $24k/year
        Other expenses: $8.4k/year
        Total expenses: $90k/year

When:   Player has 82% customer satisfaction
        World economy is strong
        No business events occur

Then:   Revenue = $50k × 1.08 (satisfaction) × 1.03 (economy)
              ≈ $55.6k

        Profit = $55.6k - $90k = -$34.4k LOSS

        Business cash decreases
        Valuation decreases
        Player gets warning in life feed
```

### Test Scenario 3: Turn Replenishment

```gherkin
Scenario: Turns replenish over time

Given:  Player has 0/10 turns
        Turn replenishment period: 4 days (96 minutes in fast mode)

When:   100 minutes pass in-game
Then:   Turns: 1/10

When:   200 more minutes pass
Then:   Turns: 2/10
        (Turn replenishment continues; cannot exceed max)
```

---

## Implementation Checklist (Phase 1–4)

### Phase 1 (Skeleton)

- [ ] Business data model with short turn budget
- [ ] Annual settlement calculation (revenue, expenses, taxes)
- [ ] Turn replenishment timer logic
- [ ] Basic turn action UI framework

### Phase 2–3

- [ ] Staff hiring, firing, morale mechanics
- [ ] Marketing and pricing actions
- [ ] Active earning (work a shift, personal sales)

### Phase 4 (Business MVP)

- [ ] All three business types fully implemented
- [ ] Equipment and upgrade system
- [ ] Business events integration
- [ ] Valuation and sale mechanics
- [ ] Turn action feedback (animations, life feed entries)

---

## Notes for Phase 1+

- **Turns as currency** – They're the primary gating mechanism for business engagement. Careful with reward pacing.
- **No turn refund on Age** – If players save turns for a year, that's intended play. Don't penalize it.
- **Annual settlement should surprise** – Business events, economic shifts, and tax calculations can swing outcomes. This keeps business play engaging.
- **Valuation is psychological** – Players like seeing their business valued. Use it to create pride or regret moments.
