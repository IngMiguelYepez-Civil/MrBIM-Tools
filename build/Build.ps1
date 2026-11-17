[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][ValidateSet('2024','2025')][string]$Year,
    [string]$RevitInstallDir = "C:\Program Files\Autodesk\Revit $Year"
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'Instale el SDK .NET 8 para compilar.'
}
foreach ($name in @('RevitAPI.dll','RevitAPIUI.dll')) {
    if (-not (Test-Path (Join-Path $RevitInstallDir $name))) { throw "No se encontró $name en $RevitInstallDir" }
}
$project = Join-Path $root "src\Revit$Year\MrBIM.Tools.$Year.csproj"
$out = Join-Path $root "dist\$Year"
# No se reutilizan DLL de otra versión de Revit: cada destino tiene proyecto y salida separados.
& dotnet build $project --configuration Release --output $out "-p:RevitInstallDir=$RevitInstallDir"
if ($LASTEXITCODE -ne 0) { throw 'La compilación falló. No se instaló ningún complemento.' }
if (-not (Test-Path (Join-Path $out 'MrBIM.Tools.dll'))) { throw 'No se produjo MrBIM.Tools.dll.' }
Set-Content -Path (Join-Path $out 'revit-target.txt') -Value $Year -Encoding ASCII
Write-Host "Compilación preparada en $out. Pruebe la carga en Revit $Year."
