param(
    [switch]$CleanPorts,
    [switch]$SkipWebApi,
    [switch]$SkipSimulator,
    [switch]$NoBrowser
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$stateDirectory = Join-Path $root ".devstack"
$statePath = Join-Path $stateDirectory "state.json"
$state = [ordered]@{
    StartedAt = (Get-Date).ToString("o")
    ShellProcessIds = @()
    BrowserProcessIds = @()
    BrowserProfilePath = $null
}

function Save-State {
    New-Item -ItemType Directory -Force -Path $stateDirectory | Out-Null
    $state | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $statePath -Encoding UTF8
}

function Test-CommandExists {
    param([string]$Name)
    return $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

function Stop-Port {
    param([int]$Port)

    $processIds = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess -Unique

    foreach ($processId in $processIds) {
        if ($processId -and $processId -ne $PID) {
            $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "Stopping process $($process.ProcessName) [$processId] on port $Port"
                Stop-Process -Id $processId -Force
            }
        }
    }
}

function Wait-ForPort {
    param(
        [int]$Port,
        [int]$TimeoutSeconds = 30
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $connection = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
        if ($connection) {
            return $true
        }

        Start-Sleep -Seconds 1
    }

    return $false
}

function Start-DevWindow {
    param(
        [string]$Title,
        [string]$WorkingDirectory,
        [string]$Command
    )

    $escapedTitle = $Title.Replace("'", "''")
    $escapedDirectory = $WorkingDirectory.Replace("'", "''")
    $escapedCommand = $Command.Replace("'", "''")
    $script = "& { `$Host.UI.RawUI.WindowTitle = '$escapedTitle'; Set-Location '$escapedDirectory'; Write-Host ''; Write-Host '== $escapedTitle ==' -ForegroundColor Cyan; Write-Host '$escapedCommand' -ForegroundColor DarkGray; Write-Host ''; $escapedCommand }"

    $process = Start-Process powershell.exe -ArgumentList @(
        "-NoExit",
        "-ExecutionPolicy", "Bypass",
        "-Command", $script
    ) -PassThru

    $state.ShellProcessIds += $process.Id
    Save-State
}

function Start-DashboardBrowser {
    $url = "http://localhost:5173/login"
    $edge = Get-Command msedge.exe -ErrorAction SilentlyContinue

    if ($edge) {
        $profilePath = Join-Path $stateDirectory "edge-profile"
        New-Item -ItemType Directory -Force -Path $profilePath | Out-Null
        $state.BrowserProfilePath = $profilePath

        $process = Start-Process -FilePath $edge.Source -ArgumentList @(
            "--app=$url",
            "--user-data-dir=$profilePath",
            "--new-window"
        ) -PassThru

        $state.BrowserProcessIds += $process.Id
        Save-State
        return
    }

    Write-Warning "msedge.exe was not found. Opening the dashboard in the default browser; Stop-Dev may not be able to close that tab automatically."
    Start-Process $url
}

Write-Host "Smart Apiary dev startup" -ForegroundColor Green
Write-Host "Root: $root"
Save-State

$requiredCommands = @("azurite", "func", "dotnet", "npm")
foreach ($command in $requiredCommands) {
    if (-not (Test-CommandExists $command)) {
        throw "Missing required command '$command'. Install it or make sure it is available on PATH."
    }
}

if ($CleanPorts) {
    Write-Host "Cleaning known dev ports..."
    7071, 5108, 5173, 10000, 10001, 10002 | ForEach-Object { Stop-Port $_ }
    Start-Sleep -Seconds 2
}

Start-DevWindow `
    -Title "Smart Apiary - Azurite" `
    -WorkingDirectory $root `
    -Command "azurite"

if (-not (Wait-ForPort -Port 10000 -TimeoutSeconds 20)) {
    Write-Warning "Azurite did not start listening on port 10000 within 20 seconds. Continuing anyway."
}

Start-DevWindow `
    -Title "Smart Apiary - Functions" `
    -WorkingDirectory (Join-Path $root "SmartApiary.Functions") `
    -Command "func start --port 7071"

if (-not (Wait-ForPort -Port 7071 -TimeoutSeconds 45)) {
    Write-Warning "Functions did not start listening on port 7071 within 45 seconds. Simulator may fail until Functions finishes starting."
}

if (-not $SkipWebApi) {
    Start-DevWindow `
        -Title "Smart Apiary - WebApi" `
        -WorkingDirectory (Join-Path $root "SmartApiary.WebApi") `
        -Command "dotnet run --project SmartApiary.WebApi.csproj"

    if (-not (Wait-ForPort -Port 5108 -TimeoutSeconds 45)) {
        Write-Warning "WebApi did not start listening on port 5108 within 45 seconds. Dashboard login may fail until WebApi finishes starting."
    }
}

if (-not $SkipSimulator) {
    Start-DevWindow `
        -Title "Smart Apiary - Simulator" `
        -WorkingDirectory $root `
        -Command "dotnet run --project SmartApiary.Simulator"
}

Start-DevWindow `
    -Title "Smart Apiary - Dashboard" `
    -WorkingDirectory (Join-Path $root "dashboard") `
    -Command "npm run dev"

if (-not $NoBrowser) {
    Start-Sleep -Seconds 4
    Start-DashboardBrowser
}

Write-Host ""
Write-Host "Started dev stack." -ForegroundColor Green
Write-Host "Dashboard: http://localhost:5173"
Write-Host "Login:     http://localhost:5173/login"
Write-Host "WebApi:    http://localhost:5108/swagger"
Write-Host "Functions: http://localhost:7071"
Write-Host ""
Write-Host "Login:"
Write-Host "  Email:    beekeeper@test.com"
Write-Host "  Password: 12341234"
Write-Host ""
Write-Host "Simulator automatically discovers registered hive devices."
