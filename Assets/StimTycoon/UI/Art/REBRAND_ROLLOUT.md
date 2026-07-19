# Stim Tycoon Rebrand Rollout

## Codebase findings

- The production UI is `Assets/StimTycoon/UI/VerticalSlice.uxml`, backed by `VerticalSliceController` and structural UI tests.
- `AppHeader` and `BottomNavigation` are the only extracted UXML templates currently used by the production screen.
- `ActionTile`, `BaseCard`, and `StatRow` exist as a second-generation component library but are not instantiated by the production screen yet.
- The four directly referenced production owners are `Theme.uss`, `Shell.uss`, `Components.uss`, and `Destinations.uss`. Imports, the CozyCorporate cascade, the old vertical-slice sheet, and the unused prototype style system have been removed.
- Runtime factories create feed, achievement, relationship, account, career-path, stat, action, skill, and placeholder elements. Stable names, tooltips, and component classes are production contracts.
- Responsive behavior is controller-driven at 360 px and below, with tests covering 320, 360, 390, 430, and 768 px, large text, safe areas, bindings, and shell structure.

## Package documentation findings

- All three installed packages are authored for uGUI; none contains UXML or a UI Toolkit controller.
- Skyden documents uGUI directly and demonstrates portrait CanvasScaler scaling. Its SVG source can still be ported correctly because UI Toolkit supports vector backgrounds and USS nine-slicing.
- Space is the only pack with reusable Unity prefabs: 164 uGUI prefabs across four authored size tiers. Its button prefabs use Sprite Swap transitions and TMP autosizing, so production ports must carry normal, hover/focus, pressed, and disabled states—not just the normal sprite.
- Jelly has two uGUI demos but no README, prefabs, or scripts. Its PNGs have no configured sprite borders, so buttons and panels are fixed-aspect art unless Stim deliberately creates and validates a derivative adapter.
- Detailed counts, versions, official package links, and integration rules live in `ASSET_MANIFEST.md`.

## Brand ownership

| Pack | Brand responsibility | Approved first-wave use |
|---|---|---|
| Skyden Free Casual GUI | Everyday interaction language | Calibrated nine-sliced SVG controls, palette, Baloo display type, aspect-contained progress accents, and the visual reference for Stim-owned responsive surfaces |
| Jelly UI Pack | Reward and milestone emphasis | Production claims, achievements, qualification-current state, result marks, and amount input |
| Space Exploration GUI Kit | Information hierarchy | Production navigation, destination identities, section pictograms, and information icons |

The intended visual sentence is: **Skyden is the language, Space is the organization, and Jelly is the delight.**

## Reference authority

- The annotated grayscale screen specifications define information architecture, module priority, monetization placement, and interaction behavior.
- The colored mobile compositions define the production visual target: cool light canvas, compact white cards, navy typography, thin blue-gray outlines, purple/blue primary accents, and dense icon-led rows.
- The reference screens define the visible production surfaces. Imported packs provide only safe decorative accents, icons, color, and typography; they do not own responsive layout.
- Any implementation that materially increases row height, frames a page heading as a hero card, or applies a display font to body content fails the reference contract.

## Production component decision

- Do not add a fourth GUI kit to solve responsive layout. The installed Skyden README explicitly targets uGUI, but UI Toolkit provides equivalent panel scaling and USS nine-slicing for its SVGs; using those capabilities avoids a second UI runtime or another adapter.
- `VerticalSlice.uxml` owns static screen composition. `AppHeader` and `BottomNavigation` are its live UXML templates.
- `UiComponentFactory`, `ActionCardFactory`, and the existing domain-specific builders own dynamic collections whose count and copy change at runtime.
- The other extracted component UXML files are reviewable structure contracts and gallery references until a live screen explicitly instantiates them; their presence must not be described as production adoption.
- Stim-owned USS cards, rows, buttons, inputs, sheets, and state treatments are the canonical responsive component system. Vendor assets may decorate that system only in the text-free slots allowed by the production tests.

## Asset constraints found during audit

- Skyden's selected button is vector art at 326×115. Its rounded rectangle has an approximately 53–55 px protected perimeter, so the production adapter uses complete four-edge USS nine-slicing at a calibrated `0.4` scale rather than scaling the whole SVG out of proportion.
- Skyden's README and demo target uGUI and configure `Scale With Screen Size` at 1080×1920 with a 0.5 match. Stim's UI Toolkit equivalent is already configured in `PanelSettings.asset` at a portrait 390×844 reference with a 0.5 match.
- Skyden's panel vectors have fixed native proportions; `panel_list_*` also contains baked labels and `panel_status_score` contains fixed HUD decoration. Those complex panels remain untouched for provenance and are not used as arbitrary responsive surfaces. Flexible cards and rows use Stim-owned USS geometry and the source palette.
- Jelly's raster buttons and inputs have fixed proportions and no responsive sprite borders. Production uses Jelly artwork only for text-free achievement/result/progress marks; claim, badge, and input controls use Stim-owned surfaces sampled from its palette.
- Space container PNGs are not stretched onto responsive cards; the native 54×54 homepage frame is used only at its square aspect ratio around destination pictograms.
- Space pictograms are standalone square assets and are safe for `scale-to-fit` icon slots.
- Vendor folders remain immutable. All bindings live in Stim-owned USS/UXML.
- Components without a safe vendor surface still meet the same visual contract: layered warm/cool surfaces, 14–18 px rounded geometry, pale-blue outlines, navy text hierarchy, framed icon wells, compact reward pills, 44 px targets, and complete interaction states.

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
   - Static shell elements remain UXML templates; runtime collections use `UiComponentFactory` or the existing domain-specific factories.
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

6. **Responsive Stim component system with kit accents**
   - The playable root opts into the production visual adapter with `st-asset-kits-integrated`.
   - Skyden's palette, type, calibrated nine-sliced SVG buttons, and aspect-contained progress art define the everyday language; flexible shell, card, row, tab, input, and sheet geometry remains owned by Stim USS.
   - Space pictograms replace the previous navigation identity and cover every destination plus contextual information icons; destination identities use the kit's native square icon-container component.
   - Jelly artwork is live in text-free achievement, progress, and outcome/ending marks; its palette and geometry are carried by responsive Stim-owned reward and input controls.
   - Icons and progress artwork explicitly use `scale-to-fit`. Skyden button SVGs use complete, calibrated four-edge nine-slicing so the corners and shadows remain intact while the center expands behind contained labels. Tests prohibit unsliced stretching and incomplete or unapproved slicing.
   - Canonical UXML components and runtime factories carry explicit kit-brand contracts so new content inherits the same language.
   - Stim-owned detail rows, stat tiles, history, age states, icon wells, chips, and form controls use a tested fallback treatment matched to the packs rather than incompatible decorative artwork.

7. **Remaining visual release gate**
   - Port the Space cash/currency prefab pattern into the header and port one complete Space button family with normal, hover/focus, pressed, and disabled states before expanding its use.
   - Finish the compact control migration across Bank, Social, Goals, Study, and Work; prefer audited pack components at their supported geometry and use custom Stim surfaces only where no pack contract fits the colored mobile references.
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
