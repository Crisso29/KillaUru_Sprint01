# Quickstart: comprobar el módulo Anomalías

## Requisitos previos

001 y 002 funcionando; migraciones `V0004` y `V0005` aplicadas (polilínea cargada: `/health/ready` = 200). Hasta que exista Ingesta (005), los eventos se inyectan con la prueba de integración con los escenarios automatizados de `tests/KillaUru.Integracion.Tests/Anomalias/`.

## 1. Pruebas automáticas

```powershell
cd backend
dotnet test --project tests/KillaUru.Anomalias.Tests --no-restore  # dominio y casos de uso
dotnet test --project tests/KillaUru.Integracion.Tests --no-restore -- --filter-namespace "KillaUru.Integracion.Tests.Anomalias"
```

## 2. Escenarios que deben pasar (los de `diseño-interno-de-modulos.md` §10 y `enfoque` §8)

| # | Escenario | Esperado |
|---|---|---|
| 1 | confidence 0.74 / 0.75 / 0.76 | rechazo / pasa / pasa |
| 2 | precisión 19 / 20 / 21 m | pasa / pasa / rechazo |
| 3 | 49 / 50 / 51 m de la vía | pasa / pasa / rechazo `FueraDelCorredor` |
| 4 | detectado hace 168 h exactas / 168 h + 1 s respecto de la **recepción** | pasa / rechazo |
| 5 | 4 celulares de un colectivo, severidad 4, mismo segundo | `PendienteConfirmacion` |
| 6 | + otro auto con sesión a 3:00 min / a 2:59 min | `Activa` / sigue pendiente |
| 7 | 2 vehículos anónimos sin sesión, severidad 5 | sigue pendiente (RN-14) |
| 8 | dispositivo con 121 eventos en la hora, severidad 2 | nace `PendienteConfirmacion`; otro vehículo sin sesión la publica |
| 9 | saltos de posición a 160 km/h separados 9 s / 10 s | no se evalúa / `VelocidadImposible` |
| 10 | mediana {2,3,3,5} / {3,4} | 3 / 4 |
| 11 | reportes en 2 días locales / 3 días locales (uno a las 22:00 Lima) | `Episodica` / `Persistente` |
| 12 | `Expirada` + reporte 5 detectado después del vencimiento | `PendienteConfirmacion` |
| 13 | `Expirada` + reporte detectado antes del vencimiento | sigue `Expirada` |
| 14 | mismo `eventoId` dos veces | 1 reporte; 2.º resultado `YaProcesado` |
| 15 | 20 reportes del mismo bache en paralelo | 1 sola anomalía |
| 16 | `Descartada` + reportes de 3 vehículos en 7 días | `porRevisar = true` |
| 17 | reloj +1 h tras el vencimiento → tarea de caducidad | `Expirada`, transición registrada, `AnomaliaCambiada` publicada |
| 18 | reloj +72 h desde la recepción → purga | `seudonimo = NULL` en `reporte_anomalia` |
| 19 | saltos de posición a 150 / 151 km/h separados 10 s (precisión 0 m) | pasa / `VelocidadImposible` (T-QA18) |
| 20 | detectado 5 min − 1 s / 5 min / 5 min + 1 s en el futuro respecto de la recepción | pasa / pasa / `TimestampFueraDeRango` (T-QA18) |
| 21 | `Reparada` ayer + reporte detectado hoy, antes de su `vence_utc` anterior | vuelve a nacer (D14) |
| 22 | purga de 72 h sobre un reporte recibido a las 10:37:12 | `seudonimo = NULL` y `recibido_utc = 10:00:00` en la misma sentencia (D3) |
| 23 | `Descartada` hace 2 años + 1 día con un reporte de hace 1 año → purga histórica | se conserva; sin reportes nuevos en 2 años → se borra (D7) |
| 24 | exportación CSV con 37 filas | registro `DatasetExportado` con `filas = 37` antes del archivo (D15) |

## 3. Administración (con token de administrador)

```powershell
curl -s http://localhost:8080/api/v1/moderacion/pendientes -H "Authorization: Bearer $env:KU_TOKEN_ADMIN"
curl -s -X POST http://localhost:8080/api/v1/anomalies/<id>/moderacion -H "Authorization: Bearer $env:KU_TOKEN_ADMIN" -H "Content-Type: application/json" -d '{"accion":"MarcarReparada"}'
curl -s -X POST http://localhost:8080/api/v1/exportaciones -H "Authorization: Bearer $env:KU_TOKEN_ADMIN" -H "Content-Type: application/json" -d '{"formato":"CSV","estados":["Activa"]}' -o anomalias.csv
```

**Esperado:** la moderación devuelve el detalle con el nuevo estado y aparece en `GET /api/v1/auditoria?accion=AnomaliaModerada`; repetir `MarcarReparada` → 409 `transicion_no_permitida`; el CSV tiene la cabecera `id,tipo,severidad,estado,persistencia,km,latitud,longitud,reportes,primer_reporte,ultimo_reporte` y ninguna columna `seudonimo`; `GET /api/v1/auditoria?accion=DatasetExportado` muestra `filas`.

## 4. Verificación de privacidad (EQ-04)

```sql
SELECT count(*) FROM anomalias.reporte_anomalia
WHERE seudonimo IS NOT NULL AND recibido_utc < now() - interval '72 hours';   -- Esperado: 0
SELECT count(*) FROM anomalias.reporte_anomalia
WHERE seudonimo IS NULL AND recibido_utc <> date_trunc('hour', recibido_utc); -- Esperado: 0 (D3)
```

## Resultado local — 2026-10-26

Ver [revisión y evidencias](revision-implementacion.md). Las pruebas usan PostgreSQL/PostGIS en Docker; requieren Docker Desktop activo. La medición `RendimientoAnomaliasTests` guarda `backend/TestResults/004-rendimiento.json`: aprobar la ejecución solo confirma que se procesaron 300 eventos; la aceptación de p95 ≤ 100 ms se evalúa aparte y sigue pendiente. EQ-12 completo necesita el spec 005.
