# Aviso de pruebas — MrBIM Tools

**Versión experimental en desarrollo: 0.2.0.**

El repositorio publica código fuente para revisión y pruebas técnicas. Todavía no se ofrece una DLL compilada ni un instalador EXE. La compatibilidad efectiva con Revit 2024 y 2025 está pendiente de validación dentro de ambas aplicaciones.

## Uso durante las pruebas

- Compile el destino correspondiente siguiendo COMPILAR.md.
- Utilice copias de modelos destinadas a pruebas; evite modelos centrales o proyectos de producción.
- Revise manualmente los resultados. La comprobación de presencia de parámetros no certifica calidad BIM, cumplimiento normativo ni cumplimiento de ISO 19650.
- Las funciones se limitan al alcance descrito en README.md. La integración con IA aún no está implementada.
- La herramienta está diseñada para consultar datos sin modificar parámetros ni geometría. Localizar elementos puede cambiar la selección y la vista; exportar crea un archivo en la ubicación elegida.
- No se garantiza ausencia de errores, continuidad de soporte ni idoneidad para un proyecto específico.

## Reportar resultados

Abra un [Issue](https://github.com/IngMiguelYepez-Civil/MrBIM-Tools/issues) e indique versión de Revit y actualización, versión de MrBIM Tools, pasos, resultado esperado y resultado observado. Comparta ejemplos mínimos sin información confidencial.

## Autoría

MrBIM · Ing. Miguel Oswaldo Yépez González. Mantenga las referencias de autoría. Este aviso describe el estado del prototipo y sus pruebas; no constituye una licencia general de redistribución o explotación comercial.
