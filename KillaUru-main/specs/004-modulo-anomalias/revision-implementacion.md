# Revisión de implementación 004 — 2026-10-26

**Implementación funcional local terminada; aceptación completa pendiente (56/58 tareas).** Se entregan dominio, procesamiento, caducidad, purgas, fachada de lectura, administración y exportación. T053 no alcanza el rendimiento requerido y T054 necesita la recepción/cola de 005. No se declara el spec cerrado al 100 % ni desplegado.

## Evidencia ejecutada

| Verificación | Resultado |
|---|---|
| Unitarias de Anomalías | 69/69 aprobadas |
| Cobertura del dominio | 264/265 líneas, **99,62 %**, superior al 90 % requerido |
| Regresión de Shared / Auditoría / Identidad | 62/62, 22/22 y 45/45 aprobadas |
| Arquitectura | 378/378 aprobadas: capas, visibilidad, dependencias, comentarios y límites de legibilidad |
| Regresión completa de integración | 173 aprobadas, 0 fallidas y 1 explícita omitida (carga de Identidad) |
| Casos adicionales de instantánea, lecturas y administración | 3/3 aprobados después de la regresión completa |
| Contrato OpenAPI final | 1/1 aprobado después de corregir los campos opcionales; seis operaciones, esquemas y respuestas 429 |
| Rendimiento local Release | 300 dispositivos, ocho trabajadores: **p95 185,995 ms**, media 69,0353 ms; **no cumple ≤ 100 ms** |

Los resultados de integración corresponden a ejecuciones separadas: 176 casos distintos aprobados más uno explícito omitido; la comprobación final del contrato repite un caso de la regresión. PostgreSQL/PostGIS real mediante Testcontainers. Logs locales ignorados por Git en `backend/TestResults/`: `004-unitarias-final.log`, `004-arquitectura-final.log`, `004-regresion-integracion.log`, `004-lecturas-final.log`, `004-contrato-final.log`, `004-rendimiento-release.log` y `004-rendimiento.json`. Cobertura: `TestResults/TestResults/004-dominio.cobertura.xml`. Evidencia de pruebas inicialmente fallidas: `004-dominio-rojo.log`, `004-agregado-rojo.log` y `004-tareas-rojo.log`.

## Trazabilidad de implementación y pruebas

Rutas de pruebas relativas a `backend/tests/`; `AnT` = `KillaUru.Anomalias.Tests`, `IntAn` = `KillaUru.Integracion.Tests/Anomalias`.

| Tareas / requisitos | Implementación | Evidencia automatizada |
|---|---|---|
| T001–T007, T015–T022 / FR-001–004, FR-032 | Migraciones, contratos, corredor e índice espacial, políticas puras | `AnT/Domain/CorredorViaTests`, `CorredorRealTests`, `CalidadTests`; `IntAn/CorredorMigracionTests`, `EsquemaAnomaliasTests` |
| T008–T014 / FR-001–020 | Calidad, plausibilidad, confirmación, persistencia, agregado y moderación | `AnT/Domain/CalidadTests`, `PoliticasTests`, `AnomaliaTests`, `ModeracionTablaTests`, `CicloVidaTests` |
| T023–T032 / FR-005–015, FR-030–034 | Casos de uso, repositorios PostGIS, transacción e instrumentación | `AnT/Application/ProcesamientoUseCaseTests`; `IntAn/ProcesamientoTests`, `RepositorioEspacialTests`, `ConcurrenciaAdicionalTests`, `DisponibilidadCorredorTests`; `ReglasAnomaliasTests` |
| T033–T040, T056, T058 / FR-018, FR-021–024 | Caducidad y purgas por lotes de 5000; tareas con reloj inyectado | `AnT/Application/CaducidadTests`, `TareasProgramadasTests`; `IntAn/MantenimientoTests`, `RetencionYBloqueosTests`, `EsquemaAnomaliasTests` |
| T041–T043 / FR-029, FR-032 | Fachada de lectura, filtros espaciales, cambios y resumen local UTC−5 | `IntAn/LecturasTests`, `MantenimientoTests`, `DisponibilidadCorredorTests` |
| T044–T047, T057 / FR-025–027, FR-030 | Cinco casos de uso y endpoints de administración, auditoría transaccional | `IntAn/ModeracionApiTests` (20 casos), `ListasAdministracionTests`, `SeguridadAnomaliasTests`, `AtomicidadAuditoriaTests`, `ConcurrenciaAdicionalTests` |
| T048–T050 / FR-028 | Estrategias CSV/GeoJSON, lectura consistente y auditoría previa | `AnT/Application/ExportacionTests`; `IntAn/ExportacionApiTests`, `InstantaneaExportacionTests`, `AtomicidadAuditoriaTests` |
| T051, T052, T055 | README, OpenAPI y checklist actualizados | `Contratos/ContratoAnomaliasTests`, revisión documental |
| T053 / SC-001 | Histograma de duración, bloqueos enviados en un lote ordenado | `IntAn/RendimientoAnomaliasTests`; medición registrada, umbral **pendiente** |
| T054 / SC-003–004 | Defensas ante ataque, subida legítima de 5 h y lluvia de 600 eventos | `IntAn/IntegridadDatoTests`: tres escenarios por servicio; HTTP/cola **pendiente de 005** |

## Coherencia de diseño

El dominio depende únicamente de BCL y `Shared.Domain`. Application adapta los DTO de Contracts. Los puertos separan procesamiento, lecturas, mantenimiento y exportación (ISP); CSV y GeoJSON implementan una estrategia común. Cada tipo tiene su archivo y comentarios en español sobre su responsabilidad; los contratos públicos documentan su uso. Las pruebas de arquitectura comprueban los límites de tamaño y separación de capas.

El procesamiento bloquea evento, seudónimo y celdas en orden estable; revalida dentro de la transacción y publica después del commit. No audita descartes ni emite métricas de riesgo. Moderación comparte transacción con Auditoría: un fallo revierte estado e historial. La exportación abre una instantánea `RepeatableRead`, cuenta y lee las mismas filas; confirma su auditoría de lectura antes de escribir al cliente y transmite por filas sin acumular el dataset. Los filtros y catálogos HTTP están documentados.

Caducidad ejecuta `FOR UPDATE SKIP LOCKED` y registra la fecha efectiva del vencimiento. Las tres tareas corren al arrancar y luego a `:05`, `:10` y 03:30 Lima. La purga horaria anticipa una hora su corte para asegurar que el seudónimo no supere 72 h, y redondea la recepción en la misma sentencia. La retención histórica conserva las descartadas con actividad reciente. Las pruebas incluyen bloqueo concurrente, límites de retención y ausencia de vínculos con cuentas.

## Geometría y límites


Se sustituyó la estimación de 333 km por la longitud del eje OSM: **335.0729634613575 km**. Inicio en el enlace PE-1S y fin en el nodo seleccionado de Ayacucho. No se alteraron vértices para ajustar una cifra. El demo mantiene el inicio funcional km 268 y termina en la longitud completa: **67.0729634613575 km**. El km 268 no se presenta como centro de Vinchos; las etiquetas del prototipo no son progresivas verificadas.

Fuente, licencia, snapshot, coordenadas, SHA-256 y reproducción: [datos del corredor](../../backend/db/datos/README.md). Se guarda la respuesta original comprimida, el GeoJSON canónico, V0005 y un CSV de 20 controles para backend/Android. Radio común 6 371 008,8 m. El total de tramos de cobertura es **336**, no una longitud decimal. RN-08, glosario, sus espejos y specs dependientes incorporan la corrección autorizada. El polígono demo de margen 200 m y ≤ 60 vértices sigue pendiente en **006 T002**.

## Pendientes de aceptación y dependencias

- **T053 / CHK027:** el p95 local Release de 185,995 ms incumple el objetivo de 100 ms. La medición usa 300 seudónimos, ocho trabajadores y reloj real; no incluye la cola 005. La prueba guarda resultados sin convertir el umbral en una aserción aprobada. El envío conjunto de bloqueos reduce viajes a la base de datos, pero no demostró cumplir el objetivo. Hace falta perfilar la contención/consultas y repetir la carga integrada en un entorno identificado; no se redujo la exigencia.
- **T054 / CHK033:** completar EQ-12 contra HTTP, cola y reintentos cuando exista 005. CHK025/CHK029 permanecen abiertos por recuperación e idempotencia de extremo a extremo, aunque el procesamiento local concurrente está probado.
- **006 T002:** construir el polígono demo desde la geometría real. Las fachadas para 006/007 están implementadas; sus consumidores todavía no.
- **001:** se conservan los pendientes operativos de su revisión, incluida telemetría sin datos personales y despliegue de SQL/imagen del mismo SHA. No se ejecutó despliegue.
- **002 T038:** partes de Identidad y Anomalías verificadas; permanece abierta la revisión de lecturas de Mapa/Dashboard cuando se implementen.

El checklist distingue la implementación verificada de estas condiciones pendientes. No se marcaron T053/T054 por el simple hecho de ejecutar una prueba.
