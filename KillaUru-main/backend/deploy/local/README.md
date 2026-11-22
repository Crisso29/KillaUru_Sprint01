# Entorno local `killauru-local`

Entorno **desechable** en el PC del autor para ejecutar k6 antes de tener la VM de pruebas.
No sustituye a `killauru-pruebas` (VM, `docker-compose.pruebas.yml`): los resultados se repiten allí antes de cerrar
las tareas de carga. [Volver al backend](../../README.md) · [Pruebas de carga](../../tests/carga/README.md)

## Qué es igual y qué cambia respecto de la VM

| Aspecto | `killauru-local` | `killauru-pruebas` (VM) |
|---|---|---|
| Recursos | API 1,25 CPU / 3 GB; BD 0,75 CPU / 3 GB | Iguales |
| Configuración | `ASPNETCORE_ENVIRONMENT=Pruebas` (límites de tasa altos, D11) | Igual |
| Migraciones | El mismo `migrar.sh`, con los mismos roles y orden | Igual |
| Imagen de la API | Construida aquí desde `deploy/Dockerfile` (amd64) | Publicada en GHCR (arm64) |
| PostgreSQL | `postgis/postgis:17-3.5` (amd64) | `killauru-postgres:17-postgis` (arm64) |
| Respaldo antes de migrar | **Omitido** (`respaldo-omitido.sh`): no hay R2 | Real, a R2 (RNF-18) |
| Dónde corre k6 | **En el mismo PC**: compite por CPU con la API | Fuera de la VM |

Por la última fila, los resultados son **«local, misma máquina»**. Sirven como evidencia del comportamiento,
no como medición de producción.

## Uso

Desde `backend/deploy/`. La primera vez, crea `.env.local` con valores aleatorios con los nombres de
`.env.example` (secciones «Base de datos» y «API»). Ese archivo está ignorado por git (`deploy/.env.*`).

```powershell
docker compose --env-file .env.local -f docker-compose.local.yml up -d --build   # construir y levantar
curl.exe http://localhost:18080/health/ready                                    # 200 = corredor cargado
docker compose --env-file .env.local -f docker-compose.local.yml down -v        # apagar y borrar la base
```

`migrar-local.sh` aplica `V0000` solo si la base está vacía y luego ejecuta `migrar.sh`, que solo aplica
lo pendiente. Con `down -v` se borra el volumen y la siguiente vez se migra desde cero.
