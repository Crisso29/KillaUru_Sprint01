# Dashboard

Módulo que calcula y expone los 6 indicadores operativos del panel para administradores.
Los datos se calculan diariamente mediante `TareaIndicadoresDiarios` y se leen a través de `GET /api/v1/dashboard/indicadores`.

## Propósito

Proveer al panel web los indicadores consolidados del sistema:
- **Eventos del día** — total de reportes válidos y variación porcentual relativa
- **Conductores activos** — únicas en los últimos N días y diferencia absoluta
- **Dispositivos únicos** — distintos del día y diferencia absoluta
- **Cobertura de red** — tramos con reporte sobre total, en puntos
- **Tasa de confirmación** — anomalías confirmadas sobre abiertas, en puntos
- **Tasa de reaparición** — reapariciones sobre expiradas, en puntos

## Decisiones de diseño

- **9 indicadores almacenados** (enum `Indicador` 1–9) → 6 mostrados, calculados al leer (R-003).
- **Privacidad (RN-17)**: solo `dia`, `indicador`, `valor` en DB; sin IDs, seudónimos ni correos.
- **Idempotencia**: `INSERT … ON CONFLICT DO NOTHING` permite recalcular sin duplicados.
- **Recuperación**: `DiasRecuperables = 2` → si la tarea falla, recupera hasta 2 días al arrancar.
- **Desfase horario**: `DesfaseHorarioHoras = -5` debe coincidir con el de Anomalías (R-008).

## Dependencias

- `KillaUru.Anomalias.Contracts` — `IAnomaliaLectura.ResumenDelDiaAsync`
- `KillaUru.Identidad.Contracts` — `IIdentidadConsultas.ContarConductoresActivosAsync`
- `KillaUru.Shared` — `ProgramaReloj`, `IMetricas`, `Roles`

## API (`/api/v1`)

| Ruta | Método | Acceso | Propósito |
|---|---|---|---|
| `/dashboard/indicadores?dias={7,14,30}` | GET | Administrador | Resumen de indicadores |

## Verificación desde `backend/`

```powershell
dotnet test tests/KillaUru.Dashboard.Tests
dotnet test tests/KillaUru.Integracion.Tests --filter "FullyQualifiedName~Dashboard"
```
