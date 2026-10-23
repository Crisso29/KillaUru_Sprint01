-- RN-10 / D9: ejecutar contra la base ANTERIOR a restaurar, con acceso solo de lectura.
-- psql -v momento_respaldo='2026-10-23T02:00:00-05:00' -f eliminaciones-posteriores-respaldo.sql
-- El resultado contiene ids internos: conservarlo cifrado y fuera del repositorio.
SELECT objetivo AS usuario_id, ocurrido_utc
FROM auditoria.registro
WHERE accion = 5 AND ocurrido_utc > :'momento_respaldo'::timestamptz
ORDER BY ocurrido_utc, id;
