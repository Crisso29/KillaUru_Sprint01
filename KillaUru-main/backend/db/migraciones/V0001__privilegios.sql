-- V0001 · privilegios · fase 1
-- Se ejecuta con killauru_migrador (`migrar.sh`): todo lo que el migrador cree después
-- queda usable por la app sin un GRANT por tabla.

ALTER DEFAULT PRIVILEGES FOR ROLE killauru_migrador IN SCHEMA identidad, ingesta, anomalias, dashboard
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO killauru_app;

-- Auditoría es append-only (RNF-31): la app solo inserta y consulta. El detalle está en la feature 002.
ALTER DEFAULT PRIVILEGES FOR ROLE killauru_migrador IN SCHEMA auditoria
    GRANT SELECT, INSERT ON TABLES TO killauru_app;

ALTER DEFAULT PRIVILEGES FOR ROLE killauru_migrador IN SCHEMA identidad, ingesta, anomalias, auditoria, dashboard
    GRANT USAGE, SELECT ON SEQUENCES TO killauru_app;
