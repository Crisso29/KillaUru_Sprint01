# Research: Módulo Dashboard

## R-001 · Qué se guarda por día

- **Decision**: 9 filas por día, una por cada número de `ResumenDia` (004) más los conductores activos: `EventosRecibidos` (1), `ConductoresActivos` (2), `DispositivosDistintos` (3), `TramosConReportes7d` (4), `TramosTotales` (5), `Confirmaciones` (6), `PendientesExpiradas` (7), `Expiraciones` (8), `Renacimientos` (9). Las tasas y la cobertura se calculan al leer.
- **Rationale**: `diseño-interno` §3.4 ya define `(dia, indicador, valor)` con `indicador smallint`; guardar conteos permite recalcular cualquier periodo (plan, Complexity Tracking).

## R-002 · Hora del cálculo y recuperación

- **Decision**: la tarea corre al arrancar y cada día a las 00:30 de Lima (05:30 UTC); el próximo instante lo da `Shared/Tareas/ProgramaReloj` (001, T-OPS05), el mismo helper de todas las tareas por reloj. En cada corrida calcula los días que faltan entre `hoy − 2` y ayer (`DiasRecuperables = 2`).
- **Rationale**: el seudónimo de un reporte se purga a las 72 h **de recibido** (RN-09). Los primeros reportes del día D (00:00) se purgan el día D+3 a las 00:00; calcular D hasta el día D+2 a las 00:30 (48,5 h después del fin de D) es seguro, más tarde ya no (`DispositivosDistintos` saldría bajo).
- **Límite aceptado**: «conductores activos» de un día recuperado tarde se cuenta en el momento del cálculo (Identidad solo sabe contar «ahora»); la diferencia es de 1–2 días en una ventana móvil de 7.
- **Confirmado (D1)**: `DiasRecuperables = 2` y el límite aceptado pasan a `diseño-interno` §3.8.

## R-003 · Valor, comparación y variación de cada indicador

- **Decision** (`CalculadoraIndicadores`), con D = último día cerrado:

| Indicador | Valor | Comparación | Variación | Serie (7 d) |
|---|---|---|---|---|
| Eventos válidos (ayer) | `EventosRecibidos(D)` | promedio D−7…D−1 | % relativo | diaria |
| Conductores activos | `ConductoresActivos(D)` | promedio D−7…D−1 | diferencia absoluta | diaria |
| Dispositivos | `DispositivosDistintos(D)` | promedio D−7…D−1 | diferencia absoluta | diaria |
| Cobertura (%) | `Tramos7d(D) ÷ TramosTotales(D) × 100` | promedio D−7…D−1 | puntos | diaria |
| Confirmación (%) | `ΣConf ÷ (ΣConf + ΣPendExp) × 100`, D−6…D | lo mismo, D−13…D−7 | puntos | tasa diaria |
| Reaparición (%) | `ΣRenac ÷ ΣExp × 100`, D−6…D | lo mismo, D−13…D−7 | puntos | tasa diaria |

- **Rationale**: el prototipo muestra «+8,1 % vs. 7 d previos» para eventos y «+4 pts» para la cobertura; RF-18 define las tasas como proporciones de transiciones (`diseño-interno` §3.8).
- **Redondeo**: valores en % con 1 decimal; promedios con 1 decimal; los conteos del día, enteros.
- **Confirmado (D1)**: las fórmulas pasan a `diseño-interno` §3.8. Reaparición = anomalía que vuelve a nacer después de expirar, hacia `Activa` o `PendienteConfirmacion` (D14).

## R-004 · Escritura idempotente

- **Decision**: `GuardarDiaAsync(DiaIndicadores)` inserta las 9 filas en una transacción con `ON CONFLICT (dia, indicador) DO NOTHING`. Un día se considera guardado si tiene sus 9 filas.
- **Rationale**: un reinicio a mitad del cálculo no deja un día a medias; repetir el cálculo no cambia la historia.

## R-005 · «Eventos válidos (ayer)»

- **Decision** (D13, D14): el resumen muestra el **último día cerrado** (ayer) y lo rotula «Eventos válidos (ayer)»; el prototipo pasa de «Eventos hoy» a «Eventos válidos (ayer)». «Eventos válidos» = reportes procesados por Anomalías (`ResumenDia.EventosRecibidos`), como dice RF-18 corregido. No se calcula «hoy» en vivo.
- **Rationale**: RF-18 y `diseño-interno` §3.8 hablan de un cálculo diario guardado; calcular el día en curso en cada petición sería otra consulta sobre `reporte_anomalia` (de Anomalías) en cada carga del panel.
- **Alternativa rechazada**: cálculo en vivo con `ResumenDelDiaAsync(hoy)` y caché de 5 min.

## R-006 · Enlace a los paneles técnicos

- **Decision**: `DashboardOptions.UrlPanelesTecnicos` (p. ej. `https://killauru.grafana.net/d/api`); la API solo lo devuelve, no consulta Grafana.
- **Rationale**: HU-13 «desde el panel accedo a los paneles técnicos»; ADR-010 deja lo técnico en Grafana Cloud.

## R-007 · Capas: qué va en Domain y qué en Application (D4)

- **Decision**: el dominio no usa contratos de otro módulo ni tipos que vea el controlador. `DiaIndicadores` se construye con valores simples (día + 9 `long`); el mapeo `ResumenDia` (004) → `DiaIndicadores` vive en `Application/MapeoResumenDia.cs`. `CalculadoraIndicadores` (Domain) devuelve `MedidaIndicador` (valor, comparación, variación, serie); `IndicadorResumen` (con la unidad) y `ResumenDashboard` son de **Application**, que es lo único que usa el controlador.
- **Rationale**: `enfoque` §3 (Adapters solo usa Application; Domain solo `Shared/Domain` y la BCL) y la prueba de arquitectura de 001 (ARQ-01, ARQ-02).

## R-008 · Desfase horario duplicado

- **Decision**: `DashboardOptions.DesfaseHorarioHoras` repite el valor de `AnomaliasOptions` porque un módulo no lee la configuración de otro (`enfoque` §5). Una prueba compara los dos valores (como en 005).
- **Rationale**: si difieren, Dashboard pediría a `ResumenDelDiaAsync` un día local distinto del que calcula Anomalías (ARQ-13).

## Confirmados por el autor el 2026-10-21

1. R-002 `DiasRecuperables = 2` y el límite aceptado de «conductores activos» — D1.
2. R-003 fórmulas de comparación (promedio de 7 días previos; tasas por sumas) — D1; reaparición según D14.
3. R-005 «Eventos válidos (ayer)», último día cerrado; el prototipo dice «Eventos válidos (ayer)» — D13, D14.
