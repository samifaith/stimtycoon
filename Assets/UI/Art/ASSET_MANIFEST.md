# Stim Tycoon UI Asset Manifest

Stim-owned UXML/USS references vendor assets in place. Do not edit or reorganize vendor folders; adapt them through `Assets/UI/Styles/StimTheme.uss` and `Components.uss`.

Functional navigation uses a Stim-owned subset of Lucide SVG icons under `Assets/UI/Icons/Lucide`. The subset retains Lucide's ISC license in that directory and may be recolored through the canonical USS adapter.

| Source | Role | Current adapter use | Evidence retained |
|---|---|---|---|
| Skyden Games — Free Casual GUI | Primary visual foundation | Palette and control-shape reference; source sprites remain deferred until sliced without intrinsic-size overflow | `README.txt`, `Third-Party Notices.txt`, Unity `AssetOrigin`, Baloo OFL |
| Space Exploration GUI Kit | Layout and information-hierarchy reference | Six-destination shell and dense-layout reference; large source sprites are not stretched onto live containers | `README.TXT`, Unity `AssetOrigin`, font OFL |
| Jelly UI Pack | Rewarding interaction accents | Reward palette, star reward icon, and pressed/highlight feedback; source button sprites remain deferred until safely sliced | Unity import metadata; bundled font Apache 2.0 file |

## Selected source assets

- `Free_Casual_GUI/Buttons/button_plain_blugreen.svg`
- `Free_Casual_GUI/Buttons/button_soft_blue.svg`
- `Jelly_UI_Pack/Sprites/Button/Primary_Btn/btnBlue_round.png`
- `Jelly_UI_Pack/Sprites/Icons/star_icon.png`

## Release gate

Before distribution, verify the account entitlement and applicable Unity Asset Store license for each complete pack, retain its original notices, record any modified derivative, and confirm redistribution excludes standalone source-asset delivery outside the game/project terms. Font licenses remain separately attributable.
