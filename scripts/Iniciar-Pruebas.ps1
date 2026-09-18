$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent
$api = Join-Path $raiz 'backend\RegionalExpress.API'
$frontend = Join-Path $raiz 'frontend'
$logs = Join-Path $raiz 'artifacts\pruebas-locales'
$procesoApi = $null
$entornoAnterior = $env:ASPNETCORE_ENVIRONMENT
function Comprobar([string]$paso) { if ($LASTEXITCODE -ne 0) { throw "No se pudo completar: $paso." } }
foreach ($puerto in @(5292,4300)) {
    if (Get-NetTCPConnection -LocalPort $puerto -State Listen -ErrorAction SilentlyContinue) { throw "El puerto $puerto ya está ocupado. Cierra la ejecución anterior de este proyecto." }
}
Push-Location $raiz
try {
    if (!(Test-Path (Join-Path $api 'appsettings.json'))) { throw 'Falta backend/RegionalExpress.API/appsettings.json con tu configuración local. No copies el ejemplo sin configurar la clave JWT.' }
    & dotnet build (Join-Path $api 'RegionalExpress.API.csproj'); Comprobar 'compilación del servidor'
    if (!(Test-Path (Join-Path $frontend 'node_modules'))) {
        Push-Location $frontend
        try { & npm.cmd ci; Comprobar 'instalación del frontend' } finally { Pop-Location }
    }
    New-Item -ItemType Directory -Path $logs -Force | Out-Null
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $dll = Join-Path $api 'bin\Debug\net10.0\RegionalExpress.API.dll'
    $procesoApi = Start-Process -FilePath 'dotnet' -ArgumentList @(('"' + $dll + '"'),'--urls','http://127.0.0.1:5292') -WorkingDirectory $api -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $logs 'api.log') -RedirectStandardError (Join-Path $logs 'api-error.log')
    $lista = $false
    for ($intento=0; $intento -lt 30; $intento++) {
        if ($procesoApi.HasExited) { throw "El servidor se detuvo. Revisa $logs\api-error.log" }
        try { $respuesta = Invoke-RestMethod 'http://127.0.0.1:5292/api/salud' -TimeoutSec 1; $lista = $respuesta.estado -eq 'Disponible'; if ($lista) { break } } catch {}
        Start-Sleep -Seconds 1
    }
    if (!$lista) { throw "El servidor no respondió. Revisa $logs\api.log" }
    Write-Host 'API lista. La página abrirá en http://localhost:4300 cuando termine Angular.'
    Write-Host 'Mantén esta terminal abierta. Ctrl+C detiene las pruebas y el servidor iniciado por este archivo.'
    Write-Host 'Para compartir por HTTPS: VS Code > Puertos > Reenviar puerto 4300 > copiar enlace.'
    Push-Location $frontend
    try { & npm.cmd run start:pruebas; Comprobar 'ejecución del frontend' } finally { Pop-Location }
} finally {
    if ($null -ne $procesoApi -and !$procesoApi.HasExited) { Stop-Process -Id $procesoApi.Id -ErrorAction SilentlyContinue }
    $env:ASPNETCORE_ENVIRONMENT = $entornoAnterior
    Pop-Location
}
