# Package Retention and Activation Policy

This is the canonical decision record for direct Unity package dependencies. **All currently installed packages are intentionally retained.** Package audits may identify cost, risk, ownership, or activation requirements, but they do not authorize removal from `Packages/manifest.json`, `Packages/packages-lock.json`, or the Unity project. Any future removal requires a new, explicit owner decision that supersedes this policy.

| Package | Current evidence | Decision |
|---|---|---|
| 2D Sprite `1.0.0` | Sprite and UI-art workflow | Keep |
| Input System `1.19.0` | Production scene uses `InputSystemUIInputModule` | Keep |
| Test Framework `1.6.0` | Repository QA suites | Keep |
| Code Coverage `1.3.0` | Local and GitHub QA reports | Keep |
| UI Toolkit / UI Builder `2.0.0` | Production UI uses UXML, USS, `UIDocument`, and UI Builder assets | Keep |
| Device Simulator devices `1.0.1` | Explicit iPhone visual matrix and repository device profiles | Keep as editor tooling; not player runtime content |
| Yarn Spinner (commit `4c0c8ef…`) | Authored dialogue and event-choice flow | Keep pinned |
| IAP `5.4.1` | No first-party purchase initializer; commerce remains gated | Keep installed; services must remain disabled until fulfillment, restore, persistence, privacy, sandbox, and device gates pass |
| LevelPlay `9.5.0` | No first-party ad initializer; rewarded ads remain gated | Keep installed; do not initialize until consent, bounded reward, privacy, iOS build, and physical-device gates pass |
| Analytics `6.3.0` | No approved first-party event implementation | Keep installed but inactive; do not collect until consent, revocation, deletion, schema, retention, and privacy gates pass |
| AI Assistant `2.14.0-pre.1` | Editor-only development tool; no runtime or first-party source dependency found | Keep installed; preview-version changes require explicit review |
| AI Inference `2.6.1` | No current on-device model feature or first-party source/serialized dependency found | Keep installed; no runtime feature may depend on it without an approved product and performance plan |
| Collab Proxy `2.12.4` | Repository uses GitHub; no first-party Unity Version Control dependency found | Keep installed as editor tooling; GitHub remains the repository authority |
| Visual Scripting `1.9.11` | No current first-party graph or C# dependency found | Keep installed; C# remains the gameplay architecture unless a feature explicitly adopts graphs |
| Timeline `1.8.12` | No current first-party `PlayableDirector`, Timeline C#, or serialized dependency found | Keep installed for potential authored transitions; no current runtime dependency claimed |
| Development feature set `1.0.2` | Meta-package owns development tooling and transitive dependencies | Keep installed; inspect ownership before upgrades, but do not detach or remove child packages |

Built-in modules such as physics, terrain, cloth, vehicles, video, VR, and XR are also retained. Build-size measurement may identify their cost, but it does not authorize manifest or lockfile changes.

## Package change protocol

1. Preserve every current direct dependency, transitive dependency, and built-in module unless the owner explicitly changes this policy.
2. Inspect direct and transitive Package Manager ownership before any version change.
3. Change at most one package or feature set per branch; keep `manifest.json` and `packages-lock.json` consistent.
4. Let Unity reimport and compile, then run setup validation plus quick/full EditMode and PlayMode suites.
5. For IAP, Analytics, LevelPlay, or other native SDK changes, produce an iOS development build and inspect generated native dependencies.
6. Record the reason, compatibility evidence, dependency-graph delta, test results, and rollback commit before merging.
7. Keep installed service packages inactive until their documented product, consent, privacy, persistence, and device-validation gates pass.
