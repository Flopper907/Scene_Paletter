#!/usr/bin/env pwsh
<#
.SYNOPSIS
Bootstraps this harness (folders, templates, skills, agents, hooks, scripts) into an existing
project. Windows/PowerShell equivalent of init-harness.sh — same behavior: never overwrites
anything already present in the target (reports what it skipped instead), safe to re-run.

.PARAMETER Target
Path to the existing project to install the harness into.

.EXAMPLE
.github/scripts/init-harness.ps1 -Target C:\path\to\existing\project
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$Target
)
$ErrorActionPreference = 'Stop'

$HarnessRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

if (-not (Test-Path -LiteralPath $Target -PathType Container)) {
    Write-Error "Not a directory: $Target"
    exit 1
}
$Target = (Resolve-Path -LiteralPath $Target).Path

$script:created = @()
$script:skipped = @()

function Copy-New {
    param([string]$SourceRel, [string]$DestRel)
    $src = Join-Path $HarnessRoot $SourceRel
    $dst = Join-Path $Target $DestRel
    if (Test-Path -LiteralPath $dst) {
        $script:skipped += "$DestRel (already exists)"
        return
    }
    $dstDir = Split-Path $dst -Parent
    if ($dstDir -and -not (Test-Path -LiteralPath $dstDir)) {
        New-Item -ItemType Directory -Path $dstDir -Force | Out-Null
    }
    Copy-Item -LiteralPath $src -Destination $dst
    $script:created += $DestRel
}

function New-DirReport {
    param([string]$Rel)
    $dst = Join-Path $Target $Rel
    if (Test-Path -LiteralPath $dst -PathType Container) {
        $script:skipped += "$Rel/ (already exists)"
    } else {
        New-Item -ItemType Directory -Path $dst -Force | Out-Null
        $script:created += "$Rel/"
    }
}

Write-Output "Initializing harness in: $Target"
Write-Output ""

# --- folders ---
New-DirReport ".agent-plans"
New-DirReport ".agent-memory/architecture"
New-DirReport ".agent-memory/decisions"
New-DirReport ".agent-memory/code-map"

# --- memory templates ---
# Explicit, hardcoded calls -- deliberately NOT a "copy everything found under .agent-memory/"
# loop. That was tried and reverted: .agent-memory/ in this harness's own repo is not a pure
# template source, it's a real, working directory that accumulates this project's own real
# content over time (decisions, gotchas, architecture notes) -- exactly like .agent-plans/ did
# (a real plan drafted for this repo leaked into every new project through the generalized loop
# before it was caught). .agent-memory/ currently holds nothing but templates and empty
# placeholders, which is *why* the same bug hasn't visibly triggered here yet -- not because the
# generalized-loop approach was actually safe. gotchas.md and the architecture/ placeholder are
# created fresh (never copied) specifically so the target always starts genuinely empty,
# regardless of what this repo's own copies may have accumulated by the time this runs.
Copy-New ".agent-memory/decisions/_template.md" ".agent-memory/decisions/_template.md"
$gotchas = Join-Path $Target ".agent-memory/gotchas.md"
if (-not (Test-Path -LiteralPath $gotchas)) {
    New-Item -ItemType File -Path $gotchas -Force | Out-Null
    $script:created += ".agent-memory/gotchas.md"
}
$archKeep = Join-Path $Target ".agent-memory/architecture/.gitkeep"
if (-not (Test-Path -LiteralPath $archKeep)) {
    New-Item -ItemType File -Path $archKeep -Force | Out-Null
    $script:created += ".agent-memory/architecture/.gitkeep"
}

# --- plan templates (canonical .github + native .claude duplicate) ---
Get-ChildItem -Path (Join-Path $HarnessRoot ".github/templates") -Filter '*.md' -File | ForEach-Object {
    Copy-New ".github/templates/$($_.Name)" ".github/templates/$($_.Name)"
}
Get-ChildItem -Path (Join-Path $HarnessRoot ".claude/templates") -Filter '*.md' -File | ForEach-Object {
    Copy-New ".claude/templates/$($_.Name)" ".claude/templates/$($_.Name)"
}

# --- usage doc, copied as HARNESS.md so it never clobbers/conflicts with the target's own README
Copy-New "README.md" "HARNESS.md"

# --- workspace rules (Copilot CLI canonical + Claude Code import) ---
Copy-New "CLAUDE.md" "CLAUDE.md"
Copy-New ".github/copilot-instructions.md" ".github/copilot-instructions.md"

# --- skills (canonical .github + native .claude mirror) ---
Get-ChildItem -Path (Join-Path $HarnessRoot ".github/skills") -Filter '*.md' -File | ForEach-Object {
    Copy-New ".github/skills/$($_.Name)" ".github/skills/$($_.Name)"
}
Get-ChildItem -Path (Join-Path $HarnessRoot ".claude/skills") -Directory | ForEach-Object {
    Copy-New ".claude/skills/$($_.Name)/SKILL.md" ".claude/skills/$($_.Name)/SKILL.md"
}
# flat duplicates of shared non-standalone docs (e.g. _plan-common.md) -- not nested in a
# SKILL.md dir since they're not independently invokable skills
Get-ChildItem -Path (Join-Path $HarnessRoot ".claude/skills") -Filter '*.md' -File | ForEach-Object {
    Copy-New ".claude/skills/$($_.Name)" ".claude/skills/$($_.Name)"
}
# reusable stack/language packs: .github/skills/lang/<name>.md -- the Copilot CLI-side source for
# these (the .claude/skills/<name>/SKILL.md mirror is already covered by the loop above, since it
# lives flat under .claude/skills/*/ like any other skill)
$langDir = Join-Path $HarnessRoot ".github/skills/lang"
if (Test-Path $langDir) {
    Get-ChildItem -Path $langDir -Filter '*.md' -File | ForEach-Object {
        Copy-New ".github/skills/lang/$($_.Name)" ".github/skills/lang/$($_.Name)"
    }
}

# --- example docs (reference material, not skills -- flat duplicates like _plan-common.md) ------
Get-ChildItem -Path (Join-Path $HarnessRoot ".github/examples") -Filter '*.md' -File | ForEach-Object {
    Copy-New ".github/examples/$($_.Name)" ".github/examples/$($_.Name)"
}
Get-ChildItem -Path (Join-Path $HarnessRoot ".claude/examples") -Filter '*.md' -File | ForEach-Object {
    Copy-New ".claude/examples/$($_.Name)" ".claude/examples/$($_.Name)"
}

# --- agents ---
Get-ChildItem -Path (Join-Path $HarnessRoot ".github/agents") -Filter '*.agent.md' -File | ForEach-Object {
    Copy-New ".github/agents/$($_.Name)" ".github/agents/$($_.Name)"
}
Get-ChildItem -Path (Join-Path $HarnessRoot ".claude/agents") -Filter '*.md' -File | ForEach-Object {
    Copy-New ".claude/agents/$($_.Name)" ".claude/agents/$($_.Name)"
}

# --- Claude Code enforcement hooks ---
Copy-New ".claude/settings.json" ".claude/settings.json"
Get-ChildItem -Path (Join-Path $HarnessRoot ".claude/hooks") -File | ForEach-Object {
    Copy-New ".claude/hooks/$($_.Name)" ".claude/hooks/$($_.Name)"
}

# --- scripts: both canonical (.github/scripts) and duplicate (.claude/scripts), bash + PowerShell
Get-ChildItem -Path (Join-Path $HarnessRoot ".github/scripts") -File | ForEach-Object {
    Copy-New ".github/scripts/$($_.Name)" ".github/scripts/$($_.Name)"
}
Get-ChildItem -Path (Join-Path $HarnessRoot ".claude/scripts") -File | ForEach-Object {
    Copy-New ".claude/scripts/$($_.Name)" ".claude/scripts/$($_.Name)"
}

# --- .gitignore ---
# Current harness default: .agent-plans/ and .agent-memory/ (except code-map/) are git-tracked —
# persisting plans/decisions is the whole point. Only the regeneratable code-map and the
# per-project hook config are excluded. If you actually want plans/memory kept out of git for this
# project, edit .gitignore yourself after running this — this script won't silently make that call.
$gitignore = Join-Path $Target ".gitignore"
if (-not (Test-Path -LiteralPath $gitignore)) { New-Item -ItemType File -Path $gitignore -Force | Out-Null }

function Add-GitignoreLine {
    param([string]$Line)
    $existing = @(Get-Content -LiteralPath $gitignore -ErrorAction SilentlyContinue)
    if ($existing -notcontains $Line) {
        Add-Content -LiteralPath $gitignore -Value $Line
        $script:created += ".gitignore: $Line"
    }
}
Add-GitignoreLine ".agent-memory/code-map/"
Add-GitignoreLine ".claude/hooks/project-commands.sh"

# --- .gitattributes ---
# Narrow, appended (never a whole-file copy that could skip entirely if the target already has
# its own .gitattributes) -- pins just the harness's own mirrored file types to LF so
# check-mirrors.ps1's byte-identical comparison can't report false drift from a Windows checkout
# with CRLF on one side. Doesn't impose a line-ending policy on the rest of the project.
$gitattributes = Join-Path $Target ".gitattributes"
if (-not (Test-Path -LiteralPath $gitattributes)) { New-Item -ItemType File -Path $gitattributes -Force | Out-Null }

function Add-GitattributesLine {
    param([string]$Line)
    $existing = @(Get-Content -LiteralPath $gitattributes -ErrorAction SilentlyContinue)
    if ($existing -notcontains $Line) {
        Add-Content -LiteralPath $gitattributes -Value $Line
        $script:created += ".gitattributes: $Line"
    }
}
Add-GitattributesLine "*.sh text eol=lf"
Add-GitattributesLine "*.ps1 text eol=lf"

Write-Output "Created:"
if ($script:created.Count -eq 0) { Write-Output "  (nothing -- everything already existed)" }
foreach ($c in $script:created) { Write-Output "  + $c" }

Write-Output ""
Write-Output "Skipped (already existed, left untouched):"
if ($script:skipped.Count -eq 0) { Write-Output "  (nothing skipped)" }
foreach ($s in $script:skipped) { Write-Output "  - $s" }

Write-Output ""
Write-Output "Next steps -- see $Target\HARNESS.md for the full guide:"
Write-Output "  1. Fill in 'Project facts' (tech stack / code style / Definition of Done) in"
Write-Output "     $Target\.github\copilot-instructions.md."
Write-Output "  2. Copy .claude\hooks\project-commands.sh.example to project-commands.sh and set"
Write-Output "     LINT_CMD/TEST_CMD so post-edit-verify.sh actually runs this project's checks."
Write-Output "     NOTE: the hooks themselves (guard-bash.sh etc.) are bash scripts, invoked by"
Write-Output "     Claude Code's hook mechanism -- on native Windows they need WSL or Git Bash on"
Write-Output "     PATH to actually run; there is no native PowerShell hook mechanism yet."
Write-Output "  3. Code-map tooling is optional, not required to draft your first plan. When you do"
Write-Output "     set it up, use the code-map skill (.github/skills/code-map.md) -- it picks a"
Write-Output "     tool per language and, if this folder holds more than one app/package (e.g. a"
Write-Output "     React frontend next to a Flutter backend), generates a separate graph per unit"
Write-Output "     instead of one merged graph. List each unit in 'Project facts' if there is more"
Write-Output "     than one."
