# Implementation Plan: Módulo Consulta de Mapa

**Branch**: `006-modulo-consulta-mapa` | **Date**: 2026-10-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/006-modulo-consulta-mapa/spec.md`

## Summary

Módulo de **solo lectura y sin tablas**. El dominio decide el alcance por rol (`PoliticaAlcance`, `ZonaDemo`, `Bbox`) sin geometría pesada; la aplicación tiene tres caminos (por celdas con caché y `ETag`, filtrado, cambios desde una fecha) detrás de `ConsultarMapaUseCase`; la infraestructura implementa el puerto `IFuenteCeldas` con el *Decorator* `FuenteCeldasCacheada` (HybridCache) sobre `FuenteCeldasAnomalias`, y el *Observer* `InvalidarCeldasMapa`. Sigue `diseño-interno-de-modulos.md` §4 y §6.5.

## Technical Context

**Language/Version**: C# 14 / .NET 10

**Primary Dependencies**: `Microsoft.Extensions.Caching.Hybrid` (HybridCache, nivel 1 = `IMemoryCache`), `System.Text.Json` (`Utf8JsonWriter` para serializar cada celda una vez), ASP.NET Core (controlador, `PipeWriter` para escribir la respuesta)

**Storage**: ninguno (lee por `IAnomaliaLectura`, 004)

**Testing**: xUnit para `ZonaDemo`, `Bbox`, `PoliticaAlcance` y `ETag`; NSubstitute para los casos de uso; `WebApplicationFactory` + Testcontainers para recorte por rol a través de la caché, 304 e invalidación; k6 para EQ-14

**Performance Goals**: acierto ~1 ms, 304 < 1 ms, fallo de caché 1 consulta B-tree (~5–20 ms) (`diseño-interno` §8); p95 < 250 ms (RNF-01)

**Constraints**: nunca mezclar alcances; respuesta armada concatenando bytes ya serializados; sin SQL; archivos ≤ 100 líneas

**Scale/Scope**: ~1 000 celdas del corredor × 2 alcances; decenas de miles de anomalías en total, pocos miles `Activa`

## Constitution Check

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | Caché, `ZonaDemo`, `ETag` y filtros como `diseño-interno` §4; 2 agregados a otras specs justificados (R-001, R-002) | ✅ |
| II. Monolito modular | Sin esquema; solo `Anomalias/Contracts` y `Shared` | ✅ |
| III. Calibrable | 5 parámetros en `MapaOptions` | ✅ |
| IV. Datos confiables | Invalidación tras el *commit* de Anomalías; `ETag` con instante de arranque | ✅ |
| V. Privacidad | Las bajas no revelan el estado; ninguna respuesta lleva datos de reportes ni seudónimos | ✅ |
| VI. Acceso en el servidor | Recorte por rol en el dominio del servidor; `estado` solo para el administrador; borde público solo sin token | ✅ |
| VII. Pruebas primero | Recorte anónimo a través de la caché probado antes del código (EQ-11) | ✅ |
| VIII. Código pequeño | Un archivo por política y por camino de consulta | ✅ |
| IX. API documentada | `mapa.openapi.yaml` | ✅ |
| X. Costo cero | Caché en memoria + borde gratuito de Cloudflare; sin Redis (ADR-006) | ✅ |

## Project Structure

### Documentation

```text
specs/006-modulo-consulta-mapa/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/mapa.openapi.yaml · contracts/contratos-internos.md
└── checklists/requirements.md · checklists/trazabilidad.md
```

### Source Code — árbol exacto

```text
backend/src/Modules/KillaUru.Mapa/
├── KillaUru.Mapa.csproj                          # referencia: Shared, Anomalias
├── MapaModule.cs                                 # options, HybridCache, Decorator, suscripción, casos de uso
├── README.md
├── Contracts/                                    # vacío a propósito
├── Domain/
│   ├── Alcance.cs                                # Anonimo, Registrado
│   ├── Bbox.cs                                   # Parse("minLon,minLat,maxLon,maxLat"), Intersecta(limites), Contiene(lat, lon)
│   ├── PoligonoGeo.cs                            # Contiene (rayos) e Intersecta(rectángulo)
│   ├── ZonaDemo.cs                               # Clasificar(celdas) → Dentro | Borde | Fuera; Contiene(lat, lon)
│   ├── PlanConsulta.cs                           # PorCeldas(alcance) | Filtrada(estados, recortarDemo) | Cambios(recortarDemo)
│   └── PoliticaAlcance.cs                        # RN-08: (rol, ¿filtros?, ¿updatedSince?) → PlanConsulta o NoPermitido
├── Application/
│   ├── MapaOptions.cs                            # sección "Mapa"
│   ├── ConsultaMapa.cs                           # rol, Bbox?, filtros, updatedSince, ifNoneMatch
│   ├── ResultadoMapa.cs                          # Contenido(cuerpo, etag?, publico) | NoModificado(etag) | Invalido | Prohibido
│   ├── Puertos/ IFuenteCeldas.cs · IVersionesCelda.cs
│   ├── CorredorMapa.cs                           # celdas del corredor y clasificación demo (se arma una vez, perezoso)
│   ├── CalculadorETag.cs                         # SHA-256(arranque, alcance, versiones) → "\"…\""
│   ├── ConsultarMapaUseCase.cs                   # aplica PoliticaAlcance y delega en uno de los tres
│   ├── ConsultarPorCeldasUseCase.cs              # bbox → celdas → ETag/304 → IFuenteCeldas
│   ├── ConsultarFiltradoUseCase.cs               # IAnomaliaLectura.FiltrarAsync + recorte demo
│   └── ConsultarCambiosUseCase.cs                # IAnomaliaLectura.CambiosDesdeAsync → altas y bajas
├── Adapters/
│   ├── AnomaliasController.cs                    # GET /api/v1/anomalies; If-None-Match, ETag, Cache-Control
│   └── Dtos/ ParametrosMapaDto.cs
└── Infrastructure/
    ├── SerializadorGeoJson.cs                    # Feature, baja, encabezado y cierre de la FeatureCollection
    ├── CeldaGeoJson.cs                           # [ImmutableObject(true)] sealed: byte[] de las features de una celda
    ├── FuenteCeldasAnomalias.cs                  # ActivasPorCeldasAsync([celda]) → filtra borde → serializa
    ├── FuenteCeldasCacheada.cs                   # Decorator con HybridCache (etiqueta celda:{id})
    ├── VersionesCelda.cs                         # ConcurrentDictionary<long, long> + InstanteArranque
    └── InvalidarCeldasMapa.cs                    # IManejador<AnomaliaCambiada>

backend/src/KillaUru.Shared/Domain/Celda.cs      # + Limites() (cambio en 001, R-002)
backend/tests/KillaUru.Mapa.Tests/               Domain/ · Application/
backend/tests/KillaUru.Integracion.Tests/Mapa/   RecorteRolApi · CacheEtagApi · CambiosApi · FiltrosApi
backend/tests/carga/mapa.k6.js                    # EQ-14
```

**Structure Decision**: plantilla de `enfoque-arquitectonico.md` §4.1 («Consulta de Mapa: recorte por rol»). El `ConsultarMapaUseCase` de docs se divide en un despachador y tres caminos para respetar las 100 líneas; el controlador conserva el nombre `AnomaliasController` de docs. `MapaOptions` va en `Application/` como dice `enfoque` §5.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| Operación nueva `IAnomaliaLectura.CeldasDelCorredor()` (cambio en 004, su R-012) | El `bbox` del corredor abarca ~150 000 celdas de rejilla; solo ~1 000 tocan la vía, y la polilínea es interna de Anomalías | Recorrer todas las celdas del `bbox` multiplica por 150 las búsquedas en caché; copiar la polilínea en Mapa duplica un dato con dos dueños |
| `Celda.Limites()` en Shared (cambio en 001) | `Bbox` y `ZonaDemo` necesitan el rectángulo de una celda | Decodificar el id de la celda dentro de Mapa duplica la fórmula de `Celda.De` |
| `MapaOptions` sin «tamaño de celda» (docs lo lista) | La celda la fija Shared y la guarda Anomalías en `anomalia.celda`; si Mapa usara otra, `AnomaliaCambiada` invalidaría la celda equivocada | — |
