# Shared helper, sourced by guard-bash.sh / guard-edit.sh — not a hook itself (leading
# underscore matches the _plan-common.md convention for shared, non-standalone files).
#
# Extracts a string field from a Claude Code hook's JSON payload, checking both the top level
# and a nested "tool_input" object. Prefers python3 for real JSON parsing: a naive
# grep/sed regex (the previous approach) stops at the first double-quote it sees, so it silently
# truncates the match at any quote that's part of a legitimately quoted argument earlier in the
# value (e.g. `git commit -m "wip" && git push --force` truncates to `git commit -m ` before a
# guard ever sees `--force`) — that's a guard-defeating bug, not a style nit. Falls back to the
# old regex only if python3 isn't on PATH, so the hook still does *something* rather than
# hard-failing open.
json_field() {
  # $1 = raw JSON payload, $2 = field name
  local json="$1" key="$2"
  if command -v python3 >/dev/null 2>&1; then
    printf '%s' "$json" | python3 -c '
import json, sys
key = sys.argv[1]
try:
    data = json.load(sys.stdin)
except Exception:
    sys.exit(0)
value = data.get(key)
if value is None and isinstance(data.get("tool_input"), dict):
    value = data["tool_input"].get(key)
sys.stdout.write(value if isinstance(value, str) else "")
' "$key"
  else
    printf '%s' "$json" | grep -o "\"$key\"[[:space:]]*:[[:space:]]*\"[^\"]*\"" | head -n1 | sed -E "s/.*\"$key\"[[:space:]]*:[[:space:]]*\"//; s/\"\$//"
  fi
}
