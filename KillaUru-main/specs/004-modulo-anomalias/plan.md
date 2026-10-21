# Implementation Plan: Módulo Anomalías

**Branch**: `004-modulo-anomalias` | **Date**: 2026-10-20 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/004-modulo-anomalias/spec.md`

## Summary

Módulo de **dominio rico**: políticas puras (`ReglasCalidad`, `PoliticaPlausibilidad`, `PoliticaConfirmacion`, `ReglaPersistencia`), el agregado `Anomalia` con su tabla de transiciones, la polilínea `CorredorVia` en memoria, el caso de uso `ProcesarReporteUseCase` con transacción y bloqueo propios, 3 tareas programadas (caducidad, purga de seudónimos, purga histórica), los casos de uso del administrador (moderación, listas, detalle, exportación) y las lecturas `IAnomaliaLectura` para Mapa y Dashboard. Sigue al pie de la letra `enfoque-arquitectonico.md` §5–§6 y `diseño-interno-de-modulos.md` §3, §5 y §6.

## Technical Context

**Language/Version**: C# 14 / .NET 10

**Primary Dependencies**: EF Core 10 + Npgsql + NetTopologySuite (solo en Infrastructure); SQL parametrizado para la caducidad masiva y las lecturas calientes; `System.Text.Json` y `CsvHelper`-libre (escritor CSV propio de pocas líneas, para no sumar paquetes)

**Storage**: esquema `anomalias` (4 tablas, `data-model.md`)

**Testing**: xUnit puro para todas las políticas y el agregado (valores límite); NSubstitute + `FakeTimeProvider` para casos de uso; Testcontainers para repositorio, bloqueos, caducidad, purgas y lecturas; `WebApplicationFactory` para moderación y exportación

**Performance Goals**: ≤ 100 ms p95 por evento (RNF-23); ~5 consultas indexadas por evento (`diseño-interno` §5)

**Constraints**: idempotencia por `eventoId`; sin interbloqueos; ninguna regla usa el reloj real ni la hora de llegada; archivos ≤ 100 líneas

**Scale/Scope**: ~48 000 eventos/día; ~35 millones de reportes en 2 años (`diseño-interno` §3.6)

## Constitution Check

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | Algoritmo y valores copiados de docs; 3 agregados justificados (R-001, R-005, R-006) | ✅ |
| II. Monolito modular | Solo usa `Auditoria/Contracts`; ofrece `IAnomaliasService` e `IAnomaliaLectura`; esquema propio | ✅ |
| III. Calibrable | 30 parámetros en `AnomaliasOptions`; `TimeProvider`; día local UTC−5 | ✅ |
| IV. Datos confiables | Idempotencia por `eventoId`; transacción y bloqueo propios; publicar tras el commit | ✅ |
| V. Privacidad | Seudónimo en columna con purga a 72 h; exportación sin seudónimos; `con_sesion` sí/no | ✅ |
| VI. Acceso en el servidor | Moderación y exportación solo para administrador, auditadas en la misma transacción | ✅ |
| VII. Pruebas primero | Todas las reglas con valores límite antes del código (`enfoque` §8, `diseño-interno` §10) | ✅ |
| VIII. Código pequeño | Agregado dividido en `partial` por responsabilidad; una política por regla | ✅ |
| IX. API documentada | `anomalias.openapi.yaml` (moderación, listas, detalle, exportación) | ✅ |
| X. Costo cero | Nada nuevo | ✅ |

## Project Structure

### Documentation

```text
specs/004-modulo-anomalias/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/anomalias.openapi.yaml · contracts/contratos-internos.md
└── checklists/requirements.md · checklists/trazabilidad.md
```

### Source Code — árbol exacto

```text
backend/src/Modules/KillaUru.Anomalias/
├── KillaUru.Anomalias.csproj                     # referencia: Shared, Auditoria
├── AnomaliasModule.cs                            # options, DbContext, corredor, políticas, casos de uso, tareas, suscripciones
├── README.md
├── Contracts/                                    # PÚBLICO
│   ├── IAnomaliasService.cs                      # EvaluarIntrinseca, ProcesarReporteAsync
│   ├── IAnomaliaLectura.cs                       # ActivasPorCeldasAsync, FiltrarAsync, CambiosDesdeAsync, ResumenDelDiaAsync, CeldasDelCorredor
│   ├── EventoReportado.cs                        # datos crudos del evento (para poder decir QUÉ está mal)
│   ├── ContextoReporte.cs                        # ConSesion, EventosEnLaHora, RecibidoUtc
│   ├── ResultadoProceso.cs                       # Procesado | Descartado(motivo) | YaProcesado
│   ├── MotivoDescarte.cs                         # enum de RN-11 (7 motivos)
│   ├── TipoAnomalia.cs                           # Bache = 1, Hundimiento = 2 (RN-01)
│   ├── EstadoAnomalia.cs                         # Activa = 1 … Descartada = 5
│   ├── AnomaliaCambiada.cs                       # IEventoDominio (AnomaliaId, Celda)
│   └── Lecturas/ AnomaliaVisible.cs · FiltroAnomalias.cs · AnomaliaCambio.cs · ResumenDia.cs
├── Domain/
│   ├── AnomaliasOptions.cs                       # POCO (enfoque §5) — Domain puede usarlo
│   ├── CorredorVia.cs                            # polilínea en memoria: DistanciaMetros, KmSobreVia, Celdas (franja de 50 m)
│   ├── GeodesiaLocal.cs                          # distancia punto–segmento y haversine (sin PostGIS)
│   ├── ReporteAnomalia.cs                        # Desde(evento, contexto) → resultado de mapeo
│   ├── ReglasCalidad.cs                          # RN-11 + RN-13: EvaluarIntrinseca(reporte, recibidoUtc)
│   ├── PoliticaPlausibilidad.cs                  # RN-15a SuperaTope · RN-15b Evaluar(reporte, anterior)
│   ├── PoliticaConfirmacion.cs                   # RN-06, RN-14, RN-15a: Decidir, EsVehiculoNuevo
│   ├── ReglaPersistencia.cs                      # RN-12
│   ├── ReglasAnomalia.cs                         # record que agrupa políticas y ventanas (TimeSpan)
│   ├── EstadoPersistencia.cs · AccionAdmin.cs · Transicion.cs · ResultadoModeracion.cs
│   ├── Anomalia.cs                               # campos, Crear, CambiarEstado
│   ├── Anomalia.Reportes.cs                      # partial: RegistrarReporte (RN-05, RN-06, RN-07, RN-12)
│   ├── Anomalia.Moderacion.cs                    # partial: tabla PermitidoDesde, Moderar, AnotarParaRevision
│   └── Anomalia.Transiciones.cs                  # partial: AnotarTransicion (una por operación)
├── Application/
│   ├── Puertos/ IAnomaliaRepositorio.cs · ITransaccionAnomalias.cs · ILecturaAnomaliasRepositorio.cs
│   ├── Puertos/ ITareasRepositorio.cs            # caducidad masiva y purgas (SQL)
│   ├── Procesamiento/ EvaluarIntrinsecaUseCase.cs · ProcesarReporteUseCase.cs · AnomaliasService.cs
│   ├── Tareas/ CaducarAnomaliasUseCase.cs · PurgarSeudonimosUseCase.cs · PurgarHistoricoUseCase.cs
│   ├── Administracion/ ModerarAnomaliaUseCase.cs · ListarPorRevisarUseCase.cs · ListarPendientesUseCase.cs
│   ├── Administracion/ ObtenerDetalleUseCase.cs · ListarTransicionesRecientesUseCase.cs
│   ├── Exportacion/ ExportarDatasetUseCase.cs · IEscritorExportacion.cs
│   └── Lecturas/ AnomaliaLectura.cs              # implementa IAnomaliaLectura
├── Adapters/
│   ├── ModeracionController.cs                   # POST /anomalies/{id}/moderacion · GET /moderacion/por-revisar · /pendientes
│   ├── DetalleAnomaliaController.cs              # GET /anomalies/{id} · GET /anomalies/transiciones
│   ├── ExportacionController.cs                  # POST /exportaciones (archivo)
│   └── Dtos/ …                                   # un DTO por archivo, con XML y <example>
└── Infrastructure/
    ├── AnomaliasDbContext.cs
    ├── Configuraciones/ AnomaliaConfig.cs · ReporteConfig.cs · TransicionConfig.cs · CorredorConfig.cs
    ├── AnomaliaRepositorio.cs                    # MasCercanaAsync, ReportesDesdeAsync, AnteriorDelSeudonimoAsync, ExisteReporteAsync, GuardarAsync
    ├── TransaccionAnomalias.cs                   # transacción + pg_advisory_xact_lock por celda + IContextoTransaccion.Unir
    ├── TareasRepositorio.cs                      # UPDATE masivo de caducidad + transiciones; purgas por lotes
    ├── LecturaAnomaliasRepositorio.cs            # proyecciones AsNoTracking y SQL de lectura
    ├── ResumenDiaRepositorio.cs                  # conteos del día (RF-18)
    ├── CargadorCorredor.cs                       # IHostedService: carga corredor_via al arrancar + IHealthCheck
    ├── Exportacion/ EscritorCsv.cs · EscritorGeoJson.cs
    └── Tareas/ TareaCaducidad.cs · TareaPurgaSeudonimos.cs · TareaPurgaHistorica.cs   # BackgroundService con PeriodicTimer

backend/db/migraciones/V0004__anomalias.sql
backend/db/migraciones/V0005__corredor_via_datos.sql       # polilínea PE-28A (datos)
backend/tests/KillaUru.Anomalias.Tests/          Domain/ · Application/
backend/tests/KillaUru.Integracion.Tests/Anomalias/   Repositorio · Concurrencia · Tareas · Lecturas · ModeracionApi · ExportacionApi
```

**Structure Decision**: plantilla de `enfoque-arquitectonico.md` §4.1 («Anomalías: dominio rico…»). Los nombres de clases del dominio y del algoritmo son los de docs; los repositorios se dividen en 4 para respetar el límite de 100 líneas y el principio de segregación (escritura del agregado, tareas masivas, lecturas, resumen).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| `ContextoReporte` gana `RecibidoUtc` (docs: `(ConSesion, EventosEnLaHora)`) | RN-11 mide la antigüedad contra la **recepción**, RN-09 purga a 72 h **de recibido**, y `reporte_anomalia.recibido_utc` necesita ese valor | Usar la hora de procesamiento descartaría eventos procesados tarde y adelantaría la purga |
| `AccionAdmin.MantenerDescartada` (docs: 3 acciones) | RN-07 dice «la reabre **o la mantiene descartada**, y en ese caso el contador se reinicia» | Sin la acción no hay forma de reiniciar el contador sin reabrir |
| Columna `transicion.actor_id` | El detalle del prototipo muestra quién hizo cada cambio (sistema o administrador) | Leer la auditoría desde Anomalías no está permitido (no hay contrato de lectura) |
| Columna `reporte_anomalia.km_via` | Cobertura del corredor (RF-18: tramos de 1 km con reportes en 7 días) y km en el detalle | Proyectar 48 000 puntos por día sobre la polilínea en cada consulta del Dashboard |
