# Research: Módulo Anomalías

## R-001 · `ContextoReporte` necesita la hora de recepción — hueco en docs

- **Problema**: docs define `ContextoReporte = (ConSesion, EventosEnLaHora)` y el esqueleto de `ProcesarReporteUseCase` revalida con `reloj.GetUtcNow()`. Pero RN-11 dice «antigüedad = hora de **recepción** − hora de detección», RN-09 purga a las 72 h **desde que se recibe** y `reporte_anomalia.recibido_utc` necesita ese dato. Si el procesamiento va atrasado (cola llena, reinicio), usar la hora actual descartaría eventos válidos y adelantaría la purga.
- **Decision**: `ContextoReporte(bool ConSesion, int EventosEnLaHora, DateTimeOffset RecibidoUtc)`. Ingesta lo llena con `evento_recibido.recibido_utc`. `ReglasCalidad.EvaluarIntrinseca(reporte, recibidoUtc)`.
- **Pendiente para el autor**: agregar `RecibidoUtc` a `ContextoReporte` en `componentes-arquitectonicos.md` §3.1 y en Nivel3 §2.2.

## R-002 · Por qué `EventoReportado` lleva datos «crudos»

- **Decision**: `EventoReportado` (contrato) lleva tipos simples y nulables (`string? Tipo`, `int? Severidad`, `double? Confidence`, `double? Latitud`…). `ReporteAnomalia.Desde(evento, contexto)` lo convierte en value objects y devuelve `EsquemaInvalido` o `TipoDesconocido` si no puede.
- **Rationale**: RN-11 pide decir **qué** falló por evento (`rechazados[{id, motivo}]`). Si el contrato tuviera value objects, un valor inválido rompería la deserialización de todo el lote.

## R-003 · Distancia a la vía y km sin PostGIS en el dominio (RN-13, ADR-015)

- **Decision**: `CorredorVia` guarda los vértices de la polilínea con su **km acumulado** (haversine). `DistanciaMetros(p)`: para cada segmento, proyección en un plano local equirectangular centrado en el segmento (error < 0,1 % a escala de 100 m); se usa un índice por rejilla de 0,005° para revisar solo los segmentos cercanos. `KmSobreVia(p)`: km del punto proyectado (para el detalle del panel y el cálculo de cobertura).
- **Rationale**: RN-13 se evalúa en la recepción **sin BD** (ADR-005 punto 1) y el dominio no puede depender de PostGIS (regla 5).
- **Carga**: `CargadorCorredor` (Infrastructure) lee `anomalias.corredor_via` al arrancar y registra un `IHealthCheck` (`/health/ready` = 503 hasta cargarlo).
- **Alternatives considered**: NetTopologySuite en el dominio (dependencia externa en Domain: prohibido).

## R-004 · Bloqueo por celdas (ADR-005 punto 4)

- **Decision**: `TransaccionAnomalias.AbrirAsync(celdas)` abre la transacción, la une a `IContextoTransaccion` y ejecuta `SELECT pg_advisory_xact_lock(@celda)` por cada celda de `Celda.Alrededor(posicion, RadioAgrupacionMetros)` en orden ascendente. Se liberan solos con el commit o el rollback.
- **Rationale**: `diseño-interno` §5 y §6.3. El orden fijo evita interbloqueos; la idempotencia se comprueba **después** de tomar el bloqueo, así dos trabajadores con el mismo evento no compiten.

## R-005 · `MantenerDescartada` — hueco en docs

- **Decision**: cuarta acción del administrador, permitida solo desde `Descartada`, que no cambia el estado, limpia `marcas_revision`, pone `por_revisar = false` y se audita (`AnomaliaModerada` con `accion = MantenerDescartada`). No registra transición (no hay cambio de estado).
- **Fuente**: RN-07 («la reabre o la mantiene descartada, y en ese caso el contador se reinicia») y el prototipo (botones «Mantener descartada» y «Reabrir»).
- **Pendiente para el autor**: agregar la fila a la tabla «Acciones del administrador» de RN-07.

## R-006 · Historial de transiciones con actor

- **Decision**: `anomalias.transicion(anomalia_id, desde, hacia, ocurrida_utc, actor_id)`; `actor_id` = id del administrador en la moderación, `NULL` si fue el sistema. Ya se agregó la tabla en `docs/03-diseño-de-software/diseño-interno-de-modulos.md` (2026-10-20); aquí se suma la columna `actor_id` por el prototipo («20 oct 10:12 UTC · admin.operaciones» / «· sistema»).
- **Privacidad**: el actor es siempre un administrador (personal interno), nunca un conductor.

## R-007 · Vencimiento al reabrir

- **Decision**: al reabrir, `VenceUtc = momento de la reapertura + caducidad(Activa, persistencia)`.
- **Rationale**: RN-07 mide la caducidad desde la última detección; si la anomalía reabierta es vieja, la tarea horaria la expiraría de inmediato, contradiciendo que el administrador «ya la verificó». *Decisión de diseño SDD.*

## R-008 · Caducidad masiva sin cargar agregados (`diseño-interno` §3.5)

- **Decision**: una sola sentencia con CTE:
  `WITH v AS (SELECT id, estado, celda, vence_utc FROM anomalias.anomalia WHERE estado IN (1,2) AND vence_utc <= @ahora FOR UPDATE SKIP LOCKED), u AS (UPDATE anomalias.anomalia a SET estado = 3, estado_desde_utc = v.vence_utc, actualizada_utc = @ahora FROM v WHERE a.id = v.id RETURNING a.id, v.estado AS desde, v.celda, v.vence_utc) INSERT INTO anomalias.transicion (anomalia_id, desde, hacia, ocurrida_utc) SELECT id, desde, 3, vence_utc FROM u RETURNING anomalia_id;`
  y luego se publica `AnomaliaCambiada` por fila. `@ahora` sale de `TimeProvider`.
- **Rationale**: la regla (cuándo vence) vive en el dominio (`vence_utc`); la tarea solo compara una fecha. `SKIP LOCKED` evita esperar a un trabajador que está procesando esa anomalía.

## R-009 · Tareas programadas

- **Decision**: `BackgroundService` con `PeriodicTimer` sobre `TimeProvider` (probable con `FakeTimeProvider`): caducidad y purga de seudónimos cada 60 min; purga histórica a diario. Cada ejecución correcta llama `IMetricas.RegistrarTareaCorrecta("caducidad" | "purga-seudonimos" | "purga-historica")` (RNF-22: alerta si pasan 26 h).
- Las purgas borran o actualizan por lotes de 5 000 filas (`diseño-interno` §3.8) para no bloquear.

## R-010 · Exportación

- **Decision**: síncrona y en *streaming* (`IAsyncEnumerable` → escritor CSV o GeoJSON → cuerpo de la respuesta). Columnas: `id, tipo, severidad, estado, persistencia, latitud, longitud, km_via, ultima_deteccion_utc, reportes, dias_distintos_15d`. Fechas `desde`/`hasta` sobre `ultima_deteccion_utc`. El nombre del archivo lleva la fecha. Se audita **antes** de empezar a escribir (si la auditoría falla, no se exporta).
- **Rationale**: volumen de decenas de miles de anomalías: cabe en una respuesta sin colas ni archivos temporales (KISS).

## R-011 · Resumen del día para Dashboard (RF-18)

- **Decision**: `ResumenDelDiaAsync(DateOnly diaLocal)` calcula, con el día local UTC−5:
  - `EventosRecibidos`: reportes con `recibido_utc` en el día;
  - `DispositivosDistintos`: seudónimos distintos no nulos entre esos reportes (se calcula a las 00:30 del día siguiente, antes de la purga de 72 h);
  - `TramosConReportes7d` y `TramosTotales`: km enteros de la vía (`KmSobreVia`) con ≥ 1 reporte en los 7 días que terminan ese día, sobre 333;
  - `Confirmaciones` (`Pendiente→Activa`), `PendientesExpiradas` (`Pendiente→Expirada`), `Expiraciones` (`→Expirada`) y `Renacimientos` (`Expirada→Activa|Pendiente`) del día, desde `transicion`.
- Para la cobertura, cada reporte guarda `km_via` al procesarse (columna nueva, calculada una vez con `CorredorVia`), así el conteo es un `COUNT(DISTINCT floor(km_via))`.

## R-012 · `CeldasDelCorredor` para Consulta de Mapa (agregado el 2026-10-21)

- **Problema**: Mapa ajusta el `bbox` a celdas (~1 000 en el corredor, `diseño-interno` §4), pero el `bbox` del corredor completo abarca ~150 000 celdas de rejilla; Mapa necesita saber cuáles tocan la vía, y la polilínea es interna de Anomalías.
- **Decision**: `IAnomaliaLectura.CeldasDelCorredor()` devuelve las celdas que toca la franja de `DistanciaMaximaViaMetros` alrededor de la polilínea, calculadas una vez al cargar `CorredorVia`. Como toda anomalía nace a ≤ 50 m de la vía (RN-13), su celda está siempre en ese conjunto.
- **Pendiente para el autor**: agregar la operación a `componentes-arquitectonicos.md` §3.1 y a Nivel3 §2.2.

## Pendientes de confirmar por el autor

1. R-001 `RecibidoUtc` en `ContextoReporte`.
2. R-005 acción `MantenerDescartada`.
3. R-006 columna `actor_id` en `transicion` y R-011 columna `km_via` en `reporte_anomalia`.
4. R-007 vencimiento al reabrir.
5. R-012 operación `CeldasDelCorredor` en `IAnomaliaLectura` (la necesita 006).
