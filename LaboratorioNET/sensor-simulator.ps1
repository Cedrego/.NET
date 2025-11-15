#!/usr/bin/env powershell
# Simple, ASCII-only PowerShell simulator for /api/sensor
# Usage:
#   .\sensor-simulator.ps1 -CarreraId "test-carrera" -CorredoresIds @("1001","1002") -CantSecciones 3 -Url "https://localhost:7263" -DelayMs 500

# Bypass TLS for local dev only
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

param(
    [string]$Url = "https://localhost:7263",
    [string]$CarreraId = "test-carrera",
    [string[]]$CorredoresIds = @('1001','1002'),
    [int]$CantSecciones = 3,
    [int]$DelayMs = 500
)

if ([string]::IsNullOrEmpty($CarreraId) -or $CorredoresIds.Count -eq 0) {
    Write-Host "Error: CarreraId and CorredoresIds are required" -ForegroundColor Red
    return
}

Write-Host "Starting simulator: Carrera=$CarreraId Corredores=$($CorredoresIds -join ',') Checkpoints=$CantSecciones DelayMs=$DelayMs"

$start = Get-Date

for ($checkpoint = 1; $checkpoint -le $CantSecciones; $checkpoint++) {
    Write-Host "-- CHECKPOINT $checkpoint --"

    foreach ($corredor in $CorredoresIds) {
        $timestamp = $start.AddSeconds(($checkpoint * 30) + (Get-Random -Minimum -5 -Maximum 5)).ToUniversalTime().ToString("o")

        $body = @{
            corredorId = $corredor
            carreraId = $CarreraId
            tiempo = $timestamp
            numeroCheckpoint = $checkpoint
        } | ConvertTo-Json -Depth 5

        Write-Host "Posting: corredor=$corredor checkpoint=$checkpoint tiempo=$timestamp"

        try {
            $resp = Invoke-RestMethod -Uri "$Url/api/sensor" -Method Post -Body $body -ContentType 'application/json'
            if ($null -ne $resp) {
                $json = $resp | ConvertTo-Json -Depth 5
                Write-Host "  Response: $json" -ForegroundColor Green
            } else {
                Write-Host "  Response: (no body)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "  Error posting: $($_.Exception.Message)" -ForegroundColor Red
        }

        Start-Sleep -Milliseconds $DelayMs
    }
}

Write-Host "Simulation finished." -ForegroundColor Cyan
#!/usr/bin/env powershell
#!/usr/bin/env powershell
# Simple, ASCII-only PowerShell simulator for /api/sensor
# Usage:
#   .\sensor-simulator.ps1 -CarreraId "test-carrera" -CorredoresIds @("1001","1002") -CantSecciones 3 -Url "https://localhost:7263" -DelayMs 500

# Bypass TLS for local dev only
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

param(
    [string]$Url = "https://localhost:7263",
    [string]$CarreraId = "test-carrera",
    [string[]]$CorredoresIds = @('1001','1002'),
    [int]$CantSecciones = 3,
    [int]$DelayMs = 500
)

if ([string]::IsNullOrEmpty($CarreraId) -or $CorredoresIds.Count -eq 0) {
    Write-Host "Error: CarreraId and CorredoresIds are required" -ForegroundColor Red
    return
}

Write-Host "Starting simulator: Carrera=$CarreraId Corredores=$($CorredoresIds -join ',') Checkpoints=$CantSecciones DelayMs=$DelayMs"

$start = Get-Date

for ($checkpoint = 1; $checkpoint -le $CantSecciones; $checkpoint++) {
    Write-Host "-- CHECKPOINT $checkpoint --"

    foreach ($corredor in $CorredoresIds) {
        $timestamp = $start.AddSeconds(($checkpoint * 30) + (Get-Random -Minimum -5 -Maximum 5)).ToUniversalTime().ToString("o")

        $body = @{
            #!/usr/bin/env powershell
            # Simple, ASCII-only PowerShell simulator for /api/sensor
            # Usage:
            #   .\sensor-simulator.ps1 -CarreraId "test-carrera" -CorredoresIds @("1001","1002") -CantSecciones 3 -Url "https://localhost:7263" -DelayMs 500

            # Bypass TLS for local dev only
            [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

            param(
                [string]$Url = "https://localhost:7263",
                [string]$CarreraId = "test-carrera",
                [string[]]$CorredoresIds = @('1001','1002'),
                [int]$CantSecciones = 3,
                [int]$DelayMs = 500
            )

            if ([string]::IsNullOrEmpty($CarreraId) -or $CorredoresIds.Count -eq 0) {
                Write-Host "Error: CarreraId and CorredoresIds are required" -ForegroundColor Red
                return
            }

            Write-Host "Starting simulator: Carrera=$CarreraId Corredores=$($CorredoresIds -join ',') Checkpoints=$CantSecciones DelayMs=$DelayMs"

            $start = Get-Date

            for ($checkpoint = 1; $checkpoint -le $CantSecciones; $checkpoint++) {
                Write-Host "-- CHECKPOINT $checkpoint --"

                foreach ($corredor in $CorredoresIds) {
                    $timestamp = $start.AddSeconds(($checkpoint * 30) + (Get-Random -Minimum -5 -Maximum 5)).ToUniversalTime().ToString("o")

                    $body = @{
                        corredorId = $corredor
                        carreraId = $CarreraId
                        tiempo = $timestamp
                        numeroCheckpoint = $checkpoint
                    } | ConvertTo-Json -Depth 5

                    Write-Host "Posting: corredor=$corredor checkpoint=$checkpoint tiempo=$timestamp"

                    try {
                        $resp = Invoke-RestMethod -Uri "$Url/api/sensor" -Method Post -Body $body -ContentType 'application/json'
                        if ($null -ne $resp) {
                            $json = $resp | ConvertTo-Json -Depth 5
                            Write-Host "  Response: $json" -ForegroundColor Green
                        } else {
                            Write-Host "  Response: (no body)" -ForegroundColor Yellow
                        }
                    } catch {
                        Write-Host "  Error posting: $($_.Exception.Message)" -ForegroundColor Red
                    }

                    Start-Sleep -Milliseconds $DelayMs
                }
            }

            Write-Host "Simulation finished." -ForegroundColor Cyan
