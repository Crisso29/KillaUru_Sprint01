-- Ejecutar con API detenida y psql -v instante_restauracion='2026-10-24T12:00:00Z'.
-- Revocar todas las familias antes de rotar KU_JWT_CLAVE_FIRMA y reabrir (D9).
UPDATE identidad.sesion_refresh
SET revocado_utc = date_trunc('hour', :'instante_restauracion'::timestamptz)
WHERE revocado_utc IS NULL;
