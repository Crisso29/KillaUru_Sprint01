#!/usr/bin/env bash
# Despliega una versión ya publicada en GHCR (T064, D10). Lo llama cd.yml por SSH, o el operador a mano.
#   desplegar.sh <sha>            despliega esa versión
#   desplegar.sh <sha-anterior>   selecciona una imagen anterior; la meta < 5 min se verifica en el simulacro
# Orden actual: pull → migrador (respalda solo si hay pendientes) → levantar → esperar /health/ready.
# Usa los SQL del checkout local: el llamador debe sincronizarlos con la versión; este script no lo hace.
# Si ready no llega a 200, falla. La compatibilidad del esquema depende de las migraciones en dos fases.
set -euo pipefail

SHA="${1:?Uso: desplegar.sh <sha>}"
cd "$(dirname "${BASH_SOURCE[0]}")"
export KU_VERSION="$SHA"

echo "→ Descargando killauru-api:$SHA"
docker compose pull api

echo "→ Migraciones (incluye el respaldo previo)"
docker compose run --rm migrador

echo "→ Levantando"
docker compose up -d --remove-orphans

echo "→ Esperando /health/ready"
for _ in $(seq 1 30); do
    if docker compose exec -T api curl -fsS http://localhost:8080/health/ready >/dev/null 2>&1; then
        echo "✓ killauru-api:$SHA desplegada y lista"
        exit 0
    fi
    sleep 5
done

echo "✗ /health/ready no respondió 200 en 150 s. Para volver atrás: ./desplegar.sh <sha-anterior>" >&2
exit 1
