# Research: Módulo Dashboard

## R-001 · Qué se guarda por día

- **Decision**: 9 filas por día, una por cada número de `ResumenDia` (004) más los conductores activos: `EventosRecibidos` (1), `ConductoresActivos` (2), `DispositivosDistintos` (3), `TramosConReportes7d` (4), `TramosTotales` (5), `Confirmaciones` (6), `PendientesExpiradas` (7), `Expiraciones` (8), `Renacimientos` (9). Las tasas y la cobertura se calculan al leer.
- **Rationale**: `diseño-interno` §3.4 ya define `(dia, indicador, valor)` con `indicador smallint`; guardar conteos permite recalcular cualquier periodo (plan, Complexity Tracking).

## R-002 · Hora del cálculo y recuperación

- **Decision**: la tarea corre al arrancar y cada día a las 00:30 de Lima (05:30 UTC). En cada corrida calcula los días que faltan entre `hoy − 2` y ayer (`DiasRecuperables = 2`).
- **Rationale**: el seudónimo de un reporte se purga a las 72 h **de recibido** (RN-09). Los primeros reportes del día D (00:00) se purgan el día D+3 a las 00:00; calcular D hasta el día D+2 a las 00:30 (48,5 h después del fin de D) es seguro, más tarde ya no (`DispositivosDistintos` saldría bajo).
- **Límite aceptado**: «conductores activos» de un día recuperado tarde se cuenta en el momento del cálculo (Identidad solo sabe contar «ahora»); la diferencia es de 1–2 días en una ventana móvil de 7.

## R-003 · Valor, comparación y variación de cada indicador

- **Decision** (`CalculadoraIndicadores`), con D = último día cerrado:

| Indicador | Valor | Comparación | Variación | Serie (7 d) |
|---|---|---|---|---|
| Eventos por día | `EventosRecibidos(D)` | promedio D−7…D−1 | % relativo | diaria |
| Conductores activos | `ConductoresActivos(D)` | promedio D−7…D−1 | diferencia absoluta | diaria |
| Dispositivos | `DispositivosDistintos(D)` | promedio D−7…D−1 | diferencia absoluta | diaria |
| Cobertura (%) | `Tramos7d(D) ÷ TramosTotales(D) × 100` | promedio D−7…D−1 | puntos | diaria |
| Confirmación (%) | `ΣConf ÷ (ΣConf + ΣPendExp) × 100`, D−6…D | lo mismo, D−13…D−7 | puntos | tasa diaria |
| Reaparición (%) | `ΣRenac ÷ ΣExp × 100`, D−6…D | lo mismo, D−13…D−7 | puntos | tasa diaria |

- **Rationale**: el prototipo muestra «+8,1 % vs. 7 d previos» para eventos y «+4 pts» para la cobertura; RF-18 define las tasas como proporciones de transiciones (`diseño-interno` §3.8).
- **Redondeo**: valores en % con 1 decimal; promedios con 1 decimal; los conteos del día, enteros.

## R-004 · Escritura idempotente

- **Decision**: `GuardarDiaAsync(DiaIndicadores)` inserta las 9 filas en una transacción con `ON CONFLICT (dia, indicador) DO NOTHING`. Un día se considera guardado si tiene sus 9 filas.
- **Rationale**: un reinicio a mitad del cálculo no deja un día a medias; repetir el cálculo no cambia la historia.

## R-005 · El prototipo dice «Eventos hoy»

- **Decision**: el resumen muestra el **último día cerrado** (ayer). No se calcula «hoy» en vivo.
- **Rationale**: RF-18 y `diseño-interno` §3.8 hablan de un cálculo diario guardado; calcular el día en curso en cada petición sería otra consulta sobre `reporte_anomalia` (de Anomalías) en cada carga del panel.
- **Pendiente para el autor**: cambiar la etiqueta del prototipo a «Eventos ayer» o aceptar el cálculo en vivo (agregaría `ResumenDelDiaAsync(hoy)` en cada petición, con caché de 5 min).

## R-006 · Enlace a los paneles técnicos

- **Decision**: `DashboardOptions.UrlPanelesTecnicos` (p. ej. `https://killauru.grafana.net/d/api`); la API solo lo devuelve, no consulta Grafana.
- **Rationale**: HU-13 «desde el panel accedo a los paneles técnicos»; ADR-010 deja lo técnico en Grafana Cloud.

## Pendientes de confirmar por el autor

1. R-002 `DiasRecuperables = 2` y el límite aceptado de «conductores activos».
2. R-003 fórmulas de comparación (promedio de 7 días previos; tasas por sumas).
3. R-005 «Eventos hoy» del prototipo vs. último día cerrado.
