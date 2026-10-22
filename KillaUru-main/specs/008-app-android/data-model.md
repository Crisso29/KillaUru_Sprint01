# Data Model: App Android KillaUru

Base: `diseño-interno-de-modulos.md` §7.2. Lo agregado está marcado con `-- (+)` y su decisión. Todo lo que guarda la app vive **solo en el celular** (RNF-10) y fuera de la copia de seguridad de Android (R-027).

## 1. Base Room `killauru.db` (versión 1, modo WAL, `exportSchema = true`)

SQL tal como lo crea Room a partir de las `@Entity` (el JSON exacto queda en `app/schemas/…/1.json`). Las horas son **milisegundos UTC** (`INTEGER`); las enumeraciones, **códigos enteros estables** (§4, R-005).

```sql
-- Eventos detectados (Buffer, RF-03, RF-06, ADR-003)
CREATE TABLE evento (
    eventoId       TEXT    NOT NULL PRIMARY KEY,   -- UUIDv7 de 36 caracteres (RF-03; R-004)
    detectadoUtc   INTEGER NOT NULL,               -- hora del fix GPS + monotónico hasta el fin de la ventana (RF-03; R-003)
    lat            REAL    NOT NULL,
    lon            REAL    NOT NULL,
    precisionM     REAL    NOT NULL,               -- precisión del fix (RN-11 la juzga el servidor)
    tipo           INTEGER NOT NULL,               -- 1 Bache · 2 Hundimiento (RN-01)
    severidad      INTEGER NOT NULL,               -- 1…5 (RN-02)
    confidence     REAL    NOT NULL,               -- ≥ 0.75 (RN-04)
    modeloVersion  TEXT    NOT NULL,               -- del manifiesto del modelo activo (ADR-004)
    seudonimo      TEXT    NOT NULL,               -- vigente al detectar (RF-27)
    estado         INTEGER NOT NULL,               -- 0 PENDING · 1 ENVIANDO · 2 SYNCED · 3 REJECTED_LOCAL · 4 EXPIRADO_LOCAL
    estadoDesde    INTEGER NOT NULL                -- hora confiable del último cambio de estado (purga a 7 días)
);
CREATE INDEX index_evento_estado_detectadoUtc ON evento (estado, detectadoUtc);  -- docs: lote más antiguo primero
CREATE INDEX index_evento_estado_estadoDesde  ON evento (estado, estadoDesde);   -- (+) purga diaria (BD-A4)

-- Copia local de anomalías visibles para el rol (RF-14, RF-26)
CREATE TABLE anomalia (
    id                 TEXT    NOT NULL PRIMARY KEY,  -- uuid del servidor
    tipo               INTEGER NOT NULL,              -- 1 Bache · 2 Hundimiento
    severidad          INTEGER NOT NULL,              -- severidad mostrada 1…5 (RN-05)
    persistente        INTEGER NOT NULL,              -- 0 Episodica · 1 Persistente (RF-09: «permanente», D13)
    lat                REAL    NOT NULL,
    lon                REAL    NOT NULL,
    km                 REAL    NOT NULL,              -- kmVia del servidor, calculado al nacer (D13)
    venceUtc           INTEGER NOT NULL,              -- RN-07; si llega null se trata como baja (solo estados finales)
    ultimaDeteccionUtc INTEGER NOT NULL,              -- (+) «Último reporte hace…» (RF-09, RNF-14; BD-A2)
    enDemo             INTEGER NOT NULL               -- (+) 1 si cae en el polígono demo (RN-21 c/d; BD-A2, R-012)
);
CREATE INDEX index_anomalia_km ON anomalia (km);                                  -- docs: rango por km (RN-16)

-- (+) Estado de la copia: una sola fila, guardada en la misma transacción que las altas y bajas (R-011)
CREATE TABLE estado_copia (
    id             INTEGER NOT NULL PRIMARY KEY,   -- siempre 1
    alcance        INTEGER NOT NULL,               -- 0 Anonimo · 1 Registrado
    generadoUtc    INTEGER,                        -- próximo updatedSince (hora del servidor, 006 R-006); NULL = descarga completa
    etag           TEXT,                           -- solo anónimo (If-None-Match)
    verificadoUtc  INTEGER                         -- hora del servidor de la última respuesta 200/304 («Actualizado hace…»)
);

-- Historial de recorridos: solo resúmenes, solo con sesión, 6 meses (RF-25, HU-10)
CREATE TABLE recorrido (
    id            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    inicioUtc     INTEGER NOT NULL,
    finUtc        INTEGER NOT NULL,
    kmDesde       REAL    NOT NULL,
    kmHasta       REAL    NOT NULL,
    kmRecorridos  REAL    NOT NULL,                -- (+) suma de |Δkm| entre fixes (ida y vuelta cuenta todo); totales del prototipo
    eventos       INTEGER NOT NULL                 -- detecciones guardadas en el viaje
);
CREATE INDEX index_recorrido_finUtc ON recorrido (finUtc);                       -- (+) purga de 6 meses

-- (+) Contadores locales: rechazos por motivo y expirados (contrato del lote: «contar los rechazos por motivo»; RF-06)
CREATE TABLE contador_local (
    clave  TEXT    NOT NULL PRIMARY KEY,           -- 'rechazo.<Motivo>' · 'expirado_local' · 'rechazado_local'
    valor  INTEGER NOT NULL
);
```

**Presupuesto (RNF-15, < 100 MB sin mapa base; BD-A8):** peor caso ~600 eventos/h durante 14 días ≈ 85 000 filas × ~250 B ≈ 20 MB; anomalías del corredor < 1 MB; 6 meses de recorridos < 100 KB. Meta de la prueba de llenado: `killauru.db` + `-wal` < 25 MB (T065).

## 2. Validaciones (`CHECK` lógicos como disparadores, R-005)

Se crean en `Callback.onCreate` (`TriggersValidacion.crear(db)`) y en cada migración que toque la tabla. Mensajes estables para las pruebas.

```sql
CREATE TRIGGER trg_evento_valida BEFORE INSERT ON evento
WHEN NEW.estado <> 0                                     -- todo evento nace PENDING
  OR NEW.tipo NOT IN (1, 2) OR NEW.severidad NOT BETWEEN 1 AND 5
  OR NEW.confidence < 0.75 OR NEW.confidence > 1
  OR NEW.lat NOT BETWEEN -90 AND 90 OR NEW.lon NOT BETWEEN -180 AND 180 OR NEW.precisionM < 0
  OR length(NEW.eventoId) <> 36 OR length(NEW.seudonimo) <> 36 OR length(NEW.modeloVersion) NOT BETWEEN 1 AND 32
BEGIN SELECT RAISE(ABORT, 'evento_invalido'); END;

CREATE TRIGGER trg_evento_transicion BEFORE UPDATE OF estado ON evento
WHEN NOT ((OLD.estado = 0 AND NEW.estado IN (1, 4))     -- PENDING → ENVIANDO | EXPIRADO_LOCAL
       OR (OLD.estado = 1 AND NEW.estado IN (0, 2, 3)))  -- ENVIANDO → PENDING | SYNCED | REJECTED_LOCAL
BEGIN SELECT RAISE(ABORT, 'transicion_invalida'); END;

CREATE TRIGGER trg_evento_inmutable BEFORE UPDATE OF eventoId, detectadoUtc, lat, lon, tipo, severidad,
    confidence, modeloVersion, seudonimo ON evento
BEGIN SELECT RAISE(ABORT, 'evento_inmutable'); END;

CREATE TRIGGER trg_anomalia_valida BEFORE INSERT ON anomalia
WHEN NEW.tipo NOT IN (1, 2) OR NEW.severidad NOT BETWEEN 1 AND 5 OR NEW.persistente NOT IN (0, 1)
  OR NEW.enDemo NOT IN (0, 1) OR NEW.km < 0 OR length(NEW.id) <> 36
BEGIN SELECT RAISE(ABORT, 'anomalia_invalida'); END;
-- (el @Upsert de Room hace INSERT y, si existe, UPDATE: trg_anomalia_valida_upd repite la condición BEFORE UPDATE)

CREATE TRIGGER trg_estado_copia_unica BEFORE INSERT ON estado_copia
WHEN NEW.id <> 1 OR NEW.alcance NOT IN (0, 1)
BEGIN SELECT RAISE(ABORT, 'estado_copia_invalido'); END;

CREATE TRIGGER trg_recorrido_valido BEFORE INSERT ON recorrido
WHEN NEW.finUtc < NEW.inicioUtc OR NEW.eventos < 0 OR NEW.kmRecorridos < 0
BEGIN SELECT RAISE(ABORT, 'recorrido_invalido'); END;
```

## 3. Estados locales del evento y transiciones (RF-06, ADR-003)

| Desde | Hacia | Disparador | Dónde |
|---|---|---|---|
| (nace) | `PENDING` (0) | Detección con confidence ≥ 0.75 (RF-03) | `GuardarEventoUseCase` |
| `PENDING` | `ENVIANDO` (1) | Entra en un lote (≤ 50, un seudónimo) | `EventoDao.tomarLote` (transacción, R-007) |
| `PENDING` | `EXPIRADO_LOCAL` (4) | `detectadoUtc` + 168 h ≤ ahora confiable; suma `expirado_local` | `ExpirarPendientesUseCase` antes de cada lote |
| `ENVIANDO` | `SYNCED` (2) | El 200 lo nombra en `aceptados`, `duplicados` o `rechazados` (suma `rechazo.<Motivo>`) | `InterpretarRespuestaLote` |
| `ENVIANDO` | `REJECTED_LOCAL` (3) | Falla **solo** en un 400/413, o el 200 lo rechaza con `id: null` en su `indice` | `PlanParticion` / `InterpretarRespuestaLote` |
| `ENVIANDO` | `PENDING` | El 200 no lo menciona; 401 sin poder renovar por red; 429/503; 5xx; *timeout*; Android detiene el trabajo; la app se abre | `InterpretarRespuestaLote`, `SincronizacionWorker`, `RecuperarEnviandoUseCase` |
| `SYNCED` · `REJECTED_LOCAL` · `EXPIRADO_LOCAL` | (borrado) | `estadoDesde` + 7 días ≤ ahora confiable | `PurgarLocalUseCase` |

Todo otro cambio lo rechazan `TransicionEvento.validar` (dominio) y `trg_evento_transicion` (BD). Un 401 o 403 **nunca** produce `REJECTED_LOCAL`.

## 4. Códigos estables (`Convertidores.kt`, prueba `CodigosEstablesTest`)

| Enumeración (dominio) | Código |
|---|---|
| `EstadoLocal` | `PENDING` 0 · `ENVIANDO` 1 · `SYNCED` 2 · `REJECTED_LOCAL` 3 · `EXPIRADO_LOCAL` 4 |
| `TipoAnomalia` | `Bache` 1 · `Hundimiento` 2 (texto en la API: `Bache`, `Hundimiento`) |
| `Alcance` | `Anonimo` 0 · `Registrado` 1 |
| `CategoriaVehiculo` (DataStore) | `LIVIANO` · `PESADO` · `MOTO` (texto) |

## 5. Consultas del DAO

```sql
-- EventoDao.tomarLote (@Transaction; R-007, BD-A6)
SELECT seudonimo FROM evento WHERE estado = 0 ORDER BY detectadoUtc, eventoId LIMIT 1;
SELECT * FROM evento WHERE estado = 0 AND seudonimo = :s ORDER BY detectadoUtc, eventoId LIMIT :limite;
UPDATE evento SET estado = 1, estadoDesde = :ahora WHERE eventoId IN (:ids);

-- EventoDao.expirar (FR-022) y EventoDao.recuperarEnviando (FR-021)
UPDATE evento SET estado = 4, estadoDesde = :ahora WHERE estado = 0 AND detectadoUtc <= :limite;   -- limite = ahora − 168 h
UPDATE evento SET estado = 0, estadoDesde = :ahora WHERE estado = 1;

-- PurgaDao (FR-023; BD-A4): una vez al día y al abrir la app, solo con ahora confiable
DELETE FROM evento    WHERE estado IN (2, 3, 4) AND estadoDesde < :ahoraMenos7d;
DELETE FROM recorrido WHERE finUtc < :ahoraMenos6Meses;
DELETE FROM anomalia  WHERE venceUtc < :ahoraMenos7d;

-- AnomaliaDao (Nivel4 §7; RN-16, RF-26)
SELECT * FROM anomalia WHERE km BETWEEN :desde AND :hasta AND venceUtc > :ahora;   -- alertas
SELECT * FROM anomalia WHERE venceUtc > :ahora;                                     -- capa del mapa
DELETE FROM anomalia WHERE id IN (:ids);                                            -- bajas de updatedSince
DELETE FROM anomalia WHERE enDemo = 0;                                              -- RN-21 c/d

-- Indicador (RF-36)
SELECT count(*) FROM evento WHERE estado IN (0, 1);
```

Las escrituras de una respuesta del mapa (altas, bajas y `estado_copia`) y las de una respuesta del lote (cambios de estado y contadores) son **una transacción** cada una (`withTransaction`).

## 6. DataStore

| Archivo | Clave | Tipo | Regla |
|---|---|---|---|
| `preferencias` | `categoria` | texto | RN-18 (por defecto: sin elegir → primer uso) |
| | `soloVerMapa` · `usarDatosMoviles` | booleano | RF-29 · RF-26 |
| | `terminos.version` / `.aceptadoUtc` · `politica.version` / `.aceptadoUtc` | texto / long | RF-37, RF-29 (anónimo) |
| | `seudonimo` · `seudonimoDesdeUtc` | texto / long | RF-27 (R-010) |
| | `ancla.utcMs` · `ancla.elapsedMs` · `ancla.bootCount` · `ancla.fuente` | long / long / int / texto | Reloj confiable (R-003) |
| | `backoff.paso` | int 0…4 | RF-07 (R-009) |
| | `mapa.version` · `corredor.version` · `demo.version` · `modelo.version` | texto | Archivos activos (R-014, R-017) |
| | `aviso.demo.viajeId` | texto | Una vez por viaje (R-035) |
| `sesion` (cifrado AES-GCM, Keystore) | `tokenAcceso` · `expiraAccesoUtc` · `tokenRenovacion` · `sesionVenceUtc` · `rol` | texto / long | ADR-009 (R-022) |
| | `permisoLocalHastaUtc` | long | RN-21 a (= `sesionVenceUtc`) |
| | `revocacionPendiente` | texto | R-023 |

## 7. Archivos verificados (`filesDir`)

| Archivo | Origen | Verificación | Activación |
|---|---|---|---|
| `mapa/corredor-pe28a.v<N>.pmtiles` (≤ 80 MB) | `mapa/manifiesto.json` | tamaño + SHA-256; descarga en `.parcial` con `Range` | renombrado atómico; si hay viaje, al terminarlo; se borra la versión anterior |
| `mapa/corredor_pe28a.v<N>.json` | idem | idem + esquema (`vertices` con km creciente) | idem; si cambia, la copia de anomalías se baja completa (R-016) |
| `mapa/tramo_demo.v<N>.geojson` | idem | idem + polígono cerrado ≤ 60 vértices | idem; si cambia, se recalcula `enDemo` |
| `modelos/<version>.tflite` | APK (`assets/modelo/`) o `modelo/manifiesto.json` | SHA-256; carga de prueba del intérprete | solo con la detección detenida; se conserva el anterior (RF-33) |

Formato de la polilínea para la app (`contracts/cdn.openapi.yaml`, `Corredor`): `{ "version": "3", "fuenteSha256": "…", "radioTierraM": 6371008.8, "vertices": [[lon, lat, kmAcumulado], …] }`.

## 8. Tipos del dominio (`*/dominio/`)

| Tipo | Forma | Regla |
|---|---|---|
| `Evento` | `eventoId`, `detectadoUtc: Instant`, `posicion: Coordenada`, `precisionM`, `tipo`, `severidad`, `confidence`, `modeloVersion`, `seudonimo`, `estado` | RF-03 |
| `TransicionEvento` | `validar(desde, hacia): Boolean` | §3 |
| `Clasificacion` | `tipo: TipoAnomalia?` (null = Normal), `severidad: Int`, `confidence: Float` | RF-02 |
| `PoliticaGuardado` | `debeGuardar(c: Clasificacion): Boolean` | RN-04 |
| `PoliticaVelocidad` | `registrar(kmh, instante): Boolean` (sostenida) | RN-03 |
| `PoliticaDeteccion` | `puedeDetectar(estado): Boolean` | RN-03, RN-18, RN-22, RF-29, RF-35 |
| `PoliticaBateria` | `enAhorro(porcentaje, cargando): Boolean` | RN-22 |
| `PoliticaReintento` | `espera(paso, u): Duration` · `esperaRetryAfter(s, u)` | RF-07 |
| `PlanParticion` | pila de sublotes: `partir(lote)`, `solo(evento)` | RF-06 |
| `PoliticaRotacion` | `debeRotar(desde, ahora): Boolean` | RN-09 |
| `PermisoLocal` | `vigente(ahora, viajeEnCurso): Boolean` | RN-21 |
| `ParametrosAviso` | `para(categoria)`: segundos, mínimoM, finPasadaM | RN-16 (*Strategy*, Nivel4 §7) |
| `PosicionEnVia` | `km: Double`, `sentido: Int (±1)`, `distanciaViaM` | RN-16 |
| `Viaje` | `id`, `inicio`, `kmInicio`, `kmRecorridos`, `eventos` | R-019 |
| `IndicadorEstado` | 7 estados + `pendientes` | RF-36 (R-032) |
| `ZonaDemo` | `contiene(Coordenada): Boolean` (rayo, ≤ 60 vértices) | RN-08 |

## 9. `ParametrosApp` (`assets/parametros.json`)

```json
{
  "VelocidadMinimaKmh": 5, "DuracionSostenidaSegundos": 10, "ConfidenceMinimo": 0.75,
  "CategoriasQueDetectan": ["LIVIANO"],
  "TiempoAvisoSegundos": 8, "DistanciaAvisoMinimaMetros": 100,
  "TiempoAvisoPesadoSegundos": 12, "DistanciaAvisoMinimaPesadoMetros": 150,
  "DistanciaFinPasadaMetros": 500, "DistanciaMaximaViaMetros": 50,
  "BateriaMinimaPorcentaje": 15, "DuracionPermisoLocalDias": 30, "ParadaFinViajeMinutos": 30,
  "RotacionSeudonimoDias": 30, "AntiguedadMaximaEnvioHoras": 168, "PurgaEstadosFinalesDias": 7,
  "MaxEventosPorLote": 50, "BackoffSegundos": [30, 120, 600, 1800, 7200], "JitterPorcentaje": 25,
  "ReanudarSincronizacionSegundosMax": 60, "RetencionHistorialMeses": 6, "PurgaAnomaliasVencidasDias": 7,
  "FrecuenciaSensoresHz": 50, "LatenciaLoteSensoresMs": 1000, "AntiguedadMaximaFixMs": 2000,
  "FusionVentanasMs": 1000, "IntervaloActualizacionAnomaliasMin": 15, "ContrasenaMinima": 10
}
```

Validación al cargar (`CargadorParametros`): todos > 0; `ConfidenceMinimo` en (0, 1]; `BackoffSegundos` creciente; `MaxEventosPorLote` ≤ 50 (contrato de 005). Si falla, la app no arranca la detección y lo registra (nunca usa valores por defecto escondidos).

## 10. Migraciones (R-006)

| Versión | Cambio | Clase | Prueba |
|---|---|---|---|
| 1 | Esquema inicial (§1, §2) | — (`onCreate`) | `EsquemaInicialTest` (Room en memoria: tablas, índices, disparadores) |
| N → N+1 | Lo que pida una spec futura | `comun/datos/bd/migraciones/MigracionNaN1.kt` | `MigracionNaN1Test` + `CadenaMigracionesTest` (1 → última con `PENDING`, `ENVIANDO` y recorridos sembrados) |

Reglas: nunca `fallbackToDestructiveMigration`; `evento` y `recorrido` se copian si se recrean (`CREATE TABLE nueva` → `INSERT … SELECT` → `DROP` → `ALTER … RENAME`); los disparadores se recrean al final de la migración; `anomalia` y `estado_copia` pueden vaciarse (`generadoUtc = NULL`).
