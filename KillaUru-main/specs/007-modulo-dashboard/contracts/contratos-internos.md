# Contratos internos del módulo Dashboard (`KillaUru.Dashboard/Contracts/`)

## 1. Lo que el módulo **ofrece**

Nada. `Contracts/` queda vacío: ningún módulo depende de Dashboard (regla 4). Mover las métricas técnicas a `Shared` fue justamente lo que evitó el ciclo Anomalías ↔ Dashboard (`componentes-arquitectonicos.md` §8).

## 2. Lo que el módulo **usa**

| Contrato | De | Para qué |
|---|---|---|
| `IAnomaliaLectura.ResumenDelDiaAsync(DateOnly diaLocal, ct)` → `ResumenDia` | Anomalías (004, R-011) | 8 de los 9 números del día |
| `IIdentidadConsultas.ContarConductoresActivosAsync(int dias, ct)` → `int` | Identidad (003) | «Conductores registrados activos» (RN-17) |
| `IMetricas.RegistrarTareaCorrecta("indicadores")` | Shared (001) | RNF-22 (alerta a las 26 h) |
| `Roles` | Shared (001) | `[Authorize]` solo administrador |
| `TimeProvider` | Host (001) | Día local, hora del cálculo |

## 3. Qué devuelve cada contrato (y qué nunca)

| Contrato | Devuelve | Nunca devuelve |
|---|---|---|
| `ResumenDelDiaAsync` | `Dia`, `EventosRecibidos`, `DispositivosDistintos`, `TramosConReportes7d`, `TramosTotales`, `Confirmaciones`, `PendientesExpiradas`, `Expiraciones`, `Renacimientos` | seudónimos, ids de reportes o de anomalías |
| `ContarConductoresActivosAsync` | un número | ids ni correos (003, «lo que no ofrece a propósito») |

Por eso ningún dato del Dashboard puede unir una cuenta con sus eventos (RN-17, RNF-10): los dos contratos devuelven solo conteos y vienen de módulos distintos.
