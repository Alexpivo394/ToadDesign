#!/bin/sh
# Runs the checks: plain and ILRepack-merged builds, both target frameworks. Exit code: number of failed runs.
# Build first: dotnet build tests/ToadDesign.Tests
cd "$(dirname "$0")/ToadDesign.Tests/bin/${CONFIGURATION:-Debug}" || exit 1
failed=0
for v in . repacked repacked-release; do
  echo "== net47 $v"
  ./net47/$v/ToadDesign.Tests.exe "$@" > run.log 2>&1 || failed=$((failed + 1))
  grep -E "FAIL|CRASH|passed|live in" run.log
  echo "== net8 $v"
  dotnet ./net8.0-windows/$v/ToadDesign.Tests.dll "$@" > run.log 2>&1 || failed=$((failed + 1))
  grep -E "FAIL|CRASH|passed|live in" run.log
done
rm -f run.log
exit $failed
