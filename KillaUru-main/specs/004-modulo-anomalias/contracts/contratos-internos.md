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
    /// Solo Activa, de las celdas pedidas (índice ix_anomalia_mapa). Para la caché por celdas de Mapa.
    Task<IReadOnlyList<AnomaliaVisible>> ActivasPorCeldasAsync(IReadOnlyCollection<long> celdas, CancellationToken ct);

    /// Con filtros (RF-14): bbox + tipo, severidades, fechas sobre la última detección y estados.
    Task<IReadOnlyList<AnomaliaCambio>> FiltrarAsync(FiltroAnomalias filtro, CancellationToken ct);

    /// Todas las anomalías con actualizada_utc > desde, con su estado actual (Mapa decide altas y bajas por rol).
    Task<IReadOnlyList<AnomaliaCambio>> CambiosDesdeAsync(DateTimeOffset desde, CancellationToken ct);

    /// Celdas que toca la franja de DistanciaMaximaViaMetros alrededor de la polilínea (RN-13), calculadas
    /// al cargar CorredorVia. Vacío si el corredor aún no está cargado. Mapa recorta el bbox con ellas (006, R-001).
    IReadOnlySet<long> CeldasDelCorredor();

    /// Conteos de un día local (UTC−5) para RF-18. Sin identidades.
    Task<ResumenDia> ResumenDelDiaAsync(DateOnly diaLocal, CancellationToken ct);
}
```

## 3. Tipos públicos

| Tipo | Definición |
|---|---|
| `EventoReportado` | ver `data-model.md` §2 |
| `ContextoReporte(bool ConSesion, int EventosEnLaHora, DateTimeOffset RecibidoUtc)` | `RecibidoUtc` agregado (R-001) |
| `ResultadoProceso` | `Procesado` · `Descartado(MotivoDescarte Motivo)` · `YaProcesado` |
| `MotivoDescarte` | 7 valores de RN-11 |
| `TipoAnomalia` | `Bache = 1`, `Hundimiento = 2` |
| `EstadoAnomalia` | `Activa = 1`, `PendienteConfirmacion = 2`, `Expirada = 3`, `Reparada = 4`, `Descartada = 5` |
| `AnomaliaCambiada(Guid AnomaliaId, Celda Celda) : IEventoDominio` | se publica tras cada commit que cambia una anomalía (procesamiento, caducidad, moderación) |
| `AnomaliaVisible`, `FiltroAnomalias`, `AnomaliaCambio`, `ResumenDia` | ver `data-model.md` §5 |

## 4. Lo que el módulo usa de otros

| Contrato | De | Para qué |
|---|---|---|
| `IAuditoriaService` | Auditoría (002) | `AnomaliaModerada`, `DatasetExportado` en la misma transacción |
| `IContextoTransaccion`, `IPublicadorEventos`, `IMetricas`, `Celda`, `Severidad`, `Confidence`, `Coordenada`, `SeudonimoInstalacion` | Shared (001) | — |
