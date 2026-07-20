# Stim Tycoon

Stim Tycoon is a choice-driven mobile life and wealth simulator. Players grow from childhood through later life while managing education, careers, businesses, money, relationships, health, goals, and legacy.

The project currently provides a broad offline gameplay foundation and a playable six-destination vertical slice. Experience convergence, content depth, production services, and iOS release hardening remain in progress.

## Project facts

| Item | Value |
|---|---|
| Engine | Unity `6000.3.19f1` (Unity 6.3 LTS) |
| Primary platform | iOS portrait, iOS 13+ |
| UI | UI Toolkit, UXML, USS, C# |
| Dialogue | Yarn Spinner |
| Saves | Versioned native JSON with integrity checks and backup recovery |
| Tests | Unity Test Framework with EditMode, PlayMode, and simulation suites |

## Current state

Implemented foundations include:

- deterministic, save-backed event and outcome resolution;
- transactional actions, autosaves, migration, rollback, and duplicate protection;
- complete-life time progression with monthly and annual simulation;
- education, careers, business, banking, investing, home, relationships, family, goals, achievements, transitions, and endings;
- six persistent destinations: Life, Study, Work, Bank, Social, and Goals;
- reusable UI Toolkit bindings, presentation states, modal arbitration, and navigation restoration;
- 100 staged Yarn events behind a disabled-by-default rollout boundary;
- repository-owned EditMode, PlayMode, visual-capture, and long-run simulation tooling.

The current execution order and all remaining product decisions live in the [master task list](docs/TASKS.md). That is the only roadmap for this repository.

## Open and run

1. Install Unity `6000.3.19f1` with iOS Build Support.
2. Open this directory as an existing Unity project.
3. Open `Assets/StimTycoon/Scenes/VerticalSlice.unity`.
4. Enter Play Mode.

If the scene must be rebuilt, use `Tools → Stim Tycoon → Create Vertical Slice Scene`.

For a setup audit, use `Tools → Stim Tycoon → Run Setup Check`.

The repository also contains Device Simulator definitions for iPhone 17, iPhone 17 Pro, and iPhone 17 Pro Max. If they are not visible, use `Tools → Stim Tycoon → Install iPhone 17 Simulator Profiles`, then reopen Device Simulator.

## Run tests

Use the local runner:

```sh
scripts/qa/run-unity-tests.sh quick
scripts/qa/run-unity-tests.sh playmode
scripts/qa/run-unity-tests.sh all
scripts/qa/run-unity-tests.sh full
scripts/qa/run-unity-tests.sh simulation
scripts/qa/run-unity-tests.sh visual
```

Artifacts are written to the ignored `Artifacts/` directory.

See the canonical [QA baseline](docs/QA_BASELINE.md) for retained counts and current-checkout evidence. The [QA strategy](docs/QA_STRATEGY.md) defines test tiers and release requirements.

## Project structure

```text
Assets/
├── StimTycoon/
│   ├── Dialogue/          # Yarn-authored event dialogue
│   ├── Domain/            # Schemas, rules, validation, and abstractions
│   ├── Editor/            # Scene, simulator, and readiness tooling
│   ├── Integrations/      # Stim-owned vendor adapters
│   ├── Runtime/           # Sessions, persistence, composition, and binders
│   ├── Scenes/            # Playable Unity scenes
│   ├── Tests/             # EditMode and PlayMode coverage
│   └── UI/                # UXML, USS, components, icons, and panel settings
├── DeviceSimulatorDevices/
└── <vendor folders>/      # Imported packages retained in place

docs/                      # Supporting specifications and audits
scripts/qa/                # Headless Unity test runners
Packages/                  # Pinned Unity packages
ProjectSettings/           # Unity project configuration
```

## Architecture rules

- C# owns eligibility, probability, effects, scheduling, transactions, and persistence.
- Yarn owns dialogue copy and choice flow; it does not mutate gameplay state directly.
- UXML owns structure and stable named-element contracts.
- USS owns layout and presentation.
- Candidate saves are validated and committed atomically before becoming active state.
- Persistent actions use stable action and instance IDs for reload-safe, single-award completion.
- RNG seed and step are persisted so outcomes are reproducible.
- Currency uses integer minor units; premium currency uses whole units in a separate wallet.
- Vendor SDKs remain behind Stim-owned interfaces.
- Existing vendor directories must not be reorganized or edited for first-party presentation work.

The frontend/wiring ownership contract is documented in [Frontend and Wiring Workflow](docs/FRONTEND_WIRING_WORKFLOW.md).

## Packages

Notable installed dependencies include:

- Unity Input System;
- Unity Test Framework and Code Coverage;
- Yarn Spinner;
- Unity IAP `5.4.1`;
- Unity LevelPlay `9.5.0`.

Installed commerce packages do not imply configured products, placements, entitlements, or production behavior. Follow the gates in the [master task list](docs/TASKS.md) and the [package checklist](docs/PACKAGE_INSTALL_CHECKLIST.md).

Keep/remove decisions and the measured removal protocol live in the [package policy and usage matrix](docs/PACKAGE_POLICY.md).

## Documentation

There are two authoritative project documents:

1. this README for repository setup and technical orientation;
2. the [master task list](docs/TASKS.md) for product decisions, status, order, and acceptance criteria.

Other files under `docs/` are supporting architecture specifications, audits, and implementation references. If a supporting document conflicts with the master task list, the master task list wins and the supporting document should be corrected.

## Version control

Do not commit `Library/`, `Temp/`, `Logs/`, generated IDE project files, local test artifacts, build outputs, credentials, signing files, or generated save data.
