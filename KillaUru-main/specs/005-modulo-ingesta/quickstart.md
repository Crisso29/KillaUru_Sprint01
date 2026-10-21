# Quickstart: comprobar el módulo Ingesta

## Requisitos previos

001 y 004 funcionando (corredor cargado: `/health/ready` = 200); migración `V0006` aplicada. Los eventos de ejemplo están sobre la polilínea de la PE-28A (`db/semillas/lotes/`).

## 1. Pruebas automáticas

```powershell
cd backend
dotnet test tests/KillaUru.Ingesta.Tests
dotnet test tests/KillaUru.Integracion.Tests --filter "FullyQualifiedName~Ingesta"
```

## 2. Escenarios que deben pasar

| # | Escenario | Esperado |
|---|---|---|
| 1 | Lote de 50 buenos | 200 · 50 `aceptados` · 50 filas `estado = 0` que pasan a 1 en segundos |
| 2 | Lote de 50 con 1 sin severidad | 200 · 49 aceptados · 1 rechazado `EsquemaInvalido` |
| 3 | Mismo lote otra vez | 200 · 50 `duplicados` · 0 filas nuevas |
| 4 | Mismo `eventoId` 2 veces en un lote | 1 aceptado · 1 duplicado |
| 5 | Evento a 51 m de la vía / confidence 0.74 | rechazado `FueraDelCorredor` / `ConfidenceBajo`, sin fila |
| 6 | `"severidad": "alta"` en un evento | solo ese evento `EsquemaInvalido` |
| 7 | Lote vacío / 51 eventos / evento sin `eventoId` / cuerpo no JSON | 400 `lote_invalido` |
| 8 | Cuerpo de 300 KB | 413 |
| 9 | Token vencido | 401, sin filas |
| 10 | Con token de conductor / sin token | `con_sesion` = true / false |
| 11 | Con token de administrador | 403 |
| 12 | Cola llena (capacidad 1 en la prueba) | 503 `saturado` + `Retry-After: 30`, sin filas |
| 13 | Matar el proceso con 500 filas `Recibido` y arrancar | todas procesadas en ≤ 2 min; 0 duplicados en `reporte_anomalia` |
| 14 | Fila `Recibido` con 6 min | `/health/ready` = 503 |
| 15 | Reloj +72 h 1 s desde la recepción → purga | `seudonimo` NULL |
| 16 | Reloj +30 días → purga | se borran las filas 1 y 2; una fila 0 se conserva |

## 3. A mano

```powershell
curl -s -X POST http://localhost:8080/api/v1/anomalies/batch -H "Content-Type: application/json" `
  -H "X-KillaUru-Seudonimo: 6b1c9d3e-2f4a-4b8e-9c7d-1a2b3c4d5e6f" -d "@db/semillas/lotes/lote-50-buenos.json"
```

## 4. Conciliación (RNF-06)

```sql
SELECT estado, count(*) FROM ingesta.evento_recibido GROUP BY estado;
-- aceptados (métrica killauru.ingesta.eventos{resultado="aceptado"}) = suma de los tres estados
SELECT count(*) FROM ingesta.evento_recibido
WHERE seudonimo IS NOT NULL AND recibido_utc < now() - interval '72 hours';   -- Esperado: 0
```

## 5. Carga (RNF-02, RNF-03)

```powershell
k6 run tests/carga/lotes.k6.js   # 300 VU × 10 lotes de 50 · p95 < 300 ms · errores < 1 % · retraso de cola ≤ 30 s
```
