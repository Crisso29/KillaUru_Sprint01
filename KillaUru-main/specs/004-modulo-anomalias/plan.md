# Implementation Plan: Módulo Anomalías

**Branch**: `004-modulo-anomalias` | **Date**: 2026-10-20 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/004-modulo-anomalias/spec.md`

## Summary

Módulo de **dominio rico**: políticas puras (`ReglasCalidad`, `PoliticaPlausibilidad`, `PoliticaConfirmacion`, `ReglaPersistencia`), el agregado `Anomalia` con su tabla de transiciones, la polilínea `CorredorVia` en memoria, el caso de uso `ProcesarReporteUseCase` con transacción y bloqueo propios, 3 tareas programadas por reloj (caducidad `:05`, purga de seudónimos `:10`, purga histórica 03:30 local; todas corren también al arrancar), los casos de uso del administrador (moderación, listas, detalle, exportación) y las lecturas `IAnomaliaLectura` para Mapa y Dashboard. Sigue al pie de la letra `enfoque-arquitectonico.md` §5–§6 y `diseño-interno-de-modulos.md` §3, §5 y §6, con las decisiones del autor del 2026-10-21 (D1, D2, D3, D4, D7, D13, D14, D15) y las correcciones técnicas de `specs/revision/plan-de-cambios.md` §B.

## Technical Context

**Language/Version**: C# 14 / .NET 10

**Primary Dependencies**: EF Core 10 + Npgsql + NetTopologySuite (solo en Infrastructure); SQL parametrizado para la caducidad masiva y las lecturas calientes; `System.Text.Json` y `CsvHelper`-libre (escritor CSV propio de pocas líneas, para no sumar paquetes)

**Storage**: esquema `anomalias` (4 tablas, `data-model.md`)

**Testing**: xUnit puro para todas las políticas y el agregado (valores límite); NSubstitute + `FakeTimeProvider` para casos de uso; Testcontainers para repositorio, bloqueos, caducidad, purgas y lecturas; `WebApplicationFactory` para moderación y exportación

**Performance Goals**: ≤ 100 ms p95 por evento (RNF-23); ~5 consultas indexadas por evento (`diseño-interno` §5)

**Constraints**: idempotencia por `eventoId`; sin interbloqueos; ninguna regla usa el reloj real ni la hora de llegada; archivos < 200 líneas; las transacciones se confirman con `IContextoTransaccion.ConfirmarAsync` (no con `CommitAsync` directo) para que corran los `AlConfirmar` (001, D15)

**Scale/Scope**: ~48 000 eventos/día; ~35 millones de reportes en 2 años (`diseño-interno` §3.6)

## Constitution Check

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | Algoritmo y valores copiados de docs; los agregados (R-001, R-005, R-006, R-007, R-011, R-012, R-013) están confirmados por el autor (D1, D13) y pasan a docs | ✅ |
| II. Monolito modular | Solo usa `Auditoria/Contracts`; ofrece `IAnomaliasService` e `IAnomaliaLectura`; esquema propio | ✅ |
| III. Calibrable | 25 parámetros en `AnomaliasOptions` (`enfoque` §5); `TimeProvider` y `ProgramaReloj`; día local UTC−5 | ✅ |
| IV. Datos confiables | Idempotencia por `eventoId`; transacción y bloqueo propios; publicar tras el commit | ✅ |
| V. Privacidad | Seudónimo en columna con purga a 72 h; exportación sin seudónimos; `con_sesion` sí/no | ✅ |
| VI. Acceso en el servidor | Moderación y exportación solo para administrador; moderación auditada en su transacción (`RegistrarAsync`), exportación como lectura sensible (`RegistrarLecturaAsync`, D15) | ✅ |
| VII. Pruebas primero | Todas las reglas con valores límite antes del código (`enfoque` §8, `diseño-interno` §10) | ✅ |
| VIII. Código pequeño | Agregado y repositorios grandes divididos en clases con una sola responsabilidad (sin `partial`) (T-ARQ08); una política por regla | ✅ |
| IX. API documentada | `anomalias.openapi.yaml` (moderación, listas, detalle, exportación; 429 en todas, T-SEG19) | ✅ |
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
├── AnomaliasModule.cs                            # options, DbContext, corredor, políticas, casos de uso, tareas, comprobación de salud
├── README.md
├── Contracts/                                    # PÚBLICO
│   ├── IAnomaliasService.cs                      # EvaluarIntrinseca, ProcesarReporteAsync
│   ├── IAnomaliaLectura.cs                       # ActivasPorCeldasAsync, FiltrarAsync, CambiosDesdeAsync, ResumenDelDiaAsync, CeldasDelCorredor
│   ├── EventoReportado.cs                        # datos crudos del evento (para poder decir QUÉ está mal)
│   ├── ContextoReporte.cs                        # ConSesion, EventosEnLaHora, RecibidoUtc
│   ├── ResultadoProceso.cs                       # Procesado | Descartado(motivo) | YaProcesado
│   ├── MotivoDescarte.cs                         # RN-11: 7 motivos + SeudonimoInconsistente = 8 (lo evalúa Ingesta, D2)
│   ├── TipoAnomalia.cs                           # Bache = 1, Hundimiento = 2 (RN-01)
│   ├── EstadoAnomalia.cs                         # Activa = 1 … Descartada = 5
│   ├── AnomaliaCambiada.cs                       # IEventoDominio (AnomaliaId, Celda)
│   └── Lecturas/ AnomaliaVisible.cs · FiltroAnomalias.cs · AnomaliaCambio.cs · ResumenDia.cs   # con KmVia (D13)
├── Domain/                                       # solo Shared/Domain y BCL; Application mapea Contracts
│   ├── AnomaliasOptions.cs                       # POCO (enfoque §5) — Domain puede usarlo
│   ├── CorredorVia.cs                            # polilínea en memoria: DistanciaMetros, KmSobreVia, Celdas (franja de 50 m)
│   ├── GeodesiaLocal.cs                          # distancia punto–segmento y haversine (sin PostGIS)
│   ├── ReporteAnomalia.cs                        # Desde(evento, contexto) → resultado de mapeo
│   ├── ReglasCalidad.cs                          # RN-11 + RN-13: EvaluarIntrinseca(reporte, recibidoUtc)
│   ├── PoliticaPlausibilidad.cs                  # RN-15a SuperaTope · RN-15b Evaluar(reporte, anterior)
│   ├── PoliticaConfirmacion.cs                   # RN-06, RN-14, RN-15a: Decidir, EsVehiculoNuevo
│   ├── ReglaPersistencia.cs                      # RN-12
│   ├── ReglasAnomalia.cs                         # record que agrupa políticas y ventanas (TimeSpan)
│   ├── EstadoPersistencia.cs · AccionAdmin.cs · Transicion.cs   # AccionAdmin = tabla de RN-07 (se queda en Domain, D4)
│   ├── Anomalia.cs                               # agregado: campos (KmVia fijo), Crear, RegistrarReporte, Moderar; delega en las clases de abajo
│   ├── ReglaRenacimiento.cs                        # RegistrarReporte delega aquí (RN-05, RN-06, RN-07, RN-12; renacer de Reparada, D14)
│   ├── TablaModeracion.cs                        # RN-07: PermitidoDesde(accion); MarcasRevision («Por revisar»)
│   └── RegistroTransiciones.cs                  # AnotarTransicion (una por operación)
├── Application/
│   ├── Puertos/ IAnomaliaRepository.cs · ITransaccionAnomalias.cs              # escritura del agregado (T-ARQ06)
│   ├── Puertos/ ICaducidadRepository.cs · IPurgasRepository.cs                 # tareas masivas (SQL)
│   ├── Puertos/ ILecturaAnomaliasRepository.cs · IResumenDiaRepository.cs      # lecturas para Mapa y Dashboard
│   ├── Puertos/ ILecturaAdministracionRepository.cs · IExportacionRepository.cs # listas y detalle; contar y leer (T-ARQ07)
│   ├── Procesamiento/ EvaluarIntrinsecaUseCase.cs · ProcesarReporteUseCase.cs · AnomaliasService.cs
│   ├── Tareas/ CaducarAnomaliasUseCase.cs · PurgarSeudonimosUseCase.cs · PurgarHistoricoUseCase.cs
│   ├── Administracion/ ComandoModeracion.cs · AccionModeracion.cs · ResultadoModeracion.cs   # lo que ve el controlador (D4)
│   ├── Administracion/ ModerarAnomaliaUseCase.cs · ListarPorRevisarUseCase.cs · ListarPendientesUseCase.cs
│   ├── Administracion/ ObtenerDetalleUseCase.cs · ListarTransicionesRecientesUseCase.cs · Vistas/ (un record por vista)
│   ├── Exportacion/ ExportarDatasetUseCase.cs · IEscritorExportacion.cs · FilaExportacion.cs
│   └── Lecturas/ AnomaliaLectura.cs              # implementa IAnomaliaLectura
├── Adapters/                                     # solo Application (enfoque §3, D4)
│   ├── ModeracionController.cs                   # POST /anomalies/{id:guid}/moderacion · GET /moderacion/por-revisar · /pendientes
│   ├── DetalleAnomaliaController.cs              # GET /anomalies/{id:guid} · GET /anomalies/transiciones
│   ├── ExportacionController.cs                  # POST /exportaciones (archivo)
│   └── Dtos/ …                                   # un DTO por archivo, con XML y <example>
└── Infrastructure/
    ├── AnomaliasDbContext.cs
    ├── Configuraciones/ AnomaliaConfig.cs · ReporteConfig.cs · TransicionConfig.cs · CorredorConfig.cs
    ├── AnomaliaRepositoryPostgis.cs              # MasCercanaAsync (FOR UPDATE), ObtenerParaActualizarAsync (FOR UPDATE), GuardarAsync
    ├── ReporteRepositoryPostgis.cs        # IReporteRepository (ISP): ReportesDesdeAsync, AnteriorDelSeudonimoAsync, ExisteReporteAsync
    ├── TransaccionAnomalias.cs                   # transacción + pg_advisory_xact_lock por celda + IContextoTransaccion.Unir
    ├── CaducidadRepository.cs                    # CTE de caducidad con FOR UPDATE SKIP LOCKED + transiciones
    ├── PurgasRepository.cs                       # purga de seudónimos (D3) y purga histórica (D7) por lotes
    ├── LecturaAnomaliasRepository.cs             # proyecciones AsNoTracking: activas, filtrado, cambios
    ├── ResumenDiaRepository.cs                   # conteos del día (RF-18, D14)
    ├── LecturaAdministracionRepository.cs        # «Por revisar», «Esperan confirmación», transiciones recientes
    ├── DetalleAnomaliaRepository.cs  # IDetalleAnomaliaRepository (ISP): detalle con reportes por día e historial
    ├── ExportacionRepository.cs                  # ContarAsync y LeerAsync (IAsyncEnumerable) con los mismos filtros
    ├── CargadorCorredor.cs                       # IHostedService: carga corredor_via al arrancar
    ├── CorredorHealthCheck.cs                    # IHealthCheck «corredor» de /health/ready (T-OPS19)
    ├── Exportacion/ EscritorCsv.cs · EscritorGeoJson.cs
    └── Tareas/ TareaCaducidad.cs · TareaPurgaSeudonimos.cs · TareaPurgaHistorica.cs   # BackgroundService: al arrancar + ProgramaReloj

backend/db/migraciones/V0004__anomalias.sql
backend/db/migraciones/V0005__corredor_via_datos.sql       # polilínea PE-28A (datos)
backend/tests/KillaUru.Anomalias.Tests/          Domain/ · Application/
backend/tests/KillaUru.Integracion.Tests/Anomalias/   Repositorio · Concurrencia · Tareas · Lecturas · ModeracionApi · ExportacionApi · Esquema
```

**Structure Decision**: plantilla de `enfoque-arquitectonico.md` §4.1 («Anomalías: dominio rico…»). Los nombres de clases del dominio, del algoritmo y del repositorio son los de docs (`IAnomaliaRepository`, `AnomaliaRepositoryPostgis`, T-ARQ06); los demás repositorios usan el mismo sufijo `Repository` (término técnico en inglés). Cada lectura o escritura tiene su puerto en `Application/Puertos/` (T-ARQ07) y los repositorios se dividen en clases con una sola responsabilidad y puertos separados (ISP, sin `partial`) para respetar el límite de 200 líneas (T-ARQ08). Los tipos que cruzan al controlador viven en Application (D4).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| `ContextoReporte` gana `RecibidoUtc` (docs: `(ConSesion, EventosEnLaHora)`; confirmado D1) | RN-11 mide la antigüedad contra la **recepción**, RN-09 purga a 72 h **de recibido**, y `reporte_anomalia.recibido_utc` necesita ese valor | Usar la hora de procesamiento descartaría eventos procesados tarde y adelantaría la purga |
| `AccionAdmin.MantenerDescartada` (docs: 3 acciones; confirmado D1) | RN-07 dice «la reabre **o la mantiene descartada**, y en ese caso el contador se reinicia» | Sin la acción no hay forma de reiniciar el contador sin reabrir |
| Columna `transicion.actor_id` (confirmado D1) | El detalle del prototipo muestra quién hizo cada cambio (sistema o administrador) | Leer la auditoría desde Anomalías no está permitido (no hay contrato de lectura) |
| Columna `reporte_anomalia.km_via` (confirmado D1) | Cobertura del corredor (RF-18: tramos de 1 km con reportes en 7 días) y km en el detalle | Proyectar 48 000 puntos por día sobre la polilínea en cada consulta del Dashboard |
| Columna `anomalia.km_via` (confirmado D13) | El mapa (todos los roles), el detalle, las listas y la exportación muestran el km de la anomalía | Calcularlo en cada lectura obligaría a cargar la polilínea en Mapa |
