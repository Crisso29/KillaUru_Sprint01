# Data Model: Módulo Consulta de Mapa

El módulo **no tiene tablas** (`componentes-arquitectonicos.md` §6). Todo lo que guarda vive en memoria y se puede perder sin consecuencias: se reconstruye desde Anomalías.

## 1. Qué ve cada rol (RN-08 → `PoliticaAlcance`)

| Rol | Petición | Plan | Estados | Recorte | Caché por celdas | `ETag` | `Cache-Control` |
|---|---|---|---|---|---|:---:|---|
| Anónimo | sin filtros | `PorCeldas(Anonimo)` | `Activa` | celdas del corredor ∩ demo; borde punto por punto | ✅ | ✅ | `public, max-age=60` |
| Anónimo | con filtros | `Filtrada([Activa], recortarDemo)` | `Activa` | `ZonaDemo.Contiene` | ❌ | ❌ | `public, max-age=60` |
| Anónimo | `updatedSince` | `Cambios(recortarDemo)` | altas `Activa` | demo; fuera del demo se omiten altas y bajas (T-SEG16) | ❌ | ❌ | `public, max-age=60` |
| Conductor | sin filtros | `PorCeldas(Registrado)` | `Activa` | celdas del corredor | ✅ | ✅ | `private, no-store` |
| Conductor | con filtros | `Filtrada([Activa])` | `Activa` | — | ❌ | ❌ | `private, no-store` |
| Conductor | `updatedSince` | `Cambios` | altas `Activa`, resto bajas | — | ❌ | ❌ | `private, no-store` |
| Administrador | cualquiera | `Filtrada(estados ?? todos)` | los pedidos; sin `estado`, los 5 (D6) | — | ❌ | ❌ | `private, no-store` |
| No administrador | con `estado` (con o sin token) | `NoPermitido` → 403 (T-SEG20) | — | — | — | — | — |

## 2. Respuesta GeoJSON

```json
{
  "type": "FeatureCollection",
  "generadoUtc": "2026-10-20T15:02:11Z",
  "features": [
    {
      "type": "Feature",
      "id": "0192f1c0-3a7e-7b11-8d2f-4c5e6a7b8c9d",
      "geometry": { "type": "Point", "coordinates": [-74.2581, -13.2105] },
      "properties": {
        "id": "0192f1c0-3a7e-7b11-8d2f-4c5e6a7b8c9d",
        "kmVia": 287.4, "tipo": "Bache", "severidad": 4, "persistencia": "Persistente",
        "ultimaDeteccionUtc": "2026-10-20T14:32:05Z", "venceUtc": "2026-10-27T14:32:05Z"
      }
    },
    { "type": "Feature", "id": "0192f0aa-1111-7222-8333-944455566677", "geometry": null,
      "properties": { "id": "0192f0aa-1111-7222-8333-944455566677", "eliminada": true } }
  ]
}
```

| Propiedad | Origen (`AnomaliaVisible` / `AnomaliaCambio`, 004) | Quién la ve |
|---|---|---|
| `id` | `Id` | todos |
| `kmVia` | `KmVia` (km sobre la vía, calculado al nacer; D13) | todos |
| `tipo` | `Tipo` (`Bache`, `Hundimiento`) | todos |
| `severidad` | `Severidad` (mostrada, RN-05) | todos |
| `persistencia` | `Persistencia` (`Episodica`, `Persistente`) | todos |
| `ultimaDeteccionUtc` | `UltimaDeteccionUtc` («hace 2 horas», RNF-14) | todos |
| `venceUtc` | `VenceUtc` (RF-26: la app deja de alertar al pasar); `null` en `Expirada`, `Reparada` y `Descartada` (T-PO08) | todos |
| `estado` | `Estado` | solo administrador |
| `eliminada` | — (solo en bajas de `updatedSince`) | anónimo y conductor |

## 3. Parámetros de la petición (`ParametrosMapaDto`)

| Parámetro | Formato | Regla |
|---|---|---|
| `bbox` | `minLon,minLat,maxLon,maxLat` | opcional (R-003) |
| `tipo` | `Bache` \| `Hundimiento` | todos |
| `severidad` | lista `1..5` separada por comas | todos |
| `desde`, `hasta` | fecha-hora ISO UTC, sobre `ultimaDeteccionUtc` | todos; `desde ≤ hasta` |
| `estado` | lista de `EstadoAnomalia` | solo administrador; otro rol, con o sin token → 403 |
| `updatedSince` | fecha-hora ISO UTC, ≤ ahora + 5 min | no combina con filtros (400) |
| `If-None-Match` | cabecera | solo camino por celdas |

## 4. Estructuras en memoria

| Estructura | Clave → valor | Vida |
|---|---|---|
| HybridCache | `celda:{id}:{Anonimo|Registrado}` → `CeldaGeoJson` (`byte[]`); etiqueta `celda:{id}` | 10 min o hasta `AnomaliaCambiada`; tope 64 MB |
| `VersionesCelda` | `long celda` → `long version` (empieza en 0) + `InstanteArranque` | mientras viva el proceso |
| `CorredorMapa` | celdas del corredor (`IReadOnlySet<long>`) + clasificación demo (`Dentro`/`Borde`) | se arma una vez |
| `MetricasMapa` | contador `killauru.mapa.cache{resultado}`: `acierto`, `fallo`, `no_modificado` (R-013) | mientras viva el proceso |

## 5. Dominio

| Clase | Firma | Regla |
|---|---|---|
| `Bbox` | `static Bbox? Parse(string?)` · `bool Intersecta((double MinLat, double MinLon, double MaxLat, double MaxLon) r)` · `bool Contiene(double lat, double lon)` | R-003 |
| `PoligonoGeo` | `bool Contiene(double lat, double lon)` (rayos) · `bool Intersecta(rectángulo)` | — |
| `ZonaDemo` | `ClaseCelda Clasificar(rectángulo)` → `Dentro` (4 esquinas dentro y ningún lado cruza), `Borde`, `Fuera` · `bool Contiene(lat, lon)` | RN-08 |
| `PoliticaAlcance` | `PlanConsulta Decidir(string rol, bool hayFiltros, bool hayEstado, bool hayUpdatedSince)` | RN-08, RF-14 |

## 6. `MapaOptions` (sección `Mapa`)

```json
"Mapa": {
  "PoligonoDemo": [[-74.2236, -13.1631], [-74.3307, -13.2420], [-74.3250, -13.2510], [-74.2180, -13.1700], [-74.2236, -13.1631]],
  "VigenciaCacheMinutos": 10,
  "MaxAgeBordeSegundos": 60,
  "TamanoCacheMb": 64
}
```

Validación al arrancar: polígono cerrado o cerrable con ≥ 4 vértices válidos; números > 0. Cada vértice es `[lon, lat]`; el real tiene ≤ 60 vértices (R-010). Los del ejemplo son ilustrativos: los reales salen de la tarea T002.
