# QA Baseline

This file is the canonical record of retained automated-test evidence. Other project documents link here instead of copying counts.

## Historical baseline

The repository's latest retained automation record, dated July 19, 2026, reports:

| Suite | Discovered/passed | Result |
|---|---:|---|
| Quick EditMode | 1,354 | Passed |
| Full EditMode | 1,355 | Passed |
| PlayMode smoke | 5 | Passed |
| Visual capture | 1 case / 48 images | Capture passed; human review must be recorded separately |

These retained counts are historical comparison data, not proof for the current checkout. NUnit XML and coverage artifacts under the ignored `Artifacts/` directory are authoritative for an individual run.

## Published local verification

- Commit: `ec19287224cf0759d2340a535a1cd89e87080234`
- Date: July 19, 2026
- Runner: local macOS
- Unity: `6000.3.19f1`

| Evidence | Status |
|---|---|
| Quick EditMode | 1,363 discovered; 1,363 passed; 0 failed; 0 skipped; 20.88 s |
| Full EditMode | 1,364 discovered; 1,364 passed; 0 failed; 0 skipped; 26.53 s |
| PlayMode smoke | 5 discovered; 5 passed; 0 failed; 0 skipped; 5.31 s |
| Full-suite coverage | 76.1% lines (6,141/8,062); 85.4% methods (790/924) |
| Visual capture | 1/1 passed in 8.46 s; all 48 images produced; contact-sheet smoke review found no obvious gross clipping or overlap; detailed checklist pending |
| Serialized missing-reference gate | Passed as part of EditMode; manual missing-script scan pending |
| iOS development build | Pending |

Artifacts: `Artifacts/editmode-quick.xml`, `Artifacts/editmode-full.xml`, `Artifacts/playmode-smoke.xml`, `Artifacts/m13-visual.xml`, coverage reports under `Artifacts/*-coverage/Report/`, and the visual checklist at `Artifacts/M13Visual/REVIEW.md`.

## CI verification

Exact commit `ec19287224cf0759d2340a535a1cd89e87080234` passed the GitHub Actions **Unity QA** workflow on July 19, 2026:

| Gate | Result |
|---|---|
| EditMode quality gate | Passed; evidence artifact uploaded |
| PlayMode smoke gate | Passed; evidence artifact uploaded |
| Nightly full-life simulation | Not scheduled for this push; skipped by workflow policy |
| `main` branch protection | Not configured; successful jobs are not yet enforced as required checks |

Workflow: `https://github.com/samifaith/stimtycoon/actions/runs/29707852897`

## Current worktree verification

The post-`ec19287` reward-bound, package-policy, terminology, and documentation changes were verified locally on July 20, 2026. These results become a published baseline only after the work receives a commit SHA and passes CI.

| Suite | Result |
|---|---|
| Quick EditMode | 1,365 discovered; 1,365 passed; 0 failed; 0 skipped; 30.02 s |
| Full EditMode | 1,366 discovered; 1,366 passed; 0 failed; 0 skipped; 28.56 s |
| PlayMode smoke | 5 discovered; 5 passed; 0 failed; 0 skipped; 5.25 s |
| Visual capture | 1/1 passed in 8.52 s; 48 images produced; detailed review pending |

## Device and release-candidate approval

| Evidence | Status |
|---|---|
| Detailed 48-image reviewer/date checklist | Pending |
| iOS development build | Pending |
| Supported physical-iPhone matrix | Pending |
| Birth-to-ending device run | Pending |
| Release-candidate approval | Pending |

## Licensing incident

The initial runner attempts were blocked by two competing Unity Licensing Client processes. Restarting Unity Hub and its licensing client produced one `Unity-LicenseClient-samifaith` channel; the entitlement audit then granted `com.unity.editor.headless`, and all four local commands completed.

Run the suites with `scripts/qa/run-unity-tests.sh quick`, `playmode`, `full`, and `visual`. Record the exact commit SHA, Unity version, counts, artifact paths, reviewer, and review date when promoting a result into this baseline.
