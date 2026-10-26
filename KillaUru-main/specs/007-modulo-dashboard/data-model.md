# Data Model: Módulo Dashboard

## 1. Tabla (`V0007__dashboard.sql`)

Igual a `diseño-interno-de-modulos.md` §3.4, más dos `CHECK` (marcados `(+)`):

```sql
CREATE TABLE dashboard.indicador_diario (
    dia        date     NOT NULL,                          -- día local (UTC−5)
    indicador  smallint NOT NULL CHECK (indicador BETWEEN 1 AND 9),   -- (+)
    valor      numeric  NOT NULL CHECK (valor >= 0),                -- (+)
    PRIMARY KEY (dia, indicador)                           -- solo números agregados (RN-17)
);
```

Sin índices extra: la llave primaria sirve para leer un rango de días y para la purga (`WHERE dia < @limite`).

## 2. Catálogo de indicadores guardados (`Indicador`)

| Código | Nombre | Origen | Definición |
|:---:|---|---|---|
| 1 | `EventosRecibidos` | `ResumenDia.EventosRecibidos` (004) | Eventos válidos: reportes procesados por Anomalías en el día local (D14) |
| 2 | `ConductoresActivos` | `IIdentidadConsultas.ContarConductoresActivosAsync(7)` (003) | Cuentas que renovaron su sesión en 7 días (RN-17) |
| 3 | `DispositivosDistintos` | `ResumenDia.DispositivosDistintos` | Seudónimos distintos con reportes ese día; solo el número |
| 4 | `TramosConReportes7d` | `ResumenDia.TramosConReportes7d` | Tramos de 1 km con ≥ 1 reporte en los 7 días que terminan ese día |
| 5 | `TramosTotales` | `ResumenDia.TramosTotales` | 336 |
| 6 | `Confirmaciones` | `ResumenDia.Confirmaciones` | Transiciones `PendienteConfirmacion → Activa` del día |
| 7 | `PendientesExpiradas` | `ResumenDia.PendientesExpiradas` | `PendienteConfirmacion → Expirada` |
| 8 | `Expiraciones` | `ResumenDia.Expiraciones` | `→ Expirada` |
| 9 | `Renacimientos` | `ResumenDia.Renacimientos` | Reapariciones: anomalías que vuelven a nacer después de expirar (`Expirada → Activa \| PendienteConfirmacion`, D14) |

## 3. Indicadores mostrados (`CalculadoraIndicadores`, `research.md` R-003)

| Clave en la API | Unidad | Fórmula del valor | Variación |
|---|---|---|---|
| `eventosDia` | eventos | `[1](D)`; el panel lo rotula «Eventos válidos (ayer)» (D13) | % vs. promedio de 7 días previos |
| `conductoresActivos` | cuentas | `[2](D)` | diferencia vs. promedio |
| `dispositivosDia` | dispositivos | `[3](D)` | diferencia vs. promedio |
| `coberturaPct` | % | `[4](D) / [5](D) × 100` | puntos vs. promedio |
| `confirmacionPct` | % | `Σ[6] / (Σ[6] + Σ[7]) × 100` en 7 días | puntos vs. 7 días previos |
| `reaparicionPct` | % | `Σ[9] / Σ[8] × 100` en 7 días | puntos vs. 7 días previos |

`D` = último día con sus 9 filas. Si un día falta, su valor es `null` y no entra en promedios ni sumas.

## 4. Tipos

| Tipo | Capa | Forma |
|---|---|---|
| `DiaIndicadores` | Domain | `DateOnly Dia` + 9 `long` (valores simples, sin tipos de otro módulo) · `IEnumerable<(Indicador, decimal)> ComoFilas()` |
| `MedidaIndicador` | Domain | `decimal? Valor`, `decimal? Comparacion`, `decimal? Variacion`, `IReadOnlyList<decimal?> Serie` (7) |
| `MapeoResumenDia` | Application | `static DiaIndicadores Desde(ResumenDia r, int conductoresActivos)` (D4) |
| `IndicadorResumen` | Application | `MedidaIndicador` + `string Unidad` (D4) |
| `ResumenDashboard` | Application | `DateOnly? UltimoDiaCerrado`, `DateTimeOffset? CalculadoUtc`, 6 `IndicadorResumen`, `IReadOnlyList<(DateOnly Dia, long? Eventos, long? EventosAnterior)> EventosPorDia`, `string UrlPanelesTecnicos` (D4) |

## 5. Puerto

| Puerto | Operaciones |
|---|---|
| `IIndicadoresRepositorio` | `Task<IReadOnlySet<DateOnly>> DiasCompletosAsync(DateOnly desde, DateOnly hasta)` · `Task GuardarDiaAsync(DiaIndicadores dia)` (transacción, `ON CONFLICT DO NOTHING`) · `Task<IReadOnlyList<DiaIndicadores?>> LeerAsync(DateOnly desde, DateOnly hasta)` · `Task<int> PurgarAsync(DateOnly antesDe)` |

`CalculadoUtc` = última ejecución correcta, que la tarea guarda en memoria (y como métrica); tras un reinicio es `null` hasta la primera corrida (que ocurre al arrancar).

## 6. `DashboardOptions` (sección `Dashboard`)

```json
"Dashboard": {
  "DiasConductorActivo": 7,
  "HoraCalculoLocal": "00:30",
  "DesfaseHorarioHoras": -5,
  "DiasRecuperables": 2,
  "RetencionDias": 730,
  "DiasComparacion": 7,
  "UrlPanelesTecnicos": "https://killauru.grafana.net/d/api"
}
```

Validación al arrancar: `DiasRecuperables` entre 1 y 2 (más rompe RN-09, `research.md` R-002); `HoraCalculoLocal` antes de las 23:59; URL absoluta `https`.
