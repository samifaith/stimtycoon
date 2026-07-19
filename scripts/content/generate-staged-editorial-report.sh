#!/bin/zsh

set -euo pipefail

PROJECT_ROOT="${0:A:h:h:h}"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
if [[ ! -x "${UNITY_EDITOR}" && -x "/Volumes/UnityDr/6000.3.19f1/Unity.app/Contents/MacOS/Unity" ]]; then
  UNITY_EDITOR="/Volumes/UnityDr/6000.3.19f1/Unity.app/Contents/MacOS/Unity"
fi
if [[ ! -x "${UNITY_EDITOR}" ]]; then
  print -u2 "Unity Editor was not found at ${UNITY_EDITOR}. Set UNITY_EDITOR to the Unity executable."
  exit 2
fi

mkdir -p "${PROJECT_ROOT}/Artifacts/Content"
"${UNITY_EDITOR}" -batchmode -nographics -quit \
  -projectPath "${PROJECT_ROOT}" \
  -executeMethod StimTycoon.Editor.StagedEditorialReportGenerator.GenerateForBatchMode \
  -logFile "${PROJECT_ROOT}/Artifacts/Content/staged-editorial-review.log"
print "${PROJECT_ROOT}/Artifacts/Content/staged-editorial-review.md"
