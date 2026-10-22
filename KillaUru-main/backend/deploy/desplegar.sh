#!/usr/bin/env bash
# Despliega una versión ya publicada en GHCR (T064, D10). Lo llama cd.yml por SSH, o el operador a mano.
#   desplegar.sh <sha>            despliega esa versión
#   desplegar.sh <sha-anterior>   vuelve atrás en < 5 min (RNF-18): la base es compatible por las migraciones en 2 fases
# Orden: respaldo → migraciones pendientes → levantar → esperar /health/ready. Si ready no llega a 200, falla.
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
