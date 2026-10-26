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
