-- V0000 · arranque · fase 1
-- Se ejecuta UNA sola vez, como superusuario, con `migrar.sh --arranque` (T-BD01).
-- Las contraseñas llegan como variables de psql desde el inventario de secretos (RNF-34):
--   psql -v pw_migrador=... -v pw_app=... -f V0000__arranque.sql

-- Roles: el migrador hace DDL solo durante el paso de migración (RNF-18); la app solo DML.
CREATE ROLE killauru_migrador LOGIN PASSWORD :'pw_migrador';
CREATE ROLE killauru_app      LOGIN PASSWORD :'pw_app';

CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS citext;   -- correo sin distinguir mayúsculas (feature 003)

REVOKE ALL ON DATABASE killauru FROM PUBLIC;
GRANT CONNECT ON DATABASE killauru TO killauru_app, killauru_migrador;
REVOKE CREATE ON SCHEMA public FROM PUBLIC;

-- Un esquema por módulo (ADR-012). Consulta de Mapa no tiene esquema: lee por el contrato de Anomalías.
CREATE SCHEMA identidad   AUTHORIZATION killauru_migrador;
CREATE SCHEMA ingesta     AUTHORIZATION killauru_migrador;
CREATE SCHEMA anomalias   AUTHORIZATION killauru_migrador;
CREATE SCHEMA auditoria   AUTHORIZATION killauru_migrador;
CREATE SCHEMA dashboard   AUTHORIZATION killauru_migrador;
CREATE SCHEMA migraciones AUTHORIZATION killauru_migrador;

-- La app usa los esquemas, pero no crea ni altera nada en ellos.
GRANT USAGE ON SCHEMA identidad, ingesta, anomalias, auditoria, dashboard TO killauru_app;
GRANT USAGE ON SCHEMA migraciones TO killauru_app;   -- solo lectura, para ChequeoMigraciones

-- Registro de migraciones aplicadas. Fase 1 = agregar; fase 2 = borrar en un despliegue posterior (RNF-18).
CREATE TABLE migraciones.aplicada (
    version      text        PRIMARY KEY,
    descripcion  text        NOT NULL,
    aplicada_utc timestamptz NOT NULL,
    fase         smallint    NOT NULL CHECK (fase IN (1, 2))
);
ALTER TABLE migraciones.aplicada OWNER TO killauru_migrador;
GRANT SELECT ON migraciones.aplicada TO killauru_app;

INSERT INTO migraciones.aplicada (version, descripcion, aplicada_utc, fase)
VALUES ('V0000', 'arranque', clock_timestamp(), 1);
