param(
    [switch]$KeepAzurite,
    [switch]$KeepBrowser
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$statePath = Join-Path (Join-Path $root ".devstack") "state.json"

function Stop-Port {
    param([int]$Port)

    $processIds = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess -Unique

    foreach ($processId in $processIds) {
        if ($processId -and $processId -ne $PID) {
            $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "Stopping $($process.ProcessName) [$processId] on port $Port"
                Stop-Process -Id $processId -Force
            }
        }
    }
}

function Stop-DevWindows {
    $processIds = @()

    if (Test-Path -LiteralPath $statePath) {
        $state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
        $processIds += @($state.ShellProcessIds)
    }

    $markedProcesses = Get-CimInstance Win32_Process |
        Where-Object {
            $_.Name -in @("powershell.exe", "pwsh.exe") -and
            $_.CommandLine -like "*Smart Apiary -*"
        } |
        Select-Object -ExpandProperty ProcessId

    $processIds += @($markedProcesses)

    foreach ($processId in ($processIds | Where-Object { $_ } | Select-Object -Unique)) {
        $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
        if ($process) {
            Write-Host "Closing dev shell $($process.ProcessName) [$processId]"
            Stop-Process -Id $processId -Force
        }
    }
}

function Stop-DashboardBrowser {
    if ($KeepBrowser) {
        return
    }

    $processIds = @()
    $browserProfilePath = $null

    if (Test-Path -LiteralPath $statePath) {
        $state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
        $processIds += @($state.BrowserProcessIds)
        $browserProfilePath = $state.BrowserProfilePath
    }

    if (-not [string]::IsNullOrWhiteSpace($browserProfilePath)) {
        $profileMatchedProcesses = Get-CimInstance Win32_Process |
            Where-Object {
                $_.Name -in @("msedge.exe", "chrome.exe") -and
                $_.CommandLine -like "*$browserProfilePath*"
            } |
            Select-Object -ExpandProperty ProcessId

        $processIds += @($profileMatchedProcesses)
    }

    foreach ($processId in ($processIds | Where-Object { $_ } | Select-Object -Unique)) {
        $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
        if ($process) {
            Write-Host "Closing dashboard browser $($process.ProcessName) [$processId]"
            Stop-Process -Id $processId -Force
        }
    }
}

Write-Host "Stopping Smart Apiary dev stack..." -ForegroundColor Yellow

7071, 5108, 5173 | ForEach-Object { Stop-Port $_ }

if (-not $KeepAzurite) {
    10000, 10001, 10002 | ForEach-Object { Stop-Port $_ }
}

Stop-DevWindows
Stop-DashboardBrowser

if (Test-Path -LiteralPath $statePath) {
    Remove-Item -LiteralPath $statePath -Force
}

Write-Host "Done." -ForegroundColor Green
Write-Host "Browser login state is cleared by opening http://localhost:5173/login after the dashboard is running."
