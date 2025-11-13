#!/usr/bin/env powershell
<#
SYNOPSIS
    Simulador sencillo para enviar peticiones al endpoint /api/sensor
#>

param(
    [string]$Url = "https://localhost:7263",
    [string]$CarreraId = "test-carrera",
    [string[]]$CorredoresIds = @('1001','1002'),
    [int]$CantSecciones = 3,
    [int]$DelayMs = 500
)

function Send-SensorData {
    param(
        [string]$Url,
        [string]$CarreraId,
        [string]$CorredorId,
        [int]$Checkpoint,
        [DateTime]$Tiempo
    )
    
    $payload = @{
        corredorId = $CorredorId
        carreraId = $CarreraId
        tiempo = $Tiempo.ToUniversalTime().ToString("o")
        numeroCheckpoint = $Checkpoint
    } | ConvertTo-Json
    
    Write-Host "📤 Enviando dato de sensor: Corredor=$CorredorId, Checkpoint=$Checkpoint" -ForegroundColor Cyan
    
    try {
        $response = Invoke-WebRequest `
            -Uri "$Url/api/sensor" `
            -Method POST `
            -Body $payload `
            -ContentType "application/json" `
            -SkipCertificateCheck
        
        $result = $response.Content | ConvertFrom-Json
        
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Dato procesado exitosamente" -ForegroundColor Green
            Write-Host "   Carrera Terminada: $($result.carreraTerminada)" -ForegroundColor Gray
        } else {
            Write-Host "⚠️ Status: $($response.StatusCode)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "❌ Error al enviar dato: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Simulate-Race {
    param(
        [string]$Url,
        [string]$CarreraId,
        [string[]]$CorredoresIds,
        [int]$CantSecciones,
        [int]$DelayMs
    )
    
    if ([string]::IsNullOrEmpty($CarreraId) -or $CorredoresIds.Count -eq 0) {
        Write-Host "❌ Error: CarreraId y CorredoresIds son requeridos" -ForegroundColor Red
        Write-Host ""
        Write-Host "Uso:" -ForegroundColor Yellow
        Write-Host "  .\sensor-simulator.ps1 -CarreraId 'tu-carrera-id' -CorredoresIds @('id1', 'id2') -CantSecciones 3" -ForegroundColor Yellow
        return
    }
    
    Write-Host ""
    Write-Host "🏃 SIMULADOR DE CARRERA" -ForegroundColor Magenta
    Write-Host "=============================================" -ForegroundColor Magenta
    Write-Host "Carrera ID: $CarreraId" -ForegroundColor Cyan
    Write-Host "Corredores: $($CorredoresIds.Count)" -ForegroundColor Cyan
    Write-Host "Checkpoints: $CantSecciones" -ForegroundColor Cyan
    Write-Host "Delay entre checkpoint: ${DelayMs}ms" -ForegroundColor Cyan
    Write-Host "=============================================" -ForegroundColor Magenta
    Write-Host ""
    
    $tiempoInicio = Get-Date
    
    # Simular cada corredor pasando por cada checkpoint
    for ($i = 0; $i -lt $CantSecciones; $i++) {
        Write-Host "🚩 CHECKPOINT $($i + 1) de $CantSecciones" -ForegroundColor Yellow
        Write-Host "=============================================" -ForegroundColor Yellow
        
        foreach ($corredorId in $CorredoresIds) {
            # Simular tiempo variable para cada corredor por checkpoint
            $tiempoCheckpoint = $tiempoInicio.AddSeconds((($i + 1) * 30) + (Get-Random -Minimum -5 -Maximum 5))
            
            Send-SensorData -Url $Url `
                           -CarreraId $CarreraId `
                           -CorredorId $corredorId `
                           -Checkpoint ($i + 1) `
                           -Tiempo $tiempoCheckpoint
            
            Start-Sleep -Milliseconds $DelayMs
        }
        
        Write-Host ""
    }
    
    Write-Host "✅ SIMULACIÓN COMPLETADA" -ForegroundColor Green
    Write-Host "=============================================" -ForegroundColor Green
    Write-Host ""
    }

# Ejecutar simulación
