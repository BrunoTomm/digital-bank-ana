# BankMore - Pre-pull imagens .NET com retry, depois docker compose up
# Uso: .\build.ps1           -> sobe em segundo plano (containers ficam online)
#      .\build.ps1 -Attach   -> sobe em primeiro plano (ver logs, Ctrl+C para parar)
#      powershell -NoExit -File .\build.ps1  (mantem janela aberta ao finalizar)

param([switch]$Attach)

Set-Location $PSScriptRoot

function Pull-WithRetry {
    param([string]$Image)
    $attempt = 0
    while ($true) {
        $attempt++
        Write-Host "Tentativa ${attempt}: docker pull $Image" -ForegroundColor Cyan
        docker pull $Image
        if ($LASTEXITCODE -eq 0) {
            Write-Host "OK: $Image" -ForegroundColor Green
            return $true
        }
        Write-Host "Falhou (EOF/rede). Reintentando em 5s..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }
}

try {
    Write-Host "Pre-pull das imagens .NET (ate funcionar)..." -ForegroundColor Cyan
    Pull-WithRetry "mcr.microsoft.com/dotnet/aspnet:8.0"
    Pull-WithRetry "mcr.microsoft.com/dotnet/sdk:8.0"

    Write-Host ""
    Write-Host "Imagens OK. Subindo docker compose..." -ForegroundColor Green
    if ($Attach) {
        docker compose up
    } else {
        docker compose up -d
        Write-Host ""
        Write-Host "Containers em execucao. Acesse: http://localhost:5000 (Current Account), http://localhost:5001 (Transfer), http://localhost:5002 (Fees)" -ForegroundColor Green
    }
} catch {
    Write-Host "ERRO: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    if ($Attach) {
        Write-Host ""
        Read-Host "Pressione Enter para fechar"
    }
}
