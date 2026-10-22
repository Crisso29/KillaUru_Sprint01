# Quickstart: comprobar la plataforma base

Guía de validación de punta a punta. No contiene código de implementación.

## Requisitos previos

- Docker con Compose v2 · SDK de .NET 10 · `curl` · k6 (solo para §8, en la PC del autor)
- Copiar `backend/deploy/.env.example` a `backend/deploy/.env` y llenar los secretos de desarrollo (nunca los de producción).

## 1. Compilar y probar

```powershell
cd backend
dotnet build                       # sin advertencias; una llamada a DateTime.UtcNow rompe el build (BannedApiAnalyzers)
dotnet test --collect:"XPlat Code Coverage"   # Shared, Arquitectura e Integracion en verde; Domain ≥ 90 %
```

**Esperado:** `LimiteDeLineasTests` en verde (ningún `.cs`, `.sql` de consultas, `.sh` o `.js` > 100 líneas); `ReglasDeCapasTests`, `ReglasDeDominioTests` y `ReglasDeModulosTests` en verde.

## 2. Levantar con un comando (US1) — orden del primer despliegue

```powershell
cd backend/deploy
docker compose up -d postgres                   # solo la BD
./migrar.sh --arranque             # una sola vez, como superusuario (KU_PG_SUPERUSUARIO): V0000
./migrar.sh                        # respaldo + V0001… con killauru_migrador (ON_ERROR_STOP, single-transaction)
docker compose up -d               # api, postgres, cloudflared, backup, alloy
docker compose exec api curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health/live    # 200
docker compose exec api curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health/ready   # 200
```

**Esperado:** existen los esquemas `identidad`, `ingesta`, `anomalias`, `auditoria`, `dashboard` y `migraciones`, y las filas `V0000` y `V0001` en `migraciones.aplicada`. `docker inspect` muestra `restart: unless-stopped` y logs `json-file` con `max-size 10m` en cada servicio.

## 3. Documentación (US2)

- Abrir `/docs` en desarrollo → página de Scalar con la etiqueta «Salud».
- Con `ASPNETCORE_ENVIRONMENT=Production`: `/docs` → **404**.

## 4. Seguridad común (US3)

| Prueba | Esperado |
|---|---|
| `Authorization: Bearer x` en un endpoint protegido | 401 `application/problem+json` con `codigo = token_invalido` y `correlacionId` |
| Endpoint protegido sin token / con token de otro rol | 401 / 403 `sin_permiso` |
| 61 consultas anónimas seguidas con el mismo `X-KillaUru-Seudonimo` | la 61.ª → 429 con `Retry-After` |
| 301 consultas anónimas desde una IP, cada una con otro seudónimo | la 301.ª → 429 (techo por IP, D2) |
| 6 registros desde una IP en una hora | el 6.º → 429 (D8) |
| Cualquier respuesta | `Cache-Control: private, no-store`, `Strict-Transport-Security`, `X-Content-Type-Options: nosniff` |
| Logs de un lote con token | sin seudónimo, IP, tokens, correos ni `sub` + `loteId` |

## 5. Salud y alertas (US4)

1. `docker compose stop postgres` → `/health/ready` responde 503.
2. Con UptimeRobot apuntando a `https://api.killauru.pe/health/ready`, la alerta por correo llega en ≤ 10 min.
3. En Grafana Cloud se ven disco, CPU y memoria de la VM, los logs de Docker y `killauru_tarea_ultima_ok{tarea="backup"}` (Alloy).
4. `docker kill -s KILL <api>` → Docker la levanta sola en ≤ 2 min.

## 6. Vuelta atrás y restauración (US5) — solo en «killauru-pruebas»

```powershell
docker compose -p killauru-pruebas -f docker-compose.pruebas.yml up -d
```

Seguir `deploy/runbooks/volver-atras.md` (`desplegar.sh <sha-anterior>`, < 5 min) y `deploy/runbooks/restaurar.md` (RTO ≤ 2 h). Antes de reabrir: 0 `sesion_refresh` sin revocar, `KU_JWT_CLAVE_FIRMA` rotada, 0 seudónimos de más de 72 h. Apagar el entorno al terminar.

## 7. CI/CD (US7)

1. Abrir un PR con una prueba rota → `ci.yml` en rojo y GitHub no deja unir a `main`.
2. Unir un PR verde → `cd.yml` publica `ghcr.io/<cuenta>/killauru-api:<sha>` y la VM responde `/health/ready` 200 con ese *tag*.

## 8. Carga, estrés, resistencia y caos (US8) — fuera de la VM, contra «killauru-pruebas»

```powershell
k6 run tests/carga/carga-combinada.k6.js   # lote p95 < 300 ms · mapa p95 < 250 ms · error < 1 % · 0 respuestas 429
k6 run tests/carga/picos.k6.js             # ráfaga de 600: 0 5xx
k6 run tests/carga/estres.k6.js            # anota el punto de quiebre
k6 run tests/carga/resistencia.k6.js       # 4 h al 50 %
./tests/carga/caos.sh                      # kill -9 bajo carga: 0 perdidos, 0 duplicados, reanuda ≤ 2 min
```

Anotar resultados en `deploy/runbooks/simulacros.md` (o lanzar `carga.yml` a mano en GitHub).
