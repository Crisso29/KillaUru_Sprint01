-- Solo datos sintéticos en killauru-pruebas. No ejecutar en producción.
-- psql -v confirmar_pruebas=true -v instante_utc='2026-10-23T12:00:00Z' -f auditoria-riesgo.sql
-- Insertar SQL NO emite métricas .NET: sirve para consulta/reconstrucción, no prueba las alertas.
\if :confirmar_pruebas
BEGIN;
INSERT INTO auditoria.registro (ocurrido_utc, actor_id, actor_rol, accion, objetivo, contexto)
VALUES
    (:'instante_utc'::timestamptz, NULL, NULL, 13, 'simulacro-exportacion', '{"formato":"CSV","filas":312,"simulacro":true}'),
    (:'instante_utc'::timestamptz, NULL, NULL, 2, 'simulacro-rol', '{"desde":"ConductorRegistrado","hacia":"Administrador","simulacro":true}');
INSERT INTO auditoria.registro (ocurrido_utc, actor_id, actor_rol, accion, objetivo, contexto)
SELECT :'instante_utc'::timestamptz, NULL, NULL, 7, 'simulacro-bloqueo', '{"duracionMinutos":5,"simulacro":true}'::jsonb
FROM generate_series(1, 5);
COMMIT;
\else
\echo 'No se confirmó el entorno de pruebas; no se insertaron datos.'
\quit 1
\endif
