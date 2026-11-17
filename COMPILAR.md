# Preparación técnica de MrBIM Tools

Esta entrega contiene el código fuente del complemento independiente. Para generar los archivos instalables se necesita un equipo Windows con las bibliotecas de la versión correspondiente de Revit.

## Destinos separados

| Versión | Proyecto | Plataforma |
|---|---|---|
| Revit 2024 | `src/Revit2024/MrBIM.Tools.2024.csproj` | .NET Framework 4.8, x64 |
| Revit 2025 | `src/Revit2025/MrBIM.Tools.2025.csproj` | .NET 8 para Windows, x64 |

No intercambiar las DLL entre versiones. Las referencias `RevitAPI.dll` y `RevitAPIUI.dll` se toman de la instalación local de Revit y no se redistribuyen.

## Requisitos de compilación

- Windows de 64 bits.
- SDK .NET 8.
- Developer Pack de .NET Framework 4.8 para el destino 2024.
- Revit 2024 y/o 2025 instalados, según el destino que se vaya a compilar.

## Pruebas de reglas, sin abrir Revit

Desde la raíz del repositorio:

```powershell
dotnet run --project tests/Rules.Tests.csproj --configuration Release
```

Estas comprobaciones evalúan las reglas y el escape de CSV; no prueban la carga ni las interacciones con Revit.

## Compilar

```powershell
.\build\Build.ps1 -Year 2024
.\build\Build.ps1 -Year 2025
```

Si Revit está instalado en otra ubicación:

```powershell
.\build\Build.ps1 -Year 2025 -RevitInstallDir 'D:\Autodesk\Revit 2025'
```

Los resultados se generan en `dist/2024` y `dist/2025`. El script se detiene si faltan las referencias o falla la compilación.

## Instalar una compilación generada

Cierra Revit. Ejecuta el script del destino correspondiente:

```powershell
.\build\Install.ps1 -Year 2024
.\build\Install.ps1 -Year 2025
```

Solo se instala para el usuario actual. Se copian la DLL y el manifiesto de MrBIM bajo `%APPDATA%\Autodesk\Revit\Addins\<año>`. Si existían archivos MrBIM, se respaldan antes de reemplazarlos. El script no cambia políticas de ejecución de Windows.

Abre la versión correspondiente de Revit y verifica la pestaña **MrBIM**, con los botones **Revisar parámetros** y **Acerca de MrBIM**. El complemento no está firmado; al cargarlo, Revit puede mostrar una advertencia de editor que debe revisarse.

Para retirar esta instalación, cierra Revit y elimina únicamente `MrBIM.Tools.addin` y la carpeta `MrBIMTools` dentro del directorio de complementos de esa versión. Esto no modifica modelos.

Si tienes instalado un prototipo previo con una pestaña MrBIM, desactívalo antes de probar esta versión para evitar botones duplicados.

## Referencias oficiales

- Registro de complementos: https://help.autodesk.com/cloudhelp/2025/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Add_In_Integration/Revit_API_Revit_API_Developers_Guide_Introduction_Add_In_Integration_Add_in_Registration_html.html
- Aplicaciones externas: https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Using_the_Autodesk_Revit_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Using_the_Autodesk_Revit_API_External_Applications_html.html
