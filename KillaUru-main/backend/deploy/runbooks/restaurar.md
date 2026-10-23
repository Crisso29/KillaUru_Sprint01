# Runbook · Restaurar un respaldo (RTO ≤ 2 h, RPO ≤ 24 h, RNF-24, D9)

1. **Detener la API** para que nadie escriba: `docker compose stop api`.
   Antes de sustituir la base, extraer las eliminaciones posteriores al respaldo si todavía puede leerse (sección siguiente).
2. **Bajar el respaldo** de R2 (`rclone copyto r2:<bucket>/killauru-<marca>.tar.age .`) en un equipo que tenga la **clave privada** de age (nunca está en la VM).
3. **Descifrar**: `age -d -i clave-privada.txt killauru-<marca>.tar.age | tar -xf -` → `roles.sql` y `killauru.dump`.
4. **Base limpia**: `docker compose down postgres && docker volume rm killauru_pgdata && docker compose up -d postgres`.
5. **Roles primero, luego datos**: `psql -U postgres -f roles.sql` y `pg_restore -U postgres -d killauru --no-owner --role=killauru_migrador killauru.dump`.
6. **Pasos de D9, antes de reabrir**:
   - Revocar todas las sesiones: `UPDATE identidad.sesion_refresh SET revocado_utc = now() WHERE revocado_utc IS NULL;`
   - Rotar `KU_JWT_CLAVE_FIRMA` en `.env` (todos vuelven a iniciar sesión una vez).
   - Arrancar la API: `docker compose up -d api`. Las purgas corren **al arrancar** (T-OPS05).
   - Comprobar 0 seudónimos con más de 72 h: `SELECT count(*) FROM ingesta.evento_recibido WHERE seudonimo IS NOT NULL AND recibido_utc < now() - interval '72 hours';` (y lo mismo en `anomalias.reporte_anomalia`).
7. **Riesgo aceptado (D9)**: las eliminaciones de cuenta, desactivaciones y cambios de rol hechos después del respaldo se pierden. Revisar a mano las solicitudes de eliminación del último día (correo de contacto) y repetirlas.
8. Anotar tiempos y resultado en `simulacros.md`.

## Eliminaciones posteriores al respaldo (002 T029, RN-10)

Antes de reemplazar una BD todavía legible, ejecutar
[`eliminaciones-posteriores-respaldo.sql`](../../db/consultas/eliminaciones-posteriores-respaldo.sql)
contra esa base anterior, indicando la hora exacta del respaldo en la variable psql `momento_respaldo`.
Guardar el resultado cifrado y fuera de Git. Después de recuperar, repetir las eliminaciones mediante
Identidad (003) y verificar su nueva auditoría antes de reabrir. No borrar directamente la auditoría.

Si la base anterior se perdió, la consulta al respaldo no puede reconstruir lo que ocurrió después:
revisar manualmente las solicitudes de eliminación del último día con CRISOLOGO o CRISSO.
Persisten el riesgo aceptado de D9 y el límite de RPO de 24 h. Registrar las solicitudes pendientes de confirmar.
La revocación de sesiones y las purgas de los pasos anteriores requieren que 003–005 ya estén implementados.
