$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent
Push-Location (Join-Path $raiz 'backend/RegionalExpress.API')
try {
    & dotnet run --no-launch-profile -- --crear-motorista-prueba
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo crear la cuenta. Revisa el mensaje anterior.' }
} finally { Pop-Location }
