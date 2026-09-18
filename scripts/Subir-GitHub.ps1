$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent
Push-Location $raiz
try {
    $remoto = (& git remote get-url origin).Trim()
    if ($remoto -ne 'https://github.com/javiercastro808/proyecto-grupal-regional-express.git') { throw 'El repositorio remoto no coincide con Regional Express.' }
    $nombre = & git config user.name
    if (!$nombre) {
        $nombre = Read-Host 'Nombre que aparecerá como autor de los cambios'
        if ([string]::IsNullOrWhiteSpace($nombre)) { throw 'Falta el nombre del autor.' }
        & git config --local user.name $nombre
    }
    $correo = & git config user.email
    if (!$correo) {
        $correo = Read-Host 'Correo de autor de GitHub (puedes usar el correo privado noreply de GitHub > Settings > Emails)'
        if ($correo -notmatch '^[^\s@]+@[^\s@]+\.[^\s@]+$') { throw 'Correo de autor no válido.' }
        & git config --local user.email $correo
    }
    & git add -- .
    if ($LASTEXITCODE -ne 0) { throw 'No se pudieron preparar los archivos.' }
    $archivos = & git diff --cached --name-only
    $privados = $archivos | Where-Object { ($_ -match '(^|/)appsettings.*\.json$' -and $_ -notmatch '/appsettings\.Example\.json$') -or $_ -match '(^|/)(App_Data|node_modules|artifacts)/|\.(bak|mdf|ldf|pfx|key|publishsettings)$|(^|/)\.env($|\.)' }
    if ($privados) { throw 'Hay archivos privados preparados. No se realizará la subida.' }
    if ($archivos) {
        & git commit -m 'Regional Express: asignaciones, sesión, seguridad, historial y preparación de publicación'
        if ($LASTEXITCODE -ne 0) { throw 'No se pudo guardar la versión local.' }
    }
    & git push -u origin main
    if ($LASTEXITCODE -ne 0) { throw 'GitHub no recibió los cambios. Completa el inicio de sesión que solicite Git y vuelve a ejecutar este archivo.' }
    Write-Host 'Subida completada: https://github.com/javiercastro808/proyecto-grupal-regional-express'
} finally { Pop-Location }
