#!/bin/sh
set -eu

repo_root="$(git rev-parse --show-toplevel)"
runner="$repo_root/tools/run-ast-grep-sdd.ps1"

run_runner() {
  if command -v pwsh >/dev/null 2>&1; then
    pwsh -NoProfile -File "$runner" "$@"
  elif command -v powershell.exe >/dev/null 2>&1; then
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$runner" "$@"
  else
    echo "ast-grep SDD hook requires pwsh or powershell.exe." >&2
    return 127
  fi
}

run_runner -Mode staged
run_runner -Mode staged -Enforce
