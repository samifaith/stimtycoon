# Stim Tycoon UI Asset Manifest

Stim-owned UXML/USS references vendor assets in place. Do not edit or reorganize vendor folders; adapt them through the four production owners in `Assets/UI/Styles`: `StimTheme.uss`, `Shell.uss`, `Components.uss`, and `Destinations.uss`.

Functional navigation uses a Stim-owned subset of Lucide SVG icons under `Assets/UI/Icons/Lucide`. The subset retains Lucide's ISC license in that directory and may be recolored through the canonical USS adapter.

The production component system is custom UI Toolkit: static composition lives in `StimVerticalSlice.uxml`, the header and navigation are live UXML templates, dynamic collections come from the runtime factories, and all flexible visual bounds come from Stim-owned USS. The installed packs are art libraries, not a second component runtime, and a new GUI pack should not be added unless it provides proven responsive UI Toolkit/UXML components that replace an identified Stim owner.

## Unity package audit

This table is the implementation source of truth for the installed package versions. It is based on every bundled README/license, the supplied demo scenes and prefabs, serialized import settings, and the official Asset Store records.

| Package | Installed Unity contents | Author-provided scaling/state model | UI Toolkit integration rule |
|---|---|---|---|
| [Free Casual GUI](https://assetstore.unity.com/packages/2d/gui/free-casual-gui-332804), v1.0 | 288 SVGs, 63 PNGs, one uGUI demo, no prefabs or UXML; `README.txt` explicitly says optimized for uGUI | Demo CanvasScaler uses 1080×1920, Scale With Screen Size, 0.5 width/height match; demo contains 34 Buttons and both Simple/Sliced Image modes | Keep Stim's equivalent Panel Settings scaling. Port selected SVGs through explicit USS adapters; use a complete calibrated nine-slice for flexible buttons and aspect-fit for fixed icons/progress. |
| [Space Game GUI Kit](https://assetstore.unity.com/packages/2d/gui/icons/space-game-gui-kit-298577), v1.1 | 2,436 PNGs, 164 uGUI prefabs, 15 demos, three utility/editor scripts, no UXML | Four authored sizes (small/medium/large/extra-large); button prefabs use fixed RectTransforms, TMP autosizing, and normal/highlighted/pressed/disabled sprite swaps; demos use 1920×1080 Scale With Screen Size | Treat the prefab hierarchy and state sprites as a porting specification. Rebuild selected components in UXML/USS with the matching size/state asset family; do not stretch a single PNG to arbitrary dimensions or try to instantiate a uGUI prefab under UIDocument. |
| [Jelly pack UI](https://assetstore.unity.com/packages/2d/gui/jelly-pack-ui-347018), v1.0.0 | 101 PNGs, two uGUI demos, no prefabs, scripts, README, or UXML; the only bundled text is the Luckiest Guy Apache 2.0 license | Both demos use 1920×1080 Scale With Screen Size and height matching; the button demo contains 52 Buttons and predominantly Simple Image components; sprite borders are unset | Use at authored aspect ratios for reward, result, progress, toggle, and popup slots. A Jelly control requires a Stim wrapper and explicit state behavior; do not infer responsive slicing from the package because it does not provide slice borders or prefab contracts. |

The packages therefore contain many Unity components, but only Space ships reusable component prefabs—and those prefabs belong to uGUI. Production adoption means porting the useful hierarchy, dimensions, typography, and interaction states into the existing UI Toolkit system, not merely referencing more sprites.

| Source | Role | Current adapter use | Evidence retained |
|---|---|---|---|
| Skyden Games — Free Casual GUI | Primary visual foundation | Calibrated nine-sliced SVG controls, palette and soft geometry for Stim-owned responsive surfaces, aspect-contained progress, and Baloo display text | `README.txt`, `Third-Party Notices.txt`, Unity `AssetOrigin`, Baloo OFL |
| Space Exploration GUI Kit | Layout and information hierarchy | All six navigation identities, destination/section pictograms, and information icons use standalone responsive-safe kit assets | `README.TXT`, Unity `AssetOrigin`, font OFL |
| Jelly UI Pack | Rewarding interaction accents | Aspect-contained achievement/result marks; palette and geometry references for Stim-owned claim, qualification, amount-input, and pressed/highlight states | Unity import metadata; bundled font Apache 2.0 file |

## Selected source assets

- `Free_Casual_GUI/Other/Progress_Bar.svg.svg`
- `Free_Casual_GUI/Buttons/button_plain_{blugreen,purplepink,cream}.svg`
- `Free_Casual_GUI/Buttons/Button_circle_orange.svg.svg`
- `Jelly_UI_Pack/Sprites/ProgressBar/progress_bg.png`
- `Jelly_UI_Pack/Sprites/Icons/star_icon.png`
- `Space_Exploration_GUI_Kit/Containers/Medium/homepage-icon-container-medium.png`
- `Space_Exploration_GUI_Kit/Picto_Icons/Dark_Purple/{home,book,rocket,buy,heart,trophy,badge,info}-64.png`

## Release gate

Vendor artwork is never stretched as one undivided image behind dynamic text. Icons and progress art use `scale-to-fit`; selected Skyden SVG buttons use a complete calibrated USS nine-slice that protects their corners and shadows while allowing the center to expand behind contained labels. Flexible shells, cards, rows, inputs, and sheets use Stim-owned USS geometry sampled from the kits. Baked-label `panel_list_*` and fixed-layout `panel_status_score` remain in the untouched vendor folder for provenance and are quarantined from Stim-owned USS; dynamic copy and layout stay in UXML/runtime elements.

Before distribution, verify the account entitlement and applicable Unity Asset Store license for each complete pack, retain its original notices, record any modified derivative, and confirm redistribution excludes standalone source-asset delivery outside the game/project terms. Font licenses remain separately attributable.
