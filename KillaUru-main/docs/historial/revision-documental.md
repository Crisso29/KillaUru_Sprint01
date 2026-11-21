# Revisión documental y de pendientes — 2026-11-15

> Registro histórico; conserva los resultados de su fecha. [Volver al índice de documentación](../README.md).

Objetivo: que la documentación describa el árbol de trabajo y permita cerrar las specs
con evidencia. Base: `5e5e86b` más cambios locales, incluidos los de organización anteriores.
[Plan de cierre](../guias/cierre-y-despliegue.md) · [Estado](../seguimiento/estado-implementacion.md)

## Qué se contrastó

- `tasks.md` y trazabilidad de las diez specs, ubicación de tests y casos omitidos.
- Controladores y configuración de Ingesta, Mapa y Dashboard frente a sus README.
- Wrapper/catálogo/build Android y workflow frente a requisitos locales documentados.
- Scripts npm, Playwright, scripts de cabeceras/presupuesto y workflows del panel.
- Parser/despacho del CLI ML y workflow de publicación.
- Workflows CD, inventario de secretos, runbooks y referencias a rutas trasladadas.
- Revisiones históricas frente al estado vigente: conservadas con fecha y alcance explícitos.

La inspección comprueba existencia y comportamiento visible en fuentes, no garantiza
todos los criterios funcionales ni reemplaza ejecutar las suites y los escenarios reales.

## Correcciones verificables

| Documento o grupo | Desfase encontrado | Corrección y evidencia |
|---|---|---|
| README Ingesta | “Sin endpoints”, rutas /lotes y auditoría de ingesta | Ruta /api/v1/anomalies/batch, recepción durable y métricas según SyncController/casos de uso |
| README Mapa | “Sin implementación”, /mapa/anomalias y /mapa/resumen-dia | /api/v1/anomalies, alcance/caché/cambios y pruebas pendientes según AnomaliasController |
| README Dashboard | Comandos antiguos, fórmulas/horarios insuficientes | Parámetros, cálculo, ejemplo y ejecución con --project |
| README ML / quickstart | CLI planeado presentado como ejecutable | Tabla de despacho real; puerta y todo real devuelven 2; publicar_r2.sh ausente |
| Android README / quickstart / implementación | JDK de ejecución y versiones mezcladas | Java 21 en CI, destino JVM 17; Gradle 9.7.1, AGP 9.3.3 y catálogo Kotlin 2.4.20 |
| Panel README / quickstart | Variable Turnstile mal escrita, script size inexistente y despliegue directo | SITEKEY; verificar-presupuesto.mjs; workflow_run tras Panel CI del SHA |
| Cabeceras panel | Comprobación integral atribuida al script | El script comprueba presencia de cinco; valores y alcance restante siguen pendientes |
| Secretos y runbook | Nombres KU_* no consumidos por cd.yml y main supuestamente protegida | Nombres efectivos documentados; protección/configuración pendientes de prueba remota |
| Contratos 001 | Descubrimiento completo y esquemas atribuidos a ContratoOpenApiTests | Inventario limitado a cuatro contratos, sin propiedades requeridas; T042 reabierta |
| Logs 001 | Captura completa atribuida al filtro sintético | Diferenciar FiltroDatosPersonalesTests de la captura real exigida; T067 reabierta |
| Tareas/checklists | Marcas de cierre con tareas requeridas todavía abiertas | Reapertura de trazabilidad según referencias explícitas y rangos de tareas |
| Rutas de suites | Referencias a carpetas antiguas | Rutas actuales por spec/propósito; destinos futuros identificados como pendientes |
| Revisiones anteriores | Hallazgos/resultados históricos confundidos con estado actual | Avisos de vigencia y enlace a cierre.md; no reescribir resultados pasados |

Fuentes principales:
[ContratoOpenApiTests](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/contratos/ContratoOpenApiTests.cs),
[FiltroDatosPersonalesTests](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/seguridad/FiltroDatosPersonalesTests.cs),
[CLI ML](../../ml/killauru_ml/__main__.py),
[Playwright](../../panel/playwright.config.ts),
[cabeceras](../../panel/scripts/verificar-cabeceras.mjs).

## Cambios de seguimiento

Entregables documentales completados y revisados:

| Spec/tarea | Entregable |
|---|---|
| 001/T055 | Los seis módulos tienen instrucciones de prueba y alcance |
| 005/T033 | README: flujo, lote, parámetros, horarios y conciliación pendiente |
| 006/T028 | README: roles, caché, ETag, cambios y ejemplos |
| 007/T021 | README: fórmulas, horarios, conteos y comandos |
| 009/T072 | README de entorno, etapas reales, datos, privacidad y publicación pendiente |

Reabiertas: **001/T042 y 001/T067**, por alcance de pruebas incompleto.
No se marcaron completadas tareas de campo, seguridad desplegada, carga, CI remoto o publicación.
Las casillas de trazabilidad que dependen de tareas abiertas se reabrieron; esto no borra código existente.

Resultado: **524/637 marcadas, 113 pendientes**. Contar tareas no mide calidad ni cobertura.

## Cómo usar los documentos desde ahora

- `spec.md` define aceptación; `plan.md` y `research.md` explican decisiones.
- `tasks.md` es la única lista de casillas de implementación.
- `pruebas.md` permite encontrar y ejecutar las suites.
- `cierre.md` resume evidencia y pendientes locales/desplegados, enlazando las tareas.
- `docs/guias/cierre-y-despliegue.md` coordina dependencias y promoción.
- Al cerrar o reabrir una tarea, actualizar el resumen de cierre y el estado global en el mismo cambio.

Los documentos académicos de `docs/` en la raíz y la copia `referencia/` describen
requisitos/arquitectura objetivo, no un inventario de pruebas aprobadas. No se cambiaron
reglas de dominio para acomodarlas a limitaciones de implementación.
La diferencia entre los nombres de secretos del diseño y los del workflow queda
expresamente registrada en 001 para conciliarla antes del cierre.

## Límites de esta revisión

No hubo despliegue, modificación de servicios externos ni consulta de configuración remota.
No se certifican precios/planes comerciales, capacidad del borde, entrega de correo o disponibilidad.
Los resultados de tests anteriores se conservan fechados; estos cambios documentales
se verifican por coherencia de conteos, inventario, referencias y comandos.
El cierre funcional exhaustivo exige el trabajo secuencial descrito en cada spec.

## Comprobaciones documentales realizadas

- 158 documentos incluidos en la comprobación de enlaces; 464 destinos locales válidos.
- Los diez inventarios de `cierre.md` coinciden con las tareas abiertas de su spec.
- Conteos por spec y total global conciliados: 524/637 marcadas, 113 pendientes.
- Sin identificadores de tarea duplicados en las listas revisadas.
- Comandos npm y versiones de herramientas contrastados con configuración local;
  no se ejecutaron instalaciones, despliegues ni servicios externos para esta revisión.
