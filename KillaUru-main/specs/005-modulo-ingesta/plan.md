# Implementation Plan: Módulo Ingesta

**Branch**: `005-modulo-ingesta` | **Date**: 2026-10-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/005-modulo-ingesta/spec.md`

## Summary

Módulo de **dominio mínimo** (estado de recepción y hora de detección) con el peso en Application e Infrastructure; el lote y su resultado son de Application (D4). `SyncController` arma un `LoteEntrante` con los eventos crudos; `RecibirLoteEventosUseCase` genera el `loteId` (D3), lee cada evento por separado, aplica «un lote = un seudónimo» (D2), llama a las reglas de recepción de Anomalías, guarda los aceptados con un solo `INSERT … ON CONFLICT DO NOTHING` y avisa por un `Channel` acotado; `ProcesadorEventosBackgroundService` (N trabajadores + barredor) entrega cada evento a `IAnomaliasService.ProcesarReporteAsync` con su contexto, marca la fila después y registra el retraso de cola (D11). Dos purgas por reloj (T-OPS05) y una comprobación de salud completan el módulo. Sigue `enfoque-arquitectonico.md` §6 y `diseño-interno-de-modulos.md` §3.2, §5, §6.4 y §6.6.

## Technical Context

**Language/Version**: C# 14 / .NET 10

**Primary Dependencies**: ASP.NET Core (controlador), `System.Threading.Channels`, EF Core 10 + Npgsql solo para la conexión y SQL parametrizado (`unnest`, `UPDATE` por lotes), `System.Text.Json` (`JsonElement` para leer cada evento por separado)

**Storage**: esquema `ingesta` (1 tabla, `data-model.md`)

**Testing**: xUnit para el mapeo tolerante, la regla del seudónimo y el resultado del lote; NSubstitute + `FakeTimeProvider` para casos de uso y el procesador; Testcontainers para la recepción, el marcado, el barredor y las purgas; `WebApplicationFactory` para el contrato HTTP; prueba de reinicio (EQ-09). Carga, picos, estrés, resistencia y caos: scripts de 001 (D11)

**Performance Goals**: lote de 50 en < 300 ms p95 (RNF-02): reglas en memoria (< 1 ms) + 1 `INSERT` (~5–15 ms); retraso de cola ≤ 30 s p95 (RNF-23, histograma `killauru.ingesta.retraso_cola_s`)

**Constraints**: 200 solo tras el *commit*; la cola es solo un aviso; ningún acceso a tablas de Anomalías; Adapters solo usa Application (D4); archivos ≤ 100 líneas; las transacciones se confirman con `IContextoTransaccion.ConfirmarAsync` (no con `CommitAsync` directo) para que corran los `AlConfirmar` (001, D15)

**Scale/Scope**: ~48 000 eventos/día; pico de ~250 eventos/s (RNF-03); ~1,5 millones de filas a 30 días (`diseño-interno` §3.6)

## Constitution Check

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | Algoritmo de `enfoque` §6 paso a paso; agregados confirmados por el autor (D1…D11) y llevados a docs; ver Complexity Tracking | ✅ |
| II. Monolito modular | Solo Application usa `Anomalias/Contracts`; Domain sin tipos de otro módulo; no expone contratos; esquema propio | ✅ |
| III. Calibrable | 10 parámetros en `IngestaOptions`; `TimeProvider` y `ProgramaReloj` | ✅ (ver Complexity Tracking: regla del seudónimo en Application) |
| IV. Datos confiables | 200 tras el *commit*; idempotencia por `evento_id`; barredor (también filas sin `lote_id`); conciliación | ✅ |
| V. Privacidad | Seudónimo en columna, fuera del `payload`; purga a 72 h con `lote_id` y hora redondeada (D3); retención 30 días | ✅ |
| VI. Acceso en el servidor | `con_sesion` por el rol y el *claim* `verificado` del token (D8), nunca por el cuerpo | ✅ |
| VII. Pruebas primero | Contrato del lote, regla del seudónimo y reinicio probados antes del código | ✅ |
| VIII. Código pequeño | Recepción, cola, procesador, barredor y purgas en archivos separados; repositorio de pendientes partido (T-ARQ08) | ✅ |
| IX. API documentada | `ingesta.openapi.yaml` con los códigos del contrato del lote | ✅ |
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
├── Domain/                                       # solo BCL y Shared/Domain (D4)
│   ├── EstadoRecepcion.cs                        # Recibido = 0, Procesado = 1, Descartado = 2
│   └── HoraDeDeteccion.cs                        # Bloque(detectadoUtc) → [hh:00, hh:00 + 1 h) en UTC (RN-15a)
├── Application/
│   ├── IngestaOptions.cs                         # sección "Ingesta"
│   ├── Puertos/ IRecepcionEventos.cs · IColaLotes.cs · IPendientesRepositorio.cs · IBarridoPendientes.cs · IPurgasRecepcion.cs
│   ├── Recepcion/ LoteEntrante.cs                # Eventos (JsonElement crudos), ConSesion, SeudonimoCabecera
│   ├── Recepcion/ LectorEvento.cs                # JsonElement → (eventoId?, EventoReportado crudo) (R-001)
│   ├── Recepcion/ SeudonimoDelLote.cs            # un lote = un seudónimo (R-010, D2)
│   ├── Recepcion/ LoteAceptado.cs                # De(lote, rechazados, loteId, ahora): aceptados sin repetidos + ComoParametros()
│   ├── Recepcion/ ResultadoLote.cs               # Aceptado(aceptados, duplicados, Rechazo[]) | Invalido | Saturado(reintentarEn) + Rechazo(Id?, Indice, Motivo)
│   ├── Recepcion/ RecibirLoteEventosUseCase.cs   # enfoque §6 (loteId UUIDv7 y recibidoUtc del lote)
│   ├── Procesamiento/ LoteRecibido.cs · EventoPendiente.cs
│   ├── Procesamiento/ ProcesarLoteUseCase.cs     # carga Recibido del lote, arma contexto, llama a Anomalías, marca, RegistrarRetrasoCola
│   ├── Procesamiento/ ArmadorContexto.cs         # ContextoReporte(ConSesion, EventosEnLaHora, RecibidoUtc)
│   ├── Procesamiento/ BarrerPendientesUseCase.cs # lotes con filas Recibido (y filas sin lote_id) → cola
│   └── Tareas/ PurgarSeudonimosRecepcionUseCase.cs · PurgarRecepcionUseCase.cs
├── Adapters/                                     # solo Application (D4)
│   ├── SyncController.cs                         # POST /api/v1/anomalies/batch (diseño-interno §6.6)
│   └── Dtos/ LoteDto.cs · RespuestaLoteDto.cs · RechazoDto.cs
└── Infrastructure/
    ├── IngestaDbContext.cs · EventoRecibidoConfig.cs
    ├── RecepcionEventosPostgres.cs               # INSERT con unnest (diseño-interno §6.4)
    ├── PendientesRepositorio.cs                  # RecibidosDelLote, ContarEnLaHora, Marcar
    ├── BarridoPendientesPostgres.cs              # LotesPendientes, MasAntiguoPendiente (T-ARQ08)
    ├── PurgasRecepcionPostgres.cs                # UPDATE/DELETE por lotes de 5 000 (data-model §3)
    ├── ColaLotesChannel.cs                       # Channel<LoteRecibido> acotado + Count + métrica ObservarCola
    ├── ProcesadorEventosBackgroundService.cs     # N trabajadores leyendo la cola
    ├── BarredorBackgroundService.cs              # al arrancar y cada 60 s
    ├── ChequeoColaIngesta.cs                     # IHealthCheck: pendiente más viejo ≤ 5 min (caché de 5 s)
    └── Tareas/ TareaPurgaSeudonimosRecepcion.cs · TareaPurgaRecepcion.cs   # al arrancar + ProgramaReloj (:10 · 03:00)

backend/db/migraciones/V0006__ingesta.sql
backend/tests/KillaUru.Ingesta.Tests/            Domain/ · Application/ · Adapters/
backend/tests/KillaUru.Integracion.Tests/Ingesta/   RecepcionApi · Procesamiento · Barredor · Reinicio · Purgas · Salud
# Carga, picos, estrés, resistencia y caos: backend/tests/carga/ de 001 (D11); Ingesta no tiene script propio
```

**Structure Decision**: plantilla de `enfoque-arquitectonico.md` §4.1 («Ingesta: Domain mínimo (estado de recepción, hora de detección)»); el lote y su resultado son de Application (D4). El procesador de docs se divide en **trabajadores** y **barredor** (dos `BackgroundService`) para respetar las 100 líneas; la lógica de ambos vive en casos de uso probables sin hilos. `IngestaOptions` va en `Application/` como dice `enfoque` §5.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| Índice `ix_recibido_lote (lote_id) WHERE estado = 0` (confirmado con D1, se lleva a docs) | El trabajador carga los eventos **de un lote**; sin índice, cada aviso recorre la tabla de 1,5 millones de filas | Encolar los eventos en el `Channel` rompe ADR-005 punto 3 (la cola solo avisa) |
| `PurgaSeudonimoHoras` repetido en `IngestaOptions` (vive en `AnomaliasOptions`) | Un módulo nunca lee la sección de otro (`enfoque` §5, ISP) | Leer `AnomaliasOptions` desde Ingesta acopla configuraciones; se agrega una prueba que compara ambos valores |
| Regla «un lote = un seudónimo» en Application, no en Domain (constitución III) | D4 deja el dominio de Ingesta en `EstadoRecepcion` y `HoraDeDeteccion`; la regla necesita el lote y la cabecera, que son de Application | Moverla a Domain obligaría a traer el lote (y `EventoReportado` de Anomalías) al dominio, que D4 prohíbe |
| Fila `Recibido` con más de 30 días no se borra (RNF-25 dice «como máximo 30 días») | Borrarla perdería un evento aceptado (RNF-06) | Riesgo anotado con alerta de pendiente > 72 h y reproceso manual (R-008, SEG-05) |
