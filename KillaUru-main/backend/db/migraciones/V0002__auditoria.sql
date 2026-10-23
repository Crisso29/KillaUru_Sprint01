-- V0002 · auditoria · fase 1
-- Rastro permanente: solo el migrador cambia el esquema; la aplicación únicamente inserta y consulta.
CREATE TABLE auditoria.registro (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ocurrido_utc timestamptz NOT NULL,
    actor_id uuid NULL,
    actor_rol smallint NULL CHECK (actor_rol IN (1, 2)),
    accion smallint NOT NULL CHECK (accion BETWEEN 1 AND 14),
    objetivo text NULL CHECK (length(objetivo) <= 100),
    contexto jsonb NOT NULL DEFAULT '{}'
);
CREATE INDEX ix_registro_fecha ON auditoria.registro (ocurrido_utc);
CREATE INDEX ix_registro_actor ON auditoria.registro (actor_id, ocurrido_utc);
CREATE INDEX ix_registro_accion ON auditoria.registro (accion, ocurrido_utc);
REVOKE UPDATE, DELETE, TRUNCATE ON auditoria.registro FROM killauru_app;
