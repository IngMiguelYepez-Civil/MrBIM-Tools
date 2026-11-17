# Estado de validación — 0.2.0

Esta versión sustituye la arquitectura del prototipo anterior. Sus resultados de pruebas anteriores no acreditan la nueva implementación.

| Comprobación | Estado |
|---|---|
| Estructura del repositorio y lectura de proyectos XML | Verificada localmente |
| Ausencia de dependencias de extensiones externas en el código | Verificada localmente |
| Pruebas C# de reglas y CSV | Preparadas; ejecución por confirmar en Actions o con SDK .NET 8 |
| Compilación contra API Revit 2024 | Pendiente |
| Compilación contra API Revit 2025 | Pendiente |
| Scripts de compilación e instalación en Windows | Pendientes de ejecución |
| Carga y funcionamiento en Revit 2024/2025 | Pendientes |

## Aceptación por versión

Realizar en un modelo de prueba, tanto en 2024 como en 2025:

1. Confirmar que carga la pestaña MrBIM y ambos botones, sin extensiones adicionales.
2. Revisar tres textos: asignado, vacío y espacios. Esperar dos VACIO.
3. Exigir un parámetro inexistente. Esperar NO_EXISTE.
4. Comprobar cero numérico, No y referencias ElementId: -1 vacío; otras opciones negativas válidas.
5. Comparar búsqueda solo en instancia y con alternativa de tipo. Un vacío de instancia no debe ocultarse con el valor de tipo.
6. Comprobar nombres duplicados: AMBIGUO.
7. Probar selección, vista y proyecto; filtrar por categoría y comprobar exclusión de elementos internos de vínculos.
8. Probar cancelación sin informe parcial.
9. Exportar CSV con tildes, comillas, saltos de línea y textos que empiecen por signos de fórmula. Comprobar conteos y columnas.
10. Seleccionar todos los hallazgos y localizar filas. Comprobar IDs y comportamiento en una vista donde no sean visibles.
11. Confirmar que no cambia ningún dato ni geometría del modelo.
12. Instalar, actualizar con respaldo y retirar el complemento para cada versión por separado.

Las pruebas automatizadas cubren reglas sin Revit. No sustituyen esta aceptación.
