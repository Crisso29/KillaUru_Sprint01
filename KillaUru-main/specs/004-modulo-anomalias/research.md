# Research: Módulo Anomalías

## R-001 · `ContextoReporte` necesita la hora de recepción — hueco en docs

- **Problema**: docs define `ContextoReporte = (ConSesion, EventosEnLaHora)` y el esqueleto de `ProcesarReporteUseCase` revalida con `reloj.GetUtcNow()`. Pero RN-11 dice «antigüedad = hora de **recepción** − hora de detección», RN-09 purga a las 72 h **desde que se recibe** y `reporte_anomalia.recibido_utc` necesita ese dato. Si el procesamiento va atrasado (cola llena, reinicio), usar la hora actual descartaría eventos válidos y adelantaría la purga.
- **Decision**: `ContextoReporte(bool ConSesion, int EventosEnLaHora, DateTimeOffset RecibidoUtc)`. Ingesta lo llena con `evento_recibido.recibido_utc`. `ReglasCalidad.EvaluarIntrinseca(reporte, recibidoUtc)`.
- **Confirmado por el autor el 2026-10-21 (D1)**: se agrega también en docs (`componentes` §3.1, Nivel3 §2.2, Nivel4, `enfoque` §6, `diseño-interno` §6).

## R-002 · Por qué `EventoReportado` lleva datos «crudos»

- **Decision**: `EventoReportado` (contrato) lleva tipos simples y nulables (`string? Tipo`, `int? Severidad`, `double? Confidence`, `double? Latitud`…). `ReporteAnomalia.Desde(evento, contexto)` lo convierte en value objects y devuelve `EsquemaInvalido` o `TipoDesconocido` si no puede.
- **Rationale**: RN-11 pide decir **qué** falló por evento (`rechazados[{id, indice, motivo}]`). Si el contrato tuviera value objects, un valor inválido rompería la deserialización de todo el lote. Application copia los datos a `ReporteEntrada`/`ContextoDeteccion` de Domain y mapea los resultados. Domain conserva sus propios catálogos internos; no referencia Contracts, conforme a la tabla de enfoque §3. Corrección de implementación del 2026-10-24: el plan original contradecía esa tabla.

## R-003 · Distancia a la vía y km sin PostGIS en el dominio (RN-13, ADR-015)

- **Decision**: `CorredorVia` guarda los vértices de la polilínea con su **km acumulado** (haversine). `DistanciaMetros(p)`: para cada segmento, proyección en un plano local equirectangular centrado en el segmento (error < 0,1 % a escala de 100 m); se usa un índice por rejilla de 0,005° para revisar solo los segmentos cercanos. `KmSobreVia(p)`: km del punto proyectado (para `anomalia.km_via`, `reporte_anomalia.km_via` y la cobertura).
- **Rationale**: RN-13 se evalúa en la recepción **sin BD** (ADR-005 punto 1) y el dominio no puede depender de PostGIS (regla 5).
- **Carga**: `CargadorCorredor` (Infrastructure) lee `anomalias.corredor_via` al arrancar y registra la comprobación `corredor` de `/health/ready` (503 hasta cargarlo) por el punto de extensión de 001 FR-019; 001 FR-018 la enumera (T-OPS19).
- **Alternatives considered**: NetTopologySuite en el dominio (dependencia externa en Domain: prohibido).

## R-004 · Bloqueo por celdas y lectura bloqueada (ADR-005 punto 4, T-BD02)

- **Decision**: `TransaccionAnomalias.AbrirAsync(celdas)` abre la transacción, la une a `IContextoTransaccion` y ejecuta `SELECT pg_advisory_xact_lock(@celda)` por cada celda de `Celda.Alrededor(posicion, RadioAgrupacionMetros)` en orden ascendente. Se liberan solos con el commit o el rollback.
- **Lectura bloqueada (T-BD02)**: dentro de su bloqueo de celda, el trabajador lee la anomalía con `SELECT … FOR UPDATE` (`MasCercanaAsync … ORDER BY distancia LIMIT 1 FOR UPDATE`) y la moderación con `ObtenerParaActualizarAsync(id)` (`… WHERE id = @id FOR UPDATE`). Ambos **revalidan** el estado leído (en `READ COMMITTED`, `FOR UPDATE` devuelve la última versión confirmada) antes de aplicar la regla. Así la caducidad no puede pisar a un trabajador ni a una moderación, ni al revés.
- **Rationale**: `diseño-interno` §5 y §6.3. El orden fijo evita interbloqueos; la idempotencia se comprueba **después** de tomar el bloqueo, así dos trabajadores con el mismo evento no compiten.
- **Alternatives considered**: token de concurrencia `xmin` con reintentos (BD-02 opción b): más código y reintentos en el camino caliente.

## R-005 · `MantenerDescartada` — hueco en docs

- **Decision**: cuarta acción del administrador, permitida solo desde `Descartada` con `por_revisar = true` (RN-07), que no cambia el estado, limpia `marcas_revision`, pone `por_revisar = false` y se audita (`AnomaliaModerada` con `accion = MantenerDescartada`). No registra transición (no hay cambio de estado).
- **Fuente**: RN-07 («la reabre o la mantiene descartada, y en ese caso el contador se reinicia») y el prototipo (botones «Mantener descartada» y «Reabrir»).
- **Confirmado por el autor el 2026-10-21 (D1)**: fila nueva en la tabla «Acciones del administrador» de RN-07 (patrones §2.1: 4 acciones).

## R-006 · Historial de transiciones con actor

- **Decision**: `anomalias.transicion(anomalia_id, desde, hacia, ocurrida_utc, actor_id)`; `actor_id` = id del administrador en la moderación, `NULL` si fue el sistema, por el prototipo («20 oct 10:12 UTC · admin.operaciones» / «· sistema»).
- **Privacidad**: el actor es siempre un administrador (personal interno), nunca un conductor.
- **Confirmado por el autor el 2026-10-21 (D1)**: columna `transicion.actor_id` en `diseño-interno` §3.3.

## R-007 · Vencimiento al reabrir

- **Decision**: al reabrir, `VenceUtc = momento de la reapertura + caducidad(Activa, persistencia)`.
- **Rationale**: RN-07 mide la caducidad desde la última detección; si la anomalía reabierta es vieja, la tarea horaria la expiraría de inmediato, contradiciendo que el administrador «ya la verificó».
- **Confirmado por el autor el 2026-10-21 (D1)**: RN-07 lo dice en docs.

## R-008 · Caducidad masiva sin cargar agregados (`diseño-interno` §3.5)

- **Decision**: una sola sentencia con CTE:
  `WITH v AS (SELECT id, estado, celda, vence_utc FROM anomalias.anomalia WHERE estado IN (1,2) AND vence_utc <= @ahora FOR UPDATE SKIP LOCKED), u AS (UPDATE anomalias.anomalia a SET estado = 3, estado_desde_utc = v.vence_utc, actualizada_utc = @ahora FROM v WHERE a.id = v.id RETURNING a.id, v.estado AS desde, v.celda, v.vence_utc) INSERT INTO anomalias.transicion (anomalia_id, desde, hacia, ocurrida_utc) SELECT id, desde, 3, vence_utc FROM u RETURNING anomalia_id;`
  y luego se publica `AnomaliaCambiada` por fila. `@ahora` sale de `TimeProvider`.
- **Rationale**: la regla (cuándo vence) vive en el dominio (`vence_utc`); la tarea solo compara una fecha. `SKIP LOCKED` salta las filas que un trabajador o una moderación tienen bloqueadas con `FOR UPDATE` (R-004); esas se revisan en la siguiente hora, y como el trabajador ya actualizó `vence_utc`, no hay actualización perdida (T-BD02).

## R-009 · Tareas programadas por reloj (T-OPS05)

- **Decision**: `BackgroundService` que corre **al arrancar** y luego espera el próximo instante con `Shared/Tareas/ProgramaReloj` (sobre `TimeProvider`, probable con `FakeTimeProvider`): caducidad cada hora en el minuto `:05`; purga de seudónimos cada hora en el minuto `:10`; purga histórica a diario a las 03:30 hora local (UTC−5). Cada ejecución correcta llama `IMetricas.RegistrarTareaCorrecta("caducidad" | "purga-seudonimos" | "purga-historica")` (RNF-22: alerta si pasan 26 h).
- **Rationale**: un `PeriodicTimer` de 24 h dispara 24 h después del arranque; con despliegues más seguidos la purga diaria no correría nunca. Correr al arrancar también publica el *gauge* tras cada reinicio.
- Las purgas borran o actualizan por lotes de 5 000 filas (`diseño-interno` §3.8) para no bloquear.

## R-010 · Exportación

- **Decision**: síncrona y en *streaming* (`IAsyncEnumerable` → escritor CSV o GeoJSON → cuerpo de la respuesta). Columnas (iguales al prototipo, T-PO11): `id, tipo, severidad, estado, persistencia, km, latitud, longitud, reportes, primer_reporte, ultimo_reporte` (`severidad` = mostrada; `km` = `anomalia.km_via`; `primer_reporte` / `ultimo_reporte` = mínima `detectado_utc` de sus reportes / `ultima_deteccion_utc`). En GeoJSON, las mismas columnas son las `properties` de cada *feature* (`FilaExportacion` del contrato) y la geometría es el punto. Fechas `desde`/`hasta` sobre `ultima_deteccion_utc`. El nombre del archivo lleva la fecha.
- **Orden (D15)**: 1) `IExportacionRepository.ContarAsync(filtro)` con los mismos filtros; 2) `IAuditoriaService.RegistrarLecturaAsync(DatasetExportado, actor, objetivo, contexto {formato, filtros, filas})` (transacción propia: es una lectura sensible); 3) `LeerAsync(filtro)` y escribir. Si la auditoría falla, no se escribe nada.
- **Rationale**: volumen de decenas de miles de anomalías: cabe en una respuesta sin colas ni archivos temporales (KISS). Contar primero da `filas` exacto al registro, como esperan 002 y el prototipo (PO-12).

## R-011 · Resumen del día para Dashboard (RF-18)

- **Decision**: `ResumenDelDiaAsync(DateOnly diaLocal)` calcula, con el día local UTC−5:
  - `EventosRecibidos`: **eventos válidos** = reportes procesados (filas de `reporte_anomalia`) con `recibido_utc` en el día (D14);
  - `DispositivosDistintos`: seudónimos distintos no nulos entre esos reportes (se calcula a las 00:30 del día siguiente, antes de la purga de 72 h);
  - `TramosConReportes7d` y `TramosTotales`: km enteros de la vía (`km_via`) con ≥ 1 reporte en los 7 días que terminan ese día, sobre los tramos de 1 km de la polilínea cargada (`ceil(longitud en km)` = 336 con la traza actual);
  - `Confirmaciones` (`Pendiente→Activa`), `PendientesExpiradas` (`Pendiente→Expirada`), `Expiraciones` (`→Expirada`) y `Renacimientos` = **reapariciones**: `Expirada → Activa | PendienteConfirmacion` (definición del glosario, D14) del día, desde `transicion`.
- Para la cobertura, cada reporte guarda `km_via` al procesarse (calculado una vez con `CorredorVia`), así el conteo es un `COUNT(DISTINCT floor(km_via))`.
- **Efecto de la purga de 72 h (D3)**: `recibido_utc` redondeado hacia abajo a la hora no cambia el día local de ningún reporte (el día UTC−5 empieza a las 05:00 UTC, una hora en punto), así que recalcular un día viejo da el mismo `EventosRecibidos`.
- **Confirmado por el autor el 2026-10-21 (D1)**: `reporte_anomalia.km_via` y las fórmulas pasan a `diseño-interno` §3.3 y §3.8.

## R-012 · `CeldasDelCorredor` para Consulta de Mapa (agregado el 2026-10-21)

- **Problema**: Mapa ajusta el `bbox` a celdas (~1 000 en el corredor, `diseño-interno` §4), pero el `bbox` del corredor completo abarca ~150 000 celdas de rejilla; Mapa necesita saber cuáles tocan la vía, y la polilínea es interna de Anomalías.
- **Decision**: `IAnomaliaLectura.CeldasDelCorredor()` devuelve las celdas que toca la franja de `DistanciaMaximaViaMetros` alrededor de la polilínea, calculadas una vez al cargar `CorredorVia`. Como toda anomalía nace a ≤ 50 m de la vía (RN-13), su celda está siempre en ese conjunto.
- **Confirmado por el autor el 2026-10-21 (D1)**: la operación está en `componentes` §3.1, Nivel3 y `diseño-interno` §4.

## R-013 · `km_via` de la anomalía, fijo al nacer (D13)

- **Decision**: columna `anomalia.km_via real NOT NULL`, calculada **al nacer** con `CorredorVia.KmSobreVia(posicion)`; como la posición es fija (RN-05), nunca cambia. Es el `km_via` del reporte que la creó. `AnomaliaVisible` y `AnomaliaCambio` suman `KmVia` (el mapa lo muestra a todos los roles, 006 R-011); detalle, listas, últimos cambios y exportación lo leen de esa columna.
- **Rationale**: el prototipo muestra «km 287,4» en la app y en el panel; calcularlo en cada lectura obligaría a cargar la polilínea en Mapa.

## R-014 · `Reparada` renace con cualquier reporte posterior a la reparación (D14)

- **Decision**: en `Anomalia.Reportes`, la condición para volver a nacer es: `Expirada` y `detectado > vence_utc`; `Reparada` y `detectado > estado_desde_utc`. Un reporte de una `Reparada` detectado antes de la reparación se absorbe sin renacer.
- **Rationale**: RN-07 pone la condición «después del vencimiento» solo para `Expirada` (PO-13). Un bache reparado ayer que alguien reporta hoy es un bache nuevo.

## R-015 · `SeudonimoInconsistente` en `MotivoDescarte` (D2)

- **Decision**: `MotivoDescarte` suma `SeudonimoInconsistente = 8`. Lo evalúa **Ingesta** en la recepción: el seudónimo del lote es el de la cabecera `X-KillaUru-Seudonimo` o, si falta, el del primer evento con seudónimo válido; todo evento con otro seudónimo sale en `rechazados` con ese motivo. Anomalías no lo evalúa ni lo devuelve (`EvaluarIntrinseca` es por evento), pero el enum es suyo porque es el catálogo único de RN-11 (y `evento_recibido.motivo` usa sus códigos 1…8).

## R-016 · Tipos de la moderación en Application (D4)

- **Decision**: `Application/Administracion/` tiene `ComandoModeracion(Guid AnomaliaId, AccionModeracion Accion, Guid ActorId)`, el enum `AccionModeracion` (mismos 4 valores, el del contrato HTTP) y `ResultadoModeracion` (`Hecha(DetalleAnomalia)` · `NoEncontrada` · `NoPermitida(EstadoAnomalia actual)`). `ModerarAnomaliaUseCase` mapea `AccionModeracion → AccionAdmin` (el dominio la conserva: es su tabla de RN-07) y `Anomalia.Moderar(accion, ahora, actorId)` devuelve `bool` (permitida o no, sin excepciones). Los controladores solo ven Application (`enfoque` §3).

## R-017 · Purgas con SQL canónico (D3, D7)

- **Seudónimos (cada hora, `:10`)**, por lotes de 5 000:
  `UPDATE anomalias.reporte_anomalia SET seudonimo = NULL, recibido_utc = date_trunc('hour', recibido_utc) WHERE evento_id IN (SELECT evento_id FROM anomalias.reporte_anomalia WHERE seudonimo IS NOT NULL AND recibido_utc < @limite LIMIT 5000);` con `@limite = ahora − (PurgaSeudonimoHoras − 1) h` (con la tarea horaria, ningún seudónimo supera las 72 h) (la sesión usa `TimeZone = 'UTC'`).
- **Histórica (diaria, 03:30)**, por lotes de 5 000, con `@limite = ahora − RetencionHistoricaDias`:
  `DELETE FROM anomalias.reporte_anomalia WHERE evento_id IN (SELECT evento_id FROM anomalias.reporte_anomalia WHERE detectado_utc < @limite LIMIT 5000);`
  `DELETE FROM anomalias.anomalia WHERE id IN (SELECT id FROM anomalias.anomalia WHERE estado IN (3, 4, 5) AND estado_desde_utc < @limite AND (estado <> 5 OR ultima_deteccion_utc < @limite) LIMIT 5000);`
- **Rationale**: D3 (la hora exacta de recepción ya no une reportes de un mismo celular) y D7 (una `Descartada` se borra solo tras 2 años sin ningún reporte nuevo; `Expirada` y `Reparada` sin cambio).

## Confirmados por el autor el 2026-10-21

1. R-001 `RecibidoUtc` en `ContextoReporte` — D1.
2. R-005 acción `MantenerDescartada` — D1.
3. R-006 columna `actor_id` en `transicion` y R-011 columna `km_via` en `reporte_anomalia` — D1; `anomalia.km_via` — D13 (R-013).
4. R-007 vencimiento al reabrir — D1.
5. R-012 operación `CeldasDelCorredor` en `IAnomaliaLectura` (la necesita 006) — D1.
6. R-011 definiciones de `EventosRecibidos` (eventos válidos) y `Renacimientos` (reaparición) — D14; R-014 `Reparada` — D14.
7. R-015 `SeudonimoInconsistente` — D2; R-016 tipos de moderación — D4; R-017 purgas — D3, D7; R-010 orden de la exportación — D15.

## Ajuste geométrico autorizado — 2026-10-24

**Revisión geométrica 2026-10-24 (004)**: el eje canónico `backend/db/datos/corredor_pe28a.geojson` mide **335,072963461 km** (335,073 km al mostrarlo). El km 0 es el extremo seleccionado en el enlace PE-1S, no el centro de Pisco; el km final es el extremo seleccionado en Ayacucho. Son distancias acumuladas del eje con radio esférico 6 371 008,8 m, no progresivas oficiales. El demo conserva el inicio geométrico km 268 y termina en `properties.demo_km_fin = properties.longitud_km`: **67,072963461 km**. El nombre histórico «Ayacucho–Vinchos» no certifica que el km 268 sea el centro de Vinchos. Los cálculos y límites usan el valor completo del GeoJSON; los valores redondeados de documentos e interfaz son solo de presentación. La generación del polígono de 200 m y su validación de ≤ 60 vértices corresponden a 006; no se reemplaza ese recorte por una comprobación de kilometraje.

Referencia reproducible: `backend/db/datos/README.md`; 20 puntos compartidos en `backend/db/datos/km_referencia.csv`. Backend y Android deben usar ese mismo CSV, radio y eje. `TramosTotales = ceil(longitud_km) = 336`. El panel no debe conservar las progresivas de pueblos del prototipo sin verificarlas contra el eje.
