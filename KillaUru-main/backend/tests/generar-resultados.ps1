# Genera los resultados de pruebas y la cobertura del backend en backend/TestResults/, con nombres legibles.
#   pwsh tests/generar-resultados.ps1            (desde backend/; requiere Docker para la integración)
# Hace lo mismo que el job de CI (.github/workflows/ci.yml): pruebas con cobertura, ReportGenerator y la puerta
# del 90 % por dominio. Diferencias: un proyecto por carpeta y un RESUMEN.md con los conteos de cada uno.
# TestResults/ está en .gitignore: para conservar una ejecución como evidencia, copiar RESUMEN.md y
# cobertura/SummaryGithub.md a docs/seguimiento/evidencias/pruebas/<fecha>/.
$ErrorActionPreference = 'Stop'
$backend = Split-Path -Parent $PSScriptRoot
$salida = Join-Path $backend 'TestResults'
$env:DOTNET_CLI_UI_LANGUAGE = 'en'   # resumen siempre en inglés para poder leerlo

function Invoke-Proyecto([string] $nombre) {
    $carpeta = Join-Path $salida $nombre
    New-Item -ItemType Directory -Force $carpeta | Out-Null
    $consola = Join-Path $carpeta 'consola.txt'
    # Sin «--»: PowerShell lo elimina, y en .NET 10 dotnet test acepta estas opciones de Microsoft.Testing.Platform directamente.
    $argumentos = @('test', '--project', (Join-Path $backend "tests/$nombre"), '--no-build',
        '--coverage', '--coverage-output-format', 'cobertura', '--results-directory', $carpeta)
    & dotnet @argumentos *> $consola
    $texto = Get-Content $consola -Raw
    [pscustomobject]@{
        Proyecto  = $nombre
        Total     = Get-Conteo $texto 'total'
        Aprobadas = Get-Conteo $texto 'succeeded'
        Fallidas  = Get-Conteo $texto 'failed'
        Omitidas  = Get-Conteo $texto 'skipped'
        Duracion  = if ($texto -match '(?m)^\s*duration:\s*(.+)$') { $Matches[1].Trim() } else { '?' }
        Codigo    = $LASTEXITCODE
    }
}

function Get-Conteo([string] $texto, [string] $clave) {
    if ($texto -match "(?m)^\s*${clave}:\s*(\d+)") { [int] $Matches[1] } else { $null }
}

function Write-Resumen($filas, [string] $puerta) {
    $sha = git -C $backend rev-parse --short HEAD
    $cambios = if (git -C $backend status --porcelain) { ' + cambios sin commit' } else { '' }
    $lineas = @(
        "# Resultados de pruebas del backend",
        "",
        "Generado el $(Get-Date -Format 'yyyy-MM-dd HH:mm') con ``tests/generar-resultados.ps1``. Código: ``$sha``$cambios.",
        "Integración con PostgreSQL/PostGIS real (Testcontainers). Omitidas = pendientes, no aprobadas.",
        "",
        "| Proyecto | Total | Aprobadas | Fallidas | Omitidas | Duración |",
        "|---|---:|---:|---:|---:|---|"
    )
    $lineas += $filas | ForEach-Object { "| $($_.Proyecto) | $($_.Total) | $($_.Aprobadas) | $($_.Fallidas) | $($_.Omitidas) | $($_.Duracion) |" }
    $suma = { param($c) ($filas | Measure-Object -Property $c -Sum).Sum }
    $lineas += "| **Total** | **$(& $suma 'Total')** | **$(& $suma 'Aprobadas')** | **$(& $suma 'Fallidas')** | **$(& $suma 'Omitidas')** | |"
    $lineas += @("", "## Cobertura", "", "Informe completo: ``cobertura/index.html``. Resumen: ``cobertura/SummaryGithub.md``.",
        "", "Puerta de CI (90 % de líneas por dominio, constitución VII):", "", '```text', $puerta.Trim(), '```')
    Set-Content -Path (Join-Path $salida 'RESUMEN.md') -Value $lineas -Encoding utf8
}

# 1. Carpeta limpia: los informes viejos falsearían la cobertura (mismo motivo que cobertura_dominio.py).
if (Test-Path $salida) { Remove-Item -Recurse -Force $salida }
New-Item -ItemType Directory $salida | Out-Null

# 2. Compilar una vez y ejecutar cada proyecto en su carpeta.
dotnet build (Join-Path $backend 'KillaUru.sln') -v q -nologo
if ($LASTEXITCODE -ne 0) { throw 'La compilación falló.' }
$proyectos = Get-ChildItem (Join-Path $backend 'tests') -Directory -Filter 'KillaUru.*.Tests' | Sort-Object Name
$filas = foreach ($p in $proyectos) { Write-Host "Probando $($p.Name)..."; Invoke-Proyecto $p.Name }

# 3. Informe de cobertura (mismo filtro que CI: sin los ensamblados de pruebas).
reportgenerator "-reports:$salida/**/*.cobertura.xml" "-targetdir:$salida/cobertura" `
    "-reporttypes:Html;MarkdownSummaryGithub;TextSummary" "-assemblyfilters:-*.Tests" | Out-Null

# 4. Puerta por dominio, la misma que CI.
$puerta = python (Join-Path $backend 'deploy/ci/cobertura_dominio.py') 90 --informes $salida 2>&1 | Out-String
$puertaOk = $LASTEXITCODE -eq 0

Write-Resumen $filas $puerta
$filas | Format-Table -AutoSize
Write-Host "Resumen: $(Join-Path $salida 'RESUMEN.md')"
$fallidas = ($filas | Measure-Object -Property Fallidas -Sum).Sum
if ($fallidas -gt 0 -or -not $puertaOk -or ($filas | Where-Object { $null -eq $_.Total })) { exit 1 }
