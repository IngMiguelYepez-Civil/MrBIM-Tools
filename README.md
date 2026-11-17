# MrBIM Tools

> **VERSIÓN EXPERIMENTAL · EN DESARROLLO**
>
> Código fuente disponible para revisión y pruebas técnicas. Aún no hay DLL ni instalador publicados y falta validar el complemento dentro de Revit 2024 y 2025. Trabaje únicamente con copias de prueba y revise los resultados antes de utilizarlos. Consulte el [aviso de pruebas](AVISO_PRUEBAS.md).

**Herramientas de control de calidad BIM para Autodesk Revit 2024 y 2025.**

**MrBIM · Ing. Miguel Oswaldo Yépez González**

MrBIM Tools permite revisar información obligatoria del modelo, identificar elementos que requieren atención y preparar informes para el seguimiento técnico.

## ¿Qué hace cada función?

| Función | Qué hace | Resultado |
|---|---|---|
| **Revisar parámetros** | Comprueba que los parámetros exigidos existan y tengan un valor asignado. Detecta también nombres duplicados y errores de lectura. | Lista de hallazgos por elemento y parámetro. |
| **Definir alcance** | Permite revisar la selección actual, la vista activa o el proyecto completo, filtrando por categorías. | Revisión centrada en los elementos elegidos. |
| **Localizar filas** | Selecciona los elementos de las filas elegidas e intenta mostrarlos en Revit al cerrar el informe. | Ubicación de los elementos que requieren revisión. |
| **Seleccionar todos** | Selecciona todos los elementos con hallazgos al cerrar el informe. | Selección agrupada para su inspección. |
| **Exportar CSV** | Guarda el resumen y todos los hallazgos, con identificadores y contexto de la revisión. | Archivo que puede abrirse en Excel. |
| **Acerca de MrBIM** | Muestra la versión, autoría y alcance de la herramienta. | Información del complemento. |

Las funciones de alcance, localización y exportación forman parte del flujo de **Revisar parámetros**. Los botones de la pestaña son **Revisar parámetros** y **Acerca de MrBIM**.

## Uso

1. Abre un proyecto y pulsa **MrBIM → Control de calidad → Revisar parámetros**.
2. Elige el alcance y marca las categorías que quieras revisar.
3. Escribe los nombres exactos de los parámetros obligatorios, separados por punto y coma o por línea. Ejemplo: `Código; Clasificación; Zona`.
4. Decide si se permite buscar en tipo cuando el parámetro no existe en instancia.
5. Revisa el informe; exporta el CSV o selecciona los elementos que quieras inspeccionar.

La revisión no modifica valores de parámetros ni geometría. Los cambios de selección y de vista se ejecutan únicamente al solicitarlos.

## Estados del informe

| Estado | Significado |
|---|---|
| **VACIO** | El parámetro existe, pero no tiene valor utilizable. |
| **NO_EXISTE** | No se encontró en el ámbito de búsqueda elegido. |
| **AMBIGUO** | Varios parámetros comparten el nombre exigido. |
| **NO_EVALUABLE** | Su almacenamiento no permite evaluar presencia con esta regla. |
| **ERROR_LECTURA** | No fue posible leerlo; el informe conserva el motivo. |

Cero y No son valores válidos. La herramienta comprueba presencia; no determina por sí sola si un dato es técnicamente correcto. Un parámetro vacío de instancia se informa como tal, aunque exista un valor en tipo.

## Versión y disponibilidad

**0.2.0 — prototipo independiente en desarrollo.** El código usa la API de Revit y se carga directamente como complemento mediante su propio archivo de registro.

**Todavía no se entrega una DLL compilada ni un instalador EXE.** La compilación de los dos destinos y las pruebas dentro de Revit están pendientes. Los archivos de desarrollo y los pasos para generar e instalar las DLL están en [COMPILAR.md](COMPILAR.md). El estado de validación está en [VALIDACION.md](VALIDACION.md).

## Límites actuales

- Revisa instancias de categorías de modelo del documento anfitrión; no recorre los elementos internos de vínculos.
- Los parámetros se identifican por nombre exacto, respetando mayúsculas y tildes; todavía no se configuran por GUID.
- El alcance de vista usa el colector de Revit, no una prueba visual de oclusión.
- La herramienta no corrige los datos, no incluye un agente de IA ni determina cumplimiento automático de ISO 19650.
- En modelos grandes, la revisión puede tardar. El progreso permite cancelar entre elementos; una cancelación no presenta un informe parcial.
- El CSV usa UTF-8 con BOM y delimitador punto y coma. Si Excel no separa las columnas, impórtalo indicando ese delimitador.

Herramienta de MrBIM. Autodesk y Revit son marcas de sus respectivos titulares.

## Participa en las pruebas

Buscamos comentarios de profesionales BIM y desarrolladores que quieran revisar el código, compilarlo y probarlo en Revit 2024 o 2025. Los pasos técnicos están en [COMPILAR.md](COMPILAR.md).

Reporte errores y propuestas en [Issues](https://github.com/IngMiguelYepez-Civil/MrBIM-Tools/issues). Indique la versión y actualización de Revit, la versión de MrBIM Tools, los pasos para reproducir el problema y el resultado esperado. No publique modelos, capturas ni datos confidenciales de proyectos.

La futura conexión con IA para explicar hallazgos y proponer correcciones está en evaluación; **no está implementada en esta versión**.
