#!/usr/bin/env pwsh
<#
.SYNOPSIS
Verifies .github/ and .claude/ are complete, byte-identical duplicates of each other's content —
every skill (including shared non-standalone docs like _plan-common.md, duplicated flat rather
than nested since they're not independently invokable), every agent, and every script. Claude
Code should never have to open a .github/ file to know what to do, and vice versa.
Windows/PowerShell equivalent of check-mirrors.sh.

Exit 0 = everything matches. Exit 1 = drift found (listed on stdout) or a duplicate is missing.
#>
$ErrorActionPreference = 'Stop'
$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Set-Location $root

$script:drift = $false

function Test-Identical {
    param([string]$SourcePath, [string]$MirrorPath, [string]$MissingLabel)
    if (-not (Test-Path -LiteralPath $MirrorPath)) {
        Write-Output "MISSING $MissingLabel`: $MirrorPath (source: $SourcePath)"
        $script:drift = $true
        return
    }
    $srcContent = Get-Content -Raw -LiteralPath $SourcePath
    $mirrorContent = Get-Content -Raw -LiteralPath $MirrorPath
    if ($srcContent -ne $mirrorContent) {
        Write-Output "DRIFT: $SourcePath and $MirrorPath differ"
        $script:drift = $true
    }
}

# --- skills: named ones get Claude's nested SKILL.md form; shared non-standalone docs (leading
# underscore, e.g. _plan-common.md) get a flat duplicate instead ---
Get-ChildItem -Path (Join-Path $root '.github/skills') -Filter '*.md' -File | ForEach-Object {
    $name = $_.BaseName
    if ($name.StartsWith('_')) {
        Test-Identical -SourcePath $_.FullName -MirrorPath (Join-Path $root ".claude/skills/$name.md") -MissingLabel 'shared-doc duplicate'
    } else {
        Test-Identical -SourcePath $_.FullName -MirrorPath (Join-Path $root ".claude/skills/$name/SKILL.md") -MissingLabel 'skill mirror'
    }
}

# --- lang packs: .github/skills/lang/<name>.md -> .claude/skills/<name>/SKILL.md (flat on the
# Claude side, same as any other named skill) ---
$langDir = Join-Path $root '.github/skills/lang'
if (Test-Path $langDir) {
    Get-ChildItem -Path $langDir -Filter '*.md' -File | ForEach-Object {
        $name = $_.BaseName
        Test-Identical -SourcePath $_.FullName -MirrorPath (Join-Path $root ".claude/skills/$name/SKILL.md") -MissingLabel 'lang-pack skill mirror'
    }
}

# --- examples: reference docs, flat duplicates like a shared non-standalone skill doc ---
Get-ChildItem -Path (Join-Path $root '.github/examples') -Filter '*.md' -File | ForEach-Object {
    Test-Identical -SourcePath $_.FullName -MirrorPath (Join-Path $root ".claude/examples/$($_.Name)") -MissingLabel 'example duplicate'
}

# --- plan templates: flat duplicates, same as examples/. Live here (not in .agent-plans/) so a
# fresh project's .agent-plans/ only ever holds that project's own real plans ---
Get-ChildItem -Path (Join-Path $root '.github/templates') -Filter '*.md' -File | ForEach-Object {
    Test-Identical -SourcePath $_.FullName -MirrorPath (Join-Path $root ".claude/templates/$($_.Name)") -MissingLabel 'template duplicate'
}

# --- agents: full duplicates, no thin pointers ---
Get-ChildItem -Path (Join-Path $root '.github/agents') -Filter '*.agent.md' -File | ForEach-Object {
    $name = $_.Name -replace '\.agent\.md$', ''
    Test-Identical -SourcePath $_.FullName -MirrorPath (Join-Path $root ".claude/agents/$name.md") -MissingLabel 'agent duplicate'
}

# --- scripts: bash + PowerShell, both folders ---
Get-ChildItem -Path (Join-Path $root '.github/scripts') -File | ForEach-Object {
    Test-Identical -SourcePath $_.FullName -MirrorPath (Join-Path $root ".claude/scripts/$($_.Name)") -MissingLabel 'script duplicate'
}

if (-not $script:drift) {
    Write-Output "All skills, agents, and scripts are identical between .github/ and .claude/."
    exit 0
} else {
    exit 1
}
