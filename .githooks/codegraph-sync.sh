#!/bin/sh

hook_name="${1:-git-hook}"

debug() {
  if [ -n "$CODEGRAPH_HOOK_DEBUG" ]; then
    echo "codegraph-hook: $*" >&2
  fi
}

repo_root="$(git rev-parse --show-toplevel 2>/dev/null)"
if [ -z "$repo_root" ]; then
  exit 0
fi

case "$repo_root" in
  //*/[A-Za-z]/*)
    unc_tail="${repo_root#//}"
    unc_tail="${unc_tail#*/}"
    drive="${unc_tail%%/*}"
    rest="${unc_tail#*/}"
    drive_lower="$(printf "%s" "$drive" | tr 'A-Z' 'a-z')"
    repo_root="/$drive_lower/$rest"
    ;;
esac

cd "$repo_root" || exit 0

run_codegraph() {
  if command -v codegraph >/dev/null 2>&1; then
    debug "using codegraph"
    codegraph "$@"
    return $?
  fi

  if command -v codegraph.cmd >/dev/null 2>&1; then
    debug "using codegraph.cmd"
    codegraph.cmd "$@"
    return $?
  fi

  if command -v powershell.exe >/dev/null 2>&1; then
    debug "using powershell.exe fallback"
    powershell.exe -NoProfile -ExecutionPolicy Bypass -Command \
      '& "$env:LOCALAPPDATA\codegraph\current\bin\codegraph.cmd" @args' \
      "$@"
    return $?
  fi

  if command -v powershell >/dev/null 2>&1; then
    debug "using powershell fallback"
    powershell -NoProfile -ExecutionPolicy Bypass -Command \
      '& "$env:LOCALAPPDATA\codegraph\current\bin\codegraph.cmd" @args' \
      "$@"
    return $?
  fi

  return 127
}

run_sync() {
  if [ -n "$CODEGRAPH_HOOK_DEBUG" ]; then
    run_codegraph sync .
  else
    run_codegraph sync . >/dev/null 2>&1
  fi
}

run_index() {
  if [ -n "$CODEGRAPH_HOOK_DEBUG" ]; then
    run_codegraph index --force .
  else
    run_codegraph index --force . >/dev/null 2>&1
  fi
}

wait_for_git_index() {
  lock_file="$(git rev-parse --git-path index.lock 2>/dev/null)"
  if [ -z "$lock_file" ]; then
    return 0
  fi

  tries=0
  while [ -e "$lock_file" ] && [ "$tries" -lt 20 ]; do
    sleep 0.25
    tries=$((tries + 1))
  done

  if [ -e "$lock_file" ]; then
    echo "codegraph: git index is locked; skipping $hook_name sync." >&2
    return 1
  fi

  return 0
}

echo "Updating CodeGraph index ($hook_name)..."

if ! wait_for_git_index; then
  exit 0
fi

if [ "$hook_name" = "pre-commit" ] && git diff --cached --name-only --diff-filter=A -- '*.cs' | grep -q .; then
  run_index
  status=$?
else
  run_sync
  status=$?
fi

if [ "$status" -ne 0 ]; then
  echo "codegraph sync failed; clearing a possible stale lock and retrying." >&2
  run_codegraph unlock . >/dev/null 2>&1 || true
  run_sync
  status=$?
fi

if [ "$status" -eq 127 ]; then
  echo "codegraph: command not found; skipping $hook_name sync." >&2
elif [ "$status" -ne 0 ]; then
  echo "codegraph sync failed; continuing without blocking $hook_name." >&2
fi

exit 0
