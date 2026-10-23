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
| 1 | Lote de 50 buenos | 200 · `loteId` del servidor · 50 `aceptados` · 50 filas `estado = 0` que pasan a 1 en segundos |
| 2 | Lote de 50 con 1 sin severidad | 200 · 49 aceptados · 1 rechazado `EsquemaInvalido` con su `indice` |
| 3 | Mismo lote otra vez | 200 · 50 `duplicados` · 0 filas nuevas |
| 4 | Mismo `eventoId` 2 veces en un lote | 1 aceptado · 1 duplicado |
| 5 | Evento a 51 m de la vía / confidence 0.74 | rechazado `FueraDelCorredor` / `ConfidenceBajo`, sin fila |
| 6 | `"severidad": "alta"` en un evento | solo ese evento `EsquemaInvalido` |
| 7 | Lote vacío / 51 eventos / sin arreglo `eventos` / cuerpo no JSON | 400 `lote_invalido` |
| 8 | Cuerpo de 300 KB | 413 |
| 9 | Token vencido | 401, sin filas |
| 10 | Con token de conductor verificado / sin token | `con_sesion` = true / false |
| 11 | Con token de administrador | 403 |
| 12 | Cola llena (capacidad 1 en la prueba) | 503 `saturado` + `Retry-After: 30`, sin filas |
| 13 | Matar el proceso con 500 filas `Recibido` y arrancar | todas procesadas en ≤ 2 min; 0 duplicados en `reporte_anomalia` |
| 14 | Fila `Recibido` con 6 min | `/health/ready` = 503 |
| 15 | Reloj +72 h 1 s desde la recepción → purga | `seudonimo` y `lote_id` NULL; `recibido_utc` redondeado a la hora |
| 16 | Reloj +30 días → purga | se borran las filas 1 y 2; una fila 0 se conserva |
| 17 | Evento en la posición 7 sin `eventoId` | 200 · `rechazados` incluye `{ id: null, indice: 7, motivo: EsquemaInvalido }` |
| 18 | Cabecera `X-KillaUru-Seudonimo: S` y un evento con seudónimo T | ese evento `SeudonimoInconsistente`, sin fila |
| 19 | Sin cabecera; eventos con S, S, T | seudónimo del lote = S; el de T `SeudonimoInconsistente` |
| 20 | Con token de conductor sin verificar (`verificado = false`) | `con_sesion` = false |
| 21 | Cuerpo con `loteId` | se ignora; la respuesta trae otro `loteId` (UUIDv7) |

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
WHERE (seudonimo IS NOT NULL OR lote_id IS NOT NULL) AND recibido_utc < now() - interval '72 hours';   -- Esperado: 0
```

## 5. Carga (RNF-02, RNF-03, RNF-23)

Ingesta no tiene script propio: se usan los de 001 (`backend/tests/carga/`), fuera de la VM y contra el entorno de pruebas (D11).

```powershell
k6 run tests/carga/carga-combinada.k6.js   # 300 sincronizando + 300 mapa · 10 min · lote p95 < 300 ms · errores < 1 % · retraso_cola_s p95 ≤ 30 s
```
