# Contratos internos del módulo Consulta de Mapa (`KillaUru.Mapa/Contracts/`)

## 1. Lo que el módulo **ofrece**

Nada. `Contracts/` queda vacío: ningún módulo depende de Mapa (regla 4). Su única cara pública es `GET /api/v1/anomalies`.

## 2. Lo que el módulo **usa**

| Contrato | De | Para qué |
|---|---|---|
| `IAnomaliaLectura.ActivasPorCeldasAsync(celdas, ct)` | Anomalías (004) | Llenar una celda de la caché (`FuenteCeldasAnomalias`), siempre de **una** celda por llamada |
| `IAnomaliaLectura.FiltrarAsync(filtro, ct)` | Anomalías (004) | Peticiones con filtros y todas las del administrador |
| `IAnomaliaLectura.CambiosDesdeAsync(desde, ct)` | Anomalías (004) | `updatedSince` |
| `IAnomaliaLectura.CeldasDelCorredor()` | Anomalías (004, R-012) | Ajustar el `bbox` a las celdas de la vía (R-001) |
| `AnomaliaVisible`, `AnomaliaCambio`, `FiltroAnomalias`, `TipoAnomalia`, `EstadoAnomalia` | Anomalías (004) | Tipos de lectura |
| `AnomaliaCambiada(AnomaliaId, Celda)` + `IManejador<T>` | Anomalías (004) + Shared (001) | Invalidar la celda (Observer, ADR-006) |
| `Celda` (con `Limites()`), `Roles` | Shared (001) | Rejilla y nombres de rol |
| `TimeProvider` | Host (001) | `generadoUtc` y validación de `updatedSince` |

## 3. Puertos propios (internos)

```csharp
internal interface IFuenteCeldas                       // diseño-interno §6.5
{
    ValueTask<CeldaGeoJson> ObtenerAsync(Celda celda, Alcance alcance, CancellationToken ct);
}

internal interface IVersionesCelda
{
    DateTimeOffset InstanteArranque { get; }
    long Version(Celda celda);                          // 0 si nunca cambió
    void Incrementar(Celda celda);
}
```

Registro en `MapaModule.cs` (el *Decorator* se quita borrando una línea):

```csharp
services.AddSingleton<IFuenteCeldas>(sp => new FuenteCeldasCacheada(
    new FuenteCeldasAnomalias(sp.GetRequiredService<IAnomaliaLectura>(), sp.GetRequiredService<CorredorMapa>()),
    sp.GetRequiredService<HybridCache>()));
services.AddSingleton<IManejador<AnomaliaCambiada>, InvalidarCeldasMapa>();
```

## 4. Garantías que espera de Anomalías

- `AnomaliaCambiada` se publica **después** del *commit* (004 FR-014): al invalidar, la siguiente lectura ya ve el cambio.
- La celda de una anomalía no cambia nunca (RN-05) y siempre pertenece a `CeldasDelCorredor()` (004 R-012).
- `VenceUtc` viene calculado por el dominio (004 FR-017); Mapa solo lo copia.
