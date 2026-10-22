# Quickstart: comprobar el módulo Dashboard

## Requisitos previos

001, 003 y 004 funcionando; migración `V0007` aplicada. Semilla `db/semillas/dashboard-60-dias.sql` con 60 días de indicadores (y 2 días faltantes a propósito). Token de administrador en `$env:KU_TOKEN_ADMIN`.

## 1. Pruebas automáticas

```powershell
cd backend
dotnet test tests/KillaUru.Dashboard.Tests
dotnet test tests/KillaUru.Integracion.Tests --filter "FullyQualifiedName~Dashboard"
```

## 2. Escenarios que deben pasar

| # | Escenario | Esperado |
|---|---|---|
| 0 | Arrancar la API a las 2026-10-21 03:00Z | la tarea corre al arrancar y guarda los días faltantes (T-OPS05) |
| 1 | Reloj 2026-10-21 05:29:59Z → 05:30:00Z | antes no corre; a las 05:30 (próximo instante de `ProgramaReloj`) guarda el día 2026-10-20 (9 filas) |
| 2 | La tarea corre dos veces el mismo día | sigue habiendo 9 filas, mismos valores |
| 3 | API apagada el 21 y el 22; arranca el 23 a las 10:00 Lima | calcula 21 y 22; no intenta el 20 si ya estaba |
| 4 | Anomalías lanza error en el cálculo | 0 filas de ese día; al arrancar otra vez, se calcula |
| 5 | Eventos D−7…D−1 = 250, 270, 260, 290, 276, 302, 300 y D = 321 | `comparacion` 278,3 · `variacion` 15,3 % |
| 6 | 7 días: 18 confirmaciones y 6 pendientes expiradas | `confirmacionPct` 75,0 |
| 7 | 7 días sin expiraciones | `reaparicionPct.valor` = `null` |
| 8 | Tramos 237 de 333 | `coberturaPct` 71,2 |
| 9 | Día faltante dentro de la serie | `null` en su posición; los promedios lo ignoran |
| 10 | Filas de hace 731 días | borradas en la siguiente corrida |
| 11 | `dias=10` / token de conductor / sin token | 400 / 403 / 401 (T-SEG20) |
| 12 | `DesfaseHorarioHoras` de Dashboard y de Anomalías | iguales (R-008) |

## 3. A mano

```powershell
curl -s "http://localhost:8080/api/v1/dashboard/indicadores?dias=14" -H "Authorization: Bearer $env:KU_TOKEN_ADMIN"
```

## 4. Privacidad (RN-17)

```sql
SELECT column_name FROM information_schema.columns WHERE table_schema = 'dashboard';
-- Esperado: dia, indicador, valor
```
