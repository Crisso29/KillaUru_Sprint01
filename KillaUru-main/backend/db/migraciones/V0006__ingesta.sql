CREATE TABLE ingesta.evento_recibido (
    evento_id      uuid        PRIMARY KEY,
    lote_id        uuid        NULL,
    recibido_utc   timestamptz NOT NULL,
    detectado_utc  timestamptz NOT NULL,
    seudonimo      uuid        NULL,
    con_sesion     boolean     NOT NULL,
    estado         smallint    NOT NULL DEFAULT 0 CHECK (estado IN (0, 1, 2)),
    motivo         smallint    NULL CHECK (motivo BETWEEN 1 AND 8),
    payload        jsonb       NOT NULL
);
CREATE INDEX ix_recibido_pendiente ON ingesta.evento_recibido (recibido_utc) WHERE estado = 0;
CREATE INDEX ix_recibido_tope      ON ingesta.evento_recibido (seudonimo, detectado_utc)
                                    WHERE seudonimo IS NOT NULL;
CREATE INDEX ix_recibido_fecha     ON ingesta.evento_recibido (recibido_utc);
CREATE INDEX ix_recibido_purga     ON ingesta.evento_recibido (recibido_utc) WHERE seudonimo IS NOT NULL;
CREATE INDEX ix_recibido_lote      ON ingesta.evento_recibido (lote_id) WHERE estado = 0;
