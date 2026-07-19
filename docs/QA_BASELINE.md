# QA Baseline

This file is the canonical record of retained automated-test evidence. Other project documents link here instead of copying counts.

## Current checkout

- Commit: `c3c88cf526379b0594d201da617f2b9cf92f160a`
- Unity: `6000.3.19f1`
- Verification target: local audit-remediation worktree based on this commit
- Date: July 19, 2026
- Runner: local macOS, Unity `6000.3.19f1`

The working tree contains audit-remediation changes after the commit above; publish a successor SHA before recording release evidence.

The automated suites below verify the local worktree, not the unmodified `c3c88cf` commit. Publish a successor SHA and reproduce the gates in CI before treating the result as release evidence.

## Latest retained local evidence

The repository's latest retained automation record, dated July 19, 2026, reports:

| Suite | Discovered/passed | Result |
|---|---:|---|
| Quick EditMode | 1,354 | Passed |
| Full EditMode | 1,355 | Passed |
| PlayMode smoke | 5 | Passed |
| Visual capture | 1 case / 48 images | Capture passed; human review must be recorded separately |

These retained counts are historical comparison data, not proof for the current checkout. NUnit XML and coverage artifacts under the ignored `Artifacts/` directory are authoritative for an individual run.

## Release evidence for current checkout

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

## Licensing incident

The initial runner attempts were blocked by two competing Unity Licensing Client processes. Restarting Unity Hub and its licensing client produced one `Unity-LicenseClient-samifaith` channel; the entitlement audit then granted `com.unity.editor.headless`, and all four local commands completed.

Run the suites with `scripts/qa/run-unity-tests.sh quick`, `playmode`, `full`, and `visual`. Record the exact commit SHA, Unity version, counts, artifact paths, reviewer, and review date when promoting a result into this baseline.
