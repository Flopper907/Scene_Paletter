#!/usr/bin/env pwsh
<#
.SYNOPSIS
Renders every .agent-plans/*.md (excluding templates) as a status table, by reading each file's
YAML frontmatter. Windows/PowerShell equivalent of list-plans.sh — same behavior, same output
shape: sorted by priority (needs-input/blocked first, done/abandoned last, ties broken by
`updated` descending) instead of filename order, plus a dependency check (duplicate ids and
depends_on references that don't match any plan found), then a dependency-safe execution order
(Kahn's algorithm) among plans not yet done/abandoned — any cycle, self-loop, mutual pair, or a
longer A->B->C->A chain, shows up the same way: plans that never reach indegree 0.
Backs the list-plans skill.
#>
$ErrorActionPreference = 'Stop'
$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Set-Location $root

function Get-FrontmatterField {
    param([string]$Path, [string]$Key)
    $inFrontmatter = 0
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^---\s*$') {
            $inFrontmatter++
            if ($inFrontmatter -ge 2) { break }
            continue
        }
        if ($inFrontmatter -eq 1 -and $line -match "^$Key\s*:\s*(.*)$") {
            $value = ($matches[1] -replace '#.*$', '').Trim()
            if ($value) { return $value }
            return '-'
        }
    }
    return '-'
}

function Get-DependsOn {
    param([string]$Path)
    $result = @()
    $inFrontmatter = 0
    $capture = $false
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^---\s*$') {
            $inFrontmatter++
            if ($inFrontmatter -ge 2) { break }
            continue
        }
        if ($inFrontmatter -ne 1) { continue }
        if ($capture) {
            if ($line -match '^\s*-\s*(.+?)\s*(#.*)?$') {
                $v = $matches[1].Trim("'", '"', ' ')
                if ($v) { $result += $v }
                continue
            } elseif ($line -match '^\s*$') {
                continue
            } else {
                $capture = $false
            }
        }
        if ($line -match '^depends_on:\s*(.*)$') {
            $rest = ($matches[1] -replace '#.*$', '').Trim()
            if ($rest -match '^\[(.*)\]$') {
                $inner = $matches[1]
                if ($inner.Trim()) {
                    $result += ($inner -split ',') | ForEach-Object { $_.Trim("'", '"', ' ') } | Where-Object { $_ }
                }
                $capture = $false
            } elseif (-not $rest) {
                $capture = $true
            }
        }
    }
    return $result
}

function Get-StatusPriority {
    param([string]$Status)
    switch ($Status) {
        'needs-input' { 0 }
        'blocked'     { 1 }
        'approved'    { 2 }
        'in_progress' { 3 }
        'critiqued'   { 4 }
        'draft'       { 5 }
        'done'        { 6 }
        'abandoned'   { 7 }
        default       { 8 }
    }
}

$plansDir = Join-Path $root '.agent-plans'
$plans = @()
$edges = @()   # each: @{ From = id; To = depId }

if (Test-Path -LiteralPath $plansDir) {
    Get-ChildItem -LiteralPath $plansDir -Filter '*.md' -File |
        Where-Object { -not $_.Name.StartsWith('_') } |
        ForEach-Object {
            $id = Get-FrontmatterField -Path $_.FullName -Key 'id'
            $status = Get-FrontmatterField -Path $_.FullName -Key 'status'
            $plans += [pscustomobject]@{
                Id       = $id
                Type     = Get-FrontmatterField -Path $_.FullName -Key 'type'
                Tier     = Get-FrontmatterField -Path $_.FullName -Key 'tier'
                Status   = $status
                Updated  = Get-FrontmatterField -Path $_.FullName -Key 'updated'
                File     = ".agent-plans/$($_.Name)"
                Priority = Get-StatusPriority $status
            }
            foreach ($dep in (Get-DependsOn -Path $_.FullName)) {
                $edges += [pscustomobject]@{ From = $id; To = $dep }
            }
        }
}

$sorted = $plans | Sort-Object -Property @{Expression = 'Priority'; Descending = $false}, @{Expression = 'Updated'; Descending = $true}

"{0,-28} {1,-8} {2,-9} {3,-11} {4,-11} {5}" -f 'ID', 'TYPE', 'TIER', 'STATUS', 'UPDATED', 'FILE'
"{0,-28} {1,-8} {2,-9} {3,-11} {4,-11} {5}" -f ('-' * 28), ('-' * 8), ('-' * 9), ('-' * 11), ('-' * 11), '----'
foreach ($p in $sorted) {
    "{0,-28} {1,-8} {2,-9} {3,-11} {4,-11} {5}" -f $p.Id, $p.Type, $p.Tier, $p.Status, $p.Updated, $p.File
}

# --- dependency validation ---
$problems = 0
$knownIds = $plans | ForEach-Object { $_.Id }

$byId = $plans | Group-Object -Property Id
foreach ($g in $byId) {
    if ($g.Name -ne '-' -and $g.Count -gt 1) {
        $files = ($g.Group | ForEach-Object { $_.File }) -join ', '
        Write-Output "DUPLICATE id: '$($g.Name)' used by $files"
        $problems++
    }
}

foreach ($e in $edges) {
    if ($knownIds -notcontains $e.To) {
        Write-Output "DANGLING depends_on: '$($e.From)' depends on '$($e.To)', which doesn't match any plan's id"
        $problems++
    }
}

# --- execution order (Kahn's algorithm) among plans not yet done/abandoned ---
# A done/abandoned plan's own ordering doesn't matter, but it still satisfies anything that
# depends on it -- excluded from the active set, but still counts as "satisfied" rather than
# "pending" for a dependent's indegree. Any plan that never reaches indegree 0 is part of a cycle
# (self-loop, mutual pair, or a longer chain -- Kahn's algorithm doesn't care which shape it is),
# so full cycle detection falls out of computing the order rather than needing its own checks.
$active = $plans | Where-Object { $_.Status -ne 'done' -and $_.Status -ne 'abandoned' }
$activeIds = New-Object System.Collections.Generic.HashSet[string]
foreach ($p in $active) { [void]$activeIds.Add($p.Id) }

$indeg = @{}
foreach ($p in $active) { $indeg[$p.Id] = 0 }
foreach ($e in $edges) {
    if ($activeIds.Contains($e.From) -and $activeIds.Contains($e.To)) {
        $indeg[$e.From]++
    }
}

$queue = New-Object System.Collections.Generic.Queue[string]
foreach ($p in $active) { if ($indeg[$p.Id] -eq 0) { $queue.Enqueue($p.Id) } }

$execOrder = @()
while ($queue.Count -gt 0) {
    $cur = $queue.Dequeue()
    $execOrder += $cur
    foreach ($e in $edges) {
        if ($e.To -eq $cur -and $activeIds.Contains($e.From)) {
            $indeg[$e.From]--
            if ($indeg[$e.From] -eq 0) { $queue.Enqueue($e.From) }
        }
    }
}

if ($execOrder.Count -lt $active.Count) {
    $leftover = $active | Where-Object { $execOrder -notcontains $_.Id } | ForEach-Object { $_.Id }
    Write-Output "CYCLE: these plans depend on each other (directly or through a chain) and have no valid execution order: $($leftover -join ' ')"
    $problems++
}

if ($problems -eq 0) {
    Write-Output ""
    Write-Output "Dependency check: OK (duplicate ids, dangling references, and dependency cycles of any"
    Write-Output "length all checked)"
    if ($active.Count -gt 0) {
        Write-Output "Suggested execution order (dependency-safe, not-yet-done/abandoned plans only):"
        Write-Output "  $($execOrder -join ' ')"
    }
}
