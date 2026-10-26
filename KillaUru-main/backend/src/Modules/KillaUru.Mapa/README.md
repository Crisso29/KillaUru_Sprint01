# Mapa

> **Estado**: pendiente de implementación — spec 006.
> El módulo está registrado en el host (`AddMapaModule`) pero no tiene endpoints ni lógica de negocio todavía.

## Propósito

Expone las anomalías activas al panel web y a la app del conductor en forma de capas
geoespaciales. Consume los contratos públicos de Anomalías (`IAnomaliaLectura`) para
obtener los datos; nunca accede directamente a la base de datos de otro módulo (ADR-002).

## Responsabilidades planeadas (spec 006)

- Devolver anomalías visibles dentro de un bounding box con filtros opcionales (tipo, severidad).
- Calcular el resumen del día por celda para la vista de calor del panel.
- Actualizar la vista en tiempo real cuando `AnomaliaCambiada` se publica (evento de Anomalías).
- Respetar los límites de tasa: anónimos 60 consultas/h, conductores 1 000/h (FR-010).

## Dependencias planeadas

- `KillaUru.Anomalias.Contracts` — `IAnomaliaLectura`, `AnomaliaVisible`, `FiltroAnomalias`, `ResumenDia`
- `KillaUru.Shared` — dominios transversales, `IPublicadorEventos` (suscripción a eventos)

## API planeada (prefijo `/api/v1`)

| Ruta | Método | Acceso | Propósito |
|---|---|---|---|
| `/mapa/anomalias` | GET | Público | Anomalías visibles en un bbox (GeoJSON) |
| `/mapa/resumen-dia` | GET | Público | Calor por celda para el panel |

> Las rutas y el contrato definitivos se especifican en `specs/006-modulo-mapa/`.

## Nota de privacidad

Las respuestas de Mapa nunca incluyen seudónimos, ni identificadores de dispositivo, ni
conteos por conductor (RN-09). Solo posición, tipo y severidad de la anomalía.

## Verificación desde `backend/`

```powershell
dotnet build KillaUru.sln
# Los tests se agregan al implementar el spec 006
```

## Bloqueante para el E2E

El test de punta a punta (`PuntaAPunta/FlujoPuntaAPuntaTests.cs`) está marcado con `Skip`
hasta que este módulo esté implementado junto con Ingesta (spec 005).
