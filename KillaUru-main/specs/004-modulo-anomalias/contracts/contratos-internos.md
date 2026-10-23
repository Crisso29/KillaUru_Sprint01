# Contratos internos del módulo Anomalías (`KillaUru.Anomalias/Contracts/`)

## 1. `IAnomaliasService` — lo usa Ingesta (005)

```csharp
public interface IAnomaliasService
{
    /// Reglas de RECEPCIÓN de RN-11 y RN-13, puras y sin BD (ADR-005 punto 1).
    /// Ingesta la llama por cada evento ANTES de responder 200. `recibidoUtc` = hora de recepción del lote.
    /// Devuelve null si el evento pasa, o el motivo del rechazo.
    MotivoDescarte? EvaluarIntrinseca(EventoReportado evento, DateTimeOffset recibidoUtc);

    /// Validar → agrupar → confirmar → clasificar (enfoque §6). Abre SU transacción y SU bloqueo por celda.
    /// Idempotente por EventoId: si ya se procesó, devuelve YaProcesado sin efectos (ADR-005 punto 5).
    /// Publica AnomaliaCambiada DESPUÉS del commit.
    Task<ResultadoProceso> ProcesarReporteAsync(EventoReportado evento, ContextoReporte contexto, CancellationToken ct);
}
```

## 2. `IAnomaliaLectura` — lo usan Consulta de Mapa (006) y Dashboard (007)

```csharp
public interface IAnomaliaLectura
{
    /// Solo Activa, de las celdas pedidas (índice ix_anomalia_mapa), con KmVia (D13). Para la caché por celdas de Mapa.
    Task<IReadOnlyList<AnomaliaVisible>> ActivasPorCeldasAsync(IReadOnlyCollection<long> celdas, CancellationToken ct);

    /// Con filtros (RF-14): bbox + tipo, severidades, fechas sobre la última detección y estados.
    Task<IReadOnlyList<AnomaliaCambio>> FiltrarAsync(FiltroAnomalias filtro, CancellationToken ct);

    /// Todas las anomalías con actualizada_utc > desde, con su estado actual (Mapa decide altas y bajas por rol).
    Task<IReadOnlyList<AnomaliaCambio>> CambiosDesdeAsync(DateTimeOffset desde, CancellationToken ct);

    /// Celdas que toca la franja de DistanciaMaximaViaMetros alrededor de la polilínea (RN-13), calculadas
    /// al cargar CorredorVia. Vacío si el corredor aún no está cargado. Mapa recorta el bbox con ellas (006, R-001).
    IReadOnlySet<long> CeldasDelCorredor();

    /// Conteos de un día local (UTC−5) para RF-18. Sin identidades. EventosRecibidos = eventos válidos (reportes
    /// procesados); Renacimientos = Expirada → Activa | PendienteConfirmacion (reaparición, D14).
    Task<ResumenDia> ResumenDelDiaAsync(DateOnly diaLocal, CancellationToken ct);
}
```

## 3. Tipos públicos

| Tipo | Definición |
|---|---|
| `EventoReportado` | ver `data-model.md` §2 |
| `ContextoReporte(bool ConSesion, int EventosEnLaHora, DateTimeOffset RecibidoUtc)` | `RecibidoUtc` agregado (R-001) |
| `ResultadoProceso` | `Procesado` · `Descartado(MotivoDescarte Motivo)` · `YaProcesado` |
| `MotivoDescarte` | `EsquemaInvalido = 1` … `VelocidadImposible = 7` (RN-11) y `SeudonimoInconsistente = 8` (D2): regla de **lote** que evalúa Ingesta en la recepción (un lote = un seudónimo: el de la cabecera `X-KillaUru-Seudonimo` o, si falta, el del primer evento con seudónimo válido); va en `rechazados` del contrato del lote. Anomalías nunca lo devuelve. Los códigos son los de `evento_recibido.motivo` (005) |
| `TipoAnomalia` | `Bache = 1`, `Hundimiento = 2` |
| `EstadoAnomalia` | `Activa = 1`, `PendienteConfirmacion = 2`, `Expirada = 3`, `Reparada = 4`, `Descartada = 5` |
| `AnomaliaCambiada(Guid AnomaliaId, Celda Celda) : IEventoDominio` | se publica tras cada commit que cambia una anomalía (procesamiento, caducidad, moderación) |
| `AnomaliaVisible`, `FiltroAnomalias`, `AnomaliaCambio`, `ResumenDia` | ver `data-model.md` §5; `AnomaliaVisible` y `AnomaliaCambio` llevan `KmVia` (`anomalia.km_via`, D13) |

## 4. Lo que el módulo usa de otros

| Contrato | De | Para qué |
|---|---|---|
| `IAuditoriaService` | Auditoría (002) | `RegistrarAsync(AnomaliaModerada, …)` dentro de la transacción de la moderación; `RegistrarLecturaAsync(DatasetExportado, …)` con `filas` antes de escribir la exportación (D15) |
| `IContextoTransaccion`, `IPublicadorEventos`, `Celda`, `Severidad`, `Confidence`, `Coordenada`, `SeudonimoInstalacion` | Shared (001) | — |
| `IMetricas` | Shared (001) | `RegistrarDescarte`, `RegistrarRetenidoPorTope`, `RegistrarDuracionProceso`, `RegistrarTareaCorrecta`; **nunca** `RegistrarAccionRiesgo` (solo Auditoría, D15) (T-OPS14) |
| `ProgramaReloj` | Shared (001, `Shared/Tareas/`) | próximo instante de las tareas: `:05`, `:10`, 03:30 local (T-OPS05) |
| Comprobación de salud | Host (001 FR-019) | `corredor` en `/health/ready` (T-OPS19) |
