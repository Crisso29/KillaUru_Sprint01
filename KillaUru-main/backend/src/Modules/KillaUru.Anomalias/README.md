# Anomalías — spec 004

Valida y agrupa **Bache** y **Hundimiento** en el eje PE-28A. Gestiona confirmación, persistencia, caducidad, moderación, retención y exportación. No almacena cuentas de conductores; `actor_id` identifica únicamente al administrador de una transición.

## Diseño y reglas

| Regla | Implementación |
|---|---|
| RN-11: confianza, GPS y fechas respecto de la recepción original | `ReporteAnomalia`, `ReglasCalidad`, `EvaluarIntrinsecaUseCase` |
| RN-13: franja inclusiva de 50 m y kilometraje del eje | `CorredorVia`, `GeodesiaLocal`, `CargadorCorredor` |
| RN-05: vecino del mismo tipo a ≤ 100 m, posición fija y mediana hacia arriba | `AnomaliaRepositoryPostgis`, `Anomalia` |
| RN-06 / RN-14: vehículos separados ≥ 3 min, sesión para graves | `PoliticaConfirmacion` |
| RN-12: ≥ 3 días locales en 15 días | `ReglaPersistencia` |
| RN-07: cinco estados, reparación, revisión y renacimiento | `Anomalia`, `TablaModeracion`, `ReglaRenacimiento` |
| RN-15: tope solo para anomalías nuevas y velocidad con margen GPS | `PoliticaPlausibilidad`, `ProcesarReporteUseCase` |
| RN-09 / RN-10: anonimización y retención | `PurgasRepository` |

Domain solo depende de BCL y `Shared.Domain`. Application adapta Contracts y coordina puertos específicos (ISP); Infrastructure implementa PostgreSQL/PostGIS; Adapters solo usa tipos de Application. Las políticas separan reglas del agregado, los repositorios aíslan persistencia y `IEscritorExportacion` permite elegir CSV o GeoJSON sin condicionales de formato en el caso de uso.

## Procesamiento y concurrencia

`IAnomaliasService.EvaluarIntrinseca` es puro. `ProcesarReporteAsync` recibe el `RecibidoUtc` original, el indicador de sesión y el conteo por hora de detección calculado por Ingesta. Primero comprueba idempotencia; revalida calidad, plausibilidad y estado antes de escribir.

`TransaccionAnomalias` bloquea evento, seudónimo y celdas ordenadas. `NpgsqlBatch` mantiene esa secuencia con un solo envío. El vecino se lee con `FOR UPDATE`; reporte, agregado e historial se guardan juntos. `AnomaliaCambiada` sale después del commit. Descartes y tiempos son métricas técnicas; el procesamiento no escribe auditoría ni emite acciones de riesgo.

Una anomalía leve puede nacer activa; una grave o retenida por tope nace pendiente. `Activa` no regresa a pendiente. `Expirada` renace con una detección posterior al vencimiento; `Reparada`, con una posterior a la reparación. `Descartada` absorbe reportes y puede quedar `PorRevisar`, que es una bandera, no un sexto estado.

## Tareas programadas

Todas se ejecutan al arrancar, con scope por pasada, cancelación y `TimeProvider`.

| Tarea | Siguiente ejecución | Escritura |
|---|---|---|
| Caducidad | Cada hora, minuto `:05` | Estados 1/2 vencidos → 3; transición con hora del vencimiento, `SKIP LOCKED` |
| Purga de seudónimos | Cada hora, minuto `:10` | Seudónimo a NULL y recepción truncada a hora en la misma sentencia |
| Purga histórica | Diario, 03:30 Lima | Reportes anteriores a 730 días; expiradas/reparadas antiguas; descartadas solo si estado y última detección son antiguos |

La anonimización anticipa una hora el límite de 72 h para compensar la frecuencia horaria. Los lotes son de 5000 filas. Una ejecución fallida no emite `RegistrarTareaCorrecta`; los nombres son `caducidad`, `purga-seudonimos` y `purga-historica`.

## API administrativa

Las seis operaciones requieren **Administrador** y documentan 401, 403 y 429.

| Método y ruta | Resultado |
|---|---|
| `GET /api/v1/anomalies/{id}` | Detalle, días locales e historial; `venceUtc` nulo en estados finales |
| `GET /api/v1/anomalies/transiciones?limite=10` | Últimas transiciones, máximo 50 |
| `POST /api/v1/anomalies/{id}/moderacion` | `MarcarReparada`, `Descartar`, `Reabrir`, `MantenerDescartada` |
| `GET /api/v1/moderacion/por-revisar` | Descartadas que requieren revisión, con severidad |
| `GET /api/v1/moderacion/pendientes` | Pendientes ordenadas por vencimiento |
| `POST /api/v1/exportaciones` | Archivo CSV o GeoJSON |

La moderación revalida bajo bloqueo y audita `AnomaliaModerada` en la misma transacción; acción incompatible → 409 `transicion_no_permitida`. Mantener descartada requiere `PorRevisar` y limpia marcas sin inventar una transición de estado.

La exportación abre `RepeatableRead`, cuenta filas, confirma `DatasetExportado` con filtros y conteo en una transacción de auditoría independiente y después escribe el stream. Un fallo de auditoría no entrega el archivo. Las once columnas son `id,tipo,severidad,estado,persistencia,km,latitud,longitud,reportes,primer_reporte,ultimo_reporte`; nunca incluye seudónimos.

Ejemplos de cuerpos JSON:

```json
{"accion":"MarcarReparada"}
```

```json
{"formato":"CSV","desdeUtc":"2026-10-01T00:00:00Z","tipo":"Bache","estados":["Activa"]}
```

## Lecturas y geometría

`IAnomaliaLectura` entrega activas por celdas, filtros combinables, cambios estrictamente posteriores a un instante y resúmenes de días UTC−5. Solo `Expirada → Activa/PendienteConfirmacion` cuenta como renacimiento. `EventosRecibidos` cuenta reportes válidos por recepción y conserva su día después del truncado; los dispositivos distintos deben capturarse antes de anonimizar.

`anomalias.corredor_via`, fila 1, se carga una vez al arrancar. Falta de traza → `/health/ready` 503; aplicar migraciones y reiniciar para cargarla. El eje canónico y su licencia están en `backend/db/datos/README.md`: 6689 vértices, 335.0729634613575 km, 336 tramos de cobertura y 20 referencias comunes con Android. El polígono demo pertenece a 006.

## Verificación

Desde `backend`, con Docker para integración y Microsoft.Testing.Platform:

```powershell
dotnet test --project tests/KillaUru.Anomalias.Tests --no-restore
dotnet test --project tests/KillaUru.Arquitectura.Tests --no-restore
dotnet test --project tests/KillaUru.Integracion.Tests --no-restore
```

Resultados y límites de la verificación: `specs/004-modulo-anomalias/revision-implementacion.md`. La medición de 300 dispositivos del procesador no reemplaza T053 con la cola 005 ni la prueba HTTP completa de EQ-12. No se han desplegado migraciones.
