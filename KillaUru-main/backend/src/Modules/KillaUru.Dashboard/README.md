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

## Cálculo y parámetros

`TareaIndicadoresDiarios` intenta recuperar días faltantes al arrancar y después ejecuta
según `Dashboard:HoraCalculoLocal` (00:30 Lima por defecto). `DiasRecuperables=2`,
`RetencionDias=730`, `DiasConductorActivo=7` y `DesfaseHorarioHoras=-5` son valores
de `DashboardOptions`; validar la configuración efectiva del despliegue.

Eventos se compara mediante variación porcentual; conductores y dispositivos, por diferencia.
Cobertura = tramos con reportes / tramos totales; confirmación = confirmaciones /
(confirmaciones + pendientes expiradas); reaparición = renacimientos / expiraciones.
Las tasas se expresan en porcentaje, se comparan por diferencia y usan `null` cuando falta
una base válida. Conservar conteos permite recomponer tasas sin promediar porcentajes indebidamente.

Ejemplo contra pruebas:

```powershell
curl.exe "$env:KU_URL_PRUEBAS/api/v1/dashboard/indicadores?dias=7" -H "Authorization: Bearer $env:KU_TOKEN_ADMIN"
```

## Cómo se prueba

Ejecutar desde `backend/`; integración requiere Docker:

```powershell
dotnet test --project tests/KillaUru.Dashboard.Tests
dotnet test --project tests/KillaUru.Integracion.Tests
```

El contrato dedicado y el cierre de trazabilidad siguen pendientes.
Consultar [pruebas](../../../../specs/007-modulo-dashboard/pruebas.md) y
[cierre](../../../../specs/007-modulo-dashboard/cierre.md).
