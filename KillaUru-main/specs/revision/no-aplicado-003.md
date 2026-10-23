# No aplicado en 003-modulo-identidad-accesos (2026-10-21)

Hallazgos de los informes `revision-*.md` sobre 003 que **no** se aplicaron al aplicar D1…D17, con su motivo.

| Hallazgo | Qué pedía | Motivo |
|---|---|---|
| SEG-13 | Tratar como acción de riesgo (alerta) `UsuarioCreado` con rol Administrador y `CuentaActivada` de un administrador | RNF-29 fija el catálogo de acciones de riesgo: exportación, cambio de rol y ≥ 5 bloqueos en 1 h. Crear o reactivar un administrador no está en ese catálogo, y `EsDeRiesgo` vive en el catálogo de 002 por acción, no por contexto. Aplicarlo exige ampliar RNF-29 en docs y cambiar 002: queda para decisión del autor. 003 ya guarda el rol en el contexto de la auditoría, así que se puede activar sin cambiar 003 |
| SEG-02 (parte) | Borrar cada fila usada al terminar la gracia; guardar solo `ultima_actividad_dia` | Reemplazado por D3 (horas truncadas + purga a 30 días) |
| SEG-06 / PO-30 | 202 genérico en el registro; hash ficticio e igualar tiempos | D8: el 409 es riesgo aceptado, mitigado por el tope de 5 registros/h por IP. La igualación de tiempos del login no tiene decisión y cambiaría el flujo |
| SEG-15 | Cerrar sesión autenticado con el token de actualización | Cambia la autenticación de la operación; sin decisión del autor. Solo se agregó el 429 (T-SEG19) |
| SEG-17 | Contar la contraseña incorrecta de `DELETE /me` como intento de RN-19 | Cambia RN-19 (que solo habla del inicio de sesión); sin decisión. Solo se agregó el 429 |
| SEG-21 | Prefijo `__Host-` en las cookies y verificación de `Origin` | Afecta a 001 (CORS y cookies comunes); no es de 003 solo |
| SEG-23 | Métrica o alerta si vence la revisión mensual | Se cubrió con el calendario del *runbook* (T-QA14, decisión canónica); una métrica nueva no está en el plan |
| SEG-25 | Validar correos en el texto libre de la auditoría | Es del módulo Auditoría (002) |
| SEG-11 | Diario de auditoría en R2 | D9 acepta la pérdida acotada por el RPO de 24 h |
| BD-20 | Mover la semilla del primer administrador a `db/arranque/` | Ya no hay semilla SQL: la reemplaza el comando `crear-admin` (T-SEG14) |
| QA-19 | Marcar como SDD las metas de 500 ms / 50 ms y definir el perfil de T055 | Sin decisión del autor; no se tocan metas |
| D15 «detalle de usuarios» | Auditar el detalle de un usuario | 003 no tiene endpoint de detalle; se auditan la lista, `GET /usuarios?ids=` y el estado de la revisión de privilegios (lista de administradores) |
