# Stim Tycoon UI Asset Manifest

Production UXML/USS references only the approved, minimal asset subset under `Assets/StimDesignSystem`. Imported tools and license evidence live under `Assets/ThirdParty`; complete GUI packs, demos, size variants, and unrelated imagery are not retained.

Functional navigation uses the approved Lucide subset under `Assets/StimDesignSystem/Icons/Lucide`. Its ISC license is retained in `Assets/ThirdParty/Notices`.

The production component system is custom UI Toolkit: static composition lives in `VerticalSlice.uxml`, the header and navigation are live UXML templates, dynamic collections come from the runtime factories, and all flexible visual bounds come from Stim-owned USS. The installed packs are art libraries, not a second component runtime, and a new GUI pack should not be added unless it provides proven responsive UI Toolkit/UXML components that replace an identified Stim owner.

## Unity package audit

This table records the source packages from which the approved subset was copied. The full packages are no longer installed.

| Source package | Audited source contents | UI Toolkit integration rule |
|---|---|---|---|
| [Free Casual GUI](https://assetstore.unity.com/packages/2d/gui/free-casual-gui-332804), v1.0 | SVG/PNG uGUI pack with one demo and no UXML | Four approved controls and one progress asset are isolated behind responsive USS adapters. |
| [Space Game GUI Kit](https://assetstore.unity.com/packages/2d/gui/icons/space-game-gui-kit-298577), v1.1 | Large fixed-size uGUI prefab/image pack | Only eight 64px icons and one icon container remain; all unrelated sci-fi imagery and size variations were removed. |
| [Jelly pack UI](https://assetstore.unity.com/packages/2d/gui/jelly-pack-ui-347018), v1.0.0 | PNG uGUI demo pack | Only the approved star and progress background remain, used at authored aspect ratios. |

The packages therefore contain many Unity components, but only Space ships reusable component prefabs—and those prefabs belong to uGUI. Production adoption means porting the useful hierarchy, dimensions, typography, and interaction states into the existing UI Toolkit system, not merely referencing more sprites.

| Source | Role | Current adapter use | Evidence retained |
|---|---|---|---|
| Skyden Games — Free Casual GUI | Primary visual foundation | Calibrated nine-sliced SVG controls, palette and soft geometry for Stim-owned responsive surfaces, aspect-contained progress, and Baloo display text | `README.txt`, `Third-Party Notices.txt`, Unity `AssetOrigin`, Baloo OFL |
| Space Exploration GUI Kit | Layout and information hierarchy | All six navigation identities, destination/section pictograms, and information icons use standalone responsive-safe kit assets | `README.TXT`, Unity `AssetOrigin` |
| Jelly UI Pack | Rewarding interaction accents | Aspect-contained achievement/result marks; palette and geometry references for Stim-owned claim, qualification, amount-input, and pressed/highlight states | Unity import metadata; bundled font Apache 2.0 file |

## Selected source assets

- `Assets/StimDesignSystem/Buttons/{plain-bluegreen,plain-purplepink,plain-cream,circle-orange}.svg`
- `Assets/StimDesignSystem/Progress/{progress-bar.svg,progress-background.png}`
- `Assets/StimDesignSystem/Icons/{home,book,rocket,buy,heart,trophy,badge,info,star}.png`
- `Assets/StimDesignSystem/Containers/icon-container.png`
- `Assets/StimDesignSystem/Fonts/Baloo-Regular.ttf`
- `Assets/StimDesignSystem/Icons/Lucide/*.svg`

## Release gate

Approved artwork is never stretched as one undivided image behind dynamic text. Icons and progress art use `scale-to-fit`; selected SVG buttons use a calibrated USS nine-slice. Flexible shells, cards, rows, inputs, and sheets remain native UI Toolkit/UXML/USS.

Before distribution, verify the account entitlement and applicable Unity Asset Store license for each complete pack, retain its original notices, record any modified derivative, and confirm redistribution excludes standalone source-asset delivery outside the game/project terms. Font licenses remain separately attributable.
