CREATE TABLE identidad.usuario (
    id                        uuid        PRIMARY KEY,                 -- UUIDv7
    correo                    citext      NOT NULL UNIQUE,
    hash_contrasena           text        NULL,                        -- (+) NULL = invitación pendiente. PBKDF2, nunca en claro
    rol                       smallint    NOT NULL CHECK (rol IN (1, 2)),   -- 1 ConductorRegistrado · 2 Administrador
    activo                    boolean     NOT NULL DEFAULT true,
    correo_verificado         boolean     NOT NULL DEFAULT false,
    intentos_fallidos         smallint    NOT NULL DEFAULT 0,          -- RN-19
    ventana_fallos_desde_utc  timestamptz NULL,                        -- (+) inicio de la ventana de 15 min
    nivel_bloqueo             smallint    NOT NULL DEFAULT 0,
    bloqueado_hasta           timestamptz NULL,
    creado_utc                timestamptz NOT NULL                     -- truncado al día (D3)
);

CREATE TABLE identidad.sesion_refresh (
    id                   uuid        PRIMARY KEY,
    usuario_id           uuid        NOT NULL REFERENCES identidad.usuario (id) ON DELETE CASCADE,
    familia_id           uuid        NOT NULL,                         -- todos los tokens de una misma sesión
    hash_token           bytea       NOT NULL UNIQUE,                  -- SHA-256 del token
    creado_utc           timestamptz NOT NULL,                         -- (+) para RN-17 (renovó en los últimos N días); truncado a la hora (D3)
    expira_utc           timestamptz NOT NULL,                         -- truncado hacia abajo a la hora (D3)
    limite_absoluto_utc  timestamptz NULL,                             -- (+) inicio + 8 h (admin); NULL (conductor); exacto
    usado_utc            timestamptz NULL,                             -- exacto (gracia de 60 s); la purga lo trunca a la hora pasados 2 min (D3)
    revocado_utc         timestamptz NULL                              -- truncado a la hora (D3)
);
CREATE INDEX ix_sesion_usuario_fk ON identidad.sesion_refresh (usuario_id);       -- completo: sirve al ON DELETE CASCADE (BD-05)
CREATE INDEX ix_sesion_familia ON identidad.sesion_refresh (familia_id);
CREATE INDEX ix_sesion_creado  ON identidad.sesion_refresh (creado_utc);          -- (+) RN-17
CREATE INDEX ix_sesion_expira  ON identidad.sesion_refresh (expira_utc);          -- (+) purga (D3)

CREATE TABLE identidad.consentimiento (
    usuario_id   uuid        NOT NULL REFERENCES identidad.usuario (id) ON DELETE CASCADE,
    documento    smallint    NOT NULL CHECK (documento IN (1, 2)),     -- 1 Política de privacidad · 2 Términos de uso
    version      text        NOT NULL,
    aceptado_utc timestamptz NOT NULL,                 -- truncado al día (D3): no revela la hora del registro
    PRIMARY KEY (usuario_id, documento, version)                       -- RF-29, RF-37
);

CREATE TABLE identidad.token_correo (
    hash_token  bytea       PRIMARY KEY,
    usuario_id  uuid        NOT NULL REFERENCES identidad.usuario (id) ON DELETE CASCADE,
    proposito   smallint    NOT NULL CHECK (proposito IN (1, 2, 3)),   -- 1 Verificación 24 h · 2 Recuperación 30 min · (+) 3 Invitación 24 h
    expira_utc  timestamptz NOT NULL,
    usado_utc   timestamptz NULL
);
CREATE INDEX ix_token_usuario ON identidad.token_correo (usuario_id, proposito) WHERE usado_utc IS NULL;
CREATE INDEX ix_token_usuario_fk ON identidad.token_correo (usuario_id);          -- completo: sirve al ON DELETE CASCADE (BD-05)

CREATE TABLE identidad.revision_privilegios (                          -- (+) RNF-30
    id             bigint      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    realizada_utc  timestamptz NOT NULL,
    actor_id       uuid        NOT NULL,                               -- sin FK: el actor puede ser eliminado después
    confirmados    smallint    NOT NULL,
    retirados      smallint    NOT NULL
);

