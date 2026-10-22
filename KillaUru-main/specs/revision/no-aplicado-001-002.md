# No aplicado (o aplicado con condición) en 001 y 002 — 2026-10-21

| Hallazgo / punto del plan | Motivo |
|---|---|
| `IContextoTransaccion.ConfirmarAsync` / `RevertirAsync` (001 data-model §3.3) | El plan pide `AlConfirmar` y «liberar tras commit/rollback», pero un `DbTransaction` no avisa cuándo se confirma. Se agregaron esos dos métodos (Decisión SDD, 001 R-005). **003, 004 y 005 deben confirmar con `ConfirmarAsync` del contexto** en vez de `tx.CommitAsync`; no se editaron por no ser de este dueño. |
| Log `CuentaEliminada {UsuarioId}` a Grafana (antiguo 002 R-009) | D9 lo vuelve innecesario (riesgo aceptado + revisión manual). Si 003 conserva una tarea que lo escribe, puede quitarse; no se tocó 003. |
| Rutas de sesión de 003 con meta p95 de 500 ms (001 FR-046) | Se toma de 003 SC-005 como meta SDD; docs (RNF-20) solo fija 250 ms global. Confirmar con el autor si se mantiene. |
| OPS-04 simulacro de recreación de la VM, OPS-13 cierre del puerto 22, OPS-23/24 (R2 y Pay As You Go), QA-11/SEG-07 (SSL Labs), SEG-21/22/24/25/26, ARQ-22 (pool/`postgresql.conf`), OPS-20 (override de desarrollo) | Fuera de la lista del plan para 001/002 o requieren decisión del autor; no se agregaron para no alterar otras cosas. El puerto 22 queda implícito en D10 (SSH por Cloudflare Access) pero sin tarea de cierre explícita. |
| SEG-13 (`UsuarioCreado` admin / `CuentaActivada` como riesgo) | Cambia el catálogo de riesgo de RNF-29; requiere decisión del autor. |
| Docs (`referencia/docs`) | Otro agente las actualiza; 001/002 siguen el plan (nombres `RegistrarLecturaAsync`, `AlConfirmar`, `retraso_cola_s`, techos ya coinciden con `componentes` §3). |
