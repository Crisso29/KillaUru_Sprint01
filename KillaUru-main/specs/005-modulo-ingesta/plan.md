# Implementation Plan: Módulo Ingesta

**Branch**: `005-modulo-ingesta` | **Date**: 2026-10-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/005-modulo-ingesta/spec.md`

## Summary

Módulo de **dominio mínimo** (lote y resultado) con el peso en Application e Infrastructure: `SyncController` lee el lote evento por evento, `RecibirLoteEventosUseCase` llama a las reglas de recepción de Anomalías, guarda los aceptados con un solo `INSERT … ON CONFLICT DO NOTHING` y avisa por un `Channel` acotado; `ProcesadorEventosBackgroundService` (N trabajadores + barredor) entrega cada evento a `IAnomaliasService.ProcesarReporteAsync` con su contexto y marca la fila después. Dos purgas y una comprobación de salud completan el módulo. Sigue `enfoque-arquitectonico.md` §6 y `diseño-interno-de-modulos.md` §3.2, §5, §6.4 y §6.6.

## Technical Context

**Language/Version**: C# 14 / .NET 10

**Primary Dependencies**: ASP.NET Core (controlador), `System.Threading.Channels`, EF Core 10 + Npgsql solo para la conexión y SQL parametrizado (`unnest`, `UPDATE` por lotes), `System.Text.Json` (`JsonElement` para leer cada evento por separado)

**Storage**: esquema `ingesta` (1 tabla, `data-model.md`)

**Testing**: xUnit para el mapeo tolerante y el resultado del lote; NSubstitute + `FakeTimeProvider` para casos de uso y el procesador; Testcontainers para la recepción, el marcado, el barredor y las purgas; `WebApplicationFactory` para el contrato HTTP; prueba de reinicio (EQ-09)

**Performance Goals**: lote de 50 en < 300 ms p95 (RNF-02): reglas en memoria (< 1 ms) + 1 `INSERT` (~5–15 ms); retraso de cola ≤ 30 s p95 (RNF-23)

**Constraints**: 200 solo tras el *commit*; la cola es solo un aviso; ningún acceso a tablas de Anomalías; archivos ≤ 100 líneas

**Scale/Scope**: ~48 000 eventos/día; pico de ~250 eventos/s (RNF-03); ~1,5 millones de filas a 30 días (`diseño-interno` §3.6)

## Constitution Check

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | Algoritmo de `enfoque` §6 paso a paso; 2 agregados justificados en Complexity Tracking | ✅ |
| II. Monolito modular | Solo usa `Anomalias/Contracts`; no expone contratos; esquema propio | ✅ |
| III. Calibrable | 10 parámetros en `IngestaOptions`; `TimeProvider` | ✅ |
| IV. Datos confiables | 200 tras el *commit*; idempotencia por `evento_id`; barredor; conciliación | ✅ |
| V. Privacidad | Seudónimo en columna, fuera del `payload`; purga a 72 h; retención 30 días | ✅ |
| VI. Acceso en el servidor | `con_sesion` por el rol del middleware, nunca por el cuerpo | ✅ |
| VII. Pruebas primero | Contrato del lote y reinicio probados antes del código | ✅ |
| VIII. Código pequeño | Recepción, cola, procesador, barredor y purgas en archivos separados | ✅ |
| IX. API documentada | `ingesta.openapi.yaml` con los 7 códigos del contrato del lote | ✅ |
| X. Costo cero | Cola en memoria; sin *broker* (ADR-005) | ✅ |

## Project Structure

### Documentation

```text
specs/005-modulo-ingesta/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/ingesta.openapi.yaml · contracts/contratos-internos.md
└── checklists/requirements.md · checklists/trazabilidad.md
```

### Source Code — árbol exacto

```text
backend/src/Modules/KillaUru.Ingesta/
├── KillaUru.Ingesta.csproj                       # referencia: Shared, Anomalias
├── IngestaModule.cs                              # options, DbContext, cola, casos de uso, trabajadores, tareas, health check
├── README.md
├── Contracts/                                    # vacío a propósito: nadie depende de Ingesta
├── Domain/
│   ├── LoteEntrante.cs                           # LoteId, Eventos (EventoReportado crudos), ConSesion
│   ├── LoteAceptado.cs                           # De(lote, rechazados, ahora): aceptados sin repetidos + ComoParametros()
│   ├── ResultadoLote.cs                          # Aceptado(aceptados, duplicados, rechazados) | Invalido | Saturado(reintentarEn)
│   ├── EstadoRecepcion.cs                        # Recibido = 0, Procesado = 1, Descartado = 2
│   └── HoraDeDeteccion.cs                        # Bloque(detectadoUtc) → [hh:00, hh:00 + 1 h) en UTC (RN-15a)
├── Application/
│   ├── IngestaOptions.cs                         # sección "Ingesta"
│   ├── Puertos/ IRecepcionEventos.cs · IColaLotes.cs · IPendientesRepositorio.cs · IPurgasRecepcion.cs
│   ├── Recepcion/ RecibirLoteEventosUseCase.cs   # enfoque §6 (con recibidoUtc del lote)
│   ├── Procesamiento/ ProcesarLoteUseCase.cs     # carga Recibido del lote, arma contexto, llama a Anomalías, marca
│   ├── Procesamiento/ ArmadorContexto.cs         # ContextoReporte(ConSesion, EventosEnLaHora, RecibidoUtc)
│   ├── Procesamiento/ BarrerPendientesUseCase.cs # lotes con filas Recibido → cola
│   └── Tareas/ PurgarSeudonimosRecepcionUseCase.cs · PurgarRecepcionUseCase.cs
├── Adapters/
│   ├── SyncController.cs                         # POST /api/v1/anomalies/batch (diseño-interno §6.6)
│   └── Dtos/ LoteDto.cs · EventoDtoLector.cs · RespuestaLoteDto.cs · RechazoDto.cs
└── Infrastructure/
    ├── IngestaDbContext.cs · EventoRecibidoConfig.cs
    ├── RecepcionEventosPostgres.cs               # INSERT con unnest (diseño-interno §6.4)
    ├── PendientesRepositorio.cs                  # RecibidosDelLote, ContarEnLaHora, Marcar, LotesPendientes, MasAntiguo
    ├── PurgasRecepcionPostgres.cs                # UPDATE/DELETE por lotes de 5 000
    ├── ColaLotesChannel.cs                       # Channel<LoteRecibido> acotado + Count + métrica ObservarCola
    ├── ProcesadorEventosBackgroundService.cs     # N trabajadores leyendo la cola
    ├── BarredorBackgroundService.cs              # al arrancar y cada 60 s
    ├── ChequeoColaIngesta.cs                     # IHealthCheck: pendiente más viejo ≤ 5 min (caché de 5 s)
    └── Tareas/ TareaPurgaSeudonimosRecepcion.cs · TareaPurgaRecepcion.cs   # PeriodicTimer sobre TimeProvider

backend/db/migraciones/V0006__ingesta.sql
backend/tests/KillaUru.Ingesta.Tests/            Domain/ · Application/ · Adapters/
backend/tests/KillaUru.Integracion.Tests/Ingesta/   RecepcionApi · Procesamiento · Barredor · Reinicio · Purgas · Salud
backend/tests/carga/lotes.k6.js                   # perfil de RNF-03 (eventos sobre la polilínea y a velocidad plausible)
```

**Structure Decision**: plantilla de `enfoque-arquitectonico.md` §4.1 («Ingesta: dominio mínimo»). El procesador de docs se divide en **trabajadores** y **barredor** (dos `BackgroundService`) para respetar las 100 líneas; la lógica de ambos vive en casos de uso probables sin hilos. `IngestaOptions` va en `Application/` como dice `enfoque` §5.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| Índice `ix_recibido_lote (lote_id) WHERE estado = 0` (docs no lo tiene) | El trabajador carga los eventos **de un lote**; sin índice, cada aviso recorre la tabla de 1,5 millones de filas | Encolar los eventos en el `Channel` rompe ADR-005 punto 3 (la cola solo avisa) |
| `PurgaSeudonimoHoras` repetido en `IngestaOptions` (vive en `AnomaliasOptions`) | Un módulo nunca lee la sección de otro (`enfoque` §5, ISP) | Leer `AnomaliasOptions` desde Ingesta acopla configuraciones; se agrega una prueba que compara ambos valores |
