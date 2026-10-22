# No aplicado en 005-modulo-ingesta (2026-10-21)

Hallazgos de los informes sobre 005 que no se aplicaron, con su motivo. Lo decidido por el autor (D2, D3, D4, D5, D8, D11) y las correcciones T-BD06, T-BD-CHK, T-OPS05 y T-ARQ08 sí están aplicados.

| Hallazgo | Qué proponía | Motivo |
|---|---|---|
| BD-08 (parte) | `CHECK ((estado = 2) = (motivo IS NOT NULL))` | El plan canónico (T-BD-CHK) solo pide `motivo BETWEEN 1 AND 8`; no se agrega más para no alterar otras cosas |
| BD-12 | Repartir lotes por `hash(seudonimo)` o bloqueo por seudónimo para RN-15b entre lotes | Sin decisión del autor; con D2 cada lote tiene un solo seudónimo, pero dos lotes del mismo seudónimo pueden seguir procesándose en paralelo. Queda como límite conocido |
| OPS-06 | Calendario 02:00 recepción, :40 seudónimos de recepción | Gana T-OPS05 (03:00 y :10) |
| OPS-07 | Nombre `purga-seudonimos-anomalias` | Gana T-OPS05 (`purga-seudonimos` en 004; `purga-seudonimos-recepcion` en 005) |
| QA-06 / OPS-15 | Histograma `…retraso_cola_ms` | Gana D11: `killauru.ingesta.retraso_cola_s` |
| QA-08, QA-21 | Prueba de caos y corte de red propios de 005 (T027b) | D11 deja carga y caos en 001 (`caos.md` + `caos.sh`); 005 los referencia (T035) |
| QA-03 | Script `rnf03.k6.js` | Gana D11: `carga-combinada.k6.js` de 001 |
| PO-26 | Un solo nombre `Ingesta:AntiguedadMaximaColaMinutos` | 005 ya usa ese nombre; el cambio es de 001 |
| PO-27 | Renombrar `LoteRecibido` (choca con el glosario «Lote») | Sin decisión del autor; renombrar tocaría el glosario. Se mantiene |
| R-002 (resto) | Cómo trata la app el 413 | D5 no lo decide; queda para 008-app-android |
| SEG-05 | Borrar o reprocesar filas `Recibido` > 30 días | Por pedido del autor se mantiene R-008 y se anota como riesgo con alerta (> 72 h) y reproceso manual (T040) |
