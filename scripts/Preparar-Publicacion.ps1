$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent
$marca = Get-Date -Format 'yyyyMMdd-HHmmss'
$salida = Join-Path $raiz "artifacts\publicacion-$marca"
$angular = Join-Path $raiz "artifacts\angular-$marca"
function Comprobar([string]$Paso) { if ($LASTEXITCODE -ne 0) { throw "Falló: $Paso. No se publicará una compilación incompleta." } }
Push-Location (Join-Path $raiz 'frontend')
try {
    if (!(Test-Path 'node_modules')) { & npm.cmd ci; Comprobar 'instalación de Angular' }
    & npm.cmd run check; Comprobar 'verificación de Angular'
    & npm.cmd run build -- --output-path $angular; Comprobar 'compilación de Angular'
} finally { Pop-Location }
Push-Location $raiz
try {
    & dotnet run --project tests/Seguridad/Seguridad.csproj --configuration Release; Comprobar 'pruebas de contraseñas'
    & dotnet publish backend/RegionalExpress.API/RegionalExpress.API.csproj --configuration Release --output $salida; Comprobar 'publicación de API'
    New-Item -ItemType Directory -Path (Join-Path $salida 'wwwroot') -Force | Out-Null
    Get-ChildItem -LiteralPath (Join-Path $angular 'browser') | Copy-Item -Destination (Join-Path $salida 'wwwroot') -Recurse -Force
    $privados = Get-ChildItem -LiteralPath $salida -Recurse -File | Where-Object { $_.Name -like 'appsettings*.json' -or $_.Extension -in @('.eml','.bak','.mdf','.ldf','.pfx','.key') }
    if ($privados) { throw 'El paquete contiene configuración o datos privados. Revísalo antes de compartirlo.' }
    if (!(Test-Path (Join-Path $salida 'wwwroot\index.html'))) { throw 'Falta el inicio de Angular.' }
    $zip = "$salida.zip"
    Compress-Archive -Path (Join-Path $salida '*') -DestinationPath $zip
    Write-Host "Paquete listo: $zip"
    Write-Host 'Configura la conexión SQL y la clave JWT en el alojamiento antes de iniciarlo.'
} finally { Pop-Location }
