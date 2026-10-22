# No aplicado en 004-modulo-anomalias (revisión 2026-10-21)

Hallazgos de los informes sobre 004 que **no** se aplicaron, con su motivo (regla del plan: si choca con una decisión o altera algo mayor, no se aplica).

| Hallazgo | Qué pedía | Motivo |
|---|---|---|
| ARQ-14 | `ActivasPorCeldasAsync(IReadOnlyCollection<Celda>)` en vez de `long` | Sin decisión del autor ni fila en el plan; cambia el contrato que ya usa 006. Se deja `long` (la clave de la celda) |
| BD-02 opción (b) | Token de concurrencia `xmin` con reintentos | El plan (T-BD02) fija `SELECT … FOR UPDATE` + revalidación |
| BD-11 | Bloqueo consultivo con dos claves o prefijo reservado | No está en el plan; cambia ADR-005 y no hay otro bloqueo consultivo hoy |
| BD-18 (parte) | `TramosTotales` desde la polilínea | Aplicado solo como definición (`ceil(longitud)` = 333 con la traza actual, R-011); el indicador lo guarda 007 |
| OPS-05 (parte) | Tabla `public.tarea_ejecucion` para la última corrida | Lo resuelve T-OPS05: todas las tareas corren al arrancar y publican el *gauge* |
| OPS-06 | Calendario con otras horas (:10 caducidad, :20 purga, 02:30 histórica) | Gana el calendario canónico de T-OPS05 (`:05`, `:10`, 03:30) |
| OPS-19 (parte) | `CargadorCorredor` reintenta cada 30 s | Con D10 el despliegue migra antes de `up`; el corredor ya está en la BD al arrancar. Solo se alinea la comprobación `corredor` |
| PO-22 (parte) | Mover el dominio de moderación fuera de Foundational | El procesamiento usa `Descartada` y «Por revisar» (RN-07); se marca la fase 8 como recortable y se anota en `tasks.md` |
| PO-29 | Prueba de parámetros duplicados entre módulos | Es de 001 (configuración común), no de 004 |
| QA-22 | Escenario de ataque EQ-12 por HTTP con k6 | Lo cubren los scripts de carga de 001 (D11); 004 conserva T054 contra el servicio |
| ARQ-11 | Auditoría de accesos administrativos por un filtro del host | D15 decide que lo hace el módulo dueño y solo para lecturas con datos personales; el mapa, el detalle y las listas de anomalías no se registran |
