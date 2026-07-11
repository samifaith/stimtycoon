# Stim Tycoon

A mobile life and wealth simulation game that combines a choice-driven life timeline with deep business, investing, asset, skill, and legacy systems.

> **Final product name:** Stim Tycoon  
> **Platform target:** iOS first, with architecture that can support Android later  
> **Status:** Product definition and MVP planning  
> **Primary reference set:** Three gameplay recordings supplied by the product owner  
> **Important:** The recordings are inspiration for interaction patterns, pacing, information hierarchy, and feature depth. Stim Tycoon must use original branding, writing, visuals, balancing, content, data, and interface components.

---

## 1. Product Vision

Stim Tycoon is a replayable life simulator where every life begins with limited control and develops through choices, chance, skill growth, relationships, work, business ownership, investing, and asset accumulation.

The player does not simply build a company or watch a character age. They build a complete life.

The game should create the feeling that:

- every year matters
- every choice can help or hurt
- money creates options but not immunity
- skills change what is possible
- relationships and health affect financial decisions
- risk can create wealth, failure, scandal, or recovery
- no two lives unfold exactly the same way

### Core fantasy

Start with an ordinary life. Learn, work, take risks, build wealth, survive setbacks, and leave a legacy.

### Core loop

```text
Age or advance time
→ Receive life, work, market, or relationship events
→ Make a choice
→ Resolve a weighted positive, neutral, or negative outcome
→ Update stats, skills, relationships, money, and world state
→ Work, learn, invest, build, or recover
→ Repeat until retirement, death, bankruptcy, or another ending
```

### Economic loop

```text
Earn
→ Save
→ Buy
→ Upgrade
→ Generate income
→ Reinvest
→ Diversify
→ Increase net worth
→ Protect or lose wealth
```

### Progression loop

```text
Attempt action
→ Gain experience
→ Improve skill
→ Unlock stronger actions, careers, businesses, and outcomes
→ Face harder opportunities and risks
```

---

## 2. Product Pillars

### 2.1 Life first

The player is a person before they are a tycoon. Childhood, education, health, family, friendships, romance, reputation, and aging shape the financial game.

### 2.2 Wealth with consequences

Money is not a decorative score. It changes access, pressure, risk, relationships, taxes, lifestyle, exposure, and legacy.

### 2.3 Randomness with logic

Outcomes are not pure coin flips. Randomness is weighted by the player's skills, stats, history, choices, relationships, resources, and current world conditions.

### 2.4 Clear mobile interaction

The game should be easy to understand on a phone. Dense simulation is expressed through compact screens, cards, progress bars, modals, lists, and a readable life feed.

### 2.5 Replayability

Different origins, personalities, skill paths, careers, markets, relationships, and random events should produce meaningfully different lives.

### 2.6 Original identity

Stim Tycoon can borrow proven interaction principles from life and business simulators, but it must establish its own tone, visual system, event writing, economy, iconography, progression, and feature combinations.

---

## 3. Target Audience

Primary players:

- mobile simulation fans
- life simulation players
- idle and incremental economy players
- business and investing game fans
- players who enjoy short sessions with long-term progression
- players who enjoy replaying alternate lives and testing strategies

Session targets:

- quick session: 2 to 5 minutes
- standard session: 10 to 20 minutes
- long session: 30 minutes or more

The player should always have at least one meaningful action available, even when they do not want to advance a year.

---

## 4. Game Structure

## 4.1 New Life Setup

A new game creates a playable character and starting world state.

MVP setup fields:

- first and last name
- pronouns or gender identity
- country: USA or Jamaica
- city type
- appearance or avatar options
- socioeconomic background
- family structure
- starting personality traits
- optional difficulty mode
- optional random life button

### Starting background examples

- working-class household
- middle-income household
- wealthy household
- unstable household
- immigrant household
- business family
- single-parent household
- foster or adoptive family

Background affects probabilities and starting access. It must not permanently determine success.

Possible effects:

- starting family wealth
- education access
- health risks
- relationship stability
- early skill exposure
- inheritance chance
- debt pressure
- local opportunity pool

---

## 4.2 Time and Aging

The primary progression action is **Age** or **Advance Year**.

Pressing it moves the life forward and may trigger:

- birthday changes
- school progression
- graduation
- job progression
- salary updates
- business results
- investment performance
- rent or mortgage events
- health changes
- relationship changes
- taxes
- family events
- market shifts
- random events
- chained consequences from prior choices

Not every system must wait for annual progression. Some actions can occur within the current year.

Examples:

- practice a skill
- speak with a relative
- apply for a job
- negotiate salary
- buy or sell an asset
- invest
- start a business
- visit a doctor
- take a vacation
- commit or avoid a risky action

### Final time model

- one major Age action advances one year
- certain event chains occur immediately
- businesses progress through both annual aging and shorter in-year turns
- annual business and investment summaries resolve at age progression
- shorter turns support operating decisions, tap-to-earn, upgrades, staffing, and time-sensitive events without changing the character's age

---

## 4.3 Main Life Screen

The main screen is the heart of the game.

### Header

Displays:

- avatar
- full name
- age
- current role, school, or occupation
- location
- cash balance
- optional net worth shortcut

### Life feed

A chronological record of the current life.

It should show:

- age labels
- major events
- decisions
- outcome summaries
- milestones
- health events
- relationship changes
- education and career movement
- business launches or failures
- investment wins and losses
- property activity
- achievements
- legal or reputation events

The feed is not just history. It should help the player understand why their current state exists.

### Primary action

A large, central **Age** button.

### Persistent navigation

Recommended MVP navigation:

1. Life
2. Career
3. Money
4. Relationships
5. Activities

A later version may separate Business, Investing, Assets, and Profile when those systems become dense enough.

### Stat strip

The six finalized core stats remain visible or easily expandable:

- Health
- Looks
- Age
- Smarts
- Happiness
- Luck

---

## 5. Character Stats

Stats are broad measures of the character's current condition. They change throughout life and influence event weighting.

## 5.1 Final core stats

The six permanent core stats are:

### Health

Represents physical condition and longevity. It affects illness, injury, recovery, work capacity, medical events, and lifespan.

### Looks

Represents appearance, presentation, and perceived attractiveness. It can influence social opportunities, relationships, public-facing careers, confidence-related events, and certain business or media outcomes.

### Age

Represents the character's current age and is also the primary life-progression value. Age controls childhood stages, school progression, career eligibility, health risk, retirement timing, and end-of-life conditions.

### Smarts

Represents reasoning, learning ability, and accumulated knowledge. It affects school outcomes, skill growth, career qualifications, investing insight, and event options.

### Happiness

Represents emotional wellbeing and life satisfaction. It affects burnout, relationships, motivation, productivity, risky coping behavior, and some endings.

### Luck

Represents favorable chance. It modifies rare opportunities, critical outcomes, windfalls, setbacks, and uncertain events without guaranteeing success.

These six names are final for MVP. Other values such as stress, reputation, charisma, discipline, confidence, morality, and influence may exist as traits, status effects, skills, or derived values rather than permanent core bars.

---

## 6. Skills System

Skills are specific learned capabilities. They are separate from broad character stats.

### Core skill loop

```text
Learn
→ Practice
→ Gain experience
→ Level skill
→ Unlock opportunities
→ Improve success odds and outcome quality
```

### Skill categories

#### Academic and technical

- mathematics
- coding
- science
- writing
- finance
- law
- medicine
- engineering

#### Business

- sales
- marketing
- accounting
- operations
- negotiation
- leadership
- management
- entrepreneurship

#### Creative

- design
- music
- acting
- photography
- culinary arts
- fashion
- content creation

#### Social

- communication
- networking
- public speaking
- empathy
- persuasion
- conflict resolution

#### Physical and practical

- fitness
- driving
- repair
- construction
- gardening
- martial arts

#### Risk and underground systems for later releases

- deception
- stealth
- security
- gambling
- street knowledge

### Skill properties

Each skill should support:

- level
- experience points
- difficulty curve
- learning prerequisites
- decay rules, if used
- related traits
- related careers
- related businesses
- event modifiers
- visible unlocks

### Skill levels

Recommended scale:

1. Untrained
2. Beginner
3. Capable
4. Skilled
5. Expert
6. Master

Numeric XP can exist beneath these labels.

### Skill acquisition

Skills can improve through:

- school
- books
- courses
- mentors
- jobs
- hobbies
- repeated actions
- business experience
- relationship influence
- random events

### Skill consequences

Skills should not guarantee success. They change probability, available choices, cost, speed, and outcome severity.

Example:

```text
Event: A supplier raises prices unexpectedly.

Low negotiation:
- Accept increase
- Cancel order

High negotiation unlocks:
- Renegotiate contract
- Offer a longer commitment for a lower rate
- Find competing bids quickly
```

---

## 7. Event and Outcome Engine

The event engine is the foundation of replayability.

Every major event can lead to randomized positive, neutral, or negative outcomes.

## 7.1 Resolution model

```text
Trigger
→ Check eligibility
→ Present event and available choices
→ Apply player choice
→ Calculate weighted outcome probabilities
→ Select outcome branch
→ Apply immediate consequences
→ Schedule delayed or chained consequences
→ Write result to life feed
```

## 7.2 Outcome classes

### Positive

Examples:

- raise
- promotion
- recovered health
- successful investment
- stronger relationship
- viral business growth
- scholarship
- valuable connection

### Neutral or mixed

Examples:

- no change
- small gain with added stress
- promotion with relocation
- business growth with debt
- relationship survives but trust falls

### Negative

Examples:

- job loss
- injury
- failed investment
- debt
- lawsuit
- breakup
- business closure
- reputation damage
- addiction
- death

### Critical outcomes

Rare branches with major consequences:

- life-changing windfall
- catastrophic business failure
- severe illness
- criminal charge
- public scandal
- acquisition offer
- inheritance
- permanent disability

## 7.3 Probability weighting

Outcome probability may be influenced by:

- relevant skills
- core stats
- personality traits
- age
- education
- health
- money available
- debt
- relationship strength
- career experience
- business experience
- reputation
- location
- market conditions
- previous choices
- active status effects
- item or asset ownership
- controlled randomness

Example formula concept:

```text
Outcome Weight =
Base Weight
+ Skill Modifier
+ Stat Modifier
+ History Modifier
+ Relationship Modifier
+ Resource Modifier
+ World Modifier
+ Random Variance
```

The exact formula should remain configurable in data rather than hard-coded throughout the app.

## 7.4 Locked risk and reward mapping

Players see an approximate risk label and an approximate reward label before committing to a choice. Exact percentages remain hidden during normal play. The labels must reflect the final probability after skills, stats, traits, history, relationships, money, location, and world conditions are applied.

### Success probability bands

| Player-facing risk | Final success chance | Design meaning |
|---|---:|---|
| Safe | 70% to 100% | A reliable choice with limited downside or limited upside |
| Moderate | 50% to 69% | A real tradeoff with a reasonable chance of success |
| Risky | 30% to 49% | Failure is more likely than success, but the upside should matter |
| Extreme | 0% to 29% | A long shot with serious consequences and a meaningful payoff |

### Reward bands

| Player-facing reward | Expected value | Typical result |
|---|---|---|
| Low | Small, contained gain | modest cash, light XP, minor relationship or stat change |
| Medium | Noticeable progress | promotion progress, useful cash, meaningful XP, stronger relationship change |
| High | Major advantage | large cash gain, career leap, business growth, rare unlock |
| Exceptional | Life-changing outcome | major wealth, public recognition, ownership, legacy-level opportunity |

### Offset rule

Higher risk must generally offer higher potential reward. A risky choice cannot be presented as exciting if its best result is weaker than a safe alternative. Exceptions are allowed only when the choice serves character, morality, relationships, or recovery rather than financial optimization.

### Calculation rule

```text
Final Success Chance =
Clamp(
  Base Success Chance
  + Skill Modifier
  + Core Stat Modifier
  + Trait Modifier
  + History Modifier
  + Relationship Modifier
  + Resource Modifier
  + Location Modifier
  + World Modifier,
  Minimum Chance,
  Maximum Chance
)
```

Default clamp: `5%` minimum and `95%` maximum unless the event is explicitly guaranteed or impossible.

The UI label is calculated from the final chance, not the base chance. A choice may move from Risky to Moderate because the player prepared well.

Relevant skills or traits may improve estimate precision in a later release, but MVP labels always use the four locked bands above.

## 7.5 Chained events

Choices can produce future events.

Example:

```text
Year 1: Borrow money from a friend.
Year 2: Friend asks for repayment.
Year 3: If unpaid, relationship falls or legal action begins.
```

Chained events need:

- originating event ID
- trigger conditions
- earliest and latest trigger age
- probability
- cancellation conditions
- consequence data

## 7.6 Anti-repetition rules

The engine should prevent:

- identical events occurring too frequently
- contradictory events
- dead characters receiving events
- children receiving adult-only actions
- events requiring assets the player does not own
- impossible relationship states
- excessive streaks of only positive or only negative outcomes

Randomness should feel surprising, not broken.

---

## 8. Personality and Traits

Traits influence behavior, event options, and probability.

Examples:

- ambitious
- cautious
- charming
- impulsive
- disciplined
- creative
- stubborn
- empathetic
- competitive
- anxious
- resilient
- dishonest

Traits are visible to the player. They can be:

- assigned at birth
- developed through repeated choices
- changed by major events
- temporary or permanent

Traits should occasionally unlock unique responses.

---

## 9. Childhood and Education

## 9.1 Childhood

Childhood cannot be skipped. Every life begins at birth and the character is born to two adults. Inclusive labels such as parents, guardians, caregivers, or other context-appropriate family wording may be used as the family evolves.

Childhood establishes family, early events, and starting direction.

Possible systems:

- parents and guardians
- siblings
- household income
- family conflict
- health conditions
- early friendships
- school quality
- hobbies
- bullying
- discipline
- adoption
- family moves

Childhood should move quickly enough that the player reaches meaningful agency without feeling trapped in a tutorial.

## 9.2 School

Education stages:

- primary school
- middle school
- high school
- college or university
- trade school
- graduate or professional school
- certificates and continuing education

School actions:

- study harder
- skip class
- join clubs
- play sports
- build relationships
- choose electives
- apply for scholarships
- change major
- drop out
- transfer

Education affects:

- intelligence
- skills
- debt
- career eligibility
- network
- stress
- future salary

---

## 10. Career System

The career system covers employment, progression, and workplace events.

## 10.1 Job structure

Each job can include:

- title
- industry
- employer type
- salary range
- required education
- required skill levels
- experience requirement
- location
- work stress
- benefits
- promotion ladder
- layoff risk
- automation risk
- reputation requirement

## 10.2 Career actions

- apply
- interview
- accept or reject offer
- negotiate compensation
- work harder
- reduce effort
- ask for promotion
- request raise
- build coworker relationships
- report misconduct
- quit
- change career
- retirement occurs automatically when the age threshold and eligibility rules are met

## 10.3 Workplace outcomes

- promotion
- raise
- bonus
- mentorship
- demotion
- burnout
- harassment or discrimination event
- layoff
- firing
- workplace injury
- public recognition
- relocation
- noncompete or legal dispute

## 10.4 MVP careers

Launch with a small but varied set of complete ladders, not dozens of shallow jobs.

Recommended five MVP career families:

1. Retail and service
2. Technology
3. Healthcare
4. Finance and business
5. Creative and media

Each family should contain entry, mid, senior, and leadership roles where appropriate.

---

## 11. Money System

## 11.1 Financial values

Track:

- cash
- income
- recurring expenses
- debt
- taxes
- investment value
- business value
- property value
- collectible value
- total assets
- total liabilities
- net worth

### Net worth

```text
Net Worth = Total Assets - Total Liabilities
```

## 11.2 Income types

- salary
- hourly work
- bonuses
- freelance income
- business profit
- rent
- dividends
- interest
- capital gains
- royalties
- inheritance
- gifts
- illegal income in later releases

## 11.3 Expense types

- housing
- utilities
- food
- transportation
- education
- healthcare
- insurance
- taxes
- debt payments
- childcare
- lifestyle
- business costs
- legal costs

## 11.4 Debt

Potential debt types:

- student loan
- credit card
- personal loan
- mortgage
- business loan
- medical debt
- tax debt

Debt should affect stress, credit access, cash flow, and event options.

## 11.5 Active earning

Tap-to-earn is included in MVP as an active earning mechanic. It must fit the life simulation and remain balanced against careers, businesses, and investing.

MVP and future implementations:

- MVP tap-to-earn work action
- temporary side hustle
- overtime shift
- gig work
- sell a service using a skill
- future small job minigames
- future longer-term career-specific interactions

It should not overpower careers, business ownership, or investing. Reward pacing, cooldowns, fatigue, or diminishing returns may be used for balance.

---

## 12. Business System

Business ownership is one of Stim Stim Tycoon's defining systems.

## 12.1 Business lifecycle

```text
Discover opportunity
→ Meet prerequisites
→ Fund startup
→ Choose strategy
→ Operate
→ Hire and upgrade
→ Handle events
→ Grow, franchise, sell, or close
```

## 12.2 Business properties

Each business can include:

- category
- name
- location
- startup cost
- ownership percentage
- revenue
- expenses
- profit
- cash reserves
- debt
- staff count
- customer satisfaction
- reputation
- capacity
- quality
- marketing level
- operational efficiency
- valuation
- risk level

## 12.3 Business actions

- start business
- purchase existing business
- choose location
- set pricing
- hire staff
- fire staff
- train staff
- increase wages
- buy equipment
- upgrade capacity
- advertise
- launch product
- open new location
- borrow money
- accept investor
- buy out partner
- sell business
- close business

## 12.4 Business events

- supplier issue
- viral demand
- bad review
- employee theft
- labor dispute
- equipment failure
- regulatory inspection
- tax audit
- competitor entry
- acquisition offer
- product recall
- lawsuit
- recession
- expansion opportunity

## 12.5 MVP business types

Recommended three complete types:

1. Food or retail storefront
2. Digital service company
3. Property rental business

These provide different economics and reuse several shared systems.

## 12.6 Business calculation

Annual profit concept:

```text
Revenue
- operating expenses
- payroll
- debt service
- taxes
- event losses
= annual profit or loss
```

Results should be influenced by:

- market demand
- pricing
- skills
- staff quality
- upgrades
- reputation
- random events
- local conditions

---

## 13. Investing System

## 13.1 MVP investment classes

- stocks
- index-style funds
- real estate

## 13.2 Later investment classes

- cryptocurrency
- bonds
- private companies
- commodities
- venture capital
- collectibles
- alternative assets

## 13.3 Stock system

Features:

- company list
- current price
- historical trend
- risk classification
- dividend yield
- buy
- sell
- holdings
- average cost
- gain or loss
- portfolio value
- annual dividend income

Stock movement may respond to:

- general market cycle
- company events
- industry events
- global events
- controlled randomness

## 13.4 Real estate investment

Features:

- market listings
- location
- purchase price
- property condition
- mortgage option
- maintenance
- taxes
- rent
- vacancy
- appreciation or depreciation
- renovation
- sell

## 13.5 Information advantage

Finance, economics, research, and networking skills can improve:

- available analysis
- risk labels
- scam detection
- negotiation
- access to opportunities

Skills should improve decisions, not reveal guaranteed outcomes.

---

## 14. Assets and Lifestyle

## 14.1 Asset categories

- homes
- rental properties
- vehicles
- businesses
- stocks
- collectibles
- luxury goods

## 14.2 Later luxury assets

- aircraft
- yachts
- islands
- rare art
- jewelry
- race cars
- museums
- casinos
- zoos

These are visible in the inspiration set as examples of endgame aspiration. They are expansion content, not MVP requirements.

## 14.3 Ownership effects

Assets may affect:

- net worth
- happiness
- upkeep
- status
- reputation
- access to events
- passive income
- insurance
- tax exposure
- theft risk

---

## 15. Relationship System

Relationship types:

- parent or guardian
- sibling
- extended family
- friend
- rival
- romantic partner
- spouse
- child
- coworker
- mentor
- business partner

## 15.1 Relationship values

Track as needed:

- closeness
- trust
- attraction
- respect
- resentment
- dependency

A simplified MVP may expose one relationship bar while maintaining hidden modifiers.

## 15.2 Relationship actions

- talk
- spend time
- give gift
- ask for help
- lend money
- borrow money
- date
- propose
- marry
- break up
- apologize
- argue
- recruit
- partner in business
- write into will
- cut off contact

## 15.3 Relationship consequences

Relationships can create:

- emotional support
- stress
- business opportunities
- referrals
- inheritance
- debt
- betrayal
- lawsuits
- marriage and divorce
- children
- caregiving duties

---

## 16. Health System

Health is both a stat and an event domain.

Systems may include:

- routine illness
- chronic conditions
- injuries
- mental health
- addiction
- disability
- medical treatment
- insurance
- fitness
- aging
- death

Health events must be written responsibly and should avoid presenting real medical advice as gameplay guidance.

### Health actions

- visit doctor
- seek treatment
- exercise
- change diet
- rest
- therapy
- rehabilitation
- ignore symptoms

Outcomes can depend on:

- age
- health stat
- stress
- money
- insurance
- prior conditions
- treatment choice
- controlled randomness

---

## 17. Activities System

Activities provide actions outside direct aging.

MVP categories:

- education
- skills
- mind and body
- social
- work and side income
- travel

Potential actions:

- read
- take course
- exercise
- meditate
- attend event
- practice instrument
- garden
- volunteer
- freelance
- travel
- shop

Later categories may include:

- crime
- gambling
- luxury services
- social media
- political activity
- supernatural or novelty modes

---

## 18. Dynamic World

The world should change independently of the player.

World state can include:

- economic cycle
- unemployment
- inflation
- interest rates
- housing market
- industry growth
- technology shifts
- public health conditions
- regional opportunity
- cultural trends

World events affect:

- jobs
- business demand
- investment prices
- borrowing
- real estate
- cost of living
- health risks

### MVP world model

Use a small number of clear global modifiers:

- strong economy
- normal economy
- recession
- high inflation
- housing boom or decline
- industry-specific growth or contraction

---

## 19. Risk, Crime, and Morality

Crime and darker risk systems are valid expansion paths, but they are not MVP requirements.

Later possibilities:

- theft
- fraud
- corporate misconduct
- tax evasion
- bribery
- scams
- investigation
- arrest
- trial
- prison

These systems need:

- age restrictions
- clear fictional framing
- consequences
- platform policy review
- careful tone

A morality or reputation model may track patterns without forcing a simplistic good-versus-evil meter.

---

## 20. Death, Retirement, and Legacy

A life ends through death or may conclude through retirement or special endings.

## 20.1 End-of-life summary

Display:

- age
- cause of ending
- final net worth
- peak net worth
- total career earnings
- business earnings
- investment earnings
- debt
- assets owned
- major relationships
- skill mastery
- achievements
- defining decisions
- reputation
- family or heirs

## 20.2 Legacy

Later legacy options:

- transfer wealth to heirs
- inherit businesses
- create dynasty
- continue as child
- family reputation
- estate taxes
- wills and trusts
- charitable foundation

## 20.3 Replay

The end screen should encourage another life through:

- new origin
- alternate strategy
- unlocked background
- achievement goals
- challenge mode
- random life

---

## 21. Achievements and Challenges

Examples:

- first $100,000
- first $1 million
- debt-free
- first business
- ten-year business survival
- master a skill
- graduate without debt
- recover from bankruptcy
- own five properties
- become a CEO
- maintain a lifelong friendship
- live past 100

Challenges can create curated runs:

- start with debt
- no college
- single-income family
- recession start
- ethical tycoon
- real estate only
- no inherited wealth

---

## 22. Screen Inventory

## 22.1 MVP screens

1. Splash and loading
2. New life setup
3. Main life timeline
4. Event decision modal
5. Outcome modal
6. Character stats
7. Skills list
8. Skill detail and practice
9. School or education
10. Career list
11. Job detail
12. Workplace actions
13. Money overview
14. Transactions or annual summary
15. Business list
16. Business creation
17. Business detail
18. Business upgrades
19. Investment overview
20. Stock market
21. Stock detail
22. Portfolio
23. Real estate market
24. Property detail
25. Owned properties
26. Relationships list
27. Relationship detail
28. Activities list
29. Health action screen
30. Profile and life statistics
31. Settings
32. Death or retirement summary
33. New life or replay screen

## 22.2 Later screens

- cryptocurrency
- luxury marketplace
- aircraft shop
- yacht shop
- collectible auctions
- casino ownership
- museum
- political career
- crime menu
- prison
- inheritance management
- dynasty tree
- social leaderboard

---

## 23. Interaction and UX Principles

### 23.1 One dominant action per screen

The primary next action should be obvious.

### 23.2 Short text, meaningful consequences

Event writing should be compact enough for mobile but specific enough to create personality and stakes.

### 23.3 Reveal detail progressively

High-level values appear first. Detailed calculations live in secondary views.

### 23.4 Persistent state clarity

The player should always know:

- who they are
- their age
- current role
- cash
- major stat changes
- why an outcome happened when explanation is appropriate

### 23.5 Fast feedback

Every action should produce visible feedback through:

- changed bars
- money movement
- small animation
- outcome card
- life feed entry
- unlock indicator

### 23.6 Accessibility

Requirements:

- dynamic text support
- screen reader labels
- sufficient contrast
- do not use color alone for meaning
- reduced motion support
- large tap targets
- captions or text for audio
- readable charts

---

## 24. Visual Direction

The inspiration references show a highly functional, list-driven mobile structure. Stim Tycoon should preserve the clarity while developing a distinct visual identity.

Recommended direction:

- contemporary editorial interface
- bold but controlled color system
- clean cards
- expressive character avatars
- compact icon library
- readable charts
- subtle celebratory animation
- stronger premium feel as wealth increases

The interface may visually evolve with the player's status, but navigation and readability should remain stable.

Avoid copying:

- logos
- exact layouts
- exact color combinations
- event text
- proprietary names
- icon assets
- character art
- exact progression values

---

## 25. Content Architecture

Most game content should be data-driven.

## 25.1 Locked event schema

All authored events must conform to one versioned Stim event contract. Dialogue System is the authoring surface. The C# simulation engine remains the authority for eligibility, probability, state mutation, scheduling, and validation.

### Required top-level fields

| Field | Type | Required | Purpose |
|---|---|---:|---|
| `schemaVersion` | integer | Yes | Version of the event contract |
| `id` | string | Yes | Stable, globally unique event ID |
| `category` | enum | Yes | Childhood, school, career, health, money, relationship, business, world, or legacy |
| `title` | localized string key | Yes | Short event heading |
| `body` | localized string key | Yes | Main event copy |
| `toneTags` | string array | Yes | Editorial guidance such as grounded, warm, tense, funny, direct |
| `ageRange` | object | Yes | Minimum and maximum eligible age |
| `locations` | string array | Yes | `USA`, `Jamaica`, or both |
| `requirements` | object | Yes | Conditions that must be true |
| `exclusions` | object | No | Conditions that must not be true |
| `choices` | array | Yes | Two or more player choices |
| `cooldownYears` | integer | Yes | Minimum years before the event can repeat |
| `repeatPolicy` | enum | Yes | `never`, `once_per_life_stage`, or `repeatable` |
| `analyticsTags` | string array | Yes | Stable tags for balancing and reporting |

### Required choice fields

| Field | Type | Required | Purpose |
|---|---|---:|---|
| `id` | string | Yes | Stable choice ID within the event |
| `label` | localized string key | Yes | Player-facing action text |
| `riskPreview` | enum or `calculated` | Yes | Safe, Moderate, Risky, Extreme, or calculated |
| `rewardPreview` | enum | Yes | Low, Medium, High, or Exceptional |
| `requirements` | object | No | Choice-specific eligibility |
| `baseSuccessChance` | number | Yes | Chance before player and world modifiers |
| `modifierRules` | array | Yes | Named modifiers evaluated by C# |
| `outcomes` | array | Yes | Positive, neutral, or negative result branches |

### Required outcome fields

| Field | Type | Required | Purpose |
|---|---|---:|---|
| `id` | string | Yes | Stable outcome ID |
| `classification` | enum | Yes | `positive`, `neutral`, or `negative` |
| `resultText` | localized string key | Yes | Player-facing result copy |
| `weightWithinResult` | number | Yes | Relative weight among outcomes in the resolved result group |
| `effects` | array | Yes | Typed state changes |
| `feedEntry` | localized string key | Yes | Life-feed summary |
| `followUps` | array | No | Events or statuses scheduled for later |
| `telemetryCode` | string | Yes | Stable analytics identifier |

### Supported effect types

- `stat_delta`
- `skill_xp`
- `cash_delta`
- `debt_delta`
- `relationship_delta`
- `reputation_delta`
- `health_condition_add`
- `health_condition_remove`
- `trait_add`
- `trait_remove`
- `status_add`
- `status_remove`
- `career_progress_delta`
- `business_metric_delta`
- `asset_add`
- `asset_remove`
- `schedule_event`
- `unlock_content`

### Canonical JSON representation

```json
{
  "schemaVersion": 1,
  "id": "career_salary_negotiation_001",
  "category": "career",
  "title": "event.career.salary_negotiation.title",
  "body": "event.career.salary_negotiation.body",
  "toneTags": ["grounded", "confident", "direct"],
  "ageRange": { "min": 18, "max": 75 },
  "locations": ["USA", "Jamaica"],
  "requirements": {
    "employmentStatus": ["employed"],
    "minimumMonthsInRole": 12
  },
  "exclusions": {
    "statuses": ["under_investigation"]
  },
  "cooldownYears": 3,
  "repeatPolicy": "repeatable",
  "analyticsTags": ["career", "negotiation", "income"],
  "choices": [
    {
      "id": "make_the_case",
      "label": "choice.career.salary_negotiation.make_case",
      "riskPreview": "calculated",
      "rewardPreview": "High",
      "baseSuccessChance": 55,
      "requirements": {},
      "modifierRules": [
        { "source": "skill.negotiation", "perLevel": 4, "cap": 20 },
        { "source": "stat.smarts", "curve": "standard_positive", "cap": 10 },
        { "source": "career.performance", "curve": "standard_positive", "cap": 15 },
        { "source": "trait.impulsive", "flat": -5 }
      ],
      "outcomes": [
        {
          "id": "raise_approved",
          "classification": "positive",
          "resultText": "outcome.career.salary_negotiation.approved",
          "weightWithinResult": 80,
          "effects": [
            { "type": "cash_delta", "mode": "annual_salary_percent", "value": 10 },
            { "type": "skill_xp", "skill": "negotiation", "value": 15 },
            { "type": "career_progress_delta", "value": 4 }
          ],
          "feedEntry": "feed.career.salary_negotiation.approved",
          "telemetryCode": "CAREER_NEGOTIATION_APPROVED"
        },
        {
          "id": "title_instead",
          "classification": "neutral",
          "resultText": "outcome.career.salary_negotiation.title_instead",
          "weightWithinResult": 20,
          "effects": [
            { "type": "career_progress_delta", "value": 8 },
            { "type": "skill_xp", "skill": "negotiation", "value": 8 }
          ],
          "feedEntry": "feed.career.salary_negotiation.title_instead",
          "telemetryCode": "CAREER_NEGOTIATION_TITLE_ONLY"
        },
        {
          "id": "request_declined",
          "classification": "negative",
          "resultText": "outcome.career.salary_negotiation.declined",
          "weightWithinResult": 100,
          "effects": [
            { "type": "stat_delta", "stat": "happiness", "value": -4 },
            { "type": "skill_xp", "skill": "negotiation", "value": 5 }
          ],
          "feedEntry": "feed.career.salary_negotiation.declined",
          "followUps": [
            { "eventId": "career_external_offer_001", "earliestYears": 1, "latestYears": 2, "chance": 25 }
          ],
          "telemetryCode": "CAREER_NEGOTIATION_DECLINED"
        }
      ]
    }
  ]
}
```

### Dialogue System implementation contract

Each Stim event maps to one Dialogue System conversation. The conversation stores copy and choice flow. Stim-owned Lua functions or sequencer commands bridge to C#.

Required Dialogue System fields:

```text
Conversation Fields
- StimEventId
- StimSchemaVersion
- Category
- MinAge
- MaxAge
- Locations
- CooldownYears
- RepeatPolicy
- AnalyticsTags

Dialogue Entry Fields
- StimChoiceId
- RiskPreview
- RewardPreview
- BaseSuccessChance
- ModifierRuleSetId
- OutcomeSetId
```

Recommended bridge calls:

```text
Stim.CanRunEvent("career_salary_negotiation_001")
Stim.GetRiskLabel("career_salary_negotiation_001", "make_the_case")
Stim.ResolveChoice("career_salary_negotiation_001", "make_the_case")
Stim.ApplyResolvedOutcome()
Stim.ScheduleFollowUps()
Stim.CommitAutosave()
```

Dialogue System conditions may hide obviously unavailable choices, but C# must validate every choice again before resolution. No gameplay state may be changed by raw Dialogue System Lua alone.

## 25.2 Phase 0 representative events

These five events are required Phase 0 content deliverables. They establish voice, structure, probability behavior, location support, and positive, neutral, and negative branching.

### Event 1: Childhood, The Grown-Folks Table

**ID:** `childhood_grown_folks_table_001`  
**Age:** 7 to 11  
**Locations:** USA and Jamaica  
**Setup:** Two adults in your household are discussing money at the table. You catch enough to know things are tight.

| Choice | Risk | Reward | Base success | Representative branches |
|---|---|---|---:|---|
| Ask what is going on | Safe | Medium | 80% | Positive: an adult explains budgeting and you gain Smarts and Financial Literacy XP. Neutral: they tell you not to worry, but your Happiness dips slightly. Negative: the conversation gets tense and you feel responsible for a problem that is not yours. |
| Offer your saved money | Moderate | Low | 60% | Positive: the adults thank you and protect your savings, raising the relationship. Neutral: they accept a small amount and promise to repay it. Negative: the money disappears into the household budget and trust falls. |
| Stay quiet and listen | Safe | Low | 75% | Positive: you learn without being pulled into the stress. Neutral: nothing changes. Negative: you misunderstand the situation and gain a temporary Worried status. |

**Voice note:** The child is observant, not written like a tiny adult. The adults are under pressure, not framed as failures.

### Event 2: School, Group Project Politics

**ID:** `school_group_project_politics_001`  
**Age:** 12 to 18  
**Locations:** USA and Jamaica  
**Setup:** Your group project is due soon. One classmate has gone quiet, another is trying to take over, and the work still needs to get done.

| Choice | Risk | Reward | Base success | Representative branches |
|---|---|---|---:|---|
| Reassign the work and set a deadline | Moderate | High | 65% | Positive: the group pulls together and you gain Leadership XP and Smarts. Neutral: the project is completed, but the friendship cools. Negative: the group resents the move and your social reputation drops. |
| Do the missing work yourself | Safe | Medium | 78% | Positive: the grade is strong and your Discipline skill improves. Neutral: the grade is fine, but Happiness drops from the extra load. Negative: the teacher notices the imbalance but still grades the group as one. |
| Tell the teacher exactly what happened | Moderate | Medium | 58% | Positive: the teacher creates individual grading. Neutral: the teacher gives the group one last chance. Negative: classmates call it disloyal and the relationship takes a hit. |

**Voice note:** No one is reduced to a stereotype. The conflict is about behavior, pressure, and accountability.

### Event 3: Career, Say the Number

**ID:** `career_salary_negotiation_001`  
**Age:** 18 to 75  
**Locations:** USA and Jamaica  
**Setup:** Your review is positive, but the raise is vague. You have one clean opening to ask for a specific number.

| Choice | Risk | Reward | Base success | Representative branches |
|---|---|---|---:|---|
| Make the case with results | Moderate | High | 55% | Positive: salary rises 8% to 12%. Neutral: you receive a stronger title or development plan. Negative: the request is declined, but Negotiation XP still increases. |
| Ask what would earn the raise | Safe | Medium | 75% | Positive: you receive measurable targets and a scheduled review. Neutral: the answer is polite but vague. Negative: your manager moves the goalposts later, triggering a follow-up event. |
| Let it pass | Safe | Low | 90% | Positive: workplace stability remains high. Neutral: nothing changes. Negative: inflation outpaces your pay and Happiness falls over time. |

**Voice note:** Confident without startup-bro language. The player is allowed to advocate for themselves without being punished by default.

### Event 4: Health, Your Body Is Asking for a Pause

**ID:** `health_persistent_fatigue_001`  
**Age:** 16 to 80  
**Locations:** USA and Jamaica  
**Setup:** You have been tired for weeks. Rest helps a little, but the feeling keeps coming back.

| Choice | Risk | Reward | Base success | Representative branches |
|---|---|---|---:|---|
| Make a medical appointment | Safe | High | 82% | Positive: the issue is identified early and treatment improves Health. Neutral: tests are inconclusive and follow-up is scheduled. Negative: the visit creates an expense and no immediate answer. |
| Reduce commitments and rest | Safe | Medium | 72% | Positive: Health and Happiness improve. Neutral: symptoms ease temporarily. Negative: an underlying condition remains and returns later. |
| Push through it | Risky | Medium | 38% | Positive: you meet an urgent obligation with no immediate decline. Neutral: performance slips. Negative: Health drops sharply and a more serious event is scheduled. |

**Voice note:** The event does not diagnose the player, shame them, or imply that rest fixes every condition.

### Event 5: Money, The Fast Return

**ID:** `money_fast_return_pitch_001`  
**Age:** 18 to 90  
**Locations:** USA and Jamaica  
**Setup:** Someone you know presents an investment that is supposed to pay back quickly. The details are thin, but the confidence is loud.

| Choice | Risk | Reward | Base success | Representative branches |
|---|---|---|---:|---|
| Ask for documents and verify the numbers | Safe | Medium | 85% | Positive: you uncover a strong legitimate opportunity or avoid a bad one. Neutral: the offer disappears when questioned. Negative: the relationship cools because the person feels challenged. |
| Invest a small amount | Risky | High | 42% | Positive: the investment pays 1.5x to 2x. Neutral: the money returns slowly with little gain. Negative: the money is lost and a trust-related follow-up event begins. |
| Go all in | Extreme | Exceptional | 18% | Positive: the return changes your financial position. Neutral: funds are locked for years. Negative: major loss, debt pressure, and relationship damage. |
| Walk away | Safe | Low | 95% | Positive: you avoid a loss and gain Financial Literacy XP. Neutral: nothing changes. Negative: the opportunity was real and you experience a temporary Regret status, not a permanent penalty. |

**Voice note:** Wealth is not coded as intelligence. Skepticism, luck, access, pressure, and relationships all matter.

## 25.3 Editorial standard for event writing

Stim Tycoon voice is sharp, human, culturally aware, and emotionally intelligent. It should feel observant rather than preachy.

Write with these rules:

- Use inclusive household, family, identity, and relationship language without turning the copy into a lesson.
- Let characters have specific motives, flaws, humor, and dignity.
- Avoid inspirational-poster dialogue, forced slang, therapy-speak, and corporate filler.
- Do not treat poverty, disability, immigration, nationality, gender, race, or family structure as a punchline or automatic tragedy.
- Do not make every good choice morally pure or every bad outcome deserved.
- Keep event bodies brief. Let the consequence carry the weight.
- Humor may be dry, situational, or character-based. It should not punch down.
- USA and Jamaica content should include local texture where it changes the situation, not random flags, accents, or stereotypes.

## 25.4 Recommended content data groups

- characters
- backgrounds
- traits
- stats
- skills
- events
- relationships
- education programs
- careers
- businesses
- investments
- properties
- assets
- world conditions
- achievements
- endings

---

## 26. Technical Architecture

## 26.1 Locked production stack

Stim Tycoon will use a Unity-first mobile game stack.

| Need | Selected technology |
|---|---|
| Game engine | Unity 6.3 LTS |
| Editor version | Latest stable `6000.3.x` patch approved by the project |
| Language | C# |
| Visual screen authoring | Unity Editor and UI Builder |
| Runtime UI | UI Toolkit |
| Layout files | UXML |
| Styling | USS |
| Reusable game content | ScriptableObjects plus validated content files |
| Branching event authoring | Dialogue System for Unity |
| Local saves | Easy Save 3 |
| Player identity | Unity Authentication |
| Apple platform identity | Apple Game Center through the Apple GameKit Unity plugin |
| Cloud saves | Unity Cloud Save |
| Achievements | Apple Game Center |
| Leaderboards | Apple Game Center, with Unity Leaderboards only when cross-platform needs justify it |
| Remote balancing | Unity Remote Config |
| Server-side logic | Unity Cloud Code where secure or authoritative logic is required |
| Analytics | Unity Analytics |
| Ads and mediation | Unity LevelPlay through the Ads Mediation package |
| Crash diagnostics | Unity Cloud Diagnostics or Sentry after production evaluation |
| Unit and integration tests | Unity Test Framework |
| Automated device flows | Unity testing plus Maestro or an equivalent mobile UI runner where practical |
| Source control | GitHub with Git LFS |
| iOS build output | Unity iOS build exported to Xcode |
| CI and distribution | GitHub Actions and Unity Build Automation as needed |
| Coding tools | Codex with Rider or VS Code |

### Version policy

- Start on Unity 6.3 LTS, not a short-support Update release.
- Install the newest stable `6000.3.x` patch available in Unity Hub when the repository is created.
- Record the exact Editor version in `ProjectVersion.txt` and the root README.
- Pin every Unity package and Asset Store dependency in source control.
- Do not auto-upgrade Unity, Dialogue System, Easy Save, LevelPlay, or Apple plugins during a sprint.
- Test dependency upgrades in a separate branch before merging.
- Commit `Packages/manifest.json` and `Packages/packages-lock.json`.
- Use iOS 13 or later as the initial minimum because current LevelPlay mediation supports iOS 13+ and Xcode 16+.
- Recheck Apple submission requirements before each release because Xcode and SDK requirements change.

## 26.2 UI implementation

Use UI Toolkit rather than the older Unity UI system for Stim Tycoon's app-like screens.

UI Builder is the main visual authoring interface. UXML defines structure, USS defines presentation, and C# binds data and behavior.

Create a custom Stim Tycoon component layer:

- `StimButton`
- `StimCard`
- `EventCard`
- `OutcomeCard`
- `ChoiceButton`
- `RiskRewardBadge`
- `StatMeter`
- `SkillMeter`
- `TraitBadge`
- `MoneyValue`
- `TimelineEntry`
- `BusinessSummary`
- `AssetRow`
- `FamilyCard`
- `SectionHeader`
- `BottomActionSheet`
- `OutcomeModal`
- `EmptyState`

Purchased or third-party UI assets may accelerate polish, but core layout, navigation, branding, and accessibility must remain owned by Stim Tycoon.

## 26.3 Branching event architecture

Dialogue System for Unity is the authoring and content-management layer for:

- event dialogue
- choices
- eligibility conditions
- positive, neutral, and negative branches
- follow-up events
- relationship interactions
- location-specific content
- skill and trait checks

Dialogue System must not become the sole owner of simulation rules. It sends commands to the custom C# simulation engine, which calculates probabilities, validates state changes, applies effects, and returns the resolved result.

```text
Dialogue and event definition
        ↓
Game command
        ↓
C# simulation engine
        ↓
Weighted outcome resolution
        ↓
State changes and follow-up scheduling
        ↓
Life feed, UI feedback, and save
```

## 26.4 Architecture layers

```text
Presentation Layer
- UI Toolkit screens
- UXML layouts
- USS styles
- reusable Stim components
- navigation and animation

Game Application Layer
- commands
- use cases
- event orchestration
- annual progression
- shorter business turns
- notifications

Simulation Domain
- character
- six core stats
- skills
- visible traits
- relationships
- career
- business
- finance
- world state
- risk and reward

Content Layer
- Dialogue System databases
- ScriptableObjects
- event definitions
- careers
- skills
- businesses
- localization
- USA and Jamaica content

Persistence and Services Layer
- Easy Save local autosaves
- versioned migrations
- Unity Authentication
- Unity Cloud Save
- Game Center identity, achievements, and leaderboards
- analytics
- ads
- remote configuration
```

## 26.5 Core custom C# systems

- age progression engine
- childhood and life-stage engine
- event eligibility engine
- seeded weighted outcome engine
- risk and reward estimator
- stat and skill engine
- visible trait engine
- relationship engine
- education and career engine
- tap-to-earn engine
- shorter-turn business engine
- annual business settlement engine
- finance and expense engine
- investment engine
- world condition engine
- retirement and ending engine
- save, migration, and synchronization engine

## 26.6 Save and account architecture

Use two coordinated save layers:

```text
Easy Save 3
- immediate local autosave
- offline recovery
- local settings and cached state

Unity Authentication + Cloud Save
- account-linked backup
- cross-device restoration
- conflict metadata
- secure player record
```

Launch flow:

1. Start the player immediately with Unity anonymous authentication.
2. Prompt the player to link Apple Game Center at an appropriate point.
3. Preserve the same Unity player identity when the account is linked.
4. Save locally after each resolved action.
5. Synchronize cloud data after significant milestones and app lifecycle events.
6. Never block core offline play solely because cloud synchronization is unavailable.

MVP save requirements:

- one active life
- automatic local save after every resolved action
- versioned save schema
- interrupted-write recovery
- authenticated cloud backup
- Game Center-linked identity
- deterministic conflict handling
- account deletion support

### 26.6.1 Locked save envelope

Every save uses a small versioned envelope around the game state. Easy Save serializes the local copy. Cloud Save stores the same logical envelope plus synchronization metadata.

```json
{
  "saveFormatVersion": 1,
  "minimumReaderVersion": 1,
  "gameBuildVersion": "0.1.0",
  "contentVersion": "2026.07.11.1",
  "saveId": "uuid",
  "playerAccountId": "unity-player-id",
  "lifeId": "uuid",
  "createdAtUtc": "2026-07-11T19:00:00Z",
  "updatedAtUtc": "2026-07-11T19:04:12Z",
  "revision": 42,
  "deviceIdHash": "non-reversible-hash",
  "rng": {
    "seed": 742981,
    "step": 188
  },
  "integrity": {
    "payloadHash": "sha256",
    "previousRevisionHash": "sha256-or-null"
  },
  "state": {
    "character": {},
    "coreStats": {},
    "skills": {},
    "traits": [],
    "relationships": [],
    "education": {},
    "career": {},
    "businesses": [],
    "finances": {},
    "assets": [],
    "health": {},
    "world": {},
    "eventHistory": [],
    "scheduledEvents": [],
    "lifeFeed": [],
    "unlocks": [],
    "adEntitlements": {},
    "analyticsConsent": {}
  }
}
```

### 26.6.2 Save invariants

A valid save must satisfy all of the following:

- `saveId`, `lifeId`, and `playerAccountId` remain stable for the life.
- `revision` increases by exactly one after each committed state-changing action.
- RNG seed and step are saved so outcomes can be reproduced during debugging.
- Event history stores event ID, choice ID, outcome ID, age, revision, and timestamp.
- Scheduled events store earliest trigger, latest trigger, chance, source event, and cancellation rules.
- Currency uses integer minor units, never floating-point values.
- Core stats are clamped to their allowed ranges during load and after migration.
- Unknown optional fields are ignored. Unknown required fields fail validation and trigger recovery.
- Authentication tokens, raw device identifiers, ad identifiers, and sensitive platform credentials are never stored inside the game-state payload.

### 26.6.3 Versioning strategy

Use three independent versions:

| Version | Example | Changes when |
|---|---|---|
| `saveFormatVersion` | `1` | Serialized structure or required field semantics change |
| `gameBuildVersion` | `0.1.0` | App build changes |
| `contentVersion` | `2026.07.11.1` | Events, balance tables, careers, businesses, or localization change |

Rules:

1. Save migrations are forward-only and sequential: `v1 → v2 → v3`.
2. Each migration is idempotent. Running it twice produces the same result as running it once.
3. Never overwrite the only known-good save during migration. Write a new candidate, validate it, then promote it.
4. Keep the two most recent validated local revisions plus one pre-migration backup.
5. A newer app must read all save versions released publicly.
6. An older app may reject a save whose `minimumReaderVersion` is newer than it supports.
7. Content changes should not require save migrations unless stored data semantics change.
8. Removed content is mapped to a safe replacement, archived state, or refund rule. It is never silently deleted.
9. Migrations must produce a structured report for QA and telemetry without exposing personal data.

### 26.6.4 Conflict resolution

Cloud conflicts are resolved in this order:

1. Prefer the save with the highest valid `revision` when both share the same ancestry.
2. If ancestry differs, prefer the save with the most recent committed gameplay action, not the latest background sync timestamp.
3. If both contain meaningful divergent progress, preserve both snapshots and present a clear restore choice.
4. Never merge RNG state, event history, money, or inventory field by field. Partial merges can duplicate rewards or corrupt causality.
5. Record the losing snapshot as a temporary recovery backup before promotion.

### 26.6.5 Autosave transaction

```text
Resolve command
→ Validate outcome
→ Apply state changes in memory
→ Increment revision
→ Serialize candidate save
→ Write temporary local file
→ Validate checksum and schema
→ Atomically promote local file
→ Update life feed
→ Queue cloud sync
```

Cloud failure never rolls back a valid local action. The sync remains queued and retries with backoff.

Later:

- multiple save slots
- Android identity provider
- expanded cross-platform accounts
- cross-device conflict UI

## 26.7 Ads architecture

Use Unity LevelPlay, installed through Unity Package Manager's Ads Mediation package.

MVP placements:

- rewarded ad for one optional outcome reroll
- rewarded ad for a temporary Luck or earnings boost
- rewarded ad for an additional short business turn
- rewarded recovery opportunity where narratively appropriate
- limited interstitials only at natural chapter or session boundaries

Rules:

- no ad is required to press Age
- no ad is required to save
- no ad is required to access the base childhood, career, business, or relationship systems
- rewards are disclosed before viewing
- ad frequency is remotely configurable
- child-directed ad treatment must follow the selected rating, storefront declarations, location, and applicable law
- the simulation must remain playable when ad inventory is unavailable

## 26.8 Deterministic testing

Randomness must support seeded runs.

This allows the team to:

- reproduce bugs
- verify event weights
- test complete lives
- compare balance changes
- run thousands of automated simulated lives
- confirm that risk labels broadly match actual outcomes
- detect impossible or contradictory state combinations

Required test groups:

- pure C# domain unit tests
- event schema validation
- weighted distribution tests
- save migration tests
- cloud conflict tests
- UI Toolkit component tests
- complete vertical-slice play tests
- iOS device build tests

## 26.9 Dependency guardrails

Third-party assets accelerate development but must remain replaceable.

- Wrap Dialogue System behind Stim-owned interfaces.
- Wrap Easy Save behind an `ISaveRepository` interface.
- Wrap LevelPlay behind an `IAdsService` interface.
- Wrap Unity Authentication and Cloud Save behind account and cloud-save interfaces.
- Store canonical game rules in Stim-owned C# classes and content schemas.
- Do not put irreversible business logic directly inside Unity scenes, visual graphs, or vendor-specific callbacks.
- Maintain an attribution and license inventory for every imported asset.

---

## 27. Analytics and Balancing

Track product and simulation health without collecting unnecessary personal data.

Useful analytics:

- new life completion rate
- average life duration
- session length
- age button frequency
- event choice distribution
- positive versus negative outcome rate
- career selection
- business survival rate
- bankruptcy rate
- death age distribution
- net worth distribution
- skill usage
- screen exits
- crash rate
- save recovery failures

Balance dashboards should reveal:

- overpowered careers
- impossible business paths
- repetitive events
- excessive negative streaks
- skills that rarely matter
- choices with no strategic value

---

## 28. Monetization Principles

Stim Tycoon launches with an ad-based monetization model. Monetization must not destroy the simulation or make failure feel like a payment trap.

### MVP ad formats

- rewarded ads for optional bonuses, rerolls, temporary boosts, or recovery opportunities
- limited interstitial ads at natural session breaks
- frequency caps and cooldowns
- no ad required to perform the core Age action
- no ad required to access basic saves or essential gameplay

### Design rules

- rewards must be clear before the player watches
- rewarded ads remain optional
- higher spending or ad viewing must not guarantee only positive outcomes
- avoid interrupting emotional, health, death, or major story moments
- avoid an interstitial after every action
- preserve PG-13+ content and advertising suitability

A future ad-free purchase may be evaluated after launch, but the launch model is ad-based.

---

## 29. MVP Definition

The MVP must prove that a complete life is fun and replayable.

## 29.1 Included

- final title and branding: Stim Tycoon
- new character creation
- three starting backgrounds
- mandatory childhood beginning with two adults
- MVP locations: USA and Jamaica
- annual aging
- shorter in-year business turns
- chronological life feed
- event choices
- weighted positive, neutral, and negative outcomes
- chained events
- six core stats
- visible core traits
- skills with levels and XP
- childhood and school progression
- five career families
- employment actions and promotions
- relationships
- health events
- cash, income, expenses, debt, and net worth
- three business types
- stocks
- basic real estate
- tap-to-earn active income
- active and passive income
- world economy conditions
- achievements
- death
- final life summary
- account-enabled save and resume
- Game Center integration
- ad-based monetization
- settings and accessibility basics

## 29.2 Excluded from MVP

- cryptocurrency
- aircraft
- yachts
- islands
- NFT systems
- museums
- casinos
- zoos
- political careers
- deep crime system
- prison
- multiplayer
- dynasty continuation
- advanced inheritance
- user-generated events
- live market data
- real-money financial mechanics

## 29.3 MVP success test

A player should be able to:

1. Create a character.
2. Grow from childhood to adulthood.
3. Develop skills.
4. Build relationships.
5. Enter and progress through a career.
6. Start at least one business.
7. Invest in stocks or property.
8. Experience both beneficial and harmful outcomes.
9. Recover from at least some setbacks.
10. Complete a life and understand what shaped the result.
11. Feel motivated to begin another life differently.

---

## 30. Development Roadmap

## Phase 0: Product Foundation

Deliverables:

- finalized game pillars
- terminology
- core stat list
- skill list
- MVP feature boundary
- screen inventory
- locked event schema and Dialogue System field mapping
- five representative authored events covering childhood, school, career, health, and money
- locked risk and reward probability bands
- save envelope, invariants, migrations, conflict handling, and versioning strategy
- economy assumptions
- original visual direction
- PG-13+ content boundaries
- USA and Jamaica content scope
- ad placement principles
- account and Game Center requirements
- repository setup

Exit criteria:

- no unresolved disagreement about the core loop
- every MVP system has an owner and definition
- all five representative events validate against schema version 1
- each representative event runs through Dialogue System into the C# resolver
- risk labels match the locked success bands after modifiers
- save version 1 passes round-trip, corruption recovery, migration, and cloud-conflict tests

## Phase 1: Simulation Skeleton

Build:

- character model
- age progression
- life feed
- stat changes
- event modal
- weighted outcome resolver
- save and load
- seeded randomness
- approximate player-facing risk and reward labels

Exit criteria:

- a test character can age from birth to death through placeholder events

## Phase 2: Skills, Education, and Relationships

Build:

- skills and XP
- practice actions
- school stages
- relationship records
- relationship actions
- skill and relationship event modifiers

Exit criteria:

- skills visibly unlock choices and alter outcomes

## Phase 3: Career and Personal Finance

Build:

- jobs and career ladders
- applications and interviews
- salary and recurring expenses
- debt
- annual finance summary
- tap-to-earn
- net worth

Exit criteria:

- a player can support or destabilize a life through work and spending

## Phase 4: Business MVP

Build:

- business creation
- three business types
- upgrades
- staffing abstraction
- revenue and expenses
- business events
- valuation and sale
- shorter-turn and yearly business progression

Exit criteria:

- businesses can succeed, stagnate, fail, or be sold for understandable reasons

## Phase 5: Investing and Property

Build:

- stock market
- portfolio
- dividends
- property market
- ownership costs
- rent
- appreciation
- sale

Exit criteria:

- investment outcomes affect the life without replacing all other play

## Phase 6: Complete Life and Legacy Summary

Build:

- health progression
- death conditions
- age-triggered retirement
- achievements
- final statistics
- replay flow

Exit criteria:

- one life can be completed from birth to final summary without developer intervention

## Phase 7: Content and Balance

Build:

- event library
- chained events
- background-specific content
- career events
- business events
- world events
- anti-repetition rules
- simulation testing tools

Exit criteria:

- multiple automated lives produce varied but plausible outcomes

## Phase 8: UX, Art, and Accessibility

Build:

- final component library
- avatar direction
- icons
- motion
- charts
- accessibility review
- device-size testing

Exit criteria:

- all core tasks are clear on supported iPhones

## Phase 9: Beta and Launch

Build:

- analytics
- crash reporting
- TestFlight
- Game Center validation
- account and cloud-save validation
- ad frequency and reward validation
- onboarding refinement
- performance tuning
- privacy disclosures
- App Store screenshots and metadata
- support workflow

Exit criteria:

- stable beta
- completed App Store review checklist
- launch candidate approved

## Phase 10: Post-Launch Expansion

Candidates:

- cryptocurrency
- deeper luxury assets
- advanced careers
- politics
- crime
- dynasty mode
- Android
- content packs
- challenge seasons

---

## 31. Suggested Repository Structure

```text
stim-tycoon/
├── app/
│   ├── life/
│   ├── career/
│   ├── money/
│   ├── relationships/
│   ├── activities/
│   └── settings/
├── src/
│   ├── components/
│   ├── domain/
│   │   ├── character/
│   │   ├── events/
│   │   ├── skills/
│   │   ├── relationships/
│   │   ├── careers/
│   │   ├── businesses/
│   │   ├── finance/
│   │   └── world/
│   ├── engines/
│   ├── content/
│   │   ├── events/
│   │   ├── careers/
│   │   ├── skills/
│   │   ├── businesses/
│   │   └── achievements/
│   ├── persistence/
│   ├── state/
│   ├── analytics/
│   ├── accessibility/
│   ├── utils/
│   └── types/
├── tests/
│   ├── unit/
│   ├── simulation/
│   └── integration/
├── docs/
│   ├── GAME_BIBLE.md
│   ├── EVENT_SCHEMA.md
│   ├── ECONOMY.md
│   ├── UX_MAP.md
│   ├── CONTENT_GUIDE.md
│   └── ADR/
├── assets/
└── README.md
```

---

## 32. Testing Strategy

### Unit tests

- stat calculations
- skill XP
- outcome weighting
- finance calculations
- business profit
- investment returns
- relationship changes
- event eligibility

### Simulation tests

Run thousands of seeded lives to test:

- average lifespan
- wealth distribution
- career completion
- bankruptcy rate
- event repetition
- skill relevance
- impossible states

### Integration tests

- create life to first age event
- school to career transition
- job income to net worth
- business purchase to annual result
- stock purchase to sale
- death to summary
- account-enabled save and resume
- Game Center integration
- ad-based monetization

### Manual testing

- content tone
- emotional plausibility
- readability
- accessibility
- animation comfort
- device performance

---

## 33. Content Writing Guide

Event writing should be:

- direct
- human
- specific
- brief
- varied in tone
- clear about the decision
- clear about the result

Avoid:

- repetitive jokes
- generic filler
- excessive exposition
- misleading options
- outcomes unrelated to the choice
- cruelty without purpose
- stereotypes presented as rules

A good event creates at least one of these:

- strategy
- tension
- surprise
- humor
- character development
- consequence

---

## 34. Final Product Decisions

These decisions are locked and override earlier assumptions:

1. Product name: **Stim Tycoon**
2. Core stats: **Health, Looks, Age, Smarts, Happiness, Luck**
3. Tone and target rating: **PG-13+**
4. Childhood cannot be skipped
5. Every life begins at birth and the character is born to two adults, using inclusive relationship wording where appropriate
6. MVP locations: **USA and Jamaica**
7. Businesses progress through both yearly aging and shorter turns
8. Tap-to-earn is included in MVP
9. Small job minigames and deeper long-term career interactions are future expansions
10. Approximate risk levels are visible
11. Higher risk should generally offer higher potential reward
12. Traits are visible
13. Retirement is triggered by age
14. Monetization is ad-based
15. Launch is account-enabled with Game Center and cloud-capable saves

---

## 35. Immediate Next Actions

1. Treat this README as the master product definition.
2. Finalize the initial skill catalog.
3. Define the event and outcome schemas and C# domain models.
4. Create the interaction map for all MVP screens.
5. Build the pure C# simulation skeleton before final visual polish.
6. Write the first 50 life events across childhood, school, career, relationships, health, and money.
7. Create automated tests for weighted positive, neutral, and negative branches.
8. Create the Unity 6.3 LTS project and pin the exact Editor and package versions.
9. Import and wrap Dialogue System, Easy Save 3, Apple GameKit, Unity Authentication, Cloud Save, and LevelPlay.
10. Build one complete vertical slice from age progression through an event, outcome, stat change, skill XP, finance change, life feed update, local save, and cloud synchronization.

---

## 36. Product Definition in One Sentence

**Stim Tycoon is a mobile life simulator where skills, choices, relationships, chance, work, business, and investing combine to create a different path to wealth, failure, recovery, and legacy in every life.**
