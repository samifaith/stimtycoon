# QA Strategy

## Optional local Odin Validator gate

Odin Validator complements the repository-owned Unity test suites on developer machines with a valid Odin license by checking serialized Unity assets, scenes, prefabs, and references before they reach runtime. Odin is not committed to this public repository and is not required to compile, run, or execute CI. The repository-owned Unity suites remain the authoritative, reproducible gates.

When Odin is installed locally, configure its main profile to include first-party assets under `Assets` and exclude imported UI kits, plugins, generated dependency assets, and vendor integrations so third-party findings do not obscure product defects.

The recommended local automation policy is:

- before Play Mode: complete the profile, log warnings, and open Validator while stopping entry on errors;
- before builds: complete the profile, log warnings, and open Validator while stopping the build on errors;
- at project startup: report errors without interrupting normal editor startup;
- show the scene validation widget only when warnings or errors exist.

Use **Tools > Odin Validator** to inspect or run the profile manually. Odin does not replace EditMode or PlayMode tests: Yarn authoring rules, age eligibility, domain behavior, callback lifecycle, UXML named-element contracts, and deterministic simulations remain enforced by repository-owned tests. UI layout and styling remain owned by UI Builder, UXML, and USS.

This document is the executable quality contract for Stim Tycoon. It complements the product milestones in `TASKS.md`: milestone acceptance criteria define what must work, while this document defines where and how that behavior is verified.

## Current baseline

The counts below are the recorded July 18, 2026 baseline, backed by retained local NUnit and coverage artifacts. They remain the comparison target rather than a claim that every future checkout is continuously green; remote activation/branch-protection proof remains an open release task.

- Unity `6000.3.19f1`
- Unity Test Framework `1.6.0`
- Unity Code Coverage `1.3.0`
- 729-case verified EditMode baseline from July 18, 2026
- 3-case verified production-scene PlayMode smoke baseline from July 17, 2026
- Production-scene PlayMode smoke assembly for scene boot, UI contract, input system, and callback lifecycle
- Yarn authoring-contract coverage for unique nodes/events and exact choice-command parity
- GitHub Actions configuration for PR/main gates and nightly deterministic simulation

The recorded full headless baseline is 729/729 EditMode in 13.2 seconds and 5/5 PlayMode smoke in 5.5 seconds on the July 18, 2026 development machine. The fast pull-request EditMode selection excludes the single `SlowSimulation` case; retain its own result independently when run.

## Test tiers

| Tier | Purpose | Blocking cadence |
|---|---|---|
| EditMode fast | Domain rules, save safety, content validation, UI contracts | Every pull request |
| PlayMode smoke | Production scene, UIDocument, EventSystem, navigation/overlay contract, lifecycle | Every pull request |
| Slow simulation | Seeded complete-life, balance, bounded histories, repeatability | Nightly and release candidate |
| Visual/device | Widths, text scales, safe areas, touch, accessibility, performance | Milestone and release candidate |
| Human exploratory | Comprehension, aesthetics, editorial safety, unexpected sequences | Milestone and release candidate |

`SlowSimulation`, `PlayModeSmoke`, and `ContentContract` are stable NUnit category names. New expensive suites must receive a category rather than silently slowing the default local loop.

## Local commands

The repository runner writes XML, logs, and coverage reports under ignored `Artifacts/` paths.

```sh
scripts/qa/run-unity-tests.sh quick
scripts/qa/run-unity-tests.sh playmode
scripts/qa/run-unity-tests.sh all
scripts/qa/run-unity-tests.sh full
scripts/qa/run-unity-tests.sh simulation
```

Set `UNITY_EDITOR` when Unity is installed somewhere other than the default `6000.3.19f1` macOS Hub location.

Close the live Unity Editor before using the headless runner; Unity permits only one process per project. The runner intentionally does not pass Unity's general-purpose `-quit` flag: the Test Framework owns shutdown after asynchronous test execution and must be allowed to write its XML and coverage artifacts first.

## Continuous integration activation

`.github/workflows/qa.yml` uses GameCI's Unity Test Runner and publishes NUnit, log, and coverage artifacts. Before making the checks required on `main`:

1. The personal-license secret names `UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD` are present. Their values are not considered configured until GameCI activates Unity successfully. Keep them in GitHub Actions secrets; never write them to source, logs, artifacts, or documentation.
2. Confirm the **Unity QA** workflow passes on the pull request and once on `main`; use `workflow_dispatch` for an explicit licensing or runner proof when needed. An HTTP 400 from Unity login, `Access token is unavailable`, or `Failed to activate ULF license` is a credential/license blocker, not a test failure.
3. Confirm the scheduled `SlowSimulation` job completes within the available runner budget.
4. Require the workflow job checks `EditMode quality gate` and `PlayMode smoke gate` in the `main` branch protection rules. The action's nested result labels are `EditMode QA` and `PlayMode Smoke QA`, but branch protection must use the PR-visible job names.
5. Do not make the nightly simulation a pull-request blocker unless its runtime becomes reliably short.

Account secrets and branch-protection changes are external repository administration; they are not stored in source control.

## Required test seams

Gameplay code must keep nondeterministic and external behavior behind replaceable boundaries:

- UTC clock and elapsed-time reconciliation;
- seeded random state;
- save repository and failure injection;
- network availability;
- ads and purchases;
- application pause/resume;
- device dimensions, safe areas, and accessibility scale.

A defect involving one of these systems is not considered reproducible until the test records the seed, clock, save fixture, and injected failure state that produced it.

## UI quality contract

UI automation is layered rather than pixel-only:

1. EditMode structure tests protect stable UXML names, stylesheet ownership, template use, aspect treatment, and required states.
2. PlayMode tests protect scene composition, binding lifecycle, input, navigation, overlays, and restored state.
3. Screenshot review covers 320, 390, 430, and 768-point widths at 100% and 130% text scale.
4. Physical-device review covers safe area, touch, suspend/resume, memory, thermal behavior, VoiceOver, and final visual approval.

The production-scene PlayMode smoke suite now exercises that eight-case width/text matrix as an early layout gate. It verifies responsive state selection, horizontal containment, and 44-point targets across persistent navigation, both time controls, Study confirmation, event continuation, and new-life actions. Passing it does not close screenshot review: text clipping, destination-specific overlap, visual hierarchy, and safe-area composition still require retained images and human approval.

Pixel comparisons begin as review artifacts. They become blocking only after render variance is measured and a stable tolerance is established.

## Content quality contract

Every Yarn option must carry a stable `#choice:` tag and resolve exactly once to the same choice ID. Each Yarn node resolves one localization-safe event ID, and node/event ownership is unique.

Before staged Yarn content enters random selection, a matching validated C# event must exist and age-boundary coverage must prove:

- exact minimum age accepted;
- exact maximum age accepted;
- adjacent ages rejected;
- financial responsibility and NPC roles are appropriate for the life stage;
- every adverse branch has an age-appropriate recovery route;
- follow-up, cancellation, cooldown, and reload behavior are reachable and duplicate-safe.

## Defect evidence

Every critical or high defect must include:

- build/commit and Unity version;
- platform, device, resolution, safe area, and text scale;
- starting save or minimal setup;
- random seed and simulated clock when relevant;
- exact steps;
- expected and actual behavior;
- screenshot/video and relevant log excerpt;
- frequency and regression status;
- automated test added, or a written reason automation is not yet practical.

## Definition of done

A gameplay change is complete only when its success, rejection, interruption, save failure, reload, and duplicate-action paths are covered at the lowest reliable test layer. A UI change is complete only when its applicable empty, hidden-by-age, locked, available, active, claimable, error, and restored states are verified without overflow or inaccessible controls.

AltTester or another black-box device driver remains a trial-stage addition. Adopt it only after a proof of concept can reliably run New Life, Advance Year with interruption/resume, and one save/reload event journey using the existing stable UXML names.
