# Implementation Plan: Módulo Dashboard

**Branch**: `007-modulo-dashboard` | **Date**: 2026-10-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/007-modulo-dashboard/spec.md`

## Summary

Módulo de **soporte con dominio liviano** (`enfoque` §4.1): una tarea diaria pide los conteos a Anomalías e Identidad y guarda 9 números por día en `dashboard.indicador_diario`; un endpoint de solo lectura arma los 6 indicadores del panel (valor, comparación, variación y serie) con una calculadora pura. Sin entidades artificiales: el dominio son el catálogo de indicadores, el día local y las fórmulas.

## Technical Context

**Language/Version**: C# 14 / .NET 10

**Primary Dependencies**: EF Core 10 + Npgsql (lectura `AsNoTracking`; escritura con SQL `INSERT … ON CONFLICT DO NOTHING`), ASP.NET Core (controlador)

**Storage**: esquema `dashboard` (1 tabla, `data-model.md`)

**Testing**: xUnit para la calculadora y el día local (valores a mano, `null`, días faltantes); `FakeTimeProvider` para la tarea al arrancar y a las 00:30 Lima con `ProgramaReloj` (001); NSubstitute + `FakeTimeProvider` para la tarea; Testcontainers para el repositorio; `WebApplicationFactory` para el endpoint

**Performance Goals**: el resumen lee ≤ 540 filas con una consulta; la tarea hace 2 llamadas por día

**Constraints**: ningún dato que identifique; ninguna lectura de tablas ajenas; archivos ≤ 100 líneas

**Scale/Scope**: 9 filas por día; ~6 600 filas en 2 años

## Constitution Check

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | Fórmulas de `diseño-interno` §3.8; comparaciones del prototipo (R-003) confirmadas y llevadas a docs (D1); «eventos válidos» y reaparición como D14 | ✅ |
| II. Monolito modular | Solo `Anomalias/Contracts` e `Identidad/Contracts`; esquema propio | ✅ |
| III. Calibrable | 7 parámetros en `DashboardOptions`; `TimeProvider` | ✅ |
| IV. Datos confiables | Día completo en una transacción; huecos como `null` | ✅ |
| V. Privacidad | Solo números (RN-17); prueba del esquema | ✅ |
| VI. Acceso en el servidor | Endpoint solo para el administrador (401 sin token, 403 otro rol; T-SEG20) | ✅ |
| VII. Pruebas primero | Calculadora y programa probados con valores a mano antes del código | ✅ |
| VIII. Código pequeño | Calculadora separada de la tarea y del controlador | ✅ |
| IX. API documentada | `dashboard.openapi.yaml` | ✅ |
| X. Costo cero | Nada nuevo; lo técnico sigue en el plan gratuito de Grafana | ✅ |

## Project Structure

### Documentation

```text
specs/007-modulo-dashboard/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/dashboard.openapi.yaml · contracts/contratos-internos.md
└── checklists/requirements.md · checklists/trazabilidad.md
```

### Source Code — árbol exacto

```text
backend/src/Modules/KillaUru.Dashboard/
├── KillaUru.Dashboard.csproj                     # referencia: Shared, Anomalias, Identidad
├── DashboardModule.cs                            # options, DbContext, casos de uso, tarea
├── README.md
├── Contracts/                                    # vacío a propósito
├── Domain/
│   ├── Indicador.cs                              # enum smallint: EventosRecibidos = 1 … Renacimientos = 9
│   ├── DiaIndicadores.cs                         # día + 9 valores simples (long); ComoFilas()
│   ├── DiaLocal.cs                               # Hoy(ahora, desfase), DiasFaltantes(guardados, hoy, recuperables)
│   ├── MedidaIndicador.cs                        # Valor, Comparacion, Variacion, Serie[7] (sin unidad)
│   └── CalculadoraIndicadores.cs                 # 6 medidas del panel a partir de los días (R-003)
├── Application/
│   ├── DashboardOptions.cs                       # sección "Dashboard"
│   ├── Puertos/ IIndicadoresRepositorio.cs
│   ├── IndicadorResumen.cs                       # MedidaIndicador + Unidad (D4, R-007)
│   ├── ResumenDashboard.cs                       # lo que devuelve ConsultarResumenUseCase al controlador (D4)
│   ├── MapeoResumenDia.cs                        # ResumenDia (004) + conductoresActivos → DiaIndicadores (D4)
│   ├── CalcularIndicadoresUseCase.cs             # días faltantes → contratos → MapeoResumenDia → GuardarDiaAsync → purga → métrica
│   └── ConsultarResumenUseCase.cs                # LeerAsync(periodo × 2) → CalculadoraIndicadores → ResumenDashboard
├── Adapters/
│   ├── DashboardController.cs                    # GET /api/v1/dashboard/indicadores
│   └── Dtos/ ResumenDashboardDto.cs · IndicadorDto.cs · EventosDiaDto.cs
└── Infrastructure/
    ├── DashboardDbContext.cs · IndicadorDiarioConfig.cs
    ├── IndicadoresRepositorio.cs                 # DiasGuardados, GuardarDia (ON CONFLICT DO NOTHING), Leer, Purgar
    └── TareaIndicadoresDiarios.cs                # BackgroundService: al arrancar y a las 00:30 Lima (ProgramaReloj de Shared + Task.Delay sobre TimeProvider)

backend/db/migraciones/V0007__dashboard.sql
backend/tests/KillaUru.Dashboard.Tests/          Domain/ · Application/
backend/tests/KillaUru.Integracion.Tests/Dashboard/   IndicadoresRepositorio · TareaIndicadores · DashboardApi
```

**Structure Decision**: plantilla de `enfoque-arquitectonico.md` §4.1 («Dashboard: indicadores de negocio; job diario»). `DashboardOptions` va en `Application/` como dice `enfoque` §5. Como todas las tareas del sistema (T-OPS05), la tarea no usa `PeriodicTimer` de 24 h: corre al arrancar y luego a la hora del reloj que calcula `Shared/Tareas/ProgramaReloj` (001). El controlador solo ve tipos de Application (`ResumenDashboard`, `IndicadorResumen`) y el dominio no usa `ResumenDia` de Anomalías (D4, R-007).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| `DashboardOptions.DesfaseHorarioHoras` repite el valor de `AnomaliasOptions` (R-008) | Un módulo no lee la configuración de otro (`enfoque` §5) y el día local debe ser el mismo que usa `ResumenDelDiaAsync` | Leer `AnomaliasOptions` rompe la regla; una prueba (T025) exige que los dos valores sean iguales |
| Se guardan los **conteos** (9) y no las tasas (6) | Una tasa de 7 días se calcula sumando numeradores y denominadores; promediar tasas diarias da otro número (días con 1 pendiente pesarían igual que días con 50) | Guardar tasas diarias impide recalcular el periodo correctamente |
