# Package Policy and Usage Matrix

This is the canonical keep/remove decision record for direct Unity package dependencies. A package is removed only on a disposable branch after serialized-reference inspection, Unity compilation, EditMode and PlayMode tests, and an iOS development build when native SDKs are involved.

| Package | Current evidence | Decision |
|---|---|---|
| 2D Sprite `1.0.0` | Sprite and UI-art workflow | Keep |
| Input System `1.19.0` | Production scene uses `InputSystemUIInputModule` | Keep |
| Test Framework `1.6.0` | Repository QA suites | Keep |
| Code Coverage `1.3.0` | Local and GitHub QA reports | Keep |
| UI Toolkit / UI Builder `2.0.0` | Production UI uses UXML, USS, `UIDocument`, and UI Builder assets | Keep |
| Device Simulator devices `1.0.1` | Explicit iPhone visual matrix and repository device profiles | Keep as editor tooling; not player runtime content |
| Yarn Spinner (commit `4c0c8ef…`) | Authored dialogue and event-choice flow | Keep pinned |
| IAP `5.4.1` | No first-party purchase initializer; commerce remains gated | Conditional keep for near-term M18 work; services must remain disabled until fulfillment gates pass |
| LevelPlay `9.5.0` | No first-party ad initializer; rewarded ads remain gated | Conditional keep for approved launch scope; validate privacy and iOS build before activation |
| Analytics `6.3.0` | No approved first-party event implementation | Removal candidate unless the consent and measurement plan enters active work |
| AI Assistant `2.14.0-pre.1` | No runtime or first-party source dependency found; editor-only development tool | Removal candidate; preview dependency requires explicit workflow owner |
| AI Inference `2.6.1` | No on-device model feature or first-party source/serialized dependency found | Removal candidate |
| Collab Proxy `2.12.4` | Repository uses GitHub; no first-party Unity Version Control dependency found | Removal candidate |
| Visual Scripting `1.9.11` | No first-party graph or C# dependency found | Removal candidate after Unity component/graph scan |
| Timeline `1.8.12` | No first-party `PlayableDirector`, Timeline C#, or serialized dependency found | Removal candidate after scene/prefab scan |
| Development feature set `1.0.2` | Meta-package ownership must be inspected in Package Manager | Review; do not remove individual transitive tools blindly |

Built-in modules such as physics, terrain, cloth, vehicles, video, VR, and XR are build-size experiment candidates, not ordinary cleanup. Determine feature-set and package dependencies in Package Manager before changing them.

## Removal protocol

1. Create a disposable branch from current `main`.
2. Inspect direct and transitive Package Manager dependencies.
3. Search C#, asmdefs, scenes, prefabs, ScriptableObjects, graphs, and build settings.
4. Remove one candidate through Package Manager.
5. Let Unity reimport and compile.
6. Run the setup check, quick/full EditMode, and PlayMode suites.
7. For IAP, Analytics, LevelPlay, or other native SDK changes, produce an iOS development build.
8. Record binary-size and dependency-graph changes before merging.
