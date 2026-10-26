# Diseño Interno de Módulos

> **Proyecto:** KillaUru · **Versión:** 3.1 · **Revisión:** 2026-10-21 · **Estado:** Vigente (rediseñado desde `01-analisis-del-sistema` y `02-arquitectura-de-software`)
> **Stack:** ASP.NET Core 10 · EF Core + NetTopologySuite · PostgreSQL + PostGIS · Kotlin + Room + WorkManager + LiteRT
> **Relacionados:** `principios-de-diseño.md`, `patrones-de-diseño.md`, `enfoque-arquitectonico.md`, `componentes-arquitectonicos.md`, `09-reglas-de-negocio.md`

## 1. Propósito

Este documento baja la arquitectura al **código y a los datos**: cómo se organiza la solución, cómo se diseñan las tablas, la caché y la base local del celular, y cómo se escribe el código de los flujos críticos. Cada decisión indica **qué métrica protege** (RNF) y **qué regla implementa** (RN), para que un cambio futuro sepa qué no debe romper.

**Módulo de referencia:** Anomalías, porque concentra las reglas de negocio. Los demás módulos siguen la misma plantilla con menos reglas.

## 2. Estructura de la solución

```text
KillaUru.sln
├── src/
│   ├── KillaUru.Api/                     Host: Program.cs, middleware, límite de tasa, salud, composición
│   ├── KillaUru.Shared/                  Kernel: value objects y Roles (Domain), IPublicadorEventos, IMetricas,
│   │                                     IContextoTransaccion (Persistencia), ProgramaReloj (Tareas)
│   └── Modules/
│       ├── KillaUru.Identidad/           un .csproj por módulo
│       ├── KillaUru.Ingesta/
│       ├── KillaUru.Anomalias/
│       ├── KillaUru.Mapa/
│       ├── KillaUru.Dashboard/
│       └── KillaUru.Auditoria/
├── tests/
│   ├── KillaUru.<Modulo>.Tests/          dominio y aplicación (sin BD)
│   ├── KillaUru.Integracion.Tests/       Testcontainers (PostgreSQL + PostGIS) y WebApplicationFactory
│   ├── KillaUru.Arquitectura.Tests/      NetArchTest y límite de 200 líneas (RNF-28)
│   └── carga/                            scripts k6 (carga, picos, estrés, resistencia) y caos; corren fuera de la VM
└── db/migraciones/                       V0000__arranque.sql (superusuario) y migraciones SQL versionadas, ejecutadas como paso explícito (RNF-18)
```

Dentro de cada proyecto de módulo:

```text
KillaUru.Anomalias/
├── Contracts/        public   → IAnomaliasService, IAnomaliaLectura, EventoReportado, ContextoReporte, MotivoDescarte, AnomaliaCambiada
├── Domain/           internal → Anomalia, ReporteAnomalia, políticas, AnomaliasOptions
├── Application/      internal → casos de uso y puertos (IAnomaliaRepository)
├── Adapters/         internal → ModeracionController, ExportacionController, DTOs HTTP
├── Infrastructure/   internal → AnomaliasDbContext, AnomaliaRepositoryPostgis, jobs
└── AnomaliasModule.cs public  → AddAnomaliasModule(...): única puerta de registro
```

**Por qué un proyecto por módulo:** todo es `internal` salvo `Contracts/` y el registro. Si otro módulo intenta usar `Anomalia` o `AnomaliasDbContext`, **no compila**. La regla 2 de `estilo-arquitectonico.md` queda garantizada por el compilador, y NetArchTest solo vigila las capas internas. Son 8 proyectos de producción, una cantidad que una persona maneja sin problema.

**`Directory.Build.props`** (común a todos): `net10.0`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `AnalysisLevel=latest-recommended`, `InvariantGlobalization=true` y `Microsoft.CodeAnalysis.BannedApiAnalyzers` con un `BannedSymbols.txt` que prohíbe `DateTime.Now/UtcNow` y `DateTimeOffset.Now/UtcNow` (el reloj es `TimeProvider`).

## 3. Diseño de la base de datos (PostgreSQL + PostGIS)

### 3.1 Criterios de modelado

| Criterio | Decisión | Qué protege |
|---|---|---|
| Un esquema por módulo, sin llaves foráneas entre esquemas | `identidad`, `ingesta`, `anomalias`, `auditoria`, `dashboard` | Independencia de módulos (regla 3, RNF-28) |
| Lo que **nunca cambia** se calcula **una vez** al escribir | La posición de la anomalía es fija (RN-05), así que su `celda` se guarda en una columna | p95 del mapa (RNF-01): se consulta por un entero, no por geometría |
| Índices **parciales** para las consultas calientes | Mapa, caducidad, barredor y purgas usan índices con `WHERE` | Índices pequeños que caben en memoria |
| Enumeraciones como `smallint` con `CHECK` | Estados, tipos y motivos | 2 bytes por fila e integridad en la BD |
| Tiempos en `timestamptz` (UTC) | Toda hora se guarda en UTC; el día local (UTC−5) se calcula en el dominio | RN-12 sin errores de zona horaria |
| Datos personales mínimos | El seudónimo vive en una **columna** que se pone en `NULL` a las 72 h (RN-09); nunca dentro del `JSONB` | Purga barata (un `UPDATE` de una columna) y fácil de auditar (EQ-04) |
| Sin particionado en v1.0 | `evento_recibido` necesita una llave única solo por `evento_id` para la idempotencia; PostgreSQL exige que la llave única de una tabla particionada incluya la columna de partición | Idempotencia de extremo a extremo (DA-02). Las purgas se hacen con `DELETE` por lotes sobre un índice |

### 3.2 Esquema `ingesta` (dueño: Ingesta)

```sql
CREATE TABLE ingesta.evento_recibido (
    evento_id      uuid        PRIMARY KEY,                 -- idempotencia (DA-02)
    lote_id        uuid        NULL,                        -- UUIDv7 generado por el servidor; NULL a las 72 h (RN-09)
    recibido_utc   timestamptz NOT NULL,                    -- una sola hora por lote; redondeada a la hora a las 72 h (RN-09)
    detectado_utc  timestamptz NOT NULL,                    -- hora GPS del celular (RF-03)
    seudonimo      uuid        NULL,                        -- NULL a las 72 h de recibido (RN-09)
    con_sesion     boolean     NOT NULL,                    -- según el token del lote (RN-14)
    estado         smallint    NOT NULL DEFAULT 0 CHECK (estado IN (0, 1, 2)),  -- 0 Recibido · 1 Procesado · 2 Descartado
    motivo         smallint    NULL CHECK (motivo BETWEEN 1 AND 8),                -- MotivoDescarte del procesamiento
    payload        jsonb       NOT NULL                     -- evento tal como llegó, sin seudónimo (ADR-014)
);
CREATE INDEX ix_recibido_pendiente ON ingesta.evento_recibido (recibido_utc) WHERE estado = 0;      -- barredor (EQ-09)
CREATE INDEX ix_recibido_tope      ON ingesta.evento_recibido (seudonimo, detectado_utc)
                                    WHERE seudonimo IS NOT NULL;                                    -- conteo de RN-15a
CREATE INDEX ix_recibido_fecha     ON ingesta.evento_recibido (recibido_utc);                        -- purga de 30 días
CREATE INDEX ix_recibido_purga     ON ingesta.evento_recibido (recibido_utc) WHERE seudonimo IS NOT NULL;  -- purga de 72 h
CREATE INDEX ix_recibido_lote      ON ingesta.evento_recibido (lote_id) WHERE estado = 0;            -- trabajador por lote
```

### 3.3 Esquema `anomalias` (dueño: Anomalías)

```sql
CREATE TABLE anomalias.anomalia (
    id                    uuid        PRIMARY KEY,          -- UUIDv7: ordenado por tiempo, buen índice
    tipo                  smallint    NOT NULL CHECK (tipo IN (1, 2)),               -- RN-01
    estado                smallint    NOT NULL CHECK (estado BETWEEN 1 AND 5),       -- RN-07
    persistencia          smallint    NOT NULL DEFAULT 0 CHECK (persistencia IN (0, 1)),  -- RN-12
    severidad_mostrada    smallint    NOT NULL CHECK (severidad_mostrada BETWEEN 1 AND 5),  -- mediana (RN-05)
    posicion              geography(Point, 4326) NOT NULL,  -- fija: la del reporte que la creó (RN-05)
    celda                 bigint      NOT NULL,             -- calculada de la posición; nunca cambia
    km_via                real        NOT NULL,             -- CorredorVia.KmSobreVia al nacer; nunca cambia (RF-14)
    ultima_deteccion_utc  timestamptz NOT NULL,
    vence_utc             timestamptz NOT NULL,             -- calculada por el dominio (RN-07)
    estado_desde_utc      timestamptz NOT NULL,
    actualizada_utc       timestamptz NOT NULL,             -- para updatedSince (RF-14)
    creada_por_tope       boolean     NOT NULL DEFAULT false,                        -- RN-15a
    marcas_revision       timestamptz[] NOT NULL DEFAULT '{}',                       -- vehículos tras descartar (RN-07)
    por_revisar           boolean     NOT NULL DEFAULT false
);
CREATE INDEX ix_anomalia_posicion  ON anomalias.anomalia USING gist (posicion);                     -- agrupación (ST_DWithin)
CREATE INDEX ix_anomalia_mapa      ON anomalias.anomalia (celda) WHERE estado = 1;                   -- mapa: solo Activa
CREATE INDEX ix_anomalia_vence     ON anomalias.anomalia (vence_utc) WHERE estado IN (1, 2);         -- caducidad
CREATE INDEX ix_anomalia_cambios   ON anomalias.anomalia (actualizada_utc);                          -- updatedSince
CREATE INDEX ix_anomalia_revision  ON anomalias.anomalia (actualizada_utc) WHERE por_revisar;        -- lista «Por revisar»

CREATE TABLE anomalias.transicion (                         -- historial de estados: base de los indicadores (RF-18)
    id            bigint      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    anomalia_id   uuid        NOT NULL REFERENCES anomalias.anomalia (id) ON DELETE CASCADE,
    desde         smallint    NULL CHECK (desde BETWEEN 1 AND 5),                -- NULL = la anomalía nació
    hacia         smallint    NOT NULL CHECK (hacia BETWEEN 1 AND 5),
    ocurrida_utc  timestamptz NOT NULL,                     -- detección, vencimiento o acción del administrador
    actor_id      uuid        NULL                          -- administrador que actuó; NULL = sistema
);
CREATE INDEX ix_transicion_fecha   ON anomalias.transicion (ocurrida_utc);                           -- resumen diario
CREATE INDEX ix_transicion_anomalia ON anomalias.transicion (anomalia_id, ocurrida_utc);             -- historial del detalle y FK

CREATE TABLE anomalias.reporte_anomalia (
    evento_id      uuid        PRIMARY KEY,                 -- idempotencia del procesamiento
    anomalia_id    uuid        NOT NULL REFERENCES anomalias.anomalia (id) ON DELETE CASCADE,
    detectado_utc  timestamptz NOT NULL,
    recibido_utc   timestamptz NOT NULL,                    -- llega en ContextoReporte; redondeada a la hora a las 72 h (RN-09)
    severidad      smallint    NOT NULL CHECK (severidad BETWEEN 1 AND 5),
    confidence     real        NOT NULL CHECK (confidence BETWEEN 0 AND 1),
    posicion       geography(Point, 4326) NOT NULL,
    precision_m    real        NOT NULL,
    km_via         real        NOT NULL,                    -- km sobre la vía: cobertura de RF-18
    seudonimo      uuid        NULL,                        -- NULL a las 72 h de recibido (RN-09)
    con_sesion     boolean     NOT NULL,                    -- sí/no, nunca quién (RN-14)
    modelo_version text        NOT NULL
);
CREATE INDEX ix_reporte_ventana    ON anomalias.reporte_anomalia (anomalia_id, detectado_utc DESC);  -- mediana, RN-06, RN-12
CREATE INDEX ix_reporte_seudonimo  ON anomalias.reporte_anomalia (seudonimo, detectado_utc DESC)
                                    WHERE seudonimo IS NOT NULL;                                      -- RN-15b
CREATE INDEX ix_reporte_purga      ON anomalias.reporte_anomalia (recibido_utc) WHERE seudonimo IS NOT NULL;
CREATE INDEX ix_reporte_historico  ON anomalias.reporte_anomalia (detectado_utc);                     -- purga de 2 años
CREATE INDEX ix_reporte_recibido   ON anomalias.reporte_anomalia (recibido_utc);                      -- resumen del día

CREATE TABLE anomalias.corredor_via (
    id    smallint PRIMARY KEY,
    traza geography(LineString, 4326) NOT NULL              -- PE-28A; se carga en memoria al arrancar (RN-13)
);
```

**Celda de rejilla:** cuadro de 0.005° × 0.005° (≈ 550 m, al menos 5 veces el radio de 100 m). Su identificador es un `bigint` calculado en `Shared/Domain/Celda`. La misma celda sirve para el bloqueo de concurrencia y para la clave de la caché; por eso su tamaño **no** es configurable por módulo.

**Vencimiento al reabrir:** cuando el administrador reabre una anomalía, `vence_utc` = momento de la reapertura + caducidad que corresponda (RN-07).

### 3.4 Esquemas `identidad`, `auditoria` y `dashboard`

```sql
CREATE TABLE identidad.usuario (
    id                 uuid PRIMARY KEY,
    correo             citext      NOT NULL UNIQUE,
    hash_contrasena    text        NOT NULL,            -- PasswordHasher (PBKDF2), nunca la contraseña
    rol                smallint    NOT NULL,            -- 1 ConductorRegistrado · 2 Administrador
    activo             boolean     NOT NULL DEFAULT true,
    correo_verificado  boolean     NOT NULL DEFAULT false,
    intentos_fallidos  smallint    NOT NULL DEFAULT 0,  -- RN-19
    nivel_bloqueo      smallint    NOT NULL DEFAULT 0,
    bloqueado_hasta    timestamptz NULL,
    creado_utc         timestamptz NOT NULL             -- truncado al día (RN-09)
);
CREATE TABLE identidad.sesion_refresh (
    id              uuid PRIMARY KEY,
    usuario_id      uuid        NOT NULL REFERENCES identidad.usuario (id) ON DELETE CASCADE,
    familia_id      uuid        NOT NULL,               -- todos los tokens de una misma sesión
    hash_token      bytea       NOT NULL UNIQUE,        -- SHA-256 del token; el token nunca se guarda
    expira_utc      timestamptz NOT NULL,               -- 8 h administrador · 30 días conductor (RF-16, RF-21); truncado hacia abajo a la hora
    usado_utc       timestamptz NULL,                   -- exacto (gracia de 60 s, ADR-009); la purga lo trunca a la hora pasados 2 min
    revocado_utc    timestamptz NULL
);
CREATE INDEX ix_sesion_usuario ON identidad.sesion_refresh (usuario_id) WHERE revocado_utc IS NULL;
CREATE TABLE identidad.consentimiento (
    usuario_id   uuid        NOT NULL REFERENCES identidad.usuario (id) ON DELETE CASCADE,
    documento    smallint    NOT NULL,                  -- 1 Política de privacidad · 2 Términos de uso
    version      text        NOT NULL,
    aceptado_utc timestamptz NOT NULL,
    PRIMARY KEY (usuario_id, documento, version)        -- RF-29, RF-37
);
CREATE TABLE identidad.token_correo (
    hash_token  bytea PRIMARY KEY,
    usuario_id  uuid        NOT NULL REFERENCES identidad.usuario (id) ON DELETE CASCADE,
    proposito   smallint    NOT NULL,                   -- 1 Verificación (24 h) · 2 Recuperación (30 min)
    expira_utc  timestamptz NOT NULL,
    usado_utc   timestamptz NULL
);
-- Purga diaria de Identidad (sección 3.8): sesiones con expira_utc o revocado_utc de hace más de 30 días
-- y token_correo vencidos o usados hace más de 30 días. Así la hora de una sesión no se puede unir a los eventos (RN-09, RNF-10)

CREATE TABLE auditoria.registro (
    id            bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ocurrido_utc  timestamptz NOT NULL,
    actor_id      uuid        NULL,                     -- identificador interno, nunca el correo (RF-30)
    actor_rol     smallint    NULL,
    accion        smallint    NOT NULL,                 -- catálogo de RF-15
    objetivo      text        NULL,
    contexto      jsonb       NOT NULL DEFAULT '{}'
);
CREATE INDEX ix_registro_fecha ON auditoria.registro (ocurrido_utc);
CREATE INDEX ix_registro_actor ON auditoria.registro (actor_id, ocurrido_utc);
REVOKE UPDATE, DELETE, TRUNCATE ON auditoria.registro FROM killauru_app;    -- append-only en la BD (RNF-31)

CREATE TABLE dashboard.indicador_diario (
    dia        date     NOT NULL,
    indicador  smallint NOT NULL,                       -- eventos, conductores activos, dispositivos, cobertura…
    valor      numeric  NOT NULL,
    PRIMARY KEY (dia, indicador)                        -- solo números agregados (RN-17)
);
```

### 3.5 Consultas críticas y el índice que las sostiene

| Consulta | SQL (forma) | Índice | Métrica |
|---|---|---|---|
| Mapa por celdas (`IAnomaliaLectura.ActivasPorCeldasAsync`, dentro de Anomalías) | `WHERE estado = 1 AND celda = ANY(@celdas)` | `ix_anomalia_mapa` (B-tree parcial) | RNF-01 |
| Mapa con filtros (`IAnomaliaLectura.FiltrarAsync`) | `WHERE ST_Intersects(posicion, @bbox) AND tipo = … AND severidad_mostrada >= … AND estado = ANY(@estados)` | `ix_anomalia_posicion` (GiST) | RNF-01 |
| Resumen diario para Dashboard (`IAnomaliaLectura.ResumenDelDiaAsync`) | `count(*)` de transiciones y reportes del día local | `ix_transicion_fecha`, `ix_reporte_recibido` | RF-18 |
| Anomalía más cercana del mismo tipo | `WHERE tipo = @t AND ST_DWithin(posicion, @p, 100) ORDER BY posicion <-> @p LIMIT 1` | `ix_anomalia_posicion` (GiST) | RNF-23 |
| Reportes de la ventana (mediana, RN-06, RN-12) | `WHERE anomalia_id = @id AND detectado_utc >= @desde` | `ix_reporte_ventana` | RNF-23 |
| Reporte anterior del seudónimo (RN-15b) | `WHERE seudonimo = @s AND detectado_utc < @t ORDER BY detectado_utc DESC LIMIT 1` | `ix_reporte_seudonimo` | RNF-23 |
| Eventos del seudónimo en la hora (RN-15a) | `count(*) WHERE seudonimo = @s AND detectado_utc >= @hora AND < @hora + 1 h` | `ix_recibido_tope` | RNF-23 |
| Cambios para la copia offline | `WHERE actualizada_utc > @desde` | `ix_anomalia_cambios` | RF-26 |
| Caducidad (cada hora) | `UPDATE … SET estado = 3 WHERE estado IN (1, 2) AND vence_utc <= @ahora RETURNING id, celda` (+ fila en `transicion`, sección 3.8) | `ix_anomalia_vence` | RN-07 |
| Barredor de recuperación | `WHERE estado = 0 AND recibido_utc < @ahora - 1 min` | `ix_recibido_pendiente` | EQ-09 |
| Eventos de un lote (trabajador) | `WHERE lote_id = @lote AND estado = 0 ORDER BY detectado_utc, evento_id` | `ix_recibido_lote` | RNF-23 |
| Purga de seudónimos (cada hora) | `WHERE seudonimo IS NOT NULL AND recibido_utc < @ahora - 72 h` | `ix_reporte_purga`, `ix_recibido_purga` | RN-09 |

**Caducidad sin cargar agregados:** el dominio calcula `vence_utc` cada vez que registra un reporte (episódica +72 h, persistente +7 días, pendiente +72 h). La tarea horaria solo compara una fecha con un `UPDATE` masivo, así la regla sigue en el dominio y el proceso cuesta una consulta.

### 3.6 Volumen esperado y holgura

Estimación con el supuesto S-01 (300 dispositivos, ~10 h de manejo al día y ~16 eventos por hora, como el ejemplo de RN-15): unos **48 000 eventos por día**.

| Tabla | Filas | Tamaño aprox. (con índices) | Comentario |
|---|---|---|---|
| `evento_recibido` (30 días) | ~1,5 millones | ~1 GB | Se purga a diario |
| `reporte_anomalia` (2 años) | ~35 millones | ~8 GB | Las consultas siempre usan `anomalia_id` o `seudonimo` |
| `anomalia` | decenas de miles | < 50 MB | El índice del mapa solo contiene las `Activa` |
| `auditoria.registro` | miles por mes | < 1 GB en años | Permanente |

Caben de sobra en el disco de la VM Always Free. Si `reporte_anomalia` pasa de ~100 millones de filas, se particiona por `detectado_utc` (sección 9).

### 3.7 Roles, migraciones y configuración

| Elemento | Decisión |
|---|---|
| Roles de BD | `killauru_migrador` (DDL, solo durante el paso de migración) y `killauru_app` (solo DML, sin `UPDATE`/`DELETE` en auditoría). Los crea `V0000__arranque.sql`, que corre `migrar.sh --arranque` **una sola vez como superusuario** (`KU_PG_SUPERUSUARIO`): roles, extensiones `postgis` y `citext`, esquemas, tabla `migraciones.aplicada` y permisos. Contraseñas en el inventario de secretos (RNF-34) |
| Migraciones | Scripts SQL versionados en `db/migraciones/`, ejecutados como paso explícito antes del despliegue, precedidos de un `pg_dump`, y en dos fases (agregar primero, borrar después) (RNF-18). V0001 en adelante corren con `killauru_migrador` y `psql -v ON_ERROR_STOP=1 --single-transaction`; Testcontainers aplica los mismos scripts con los mismos roles. Primer despliegue: arranque → migraciones → `docker compose up -d` |
| Backup | `pg_dumpall --roles-only` + `pg_dump`, cifrados a R2 (RNF-07) |
| EF Core | Un `DbContext` por módulo con `HasDefaultSchema`; NetTopologySuite para `geography`; lecturas con `AsNoTracking`; consultas compiladas en el camino caliente |
| Conexiones | Un pool de Npgsql de 30 conexiones como máximo (`Maximum Pool Size=30`), suficiente para los trabajadores, las peticiones HTTP y las tareas |
| PostgreSQL | `shared_buffers` ≈ 25 % de la RAM, `effective_cache_size` ≈ 70 %, `random_page_cost = 1.1` (disco SSD) y `jit = off` (consultas cortas). Ajustar en el piloto |

### 3.8 Tareas de mantenimiento (SQL)

| Tarea | Frecuencia | Sentencia (forma) | Regla |
|---|---|---|---|
| Caducidad | Al arrancar y cada hora (minuto :05) | `UPDATE anomalias.anomalia a SET estado = 3 … FROM (SELECT id, estado FROM anomalias.anomalia WHERE estado IN (1, 2) AND vence_utc <= @ahora FOR UPDATE SKIP LOCKED) v WHERE a.id = v.id RETURNING a.id, a.celda, v.estado`; en la misma transacción inserta en `transicion` (`v.estado` → 3, en `vence_utc`) y, tras el *commit*, publica `AnomaliaCambiada` por cada fila. Solo salta una fila mientras un trabajador o la moderación la tiene con `FOR UPDATE` (milisegundos, sección 6.3) | RN-07, RF-18 |
| Purga de seudónimos | Al arrancar y cada hora (minuto :10) | `UPDATE ingesta.evento_recibido SET seudonimo = NULL, lote_id = NULL, recibido_utc = date_trunc('hour', recibido_utc) WHERE seudonimo IS NOT NULL AND recibido_utc < @ahora - interval '72 hours'` y `UPDATE anomalias.reporte_anomalia SET seudonimo = NULL, recibido_utc = date_trunc('hour', recibido_utc) WHERE …` (lo mismo), cada una en **una** sentencia | RN-09 |
| Purga de recepción | Al arrancar y diaria a las 03:00 (hora local) | `DELETE … WHERE recibido_utc < @ahora - interval '30 days'`, en lotes de 5 000 | RN-10 |
| Purga histórica | Al arrancar y diaria a las 03:30 | Borra reportes con `detectado_utc < @limite` (2 años) y `DELETE FROM anomalias.anomalia WHERE estado IN (3, 4, 5) AND estado_desde_utc < @limite AND (estado <> 5 OR ultima_deteccion_utc < @limite)`: una `Descartada` solo se borra tras 2 años sin ningún reporte nuevo | RN-10 |
| Purga de Identidad | Al arrancar y diaria a las 03:45 | `DELETE FROM identidad.sesion_refresh WHERE expira_utc < @ahora - interval '30 days' OR revocado_utc < @ahora - interval '30 days'`; `DELETE FROM identidad.token_correo WHERE expira_utc < @ahora - interval '30 days' OR usado_utc < @ahora - interval '30 days'`; `UPDATE identidad.sesion_refresh SET usado_utc = date_trunc('hour', usado_utc) WHERE usado_utc < @ahora - interval '2 minutes' AND usado_utc <> date_trunc('hour', usado_utc)` | RN-09, RNF-10 |
| Indicadores | Al arrancar y diaria a las 00:30 hora local, antes de que la purga de 72 h borre los seudónimos | Dashboard pide a Anomalías `ResumenDelDiaAsync(dia)` y a Identidad `ContarConductoresActivosAsync(7)` por cada día que falte entre `hoy − DiasRecuperables` y ayer (`DiasRecuperables = 2`: más días ya no tendrían seudónimos), y guarda solo los números en `dashboard.indicador_diario` (detalle abajo) | RN-17, RF-18 |

**De dónde sale cada indicador de RF-18** (Dashboard nunca lee tablas ajenas; todo llega por contrato):

| Indicador | Fuente | Cálculo |
|---|---|---|
| Eventos válidos por día | `anomalias.reporte_anomalia` | Reportes procesados (eventos válidos) recibidos en el día local; se compara con el promedio de los 7 días previos (% relativo). Los descartados se ven en Grafana (RNF-22) |
| Conductores registrados activos | `IIdentidadConsultas` | Cuentas que renovaron su sesión en los últimos 7 días (RN-17); se compara con el promedio de los 7 días previos (diferencia absoluta) |
| Dispositivos que aportaron datos | `anomalias.reporte_anomalia` | Seudónimos distintos con reportes recibidos en el día; solo se guarda el número; se compara con el promedio de los 7 días previos (diferencia absoluta) |
| Cobertura | `reporte_anomalia` + `corredor_via` | Tramos de 1 km de la polilínea (`km_via` entero) con ≥ 1 evento válido en los 7 días que terminan ese día ÷ total de tramos × 100; se compara en puntos con el promedio de los 7 días previos |
| Tasa de confirmación | `anomalias.transicion` | Σ `Pendiente→Activa` ÷ (Σ `Pendiente→Activa` + Σ `Pendiente→Expirada`) × 100 en los últimos 7 días; se compara en puntos con los 7 días anteriores |
| Tasa de reaparición | `anomalias.transicion` | Σ anomalías que **vuelven a nacer** después de expirar (hacia `Activa` o `PendienteConfirmacion`) ÷ Σ anomalías que pasaron a `Expirada` × 100 en los últimos 7 días (RN-07); se compara en puntos con los 7 días anteriores |

Valores en % con 1 decimal; promedios con 1 decimal; conteos del día, enteros.

**Calendario por reloj (no por `PeriodicTimer` de 24 h):** todas las tareas corren **al arrancar** y luego en su minuto u hora local fija, calculada con `Shared/Tareas/ProgramaReloj` sobre `TimeProvider`; un reinicio o un despliegue no las retrasa. Horas fijas: caducidad `:05`, purgas de seudónimo `:10`; 00:30 indicadores, 02:00 backup (host), 03:00 purga de recepción, 03:30 purga histórica, 03:45 purga de Identidad.

Cada tarea guarda su última ejecución correcta (métrica `killauru.tarea.ultima_ok{tarea}`, RNF-22) con su nombre: `caducidad`, `purga-seudonimos`, `purga-seudonimos-recepcion`, `purga-recepcion`, `purga-historica`, `purga-identidad`, `indicadores`, `backup`. El backup, que corre fuera de la API, escribe la hora de su última ejecución correcta en un archivo `.prom` que Grafana Alloy convierte en la misma métrica. Grafana alerta si pasan más de 26 h sin ejecutarse.

## 4. Diseño de la caché del mapa

```mermaid
flowchart LR
    REQ["GET /anomalies?bbox"] --> UC["ConsultarMapaUseCase"]
    UC -- "bbox → celdas (recortadas por rol)" --> ETAG{"ETag de las celdas<br/>= versiones en memoria"}
    ETAG -- "igual al If-None-Match" --> R304["304 sin tocar la BD"]
    ETAG -- "distinto" --> DEC["FuenteCeldasCacheada<br/>(HybridCache)"]
    DEC -- "falla de caché" --> PG["FuenteCeldasAnomalias<br/>IAnomaliaLectura.ActivasPorCeldasAsync"]
    COR["IAnomaliaLectura.CeldasDelCorredor<br/>(una vez)"] -. "celdas permitidas" .-> UC
    REQ -- "con filtros" --> FIL["IAnomaliaLectura.FiltrarAsync<br/>(sin caché por celdas)"]
    EVT["AnomaliaCambiada"] --> INV["InvalidarCeldasMapa"]
    INV -- "versión++ y elimina por etiqueta" --> DEC
```

| Aspecto | Decisión | Motivo |
|---|---|---|
| Origen de los datos | `FuenteCeldasAnomalias` llama a `IAnomaliaLectura.ActivasPorCeldasAsync` (Contracts de Anomalías). El SQL vive en la implementación de ese contrato, **dentro de Anomalías** | Consulta de Mapa no tiene esquema ni `DbContext` (regla 3 de `estilo-arquitectonico.md`) |
| Tecnología | **HybridCache** de .NET (su nivel 1 es `IMemoryCache` en proceso, ADR-006) | Protección contra estampida (una sola consulta por clave aunque lleguen 300 peticiones a la vez), invalidación por etiqueta y un nivel 2 (Redis) **opcional** sin cambiar el código cuando haya más de una instancia (S-03) |
| Clave | `celda:{id}:{alcance}`, con alcance = `Anonimo` o `Registrado`. La celda es la de `Shared` (0,005°), la misma que guarda Anomalías: Mapa no tiene un tamaño de celda configurable | Nunca se mezclan roles (RNF-27); `AnomaliaCambiada` invalida exactamente la celda guardada. El administrador, que usa filtros, no pasa por la caché |
| Valor | Las *features* de la celda ya **serializadas a UTF-8**, cada una con su `kmVia` (calculado al nacer la anomalía) | La respuesta se arma concatenando bytes; no se vuelve a serializar en cada petición |
| Celdas del corredor | Pide **una vez** a Anomalías `IAnomaliaLectura.CeldasDelCorredor()` (celdas que toca la franja de 50 m de la polilínea, ~1 000) y ajusta el `bbox` de toda petición a esas celdas; el anónimo, además, al polígono demo | El `bbox` del corredor abarca ~150 000 celdas; sin cálculo espacial por petición ni SQL propio |
| Recorte del anónimo | `ZonaDemo` precalcula al arrancar qué celdas están **dentro** del polígono demo y cuáles en el **borde**. Las del borde se filtran punto por punto en memoria **antes** de guardarse con alcance `Anonimo` | Sin cálculo espacial por petición |
| Vigencia | 10 min absolutos **más** invalidación inmediata por `AnomaliaCambiada` | Los datos nunca quedan viejos más de lo que tarda un evento en llegar |
| ETag | Hash de (instante de arranque del proceso, alcance, versiones de las celdas pedidas) | Un 304 se resuelve en memoria. El instante de arranque evita que, tras un reinicio, una versión repetida dé un 304 falso |
| Tamaño | Límite de 64 MB (`SizeLimit`) | El corredor tiene ~1 000 celdas × 2 alcances: cabe holgado |
| Borde (Cloudflare) | `Cache-Control: public, max-age=60` **solo** sin token; con token, `private, no-store` | Ningún anónimo recibe una respuesta de un registrado (ADR-006) |
| Filtros (`tipo`, `severidad`, `desde`, `hasta`; `estado` solo para el administrador, RF-14) | Una petición con filtros **no usa la caché por celdas**: va a `IAnomaliaLectura.FiltrarAsync` con el mismo recorte por rol (`ZonaDemo` para el anónimo) y el índice GiST. Sin ETag por versiones (no hay 304). Si es anónima, Cloudflare la puede cachear 60 s, porque los filtros forman parte de la URL | La app no usa filtros (descarga todas las `Activa` para su copia offline, RF-26); los usa el panel, con poco tráfico. Cachear cada combinación de filtros multiplicaría las entradas sin beneficio |
| `updatedSince` | No usa la caché; consulta `ix_anomalia_cambios` y devuelve las bajas como `{id, eliminada: true}` | Respuesta personalizada por fecha, poco frecuente |

## 5. Ingesta y procesamiento concurrente

| Paso | Diseño | Métrica |
|---|---|---|
| 1. Recepción | El servidor genera el `loteId`; un lote = un seudónimo (otro seudónimo → `SeudonimoInconsistente`); reglas de recepción en memoria (sin BD) y **un solo `INSERT`** con `unnest` para todo el lote (sección 6.4) | RNF-02: < 300 ms p95 |
| 2. Aviso | `Channel<LoteRecibido>` acotado (`IngestaOptions.CapacidadCola`, inicial 2 000 lotes). Si está lleno → 503 con `Retry-After` | Contrapresión (DA-05) |
| 3. Procesamiento | `IngestaOptions.Trabajadores` consumidores en paralelo (inicial = núcleos de la VM). Cada evento va en **su propia transacción** de Anomalías con `pg_advisory_xact_lock` sobre sus celdas, en orden ascendente; dentro del bloqueo lee la anomalía con `SELECT … FOR UPDATE` y revalida su estado (así la caducidad o la moderación no se pisan con el trabajador) | RNF-23: ≤ 100 ms por evento; retraso de cola (procesado − recibido) ≤ 30 s p95, histograma `killauru.ingesta.retraso_cola_s`; celdas distintas en paralelo, la misma zona en serie, sin interbloqueos |
| 4. Recuperación | Barredor cada 60 s y al arrancar | EQ-09 |
| 5. Idempotencia | Llave primaria `evento_id` en las dos tablas | DA-02 |

**Capacidad estimada:** un evento cuesta ~5 consultas indexadas (~5–15 ms). Con 4 trabajadores son ≥ 250 eventos por segundo. El pico de RNF-03 (300 clientes × 500 eventos en 10 min) es de ~250 eventos por segundo, así que la cola se mantiene por debajo del límite de 30 s de RNF-23. Si no alcanza en la prueba de carga (k6 fuera de la VM contra el entorno de pruebas R-11, con límites de CPU y memoria; VM A1 de 4 OCPU / 24 GB), se suben los trabajadores por configuración.

## 6. Código de referencia (backend)

El código es un **esqueleto fiel**: muestra la forma y el tamaño esperados, no cada línea.

### 6.1 Kernel compartido

```csharp
// KillaUru.Shared/Domain — value objects: inmutables y siempre válidos
public readonly record struct Severidad
{
    public int Valor { get; }
    private Severidad(int valor) => Valor = valor;            // nadie crea una severidad inválida

    public static Severidad? Crear(int v) => v is >= 1 and <= 5 ? new(v) : null;

    // RN-05: mediana de la ventana, redondeada hacia arriba
    public static Severidad MedianaHaciaArriba(IEnumerable<Severidad> valores)
    {
        var v = valores.Select(s => s.Valor).Order().ToArray();
        var m = v.Length % 2 == 1 ? v[v.Length / 2] : (v[v.Length / 2 - 1] + v[v.Length / 2]) / 2.0;
        return new((int)Math.Ceiling(m));
    }
}

public readonly record struct Celda(long Id)
{
    private const double Paso = 0.005;   // ≈ 550 m ≥ 5 × RadioAgrupacionMetros

    public static Celda De(Coordenada c) =>
        new(((long)Math.Floor(c.Latitud / Paso) + 18_000) * 100_000 + (long)Math.Floor(c.Longitud / Paso) + 36_000);

    // Celdas que toca un cuadro de ±metros alrededor del punto, en orden fijo (evita interbloqueos)
    public static IReadOnlyList<Celda> Alrededor(Coordenada c, double metros)
    {
        var d = metros / 100_000.0;   // grados, con holgura
        return new[] { (-d, -d), (-d, d), (d, -d), (d, d) }
            .Select(x => De(c with { Latitud = c.Latitud + x.Item1, Longitud = c.Longitud + x.Item2 }))
            .Distinct().OrderBy(x => x.Id).ToArray();
    }

    // Rectángulo de la celda en grados: Consulta de Mapa ajusta el bbox sin repetir la fórmula
    public (double Sur, double Oeste, double Norte, double Este) Limites()
    {
        long fila = Id / 100_000 - 18_000, columna = Id % 100_000 - 36_000;
        return (fila * Paso, columna * Paso, (fila + 1) * Paso, (columna + 1) * Paso);
    }
}
```

### 6.2 Dominio de Anomalías

```csharp
// Anomalia.cs — agregado: única puerta para cambiar estado, severidad y persistencia
internal sealed class Anomalia
{
    // RN-07: acciones del administrador y desde qué estados valen
    private static readonly Dictionary<AccionAdmin, EstadoAnomalia[]> PermitidoDesde = new()
    {
        [AccionAdmin.MarcarReparada] = [EstadoAnomalia.Activa, EstadoAnomalia.PendienteConfirmacion],
        [AccionAdmin.Descartar]      = [EstadoAnomalia.Activa, EstadoAnomalia.PendienteConfirmacion, EstadoAnomalia.Expirada],
        [AccionAdmin.Reabrir]        = [EstadoAnomalia.Reparada, EstadoAnomalia.Descartada],
        [AccionAdmin.MantenerDescartada] = [EstadoAnomalia.Descartada],   // reinicia el contador de «Por revisar»
    };

    public Guid Id { get; private set; }
    public TipoAnomalia Tipo { get; private set; }
    public Coordenada Posicion { get; private set; }          // fija (RN-05)
    public Celda Celda { get; private set; }
    public float KmVia { get; private set; }                 // km sobre la vía, calculado al nacer (fijo)
    public EstadoAnomalia Estado { get; private set; }
    public Severidad SeveridadMostrada { get; private set; }
    public EstadoPersistencia Persistencia { get; private set; }
    public DateTimeOffset UltimaDeteccionUtc { get; private set; }
    public DateTimeOffset VenceUtc { get; private set; }
    public DateTimeOffset EstadoDesdeUtc { get; private set; }
    public bool CreadaPorTope { get; private set; }
    public bool PorRevisar { get; private set; }
    public List<DateTimeOffset> MarcasRevision { get; private set; } = [];
    public List<Transicion> Transiciones { get; } = [];      // nuevas en esta operación; el repositorio las inserta (RF-18)
    private bool _nueva;

    private Anomalia() { }   // EF Core

    public static Anomalia Crear(ReporteAnomalia r, bool porTope) => new()
    {
        Id = Guid.CreateVersion7(), Tipo = r.Tipo, Posicion = r.Ubicacion, Celda = Celda.De(r.Ubicacion), KmVia = r.KmVia,
        Estado = EstadoAnomalia.PendienteConfirmacion,          // RegistrarReporte decide si se publica
        SeveridadMostrada = r.Severidad, UltimaDeteccionUtc = r.DetectadoUtc,
        EstadoDesdeUtc = r.DetectadoUtc, CreadaPorTope = porTope, _nueva = true,
    };

    // RN-05, RN-06, RN-07, RN-12 y RN-14. `ventana` ya incluye el reporte nuevo.
    public void RegistrarReporte(ReporteAnomalia r, IReadOnlyList<ReporteAnomalia> ventana, ReglasAnomalia reglas)
    {
        EstadoAnomalia? antes = _nueva ? null : Estado;
        if (Estado == EstadoAnomalia.Descartada) { AnotarParaRevision(r, ventana, reglas); return; }
        if (Estado is EstadoAnomalia.Expirada or EstadoAnomalia.Reparada)
        {
            var vencio = Estado == EstadoAnomalia.Expirada ? VenceUtc : EstadoDesdeUtc;
            if (r.DetectadoUtc <= vencio) return;                // detectado antes de vencer: no reactiva
            CambiarEstado(EstadoAnomalia.PendienteConfirmacion, r.DetectadoUtc);   // vuelve a nacer
        }

        UltimaDeteccionUtc = r.DetectadoUtc > UltimaDeteccionUtc ? r.DetectadoUtc : UltimaDeteccionUtc;
        SeveridadMostrada = Severidad.MedianaHaciaArriba(ventana.Select(x => x.Severidad));
        Persistencia = reglas.Persistencia.Evaluar(ventana.Select(x => x.DetectadoUtc), UltimaDeteccionUtc);
        if (Estado == EstadoAnomalia.PendienteConfirmacion)
            CambiarEstado(reglas.Confirmacion.Decidir(ventana, SeveridadMostrada, CreadaPorTope, UltimaDeteccionUtc), UltimaDeteccionUtc);
        VenceUtc = UltimaDeteccionUtc + reglas.Caducidad(Estado, Persistencia);
        AnotarTransicion(antes, UltimaDeteccionUtc);
    }

    // Devuelve si la acción vale desde el estado actual; Application arma su ResultadoModeracion
    public bool Moderar(AccionAdmin accion, DateTimeOffset ahora, ReglasAnomalia reglas)
    {
        if (!PermitidoDesde[accion].Contains(Estado)) return false;
        if (accion == AccionAdmin.MantenerDescartada && !PorRevisar) return false; // RN-07
        var antes = Estado;
        if (accion != AccionAdmin.MantenerDescartada)          // mantener: sin cambio de estado ni transición
            CambiarEstado(accion switch
            {
                AccionAdmin.MarcarReparada => EstadoAnomalia.Reparada,
                AccionAdmin.Descartar      => EstadoAnomalia.Descartada,
                _                          => EstadoAnomalia.Activa,   // reabrir: verificada por el administrador
            }, ahora);
        if (Estado == EstadoAnomalia.Activa) VenceUtc = ahora + reglas.Caducidad(Estado, Persistencia);   // reabrir: vence desde ahora
        MarcasRevision.Clear(); PorRevisar = false;
        AnotarTransicion(antes, ahora);
        return true;
    }

    // Una sola transición por operación: el paso interno por PendienteConfirmacion al nacer o
    // al volver a nacer no se registra si termina en Activa (así la tasa de confirmación no se infla)
    private void AnotarTransicion(EstadoAnomalia? antes, DateTimeOffset en)
    {
        if (antes != Estado) Transiciones.Add(new Transicion(antes, Estado, en));
        _nueva = false;
    }

    // RN-07: una Descartada absorbe reportes; ≥ 3 vehículos en 7 días → «Por revisar»
    private void AnotarParaRevision(ReporteAnomalia r, IReadOnlyList<ReporteAnomalia> ventana, ReglasAnomalia reglas)
    {
        if (reglas.Confirmacion.EsVehiculoNuevo(r, ventana)) MarcasRevision.Add(r.DetectadoUtc);
        MarcasRevision.RemoveAll(m => m < r.DetectadoUtc - reglas.VentanaRevision);
        PorRevisar = MarcasRevision.Count >= reglas.VehiculosRevision;
    }

    private void CambiarEstado(EstadoAnomalia nuevo, DateTimeOffset desde) => (Estado, EstadoDesdeUtc) = (nuevo, desde);
}
```

```csharp
// PoliticaConfirmacion.cs — RN-06, RN-14 y RN-15a en un solo lugar
internal sealed class PoliticaConfirmacion(AnomaliasOptions o)
{
    public EstadoAnomalia Decidir(IReadOnlyList<ReporteAnomalia> ventana, Severidad mostrada, bool porTope, DateTimeOffset ultima)
    {
        var recientes = ventana.Where(r => r.DetectadoUtc >= ultima.AddHours(-o.VentanaConfirmacionHoras)).ToList();
        var grave = EsAlta(mostrada) || recientes.Any(r => EsAlta(r.Severidad));
        if (!grave && !porTope) return EstadoAnomalia.Activa;                       // severidad 1–3: se publica
        return HayDosVehiculos(recientes, exigeSesion: grave)                       // RN-14 solo para lo grave
            ? EstadoAnomalia.Activa : EstadoAnomalia.PendienteConfirmacion;
    }

    // Distinto vehículo = distinto seudónimo y ≥ 3 min de separación (los celulares de un colectivo cuentan como uno)
    public bool EsVehiculoNuevo(ReporteAnomalia r, IReadOnlyList<ReporteAnomalia> ventana) =>
        ventana.Where(x => x.EventoId != r.EventoId && x.Seudonimo is not null)
               .All(x => SonVehiculosDistintos(x, r));

    private bool HayDosVehiculos(List<ReporteAnomalia> rs, bool exigeSesion) =>
        rs.Any(a => rs.Any(b => SonVehiculosDistintos(a, b) && (!exigeSesion || a.ConSesion || b.ConSesion)));

    private bool SonVehiculosDistintos(ReporteAnomalia a, ReporteAnomalia b) =>
        a.Seudonimo is { } sa && b.Seudonimo is { } sb && sa != sb &&
        (a.DetectadoUtc - b.DetectadoUtc).Duration() >= TimeSpan.FromMinutes(o.SeparacionMinimaMinutos);

    private bool EsAlta(Severidad s) => s.Valor >= o.SeveridadAltaDesde;
}
```

`ReglasAnomalia` es un `record` que agrupa lo que necesita el agregado: las políticas (`Confirmacion`, `Persistencia`) y los parámetros de `AnomaliasOptions` ya convertidos a `TimeSpan` (radio, ventanas, caducidades). Así el agregado recibe **un** parámetro en vez de diez.

**Por qué la revisión de una `Descartada` usa marcas y no seudónimos:** RN-07 cuenta vehículos en 7 días, pero el seudónimo se borra a las 72 h (RN-09). Por eso, cuando llega cada reporte (y su seudónimo todavía existe), se decide si es un vehículo nuevo y se guarda solo **la hora** en `marcas_revision`. Límite aceptado: un mismo celular que pasa por el punto en días distintos, con más de 72 h entre pasadas, puede contar dos veces. El efecto es solo pedirle al administrador que revise; nada se publica solo.

### 6.3 Caso de uso de Anomalías

```csharp
// ProcesarReporteUseCase.cs — coordina; las reglas viven en el dominio
internal sealed class ProcesarReporteUseCase(
    IAnomaliaRepository repositorio, ReglasCalidad calidad, PoliticaPlausibilidad plausibilidad,
    ReglasAnomalia reglas, IPublicadorEventos eventos, IMetricas metricas)
{
    // contexto = ContextoReporte(ConSesion, EventosEnLaHora, RecibidoUtc), armado por Ingesta
    public async Task<ResultadoProceso> EjecutarAsync(EventoReportado evento, ContextoReporte contexto, CancellationToken ct)
    {
        var reporte = ReporteAnomalia.Desde(evento, contexto);   // km_via con CorredorVia.KmSobreVia
        await using var tx = await repositorio.AbrirAsync(Celda.Alrededor(reporte.Ubicacion, reglas.Radio), ct);
        if (await repositorio.ExisteReporteAsync(reporte.EventoId, ct)) return ResultadoProceso.YaProcesado;

        var motivo = calidad.EvaluarIntrinseca(reporte, contexto.RecibidoUtc)   // mismo instante que al recibir
                  ?? plausibilidad.Evaluar(reporte, await repositorio.AnteriorDelSeudonimoAsync(reporte, ct));
        if (motivo is { } m) { metricas.RegistrarDescarte(m); return ResultadoProceso.Descartado(m); }

        var anomalia = await repositorio.MasCercanaAsync(reporte, ct)          // SELECT … FOR UPDATE: revalida el estado bajo el bloqueo
                    ?? Anomalia.Crear(reporte, porTope: plausibilidad.SuperaTope(contexto.EventosEnLaHora));
        var ventana = await repositorio.ReportesDesdeAsync(anomalia.Id, reporte.DetectadoUtc - reglas.VentanaMaxima, ct);
        anomalia.RegistrarReporte(reporte, [.. ventana, reporte], reglas);

        await repositorio.GuardarAsync(anomalia, reporte, ct);
        await tx.ConfirmarAsync(ct);
        await eventos.PublicarAsync(new AnomaliaCambiada(anomalia.Id, anomalia.Celda), ct);   // después del commit
        return ResultadoProceso.Procesado;
    }
}
```

`AbrirAsync` abre la transacción, la une a `IContextoTransaccion` y toma `pg_advisory_xact_lock` por cada celda en el orden recibido. `MasCercanaAsync` lee la anomalía con `SELECT … FOR UPDATE` dentro de ese bloqueo, así la caducidad (`FOR UPDATE SKIP LOCKED`, sección 3.8) y la moderación no pisan su cambio. Los bloqueos se liberan solos con el *commit* o el *rollback*.

### 6.4 Ingesta: un lote = una consulta

```csharp
// RecepcionEventosPostgres.cs — persiste los aceptados en un solo viaje a la BD (RNF-02)
internal sealed class RecepcionEventosPostgres(IngestaDbContext db) : IRecepcionEventos
{
    private const string Sql = """
        INSERT INTO ingesta.evento_recibido (evento_id, lote_id, recibido_utc, detectado_utc, seudonimo, con_sesion, payload)
        SELECT u.id, @lote, @recibido, u.detectado, u.seudonimo, @sesion, u.payload   -- @lote: UUIDv7 del servidor
        FROM unnest(@ids, @detectados, @seudonimos, @payloads) AS u(id, detectado, seudonimo, payload)
        ON CONFLICT (evento_id) DO NOTHING
        RETURNING evento_id AS "Value"
        """;

    public async Task<IReadOnlySet<Guid>> InsertarAsync(LoteAceptado lote, CancellationToken ct) =>
        (await db.Database.SqlQueryRaw<Guid>(Sql, lote.ComoParametros()).ToListAsync(ct)).ToHashSet();
        // ids devueltos = aceptados nuevos; el resto del lote aceptado = duplicados
}
```

### 6.5 Consulta de Mapa: *Decorator* sobre un puerto de un método

```csharp
internal interface IFuenteCeldas
{
    ValueTask<CeldaGeoJson> ObtenerAsync(Celda celda, Alcance alcance, CancellationToken ct);
}

// Decorator: misma interfaz, agrega la caché sin tocar FuenteCeldasAnomalias (abierto/cerrado)
internal sealed class FuenteCeldasCacheada(IFuenteCeldas origen, HybridCache cache) : IFuenteCeldas
{
    public ValueTask<CeldaGeoJson> ObtenerAsync(Celda celda, Alcance alcance, CancellationToken ct) =>
        cache.GetOrCreateAsync($"celda:{celda.Id}:{alcance}",
            token => origen.ObtenerAsync(celda, alcance, token),
            tags: [$"celda:{celda.Id}"], cancellationToken: ct);
}

// Observer: Anomalías avisa; Mapa invalida (Anomalías no sabe que esto existe)
internal sealed class InvalidarCeldasMapa(HybridCache cache, VersionesCelda versiones) : IManejador<AnomaliaCambiada>
{
    public async Task ManejarAsync(AnomaliaCambiada e, CancellationToken ct)
    {
        versiones.Incrementar(e.Celda);
        await cache.RemoveByTagAsync($"celda:{e.Celda.Id}", ct);
    }
}
```

`CeldaGeoJson` es un tipo `sealed` marcado con `[ImmutableObject(true)]` que guarda solo un `byte[]`: así HybridCache devuelve la misma instancia en cada acierto, sin copiarla. Registro en `MapaModule.cs`: `services.AddSingleton<IFuenteCeldas>(sp => new FuenteCeldasCacheada(new FuenteCeldasAnomalias(sp.GetRequiredService<IAnomaliaLectura>()), ...))`. Quitar la caché es borrar esa envoltura. `FuenteCeldasAnomalias` es un adaptador de una línea: pide las `Activa` de la celda a `IAnomaliaLectura` y las serializa a GeoJSON; Consulta de Mapa nunca escribe SQL.

### 6.6 Controlador: solo traduce HTTP

```csharp
[ApiController, Route("api/v1/anomalies")]
internal sealed class SyncController(RecibirLoteEventosUseCase recibir) : ControllerBase   // ResultadoLote es de Application
{
    [HttpPost("batch"), RequestSizeLimit(256 * 1024)]
    public async Task<IActionResult> Recibir(LoteDto dto, CancellationToken ct) =>
        await recibir.EjecutarAsync(dto.ALote(User), ct) switch
        {
            ResultadoLote.Aceptado r => Ok(RespuestaLoteDto.De(r)),
            ResultadoLote.Saturado s => this.Saturado(s.ReintentarEn),     // 503 + Retry-After
            _                        => BadRequest(),                      // lote ilegible, sin eventos, vacío o > 50
        };
}
```

### 6.7 Identidad y Accesos

| Tema | Diseño |
|---|---|
| Contraseñas | `PasswordHasher<Usuario>` de `Microsoft.Extensions.Identity.Core` (PBKDF2 con sal), sin adoptar todo ASP.NET Identity |
| Token de acceso | JWT de 5 min firmado con una clave del inventario de secretos (RNF-34); lleva solo `sub`, `rol` y `verificado` (correo verificado: Ingesta marca `con_sesion` solo si el rol es `ConductorRegistrado` y `verificado = true`, RN-14) |
| Token de actualización | 32 bytes aleatorios; se guarda su SHA-256. Cada uso rota el token dentro de la misma `familia_id`. Si se reusa uno ya usado dentro de 60 s, se emite un par nuevo; después de 60 s, se revoca toda la familia (ADR-009) |
| Desactivación | Revoca todas las familias del usuario; el JWT vigente muere solo en ≤ 5 min (RNF-26) |
| Bloqueo | `intentos_fallidos`, `nivel_bloqueo` y `bloqueado_hasta` en `usuario`, según RN-19; respuesta idéntica exista o no la cuenta |
| Auditoría | `IAuditoriaService.RegistrarAsync` dentro de la misma transacción de la acción, unida en `IContextoTransaccion` (si falta, lanza `InvalidOperationException`, ADR-013); el login fallido también entra en la transacción que actualiza `intentos_fallidos`. Las lecturas sensibles (lista y detalle de usuarios, `GET /usuarios?ids=`) usan `RegistrarLecturaAsync` |

## 7. App Android

### 7.1 Estructura

Un solo módulo Gradle organizado **por funcionalidad**, cada una con sus tres capas (`dominio`, `datos`, `ui`): `deteccion`, `sincronizacion`, `mapa`, `alertas`, `sesion`, `historial`, `actualizador`. Hilt conecta las implementaciones. Un solo módulo evita la complejidad de compilación de varios; la separación por paquetes y la regla de dependencia se revisan con Konsist o una prueba equivalente.

### 7.2 Base local (Room / SQLite)

```kotlin
@Entity(tableName = "evento", indices = [Index("estado", "detectadoUtc")])
data class EventoEntity(
    @PrimaryKey val eventoId: String,          // UUID generado en el celular (RF-03)
    val detectadoUtc: Long,                    // hora del fix GPS, en ms (RF-03)
    val lat: Double, val lon: Double, val precisionM: Float,
    val tipo: Int, val severidad: Int, val confidence: Float,
    val modeloVersion: String, val seudonimo: String,
    val estado: EstadoLocal,                   // PENDING · ENVIANDO · SYNCED · REJECTED_LOCAL · EXPIRADO_LOCAL
    val estadoDesde: Long,                     // para purgar estados finales a los 7 días
)

@Entity(tableName = "anomalia", indices = [Index("km")])
data class AnomaliaEntity(
    @PrimaryKey val id: String,
    val tipo: Int, val severidad: Int, val persistente: Boolean,
    val lat: Double, val lon: Double,
    val km: Double,                            // kmVia que envía el servidor (calculado al nacer la anomalía)
    val venceUtc: Long,                        // la envía el servidor (RN-07): sin recalcular reglas en el celular
)

@Entity(tableName = "recorrido")
data class RecorridoEntity(                    // historial: solo resúmenes, 6 meses (RF-25)
    @PrimaryKey(autoGenerate = true) val id: Long = 0,
    val inicioUtc: Long, val finUtc: Long, val kmDesde: Double, val kmHasta: Double, val eventos: Int,
)

@Dao
interface AnomaliaDao {
    @Query("SELECT * FROM anomalia WHERE km BETWEEN :desde AND :hasta AND venceUtc > :ahora")
    suspend fun enTramo(desde: Double, hasta: Double, ahora: Long): List<AnomaliaEntity>

    @Upsert suspend fun guardar(lista: List<AnomaliaEntity>)
    @Query("DELETE FROM anomalia WHERE id IN (:ids)") suspend fun borrar(ids: List<String>)   // bajas de updatedSince
}
```

| Dato | Dónde | Por qué |
|---|---|---|
| Eventos, anomalías locales, recorridos | Room (SQLite, modo WAL) | Consultas indexadas y transacciones |
| Seudónimo, categoría, «Solo ver el mapa», versiones aceptadas, última sincronización | DataStore | Ajustes pequeños de clave y valor |
| Tokens y permiso local (RN-21) | DataStore cifrado con Android Keystore | Datos sensibles |
| Mapa base (PMTiles), polilínea con kilómetros acumulados, modelo TFLite | Archivos verificados con SHA-256 | Archivos grandes y versionados por manifiesto |

**Presupuesto de almacenamiento (RNF-15, < 100 MB sin mapa base):** un evento ocupa ~150 B y se purga a los 7 días de su estado final; el corredor completo son unos miles de anomalías (< 1 MB); 6 meses de recorridos son < 1 MB. El total esperado es de unos pocos MB.

### 7.3 Alertas a lo largo de la vía (RN-16) sin geometría pesada

1. La **polilínea** del corredor se descarga con el kilómetro acumulado de cada vértice.
2. Cada anomalía descargada trae su `kmVia`, calculado **una vez** por el servidor al nacer con la misma polilínea; se guarda como `km`.
3. Con cada fix GPS (1 Hz), `Corredor.proyectar(fix)` busca el segmento cercano **a partir del último segmento usado** (costo casi constante) y devuelve el `km` actual y el **sentido** de la marcha. Si el vehículo está a más de 50 m de la vía, no hay alerta.
4. La consulta de alertas es un **rango por índice**: `km` entre la posición actual y la posición más la distancia de aviso, en el sentido de la marcha.

```kotlin
class ProximidadUseCase(private val corredor: Corredor, private val dao: AnomaliaDao, private val reloj: Clock) {
    private val avisadas = mutableMapOf<String, Double>()          // id → km, para «una vez por pasada»

    suspend fun evaluar(fix: Location, p: ParametrosAviso): List<AnomaliaEntity> {
        val pos = corredor.proyectar(fix) ?: return emptyList()    // fuera de la vía
        val alcanceKm = maxOf(p.minimoM, fix.speed * p.segundos) / 1000.0   // RN-16 (Strategy por categoría)
        avisadas.entries.removeIf { abs(it.value - pos.km) > p.finPasadaKm } // la pasada terminó (500 m)
        val (desde, hasta) = if (pos.sentido > 0) pos.km to pos.km + alcanceKm else pos.km - alcanceKm to pos.km
        return dao.enTramo(desde, hasta, reloj.millis()).filter { avisadas.putIfAbsent(it.id, it.km) == null }
    }
}
```

Si la lista trae más de una anomalía, `Alertas` emite **una sola** alerta que dice cuántas son (RN-16), con el sonido de RN-20.

### 7.4 Detección eficiente en batería (RNF-15)

| Técnica | Efecto |
|---|---|
| Sensores en lote (`registerListener` con `maxReportLatencyUs` ≈ 1 s) | El procesador despierta 1 vez por segundo, no 50 |
| Acelerómetro y giroscopio **apagados** por debajo de la velocidad de RN-03 o en las situaciones de RN-18 y RN-22 | Cero costo de sensores con el vehículo detenido |
| **Prefiltro de energía**: la ventana solo pasa al modelo si su variación vertical supera un umbral mínimo | En una pista lisa, el modelo casi no corre |
| GPS a 1 Hz con `LocationManagerCompat` (sin depender de Google Play Services) | Funciona en cualquier Android con GPS |
| Inferencia LiteRT con un intérprete reutilizado y *buffers* preasignados | Sin basura en memoria por ventana; < 50 ms (RNF-04) |

### 7.5 Sincronización

- `WorkManager` con un **trabajo único** (`sync`) y la restricción de red conectada. Además, un `NetworkCallback` lo lanza apenas vuelve la señal (≤ 60 s, RNF-33).
- Cada ejecución toma hasta 50 eventos `PENDING`, **los más antiguos primero** (índice `estado, detectadoUtc`), los marca `ENVIANDO` y aplica el contrato de RF-06.
- El backoff de RF-07 lo calcula la app y programa el siguiente trabajo con `setInitialDelay`, porque el backoff propio de WorkManager no sigue esa secuencia.
- Al iniciar la app, todo evento que quedó en `ENVIANDO` vuelve a `PENDING`.

## 8. Presupuesto de rendimiento frente a las métricas

| Métrica | Meta | Costo esperado por diseño | Dónde se mide |
|---|---|---|---|
| RNF-01 mapa | p95 < 250 ms (API) | Acierto: ~1 ms · 304: < 1 ms · fallo de caché: 1 consulta por B-tree (~5–20 ms) | Grafana p95 + k6 |
| RNF-02 lote | p95 < 300 ms | Reglas en memoria (< 1 ms) + 1 `INSERT` con `unnest` (~5–15 ms) | Grafana p95 |
| RNF-23 procesamiento | ≤ 100 ms por evento; retraso de cola ≤ 30 s p95 | ~5 consultas indexadas (~5–15 ms) | Métricas internas (`killauru.anomalias.proceso_ms`, `killauru.ingesta.retraso_cola_s`) |
| RNF-03 concurrencia | 300 dispositivos | Cola acotada + N trabajadores + bloqueo por celda | k6 fuera de la VM contra el entorno de pruebas: carga combinada, picos, estrés y resistencia |
| RNF-04 inferencia | p95 < 50 ms | MLP cuantizado: pocos ms | *Benchmark* en 3 celulares |
| RNF-15 batería y almacenamiento | ≤ 5 %/h · < 100 MB | Sensores en lote, prefiltro y purgas | Perfilado en campo |
| RNF-06 ningún evento perdido | 0 pérdidas | 200 solo tras el *commit*; barredor | Prueba de reinicio (EQ-09) y caos (`kill -9` bajo carga) |

## 9. Escalabilidad: qué cambia cuando el sistema crezca

| Disparador | Cambio | Qué **no** cambia |
|---|---|---|
| Más de una instancia de la API (S-03) | Nivel 2 de HybridCache con Redis; `VersionesCelda` en Redis; el barredor de cada instancia toma filas con `FOR UPDATE SKIP LOCKED` (la tabla de recepción ya es una cola durable) | Casos de uso, dominio y contratos |
| `reporte_anomalia` > ~100 millones de filas | Particionar por `detectado_utc` (la idempotencia ya la garantiza `evento_recibido`) | Consultas, que ya filtran por fecha |
| Analítica de tramos de v1.5 con consultas pesadas | Réplica de lectura o almacén analítico (ADR-014) | El esquema transaccional |
| Carga de lectura del mapa | Extraer Consulta de Mapa como servicio; `AnomaliaCambiada` pasa a un Outbox | Anomalías |
| Más corredores viales | `corredor_via` ya admite varias filas; la celda es global | El modelo de datos |

## 10. Pruebas clave del diseño

| Prueba | Tipo | Qué asegura |
|---|---|---|
| `PoliticaConfirmacion`: mismo colectivo (4 celulares, mismo segundo) → sigue pendiente; otro auto a los 3:00 min con sesión → `Activa`; a los 2:59 → pendiente | Unitaria | RN-06, RN-14 |
| `Anomalia`: `Expirada` + reporte de severidad 5 → `PendienteConfirmacion`; reporte detectado antes de vencer → sin cambios; `Reparada` + reporte detectado después de la reparación → vuelve a nacer; `MantenerDescartada` limpia las marcas sin transición | Unitaria | RN-07 |
| `Severidad.MedianaHaciaArriba`: {2, 3, 3, 5} → 3; {3, 4} → 4 | Unitaria | RN-05 |
| `Anomalia`: nace con severidad 1 → una transición `null → Activa` (nunca `Pendiente → Activa`); nace con severidad 5 y luego se confirma → `null → Pendiente` y `Pendiente → Activa` | Unitaria | RF-18 (tasa de confirmación) |
| Lote de 50 con 1 evento mal formado → 200 con 49 aceptados y 1 rechazado (con su `indice`; `id: null` si no trae `eventoId`); evento con otro seudónimo → `SeudonimoInconsistente` | API | RF-11, RN-11 |
| Mismo `bbox` con token y luego sin token a través de la caché → el anónimo recibe solo el tramo demo | Integración | RNF-27, EQ-11 |
| Matar el proceso a mitad de un lote → conciliación exacta | Caos | RNF-06, EQ-09 |
| Purga horaria → ningún seudónimo ni `lote_id` con más de 72 h en ninguna tabla y `recibido_utc` redondeado a la hora; purga de Identidad → ninguna sesión vencida o revocada de más de 30 días | Integración | RN-09, EQ-04 |
| Calendario de tareas: con `FakeTimeProvider`, cada tarea corre al arrancar y en su minuto u hora fija | Unitaria | RNF-22 |
| Ninguna clase de `Domain` usa EF Core, ASP.NET, `Contracts/` de otro módulo ni `DateTime.UtcNow`; Adapters no usa Domain; Application no usa Adapters ni Infrastructure; ningún archivo `.cs`, `.sql` de consultas, `.sh` ni `.js` supera 199 líneas (excepto migraciones) | Arquitectura | RNF-28 |
| Cobertura con coverlet ≥ 90 % en los proyectos `Domain`, en cada PR (GitHub Actions) | CI | RNF-28 |
| Carga combinada, picos, estrés, resistencia y caos con k6, fuera de la VM | Sistema | RNF-03, RNF-06, EQ-06 |

## 11. Trazabilidad

| Sección | Reglas y requisitos |
|---|---|
| 3 Base de datos | RN-05, RN-07, RN-09, RN-10, RN-12, RN-14, RN-15, RN-17, RN-19, RF-15, RF-18, RF-29, RF-37, RNF-18, RNF-31, RNF-34, DA-02 |
| 4 Caché | RF-14, RNF-01, RNF-27, ADR-006, EQ-11, EQ-14 |
| 5 Concurrencia | RNF-02, RNF-03, RNF-23, ADR-005, EQ-06, EQ-09 |
| 6 Código | RN-05, RN-06, RN-07, RN-11, RN-14, RN-15, ADR-002, ADR-009, ADR-012, ADR-013 |
| 7 App | RF-03, RF-06, RF-10, RF-26, RN-16, RN-20, RN-21, RN-22, RNF-15, RNF-33 |
