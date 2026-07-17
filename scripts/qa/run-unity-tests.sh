#!/bin/zsh

set -euo pipefail

ROOT_DIR="${0:A:h:h:h}"
DEFAULT_UNITY_EDITOR="/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity"
if [[ ! -x "${DEFAULT_UNITY_EDITOR}" && -x "/Volumes/UnityDr/6000.3.19f1/Unity.app/Contents/MacOS/Unity" ]]; then
  DEFAULT_UNITY_EDITOR="/Volumes/UnityDr/6000.3.19f1/Unity.app/Contents/MacOS/Unity"
fi
UNITY_EDITOR="${UNITY_EDITOR:-${DEFAULT_UNITY_EDITOR}}"
ARTIFACTS_DIR="${ROOT_DIR}/Artifacts"
COVERAGE_OPTIONS="generateAdditionalMetrics;generateHtmlReport;generateBadgeReport;assemblyFilters:+StimTycoon.Domain,+StimTycoon.Runtime"

if [[ ! -x "${UNITY_EDITOR}" ]]; then
  print -u2 "Unity Editor was not found at ${UNITY_EDITOR}. Set UNITY_EDITOR to the Unity executable."
  exit 2
fi

mkdir -p "${ARTIFACTS_DIR}"

run_editmode() {
  local category="$1"
  local label="$2"
  local args=(
    -batchmode \
    -nographics \
    -projectPath "${ROOT_DIR}" \
    -runTests \
    -testPlatform EditMode \
    -testResults "${ARTIFACTS_DIR}/${label}.xml" \
    -enableCodeCoverage \
    -coverageResultsPath "${ARTIFACTS_DIR}/${label}-coverage" \
    -coverageOptions "${COVERAGE_OPTIONS}" \
    -logFile "${ARTIFACTS_DIR}/${label}.log"
  )
  if [[ -n "${category}" ]]; then
    args+=(-testCategory "${category}")
  fi
  "${UNITY_EDITOR}" "${args[@]}"
}

run_playmode() {
  "${UNITY_EDITOR}" \
    -batchmode \
    -nographics \
    -projectPath "${ROOT_DIR}" \
    -runTests \
    -testPlatform PlayMode \
    -testCategory PlayModeSmoke \
    -testResults "${ARTIFACTS_DIR}/playmode-smoke.xml" \
    -enableCodeCoverage \
    -coverageResultsPath "${ARTIFACTS_DIR}/playmode-smoke-coverage" \
    -coverageOptions "${COVERAGE_OPTIONS}" \
    -logFile "${ARTIFACTS_DIR}/playmode-smoke.log"
}

case "${1:-quick}" in
  quick)
    run_editmode '!SlowSimulation' editmode-quick
    ;;
  playmode)
    run_playmode
    ;;
  all)
    run_editmode '!SlowSimulation' editmode-quick
    run_playmode
    ;;
  full)
    run_editmode '' editmode-full
    run_playmode
    ;;
  simulation)
    run_editmode SlowSimulation editmode-simulation
    ;;
  *)
    print -u2 "Usage: $0 [quick|playmode|all|full|simulation]"
    exit 2
    ;;
esac
