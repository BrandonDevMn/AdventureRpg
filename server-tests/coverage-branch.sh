#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UNIT_TESTS="$SCRIPT_DIR/UnitTests"
REPORT_DIR="$SCRIPT_DIR/.coverage-report"

dotnet test "$UNIT_TESTS" \
  --settings "$UNIT_TESTS/coverage.runsettings" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$SCRIPT_DIR/.test-results" \
  --nologo -q 2>/dev/null

COVERAGE_FILE=$(find "$SCRIPT_DIR/.test-results" -name "coverage.cobertura.xml" | xargs ls -t | head -1)

"$HOME/.dotnet/tools/reportgenerator" \
  -reports:"$COVERAGE_FILE" \
  -targetdir:"$REPORT_DIR" \
  -reporttypes:TextSummary 2>/dev/null

grep "Branch coverage" "$REPORT_DIR/Summary.txt" | grep -oP '[\d.]+(?=%)'
