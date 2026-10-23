# Data Model: Módulo Anomalías

## 1. Tablas (`V0004__anomalias.sql`)

Base: `diseño-interno-de-modulos.md` §3.3. Lo agregado está marcado con `-- (+)` (ver `plan.md`, Complexity Tracking).

```sql
CREATE TABLE anomalias.anomalia (
    id                    uuid        PRIMARY KEY,                               -- UUIDv7
    tipo                  smallint    NOT NULL CHECK (tipo IN (1, 2)),           -- RN-01: 1 Bache · 2 Hundimiento
    estado                smallint    NOT NULL CHECK (estado BETWEEN 1 AND 5),   -- 1 Activa · 2 PendienteConfirmacion · 3 Expirada · 4 Reparada · 5 Descartada
    persistencia          smallint    NOT NULL DEFAULT 0 CHECK (persistencia IN (0, 1)),  -- 0 Episodica · 1 Persistente (RN-12)
    severidad_mostrada    smallint    NOT NULL CHECK (severidad_mostrada BETWEEN 1 AND 5),
    posicion              geography(Point, 4326) NOT NULL,                       -- fija (RN-05)
    celda                 bigint      NOT NULL,                                  -- Celda.De(posicion); nunca cambia
    km_via                real        NOT NULL,                                  -- (+) D13: CorredorVia.KmSobreVia al nacer; nunca cambia
    ultima_deteccion_utc  timestamptz NOT NULL,
    vence_utc             timestamptz NOT NULL,                                  -- calculada por el dominio (RN-07)
    estado_desde_utc      timestamptz NOT NULL,
    actualizada_utc       timestamptz NOT NULL,                                  -- updatedSince (RF-14)
    creada_por_tope       boolean     NOT NULL DEFAULT false,                    -- RN-15a
    marcas_revision       timestamptz[] NOT NULL DEFAULT '{}',                   -- RN-07 «Por revisar»
    por_revisar           boolean     NOT NULL DEFAULT false
);
CREATE INDEX ix_anomalia_posicion ON anomalias.anomalia USING gist (posicion);
CREATE INDEX ix_anomalia_mapa     ON anomalias.anomalia (celda) WHERE estado = 1;
CREATE INDEX ix_anomalia_vence    ON anomalias.anomalia (vence_utc) WHERE estado IN (1, 2);
CREATE INDEX ix_anomalia_cambios  ON anomalias.anomalia (actualizada_utc);
CREATE INDEX ix_anomalia_revision ON anomalias.anomalia (actualizada_utc) WHERE por_revisar;

CREATE TABLE anomalias.reporte_anomalia (
    evento_id      uuid        PRIMARY KEY,                                      -- idempotencia (ADR-005)
    anomalia_id    uuid        NOT NULL REFERENCES anomalias.anomalia (id) ON DELETE CASCADE,
    detectado_utc  timestamptz NOT NULL,
    recibido_utc   timestamptz NOT NULL,                                         -- llega en ContextoReporte (R-001); a las 72 h, date_trunc('hour') (D3)
    severidad      smallint    NOT NULL CHECK (severidad BETWEEN 1 AND 5),
    confidence     real        NOT NULL CHECK (confidence BETWEEN 0 AND 1),
    posicion       geography(Point, 4326) NOT NULL,
    precision_m    real        NOT NULL CHECK (precision_m >= 0),
    km_via         real        NOT NULL,                                         -- (+) R-011
    seudonimo      uuid        NULL,                                             -- NULL a las 72 h de recibido (RN-09)
    con_sesion     boolean     NOT NULL,                                         -- sí/no, nunca quién (RN-14)
    modelo_version text        NOT NULL
);
CREATE INDEX ix_reporte_ventana   ON anomalias.reporte_anomalia (anomalia_id, detectado_utc DESC);
CREATE INDEX ix_reporte_seudonimo ON anomalias.reporte_anomalia (seudonimo, detectado_utc DESC) WHERE seudonimo IS NOT NULL;
CREATE INDEX ix_reporte_purga     ON anomalias.reporte_anomalia (recibido_utc) WHERE seudonimo IS NOT NULL;
CREATE INDEX ix_reporte_historico ON anomalias.reporte_anomalia (detectado_utc);
CREATE INDEX ix_reporte_recibido  ON anomalias.reporte_anomalia (recibido_utc);                  -- (+) resumen del día

CREATE TABLE anomalias.transicion (
    id            bigint      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    anomalia_id   uuid        NOT NULL REFERENCES anomalias.anomalia (id) ON DELETE CASCADE,
    desde         smallint    NULL CHECK (desde BETWEEN 1 AND 5),                -- NULL = nació
    hacia         smallint    NOT NULL CHECK (hacia BETWEEN 1 AND 5),
    ocurrida_utc  timestamptz NOT NULL,
    actor_id      uuid        NULL,                                              -- (+) administrador; NULL = sistema (R-006)
    CHECK (desde IS DISTINCT FROM hacia)
);
CREATE INDEX ix_transicion_fecha    ON anomalias.transicion (ocurrida_utc);
CREATE INDEX ix_transicion_anomalia ON anomalias.transicion (anomalia_id, ocurrida_utc);       -- (+) historial del detalle

CREATE TABLE anomalias.corredor_via (
    id    smallint PRIMARY KEY,
    traza geography(LineString, 4326) NOT NULL                                   -- PE-28A; se carga en memoria (RN-13)
);
```

`V0005__corredor_via_datos.sql` inserta la fila 1 con la polilínea de la PE-28A (Pisco km 0 → Ayacucho km 333).

**Revisión 2026-10-21 (T-BD-CHK):** CHECK de `persistencia`, `confidence`, `precision_m`, `transicion.desde/hacia`; se quita `ix_anomalia_pendiente` porque sobra: la lista «Esperan confirmación» (estado 2, orden por `vence_utc`) la sirve `ix_anomalia_vence` (parcial sobre estados 1 y 2). Las dos FK del esquema apuntan a `anomalias.anomalia` (ninguna FK entre esquemas; la prueba es de 001).

## 2. Contratos de entrada (`Contracts/`)

| Tipo | Campos | Nota |
|---|---|---|
| `EventoReportado` | `Guid EventoId`, `DateTimeOffset? DetectadoUtc`, `double? Latitud`, `double? Longitud`, `double? PrecisionMetros`, `string? Tipo`, `int? Severidad`, `double? Confidence`, `string? ModeloVersion`, `Guid? Seudonimo` | Crudo a propósito (R-002) |
| `ContextoReporte` | `bool ConSesion`, `int EventosEnLaHora`, `DateTimeOffset RecibidoUtc` | Lo arma Ingesta (R-001) |
| `MotivoDescarte` | `EsquemaInvalido = 1`, `ConfidenceBajo = 2`, `PrecisionGpsInsuficiente = 3`, `TimestampFueraDeRango = 4`, `TipoDesconocido = 5`, `FueraDelCorredor = 6`, `VelocidadImposible = 7`, `SeudonimoInconsistente = 8` | RN-11; el 8 lo evalúa Ingesta en la recepción (D2, R-015) |
| `ResultadoProceso` | `Procesado` · `Descartado(MotivoDescarte)` · `YaProcesado` | Ingesta marca `PROCESADO` o `DESCARTADO` |

**Mapeo a `ReporteAnomalia`** (`ReporteAnomalia.Desde`): falta un campo o un valor no se puede convertir en su value object → `EsquemaInvalido`; `Tipo` no es `Bache` ni `Hundimiento` → `TipoDesconocido`. `Normal` nunca llega (la app no lo envía); si llegara → `TipoDesconocido`.

## 3. Agregado `Anomalia`

### 3.1 Estados y visibilidad (RN-07)

| Estado | Código | ¿Visible a conductores? | ¿Alerta? | ¿Puede vencer? |
|---|:---:|:---:|:---:|:---:|
| `Activa` | 1 | ✅ | ✅ | ✅ |
| `PendienteConfirmacion` | 2 | ❌ | ❌ | ✅ (72 h) |
| `Expirada` | 3 | ❌ | ❌ | — |
| `Reparada` | 4 | ❌ | ❌ | — |
| `Descartada` | 5 | ❌ | ❌ | — |

### 3.2 Transiciones

| Desde | Hacia | Disparador | Regla |
|---|---|---|---|
| (nace) | `Activa` | reporte leve sin tope | RN-06 |
| (nace) | `PendienteConfirmacion` | reporte 4–5, o creada por tope | RN-06, RN-15a |
| `PendienteConfirmacion` | `Activa` | 2.º vehículo (≥ 3 min; con sesión si es grave) | RN-06, RN-14 |
| `Activa` / `PendienteConfirmacion` | `Expirada` | tarea horaria: `vence_utc ≤ ahora` | RN-07 |
| `Expirada` | `Activa` o `PendienteConfirmacion` | reporte detectado **después** de `vence_utc` (vuelve a nacer = reaparición) | RN-06, RN-07 |
| `Reparada` | `Activa` o `PendienteConfirmacion` | reporte detectado **después** de `estado_desde_utc` (la reparación) | RN-06, RN-07, D14 |
| `Activa` / `PendienteConfirmacion` | `Reparada` | administrador: `MarcarReparada` | RN-07 |
| `Activa` / `PendienteConfirmacion` / `Expirada` | `Descartada` | administrador: `Descartar` | RN-07 |
| `Reparada` / `Descartada` | `Activa` | administrador: `Reabrir` | RN-07 |
| `Descartada` | `Descartada` (sin transición) | administrador: `MantenerDescartada` | RN-07 (R-005) |

**Una sola transición por operación** (`Anomalia.Transiciones.cs`): se compara el estado al empezar con el estado al terminar. Así el paso interno por `PendienteConfirmacion` al nacer o renacer no se registra si termina en `Activa`.

### 3.3 Vencimiento (`vence_utc`)

| Estado al terminar la operación | `vence_utc` |
|---|---|
| `Activa` episódica | `ultima_deteccion_utc + 72 h` |
| `Activa` persistente | `ultima_deteccion_utc + 7 días` |
| `PendienteConfirmacion` | `ultima_deteccion_utc + 72 h` |
| `Activa` tras `Reabrir` | `momento de la reapertura + caducidad(persistencia)` (R-007) |
| `Expirada`, `Reparada`, `Descartada` | no cambia en la BD (la tarea solo mira estados 1 y 2); la API devuelve `venceUtc = null` (T-PO08) |

### 3.4 «Por revisar» (RN-07, `diseño-interno` §6.2)

- Al llegar un reporte a una `Descartada`: si su seudónimo es distinto de todos los de la ventana **y** está a ≥ 3 min de ellos (`PoliticaConfirmacion.EsVehiculoNuevo`), se agrega su hora de detección a `marcas_revision`.
- Se quitan las marcas de más de 7 días; `por_revisar = (marcas ≥ 3)`.
- `Reabrir` y `MantenerDescartada` limpian las marcas.

## 4. Políticas del dominio (firmas)

| Clase | Firma | Regla |
|---|---|---|
| `ReglasCalidad` | `MotivoDescarte? EvaluarIntrinseca(ReporteAnomalia r, DateTimeOffset recibidoUtc)` | RN-11, RN-13 |
| `PoliticaPlausibilidad` | `MotivoDescarte? Evaluar(ReporteAnomalia r, ReporteAnomalia? anterior)` · `bool SuperaTope(int eventosEnLaHora)` (`> 120`) | RN-15 |
| `PoliticaConfirmacion` | `EstadoAnomalia Decidir(IReadOnlyList<ReporteAnomalia> ventana, Severidad mostrada, bool porTope, DateTimeOffset ultima)` · `bool EsVehiculoNuevo(ReporteAnomalia r, IReadOnlyList<ReporteAnomalia> ventana)` | RN-06, RN-14, RN-15a |
| `ReglaPersistencia` | `EstadoPersistencia Evaluar(IEnumerable<DateTimeOffset> detecciones, DateTimeOffset ahora)` | RN-12 |
| `CorredorVia` | `double DistanciaMetros(Coordenada p)` · `double KmSobreVia(Coordenada p)` | RN-13 |

## 5. Lecturas (`Contracts/Lecturas/`)

| Tipo | Campos |
|---|---|
| `AnomaliaVisible` | `Id`, `Tipo`, `Severidad`, `Persistencia`, `Latitud`, `Longitud`, `KmVia` (D13), `Celda`, `UltimaDeteccionUtc`, `VenceUtc` |
| `FiltroAnomalias` | `Bbox (minLon, minLat, maxLon, maxLat)`, `Tipo?`, `Severidades[]?`, `DesdeUtc?`, `HastaUtc?` (sobre `ultima_deteccion_utc`), `Estados[]` (por defecto solo `Activa`) |
| `AnomaliaCambio` | lo de `AnomaliaVisible` + `Estado`, `ActualizadaUtc` |
| `ResumenDia` | `Dia`, `EventosRecibidos` (eventos válidos = reportes procesados, D14), `DispositivosDistintos`, `TramosConReportes7d`, `TramosTotales`, `Confirmaciones`, `PendientesExpiradas`, `Expiraciones`, `Renacimientos` (`Expirada → Activa \| PendienteConfirmacion`, D14) |

## 6. Vistas para el administrador (DTOs de la API)

| DTO | Campos |
|---|---|
| `DetalleAnomalia` | `id`, `tipo`, `severidadMostrada`, `estado`, `persistencia`, `latitud`, `longitud`, `kmVia`, `diasDistintos15d`, `reportes`, `ultimaDeteccionUtc`, `venceUtc` (nulo en `Expirada`, `Reparada` y `Descartada`, T-PO08), `visibleConductores`, `porRevisar`, `vehiculosRevision`, `reportesPorDia[{dia, cantidad}]` (15 días), `historial[{desde, hacia, ocurridaUtc, actorId}]` |
| `ElementoPorRevisar` | `id`, `tipo`, `kmVia`, `severidadMostrada` (T-PO19), `vehiculos7d`, `reportes`, `ultimaDeteccionUtc` |
| `ElementoPendiente` | `id`, `tipo`, `kmVia`, `severidadMostrada`, `reportes`, `primerReporteUtc`, `venceUtc` |
| `TransicionReciente` | `anomaliaId`, `tipo`, `kmVia`, `desde`, `hacia`, `ocurridaUtc`, `actorId` |

`kmVia` de todas las vistas sale de `anomalia.km_via` (D13).

## 7. Tipos de Application para la moderación y la exportación (D4, D15)

| Tipo | Definición |
|---|---|
| `AccionModeracion` | enum `MarcarReparada`, `Descartar`, `Reabrir`, `MantenerDescartada` (el del contrato HTTP); `ModerarAnomaliaUseCase` lo mapea a `AccionAdmin` (Domain) |
| `ComandoModeracion` | `Guid AnomaliaId`, `AccionModeracion Accion`, `Guid ActorId` |
| `ResultadoModeracion` | `Hecha(DetalleAnomalia)` · `NoEncontrada` · `NoPermitida(EstadoAnomalia actual)` → 200 / 404 / 409 |
| `FilaExportacion` | `id, tipo, severidad, estado, persistencia, km, latitud, longitud, reportes, primer_reporte, ultimo_reporte` (T-PO11; sin seudónimo) |

## 8. `AnomaliasOptions` (sección `Anomalias`)

25 parámetros, valores y reglas en `spec.md` («Reglas y parámetros»), iguales a `enfoque-arquitectonico.md` §5. Las horas de las tareas no son parámetros del módulo: siguen el calendario de 001 (T-OPS05).
