# Package Install Checklist

Install and verify packages in this order. Commit `Packages/`, `ProjectSettings/`, and imported package assets after each green compile so vendor changes stay easy to isolate.

- [ ] Open the project with Unity `6000.3.0f1` (Unity 6.3 LTS) and install iOS Build Support in Unity Hub.
- [ ] Confirm the project compiles and Edit Mode tests pass before importing vendors.
- [ ] Run **Tools → Stim Tycoon → Run Setup Check** and resolve every `FAIL` reported in the Console.
- [x] Use the native atomic JSON repository for local saves. Easy Save 3 remains an optional adapter behind `STIM_EASY_SAVE_3`.
- [ ] Import **Dialogue System for Unity** from My Assets; keep its SDK types behind `IStimDialogueBridge`.
- [ ] Install **Authentication** and **Cloud Save** from **Window → Package Manager → Unity Registry**.
- [ ] Install Apple's **GameKit** plugin for Game Center sign-in and link it through `IStimAccountService`.
- [ ] Install **Unity LevelPlay / Ads Mediation** last and keep placement calls behind `IStimAdsService`.
- [ ] Re-run Edit Mode tests, make an iOS development build, and record the exact installed versions before upgrading any vendor package.

Easy Save wiring currently stores the versioned save envelope as a single JSON value under `stim.autosave.latest.v1`. Invalid envelopes are rejected before disk writes. Do not enable `STIM_EASY_SAVE_3` until the package import has completed.
