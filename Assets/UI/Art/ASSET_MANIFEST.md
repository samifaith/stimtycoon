# Stim Tycoon UI Asset Manifest

Stim-owned UXML/USS references vendor assets in place. Do not edit or reorganize vendor folders; adapt them through the four production owners in `Assets/UI/Styles`: `StimTheme.uss`, `Shell.uss`, `Components.uss`, and `Destinations.uss`.

Functional navigation uses a Stim-owned subset of Lucide SVG icons under `Assets/UI/Icons/Lucide`. The subset retains Lucide's ISC license in that directory and may be recolored through the canonical USS adapter.

| Source | Role | Current adapter use | Evidence retained |
|---|---|---|---|
| Skyden Games — Free Casual GUI | Primary visual foundation | Palette, control-shape reference, and vector treatment on the primary time control; broader use remains gated by responsive checks | `README.txt`, `Third-Party Notices.txt`, Unity `AssetOrigin`, Baloo OFL |
| Space Exploration GUI Kit | Layout and information-hierarchy reference | Standalone pictograms identify selected sections; large source sprites are not stretched onto live containers | `README.TXT`, Unity `AssetOrigin`, font OFL |
| Jelly UI Pack | Rewarding interaction accents | Reward palette, square star icon for claims and achievements, and pressed/highlight feedback; unsliced panels and mismatched-aspect buttons remain out of production | Unity import metadata; bundled font Apache 2.0 file |

## Selected source assets

- `Free_Casual_GUI/Buttons/button_plain_blugreen.svg`
- `Free_Casual_GUI/Buttons/button_soft_blue.svg`
- `Jelly_UI_Pack/Sprites/Button/Primary_Btn/btnBlue_round.png`
- `Jelly_UI_Pack/Sprites/Button/Primary_Btn/btnPink_wide.png`
- `Jelly_UI_Pack/Sprites/Icons/star_icon.png`
- `Space_Exploration_GUI_Kit/Picto_Icons/Dark_Purple/{book,home,heart,badge}-64.png`

## Release gate

Before distribution, verify the account entitlement and applicable Unity Asset Store license for each complete pack, retain its original notices, record any modified derivative, and confirm redistribution excludes standalone source-asset delivery outside the game/project terms. Font licenses remain separately attributable.
