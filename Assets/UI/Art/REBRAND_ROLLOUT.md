# Stim Tycoon Rebrand Rollout

## Codebase findings

- The production UI is `Assets/UI/StimVerticalSlice.uxml`, backed by `StimVerticalSliceController` and structural UI tests.
- `AppHeader` and `BottomNavigation` are the only extracted UXML templates currently used by the production screen.
- `ActionTile`, `BaseCard`, and `StatRow` exist as a second-generation component library but are not instantiated by the production screen yet.
- The four directly referenced production owners are `StimTheme.uss`, `Shell.uss`, `Components.uss`, and `Destinations.uss`. Imports, the CozyCorporate cascade, the old vertical-slice sheet, and the unused prototype style system have been removed.
- Runtime factories create feed, achievement, relationship, account, career-path, stat, action, skill, and placeholder elements. Stable names, tooltips, and component classes are production contracts.
- Responsive behavior is controller-driven at 360 px and below, with tests covering 320, 360, 390, 430, and 768 px, large text, safe areas, bindings, and shell structure.

## Brand ownership

| Pack | Brand responsibility | Approved first-wave use |
|---|---|---|
| Skyden Free Casual GUI | Everyday interaction language | Primary time-advance button and later standard controls |
| Jelly UI Pack | Reward and milestone emphasis | Achievement/claim accents; button art remains gallery-only until a matching-aspect production slot exists |
| Space Exploration GUI Kit | Information hierarchy | Standalone pictograms for sections and destinations |

The intended visual sentence is: **Skyden is the language, Space is the organization, and Jelly is the delight.**

## Reference authority

- The annotated grayscale screen specifications define information architecture, module priority, monetization placement, and interaction behavior.
- The colored mobile compositions define the production visual target: cool light canvas, compact white cards, navy typography, thin blue-gray outlines, purple/blue primary accents, and dense icon-led rows.
- Asset packs support that target selectively. They do not override the reference hierarchy or turn routine cards into decorative game panels.
- Any implementation that materially increases row height, frames a page heading as a hero card, or applies a display font to body content fails the reference contract.

## Asset constraints found during audit

- Skyden's selected button is vector art at 326×115 and is safe on similarly proportioned fixed-height controls.
- Jelly's raster buttons have no sprite borders. They must be restricted to matching aspect ratios until Stim-owned 9-slice derivatives are approved.
- Space container PNGs are multiple-sprite imports with no sprite borders. They must not be stretched onto responsive cards.
- Space pictograms are standalone square assets and are safe for `scale-to-fit` icon slots.
- Vendor folders remain immutable. All bindings live in Stim-owned USS/UXML.

## Implemented migration sequence

1. **Foundation adapter**
   - Bind one production-safe asset from each pack.
   - Preserve every controller element name and query contract.
   - Keep vendor bindings opt-in through `st-brand-*` classes.
   - Completed: resolve the cascade into four exclusive production owners with no USS imports or duplicate exact selectors.

2. **Component gallery**
   - The non-production `ComponentGallery.uxml` now covers pack-backed buttons, disabled state, dashboard cards, progress, section icons, and reward presentation.
   - Add inputs, dialogs, and event-choice variants as those canonical components are introduced.
   - Validate 320, 390, 430, and 768 px layouts before promoting components.

3. **Canonical components**
   - Live cards, headings, feed rows, stats, relationships, achievements, accounts, and career paths now use production component contracts.
   - Static shell elements remain UXML templates; runtime collections use `StimUiComponentFactory` or the existing domain-specific factories.
   - Reviewable UXML contracts now exist for `SectionHeader`, `FeedRow`, `StatTile`, `AchievementRow`, `ActionCard`, and `InfoBanner` under `Assets/StimTycoon/UI/Components`; their stable root names are protected by EditMode tests.
   - The unused duplicate style system has been removed.

4. **Application shell**
   - Header and navigation occupy explicit non-shrinking template slots around one active destination scroller.
   - The Life-only action dock contains New Life, Advance Month, and Year. The profile cluster opens Life Summary and restores the prior destination and scroll state when closed.
   - Each destination retains an independent scroll offset across navigation; restoration is rescheduled after rebuilt content receives its final scroll range.
   - Baloo is opt-in for display headings and branded actions; dense values and body content keep the normal UI font.

5. **Core screen migration**
   - Life leads with Life Feed and Core Stats; Age Progression, detailed stats, skills, and home are in Life Summary.
   - Study, Work, Bank, Social, and Goals follow the reference module order while rendering only systems backed by current save data.
   - Bank now separates Savings, Credit/Cash Flow, and Investing into exclusive persistent tabs while retaining the shared net-worth context.
   - Qualification study cards now open a focused confirmation sheet with numeric effects, readiness, and monthly timing before the existing transactional action executes.
   - The age-appropriate Education catalog presents Applied Finance, Community Health, and Sustainable Trades over the migration-safe General/Academic/Vocational state, with qualification status, real career consequences, material requirements, and direct focus for resolvable choices.
   - Store, Stim+, ads, season pass, currencies, and mini-games remain intentionally absent.

6. **Remaining visual release gate**
   - Add focus/pressed/disabled states, restrained reward motion, long-text checks, and controller navigation.
   - Capture and approve the six destinations at every target width in the Unity runtime, then verify Asset Store entitlement, notices, font attribution, and exclusion of standalone vendor sources from distributable exports.

## Age-appropriate visibility rule

- Options from a later life stage are not rendered early merely to advertise a future unlock.
- Current-stage options may remain visible and locked for actionable requirements such as cash, qualification XP, skill level, cooldown, or prerequisite state.
- The live shell currently applies this boundary to formal education paths, adult career/business previews, manual work, investing, compatible-person discovery, and retirement.

## Definition of done for each migrated component

- Uses a canonical `st-*` class and an opt-in `st-brand-*` asset binding.
- Works at supported widths without clipped text or distorted art.
- Retains a 44 px minimum interactive target.
- Has normal, hover/focus, pressed, and disabled behavior.
- Preserves accessibility metadata and keyboard/controller focus.
- Is covered by a structural test before replacing repeated live markup.
