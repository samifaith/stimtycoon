# Frontend and Wiring Collaboration Contract

This is the working agreement for parallel UI production. The visual frontend can move quickly without absorbing gameplay complexity, while wiring can remain deterministic, save-safe, testable, and resilient to layout revisions.

## Ownership

> **Active handoff note:** the checked-in UI is intentionally a stripped structural shell. The frontend/UI owner will load and place sprites directly in Unity UI Builder. Wiring and automation work must not add, replace, or restyle production sprites or independently perform a final theme pass unless the frontend owner explicitly requests it. Preserve stable named elements and behavioral seams so UI Builder changes can land without controller churn.

### Frontend/UI owner

Owns:

- visual hierarchy, UXML composition, USS, spacing, typography, color, art, animation, and responsive presentation;
- the last-loaded `Assets/UI/Styles/FrontendCanvas.uss` presentation layer; this neutral canvas is the canonical place to rebuild colors, surfaces, borders, radii, and decorative container art;
- choosing and adapting approved kit components through Stim-owned UXML/USS;
- aspect ratio, nine-slicing, text containment, safe-area composition, and visual states;
- runtime screenshots at the supported device widths and text scales;
- proposing a new visual component or a deliberate change to an existing binding contract.

Does not need to own:

- save schemas, domain eligibility, event selection, money calculations, cooldowns, rewards, or rollback;
- controller callbacks, service calls, persistence, content registration, or automated interaction tests;
- ad, purchase, cloud, account, or analytics SDK behavior.

### Wiring/integration owner

Owns:

- querying named UXML elements and binding view state to them;
- pairing every persistent callback registered during enable/bind with deterministic disable/unbind teardown; element-owned callbacks on generated rows must leave with their discarded subtree;
- turning taps and input into typed commands against domain/application services;
- eligibility, age omission, relevant lock reasons, previews, confirmations, timers, cooldowns, claims, and terminal states;
- atomic persistence, rollback, reload reconciliation, idempotency, duplicate protection, and Life Feed output;
- event/Yarn ID parity, catalog registration, branch reachability, localization-safe IDs, and content validation;
- structural, interaction, domain, persistence, and regression tests;
- accessibility semantics, focus order, exact-value tooltips/labels, and non-color status meaning;
- merging a completed visual slice with its behavior on a clean integration branch.

## The UI binding API

UXML `name` values used by a controller are an API. Classes and surrounding visual containers may change freely; required names must remain unique and keep a compatible UI Toolkit element type unless the visual and wiring changes land together.

Examples:

- `cash-value` remains a `Label` even if the cash pill is completely redesigned.
- `advance-month` remains a `Button` even if its art, position, children, and classes change.
- `relationship-list` remains the collection host even if each row is redesigned.
- `event-sheet`, `choices`, `result-card`, and `event-continue` remain one coherent modal state surface.

Rules:

1. Do not encode gameplay state in USS selectors or static UXML copy.
2. Do not rename a bound element to make a visual variant; add or change classes instead.
3. If a component genuinely needs a different element type or hierarchy, record the proposed contract change before removing the old name.
4. Repeating rows are created by Stim-owned factories/binders; UXML owns their host and reusable templates.
5. The binder supplies dynamic text, accessibility labels, tooltips, visibility, enabled state, classes, progress values, and callbacks.
6. Exact financial values remain available through detail screens and accessibility/tooltips even when a compact header uses `K/M/B` formatting.
7. Age-inappropriate actions do not exist in the visual tree. Relevant but unmet actions render an explicit locked state and actionable reason.

## Neutral presentation baseline

`FrontendCanvas.uss` loads after the structural styles and is scoped by `st-frontend-canvas` on the playable root. It clears the inherited prototype palette, borders, radii, and shape-bearing button/progress art while deliberately retaining layout, spacing, visibility, overflow containment, touch targets, icon/illustration art, aspect-fit rules, and all runtime binding classes.

Frontend work should replace the scoped neutral rules incrementally. Do not remove `st-frontend-canvas`, reorder the five canonical stylesheet entry points, or move gameplay meaning into USS. Locked, disabled, selected, success, and failure states must continue to have a text, icon, or accessibility signal in addition to any future color treatment.

## UI Builder, sprites, stylesheets, and input

- `Assets/UI/StimVerticalSlice.uxml` is the playable UI Builder document. Its five directly attached USS files are the canonical cascade, with `FrontendCanvas.uss` loaded last for frontend ownership.
- The scene `UIDocument` owns its Panel Settings and Source Asset references in the Inspector. Runtime code validates those references but must not replace them, so scene preview, UI Builder, and Play mode all use the same assets.
- Feed Row, Achievement Row, and Action Card are UI Builder-authored UXML components. Runtime factories clone those templates and only bind dynamic content, state classes, progress, and callbacks. Do not recreate their visual hierarchy in controller code.
- Destination cards live under their real destination hosts in UXML; runtime code does not reparent them. A visual slot with an authored sprite child is preserved, while an empty slot receives a fallback placeholder at runtime.
- `com.unity.2d.sprite` is a direct dependency. Import raster UI art as `Sprite (2D and UI)`, retain transparency, disable mipmaps unless the asset needs scaled/world rendering, and use `scale-to-fit` for unsliced art or complete nine-slice metadata for stretchable controls.
- The playable scene has one explicit `EventSystem` with `InputSystemUIInputModule`, matching the project’s New Input System setting. Do not add `StandaloneInputModule` or a second EventSystem. UI Toolkit has a built-in runtime event path, but this explicit module is retained for inspectable/remappable input and future mixed UI requirements.
- USS owns visual presentation. Runtime C# may set data-driven geometry such as progress width and safe-area padding, but visual states use semantic classes such as `is-error`, `locked`, `selected`, and `claimable` rather than inline colors, sprites, borders, fonts, or radii.

## Handoff format for each visual slice

The UI handoff should identify:

- destination/component name;
- edited UXML, USS, icons, and art paths;
- required dynamic values;
- interactive elements and expected gestures;
- visual examples of ready, pressed, disabled, locked, selected, empty, error, loading, active, cooldown, claimable, and completed states that apply;
- screenshots at 320, 390, 430, and 768 points when layout is ready;
- any proposed binding-name or element-type change.

The wiring return should include:

- the bound values and callbacks;
- the service/domain command used by every action;
- age/requirement/resource/lifecycle branches implemented;
- save, rollback, reload, and duplicate behavior;
- structural and interaction tests added;
- remaining visual-only or device-only verification.

## Branch and merge discipline

- UI work uses a short-lived `ui-<destination>-<slice>` branch.
- Wiring work uses a short-lived `wiring-<destination>-<slice>` branch.
- Integration happens on a fresh branch based on the current `main`; do not develop new work directly on dirty `main`.
- Keep a slice small enough to review as one component or one coherent flow.
- Avoid editing the same UXML/USS region and controller method in parallel. Land the visual contract first when possible, then wire it.
- Commit generated Unity `.meta` files with their assets. Do not commit `Library`, `Temp`, `Logs`, `Obj`, or user-local settings.
- Merge only when compile/import is clean, applicable tests pass, documentation matches reality, and `git diff --check` is clean.

## Wiring architecture direction

`StimVerticalSliceController` currently binds the complete vertical slice and is too large for sustained parallel UI work. Extraction should be incremental, not a rewrite:

1. Keep `StimGameSessionService` and dedicated application services as the gameplay authority.
2. Keep the root controller responsible for composition, modal arbitration, pending events/transitions, and destination navigation.
3. Extract one binder at a time: Shell, Life, Study, Work, Bank, Social, Goals, then modal sheets.
4. Each binder receives an already-validated view root and services; it never owns persistence or canonical game rules.
5. Introduce small immutable view-state builders where several labels/classes represent one domain state.
6. Route all mutations through typed service calls; never mutate the active save from a binder.
7. Retain one canonical launch event catalog. The controller must not duplicate an event registration list.

Current extraction progress: `StimShellBinder` owns shared shell behavior; `StimLifeBinder` owns deterministic Life Feed grouping, empty state, template-row binding, and visible-count presentation; `StimHomeBinder` owns Home condition/progress, action/upgrade rows, retry control, and transaction feedback; `StimStudyBinder` owns the study catalog plus confirmation-sheet presentation; `StimWorkBinder` owns the career-path preview and manual-work presentation; `StimBankBinder` owns account, transfer, cash-flow, credit, investing, history, and tab presentation; `StimSocialBinder` owns relationship list, discovery, and detail presentation; `StimGoalsBinder` owns goal/achievement row presentation and empty state; `StimNewLifeBinder` owns New Life modal controls, optional-action presentation, and recoverable error display; `StimEventSheetBinder` owns event/result copy, semantic visibility, the choice host, and Continue presentation; and `StimFinalLifeBinder` owns final-life copy plus its New Life action control. The controller retains Home mutations and eligibility authority, ending decisions, event resolution, choice construction/callbacks, time advancement, recovery, workflow persistence, save creation, claim authority, pending-event arbitration, and destination/modal routing.

Proposed boundary:

```text
UXML / USS / art
        ↓ stable names and component hosts
destination binder / view-state builder
        ↓ typed command and result
application or session service
        ↓ candidate save transaction
validation → commit/rollback → Life Feed
```

## Immediate wiring backlog

### W0 — Stabilize the collaboration seam

- [x] Treat bound UXML names as a protected API with structural tests.
- [x] Register the playable event catalog from `CreateLaunchAlphaCatalog()` rather than a second controller-owned list.
- [x] Keep exact header money values in accessible tooltips while compacting large visible values.
- [x] Create an explicit binding manifest grouped by Shell/Life/Study/Work/Bank/Social/Goals/modal ownership.
- [ ] Replace source-text regex checks with behavior tests where practical; retain structural tests for UXML/USS ownership.
- [x] Extract shared header, navigation, safe-area geometry, and time-control callback ownership into the Shell binder.
- [x] Complete the Shell binder by moving global modal arbitration and shell view-state rendering out of the vertical-slice controller; shell navigation and time actions are rejected while a blocking modal is active.

### W1 — Shared UI state machine

- [x] Implement common `available`, `locked`, `disabled`, `selected`, `active`, `cooldown`, `claimable`, `claimed`, `empty`, `loading`, `error`, `offline`, and `terminal` presentation helpers; action cards, achievement rows, path rows, and feedback use the shared vocabulary.
- [x] Centralize pending-event/transition/modal arbitration so shell and destination actions cannot mutate behind a blocking decision; only callbacks owned by the active modal may run until it is resolved.
- [x] Persist or safely reconstruct multi-step controller workflows: queued Advance Year remaining months/completion state and pending Study confirmation now live in additive save state, autosave transactionally, migrate safely, and reconstruct after reload.
- [x] Add uniform confirmation, save-failure/rollback, retry, and return-context behavior. Bank, Home, Social discovery, and manual work expose duplicate-safe retries; irreversible terminal failures suppress retry commands; modal closes restore exact workspace context; and Advance Year/Study workflow persistence failures stop progression, retain or roll back local state, and expose safe retry behavior.
- [x] Add reload-safe navigation/deep-link state for `Go` actions and ordinary navigation, including active destination, selected Bank tab, selected Social entity, and active-destination scroll restoration with stale-value validation.

### W2 — Authored content wiring

Five staged Yarn files currently contain 100 new dialogue nodes: Childhood, School, Career, Health, and Money. Yarn Spinner imports them through `StimTycoon.yarnproject`, but a node is not playable until a matching validated `StimEvent` and every referenced choice ID exist in the canonical C# catalog.

Current staged progress: all 100 Childhood, School, Career, Health, and Money nodes have compact data-driven definitions with explicit age ranges, requirements, effects, cooldowns, telemetry, production validation, and exact Yarn choice parity. They intentionally remain outside the launch catalog until pacing, distribution, and editorial review.

- [x] Add data-driven `StimEvent` definitions for the 100 staged nodes without creating a 100-method controller/catalog file.
- [x] Assign explicit age ranges, timing, requirements, cooldown/repeat policy, locations, outcome weights, meaningful effects, feed text, and telemetry to every event.
- [x] Require `minor_cash_agency` only where a child truly has appropriate financial agency; all staged cash-charging outcomes are currently age 18+ and automated coverage protects that boundary.
- [x] Add an automated Yarn-to-catalog contract test for every `stim_resolve_choice` event and choice ID.
- [ ] Run production validation, age-boundary checks, follow-up reachability, anti-repetition coverage, seeded distribution tests, and human editorial review before adding the batches to random selection.
- [ ] Register batches gradually by life stage/category so event pacing and balance can be measured rather than enabling all 100 at once.

### W3 — Destination wiring sequence

1. Shell and shared modal/state components.
2. Life compact feed, Next Up, goal preview, and time-control branches.
3. Home/inventory and Social/family durable state.
4. Work career/business workspace.
5. Goals board and achievement claim states.
6. Study Match, then Shift Match on the same reusable timed mini-game lifecycle.
7. Settings/accessibility and production-service adapters.

Detailed component and branch-state IDs remain in `REFERENCE_UI_GAP_ANALYSIS.md`; milestone ordering remains in `TASKS.md`.

## Definition of ready for visual work

A wiring slice is ready for frontend treatment when its required data, actions, state branches, and stable names are listed even if presentation is plain. A visual slice is ready for wiring when the component hierarchy is stable enough that dynamic labels, hosts, buttons, and progress elements can be named without another structural rewrite.

## Definition of integrated

A slice is integrated only when:

- the visual component matches the approved hierarchy at supported widths;
- every applicable branch has visible behavior and a testable state;
- actions preview consequences and commit through the correct service;
- reload, duplicate input, save failure, and return navigation are safe;
- age-inappropriate options are absent and relevant locks are explicit;
- accessibility exposes the same meaning as the visuals;
- documentation and task status describe what is actually playable.
