[CmdletBinding()]
param([Parameter(Mandatory=$true)][ValidateSet('2024','2025')][string]$Year)
$ErrorActionPreference = 'Stop'
if (Get-Process -Name Revit -ErrorAction SilentlyContinue) { throw 'Cierre Revit antes de instalar.' }
$root = Split-Path $PSScriptRoot -Parent
$source = Join-Path $root "dist\$Year"
$dll = Join-Path $source 'MrBIM.Tools.dll'
$targetFile = Join-Path $source 'revit-target.txt'
if (-not (Test-Path $dll) -or -not (Test-Path $targetFile)) { throw "Compile primero el destino $Year con Build.ps1." }
if ((Get-Content $targetFile -Raw).Trim() -ne $Year) { throw 'La compilación no corresponde a la versión elegida.' }
$addins = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$Year"
$destination = Join-Path $addins 'MrBIMTools'
$manifest = Join-Path $addins 'MrBIM.Tools.addin'
$oldDll = Join-Path $destination 'MrBIM.Tools.dll'
# Respalda exclusivamente los archivos MrBIM antes de actualizarlos.
if ((Test-Path $manifest) -or (Test-Path $oldDll)) {
    $backup = Join-Path $env:LOCALAPPDATA ('MrBIM\Backups\' + $Year + '\' + [Guid]::NewGuid().ToString('N'))
    New-Item -Path $backup -ItemType Directory -Force | Out-Null
    if (Test-Path $manifest) { Copy-Item $manifest $backup }
    if (Test-Path $oldDll) { Copy-Item $oldDll $backup }
    Write-Host "Respaldo anterior: $backup"
}
New-Item -Path $destination -ItemType Directory -Force | Out-Null
Copy-Item $dll $oldDll -Force
$assemblyPath = [System.Security.SecurityElement]::Escape($oldDll)
$xml = @"
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>MrBIM Tools</Name>
    <Assembly>$assemblyPath</Assembly>
    <AddInId>2B07C2F1-5E6F-48A8-BAB9-4D0DF85DF614</AddInId>
    <FullClassName>MrBIM.Tools.App</FullClassName>
    <VendorId>MRBM</VendorId>
    <VendorDescription>MrBIM - Ing. Miguel Oswaldo Yepez Gonzalez</VendorDescription>
  </AddIn>
</RevitAddIns>
"@
[System.IO.File]::WriteAllText($manifest, $xml, (New-Object System.Text.UTF8Encoding($false)))
Write-Host "Archivos instalados para el usuario actual. Abra Revit $Year y verifique la pestaña MrBIM."
