# Runbook · Simulacros y verificaciones periódicas (T054, T079, T081, FR-043)

Todos se hacen en el entorno de pruebas `killauru-pruebas` (R-11), nunca en producción.

## Calendario

| Cada | Verificación | Runbook | Meta |
|---|---|---|---|
| mes | restaurar el último respaldo | `restaurar.md` | RTO ≤ 2 h, 0 sesiones sin revocar, 0 seudónimos > 72 h |
| mes | revisión de privilegios (feature 003) | — | registro en la auditoría (RNF-30) |
| mes | costo y uso de Grafana | `monitoreo.md` | < US$ 8; < 80 % del plan |
| mes y antes de cada versión | carga, picos, estrés, resistencia, caos | `tests/carga/` | tabla de abajo |
| semestre | incidente de datos personales EQ-13 (002 T035) | [incidentes-datos-personales.md](incidentes-datos-personales.md) | borrador listo en ≤ 48 h; sin envío real durante el ejercicio |
| semestre | recrear la VM | `recrear-vm.md` | ≤ 4 h |

## Pruebas de sistema (D11) — dónde y cómo

- k6 corre **fuera de la VM** (PC del autor o `carga.yml`) contra `killauru-pruebas` (puerto 18080), en horario de poco tráfico.
- Error = todo 5xx; un 503 con `Retry-After` solo se acepta en `estres`; un 429 no debe aparecer (límites altos en `appsettings.Pruebas.json`).

| Prueba | Script | Criterio de aprobación |
|---|---|---|
| Carga | `carga-combinada.k6.js` | 300 sincronizando + 300 mapa, 10 min: lote p95 < 300 ms, mapa p95 < 250 ms, errores < 1 %, `retraso_cola_s` p95 ≤ 30 s |
| Picos | `picos.k6.js` | ráfaga de 600: errores < 1 %, la cola se vacía sola |
| Estrés | `estres.k6.js` | se anota el punto de quiebre (VU donde p95 > 2× la meta o errores > 1 %) |
| Resistencia | `resistencia.k6.js` | 4 h al 50 %: memoria estable, cola sin crecer |
| Caos | `caos.sh` | `kill -9` bajo carga: 0 eventos perdidos y 0 duplicados |

## Registro de resultados

| Fecha | Prueba | Resultado | Notas |
|---|---|---|---|
| | | | |

Para EQ-13, CRISOLOGO coordina y CRISSO lo reemplaza. Registrar hora de conocimiento, contención,
reconstrucción y borrador listo; adjuntar ids sintéticos y constancia de alertas. Primera ejecución y próxima fecha:
pendientes de programación en killauru-pruebas (002 T031). Repetir cada seis meses desde la ejecución documentada.
