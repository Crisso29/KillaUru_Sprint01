# Quickstart: comprobar el módulo Consulta de Mapa

## Requisitos previos

001, 003 y 004 funcionando (corredor cargado); semilla `db/semillas/mapa-roles.sql` con anomalías en 4 grupos: dentro del demo, en una celda de borde pero fuera, fuera del demo, y no `Activa` (una por estado). Tokens en `$env:KU_TOKEN_CONDUCTOR` y `$env:KU_TOKEN_ADMIN`.

## 1. Pruebas automáticas

```powershell
cd backend
dotnet test tests/KillaUru.Mapa.Tests
dotnet test tests/KillaUru.Integracion.Tests --filter "FullyQualifiedName~Mapa"
```

## 2. Escenarios que deben pasar

| # | Escenario | Esperado |
|---|---|---|
| 1 | Anónimo, `bbox` del corredor | solo el grupo «dentro»; `public, max-age=60`; `ETag` |
| 2 | Conductor, mismo `bbox`, y luego anónimo otra vez | conductor: todas las `Activa`; anónimo: igual que el 1 (EQ-11) |
| 3 | Conductor repite con `If-None-Match` | 304, 0 consultas a la BD |
| 4 | Cambiar una anomalía de la zona y repetir el 3 | 200 con `ETag` nuevo y el cambio visible |
| 5 | Conductor con `estado=Expirada` | 403 |
| 6 | Administrador sin filtros | `Activa` + `PendienteConfirmacion`, con `estado` |
| 7 | Administrador `estado=Descartada&tipo=Bache` | solo descartadas de tipo bache |
| 8 | Conductor `severidad=4,5` | `Activa` 4–5 del corredor, sin `ETag` |
| 9 | `updatedSince=T`, luego una anomalía expira | conductor: `{id, eliminada: true}`; anónimo: la baja solo si estaba en el demo |
| 10 | `updatedSince` + `tipo` / en el futuro / `bbox=1,2,3` | 400 |
| 11 | 300 peticiones a la vez a una celda fría | 1 llamada a `ActivasPorCeldasAsync` |
| 12 | Reiniciar la API y repetir con el `ETag` viejo | 200 (nunca un 304 falso) |

## 3. A mano

```powershell
curl -si "http://localhost:8080/api/v1/anomalies?bbox=-76.20,-14.10,-74.10,-13.05"                       # anónimo
curl -si "http://localhost:8080/api/v1/anomalies" -H "Authorization: Bearer $env:KU_TOKEN_CONDUCTOR"
curl -si "http://localhost:8080/api/v1/anomalies" -H "Authorization: Bearer $env:KU_TOKEN_CONDUCTOR" -H 'If-None-Match: "<etag>"'
curl -s  "http://localhost:8080/api/v1/anomalies?estado=PendienteConfirmacion" -H "Authorization: Bearer $env:KU_TOKEN_ADMIN"
```

## 4. Borde y carga

- Cloudflare: `curl -sI https://api.killauru.pe/api/v1/anomalies` dos veces → la segunda con `cf-cache-status: HIT`; con `Authorization` → siempre `BYPASS` o `DYNAMIC`.
- `k6 run tests/carga/mapa.k6.js` → 300 VU, p95 < 250 ms en la API (EQ-14).
