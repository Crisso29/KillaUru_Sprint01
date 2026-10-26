# Dashboard

> **Estado**: pendiente de implementación — spec 007 (o integrado en spec 006).
> El módulo está registrado en el host (`AddDashboardModule`) pero no tiene endpoints ni lógica de negocio todavía.

## Propósito

Provee al panel web los datos agregados que necesita para sus vistas de gestión:
métricas históricas, resúmenes por zona, exportaciones y el estado operativo del sistema.
A diferencia de Mapa (que muestra en tiempo real), Dashboard responde a consultas
analíticas que toleran latencia de segundos o minutos.

## Responsabilidades planeadas

- Series de tiempo de anomalías por tipo, severidad y zona geográfica.
- Conteos de conductores activos y lotes enviados en un período.
- Estado del corredor y salud del pipeline de procesamiento.
- Vista de resumen para la revisión mensual de privilegios (spec 003).

## Dependencias planeadas

- `KillaUru.Anomalias.Contracts` — `IAnomaliaLectura`, `ResumenDia`
- `KillaUru.Auditoria.Contracts` — lecturas administrativas para métricas de riesgo
- `KillaUru.Shared` — dominios transversales

## API planeada (prefijo `/api/v1`)

| Ruta | Método | Acceso | Propósito |
|---|---|---|---|
| `/dashboard/metricas` | GET | Administrador | Series históricas por tipo y período |
| `/dashboard/zonas` | GET | Administrador | Resumen por zona geográfica |
| `/dashboard/estado` | GET | Administrador | Estado del pipeline y corredor |

> Las rutas y el contrato definitivos se especifican en el spec correspondiente.

## Nota de diseño

Dashboard no tiene su propia base de datos ni replica datos; consume puertos de lectura
de Anomalías y Auditoría. Esto evita inconsistencias y mantiene una única fuente de verdad (ADR-002).
Las consultas analíticas lentas deben usar índices dedicados en los esquemas de origen,
no vistas materializadas en el esquema `dashboard`.

## Verificación desde `backend/`

```powershell
dotnet build KillaUru.sln
# Los tests se agregan al implementar el spec correspondiente
```
