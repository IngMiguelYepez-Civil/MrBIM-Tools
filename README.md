# MrBIM Tools 0.1.0 — Prototipo

**Marca:** MrBIM · **Responsable:** Ing. Miguel Oswaldo Yépez González.

Extensión propia de pyRevit para revisar información obligatoria en modelos de Revit 2024 y 2025. Incluye código fuente editable. No requiere un servicio de IA ni acceso a internet durante la revisión.

## Estado

Las reglas independientes del modelo se verificaron con pruebas automatizadas. El código se preparó para IronPython 2.7 y las APIs de Revit 2024/2025, pero **todavía no se ha ejecutado en Revit**. La compatibilidad efectiva y la interfaz deben validarse en ambas versiones antes de usarlo como herramienta habitual.

## Instalación manual en Windows

1. Instala una distribución oficial de pyRevit que admita tu versión de Revit. Descargas: https://github.com/pyrevitlabs/pyRevit/releases
2. Descomprime este ZIP en una carpeta permanente, por ejemplo `C:\MrBIM\MrBIM_Tools_0.1.0`.
3. Comprueba que dentro esté la carpeta `MrBIM.extension`, que contiene `lib` y `MrBIM.tab`.
4. En Revit abre **pyRevit → Settings → Custom Extension Directories**. Agrega la carpeta PADRE de `MrBIM.extension`: en el ejemplo, `C:\MrBIM\MrBIM_Tools_0.1.0`.
5. Guarda la configuración y recarga pyRevit o reinicia Revit. Debe aparecer la pestaña **MrBIM**, panel **QAQC**.
6. Repite la comprobación en Revit 2024 y Revit 2025. Si pyRevit no aparece en alguna versión, comprueba su instalación/asociación a esa versión antes de cargar MrBIM.

Esta entrega no incluye instalador EXE ni sustituye la instalación de pyRevit. Los comandos están escritos para su motor IronPython 2.7.

## Primer uso

1. Abre un proyecto, no una familia.
2. Pulsa **MrBIM → QAQC → Revisar parámetros**.
3. Elige selección actual, vista activa o proyecto completo.
4. Selecciona las categorías de modelo que quieras revisar.
5. Escribe los nombres exactos de los parámetros, separados por punto y coma. Ejemplo: `Código; Clasificación; Zona`. Escribe los nombres reales del proyecto; los nombres dependen de su configuración e idioma. No es necesario que el parámetro exista para poder exigirlo.
6. Elige buscar solo en instancia o buscar en tipo únicamente cuando el parámetro no exista en instancia.
7. Consulta los hallazgos. Los identificadores del informe son enlaces para seleccionar/localizar elementos mediante pyRevit; su visibilidad depende de la vista.
8. Opcionalmente guarda el CSV y selecciona todos los elementos afectados.

La herramienta no edita el modelo. Cambia la selección si lo solicitas y escribe el CSV únicamente en la ubicación que elijas.

## Interpretación

| Estado | Significado |
|---|---|
| VACIO | Sin valor, texto vacío o formado por espacios, ID -1 o número no finito. |
| NO_EXISTE | No se encontró el parámetro en el ámbito de búsqueda elegido. |
| AMBIGUO | Más de un parámetro tiene exactamente ese nombre en el mismo ámbito. Debe resolverse la ambigüedad antes de aprobar el dato. |
| NO_EVALUABLE | El tipo de almacenamiento no admite una comprobación genérica de presencia. |
| ERROR_LECTURA | La API no permitió leerlo; el detalle se incluye en el CSV. |

Los valores numéricos 0 y el valor No de un parámetro Sí/No son válidos. Solo se comprueba presencia, no que el contenido sea correcto. Si un parámetro existe en instancia pero está vacío, se informa VACIO y no se oculta el problema buscando un valor en tipo.

## Alcance y límites

- Consulta el modelo anfitrión; no recorre los documentos vinculados.
- Solo incluye instancias de categorías de modelo. No audita tipos que no estén utilizados, anotaciones ni familias abiertas como documento.
- Vista activa usa el colector de la API de Revit para esa vista: no es una comprobación visual de píxeles ni de oclusión.
- Nombres duplicados se reportan; esta versión no permite exigir por GUID o BuiltInParameter.
- Muestra hasta 200 hallazgos en pantalla; el CSV contiene todos.
- El CSV usa UTF-8 con BOM y separador punto y coma. Si Excel no lo separa automáticamente, importa el archivo indicando ese delimitador. Incluye ElementId, UniqueId, contexto y errores. Los textos susceptibles de interpretarse como fórmulas reciben un apóstrofo inicial por seguridad.
- Una revisión cancelada no genera un informe parcial.
- El botón Acerca de muestra versión y límites del prototipo.
- No incluye revisión ISO 19650 automática, etiquetado, correcciones, IA ni MCP. Esas funciones pueden incorporarse en siguientes versiones con reglas definidas por proyecto.

## Prueba de aceptación en Revit 2024 y 2025

Usar un modelo de prueba en cada versión y registrar resultado:

1. Crear/usar un parámetro de texto de instancia en tres elementos: uno con texto, otro vacío y otro con espacios. Esperado: dos VACIO.
2. Exigir un nombre que no exista. Esperado: NO_EXISTE en cada elemento revisado.
3. Usar un parámetro numérico con 0 y uno Sí/No con No. Esperado: ambos OK si tienen valor asignado.
4. Revisar un parámetro exclusivo de tipo con ambas políticas: NO_EXISTE en solo instancia y resultado del tipo con búsqueda alternativa.
5. Si existen dos parámetros con el mismo nombre en un elemento, esperar AMBIGUO.
6. Comprobar los tres alcances y selección por categorías. Los elementos dentro de vínculos no deben aparecer.
7. Abrir CSV con tildes, comillas y nombres con punto y coma. Verificar columnas, conteos y selección de IDs.
8. Verificar que no se genera una transacción ni se modifican valores o geometría. Cancelar durante la revisión y comprobar que no presenta resultados parciales.

## Código y autoría

Los archivos `.py` de esta entrega son el código fuente de MrBIM Tools. No se redistribuye código de pyRevit ni el SDK de Autodesk. Estos componentes se instalan por separado y conservan sus respectivas licencias y marcas. Esta entrega no es una cesión contractual ni una licencia comercial definitiva.

## Referencias técnicas

- Estructura de extensiones: https://docs.pyrevitlabs.io/extensions/
- Instalación de extensiones: https://pyrevitlabs.notion.site/Install-Extensions-0753ab78c0ce46149f962acc50892491
- Formularios: https://docs.pyrevitlabs.io/reference/pyrevit/forms/
- Salida y enlaces a elementos: https://docs.pyrevitlabs.io/reference/pyrevit/output/

Preparado el 17 de noviembre de 2026.
