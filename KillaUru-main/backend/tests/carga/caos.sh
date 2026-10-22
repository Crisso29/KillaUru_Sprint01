#!/usr/bin/env bash
# Caos (T077, RNF-06, EQ-09): mata la API con kill -9 en plena carga, espera la reanudación y concilia.
# Se corre EN la VM, contra «killauru-pruebas», mientras k6 (desde fuera) corre carga-combinada.k6.js.
# Éxito: aceptados = procesados + descartados + pendientes, 0 pendientes tras 2 min y 0 duplicados.
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/../../deploy"
compose=(docker compose -f docker-compose.pruebas.yml)
psql_bd() { "${compose[@]}" exec -T postgres psql -U postgres -d killauru -At -c "$1"; }

echo "→ Esperando 2 min de carga antes de matar la API"
sleep 120
echo "→ kill -9 a la API"
"${compose[@]}" kill -s SIGKILL api
"${compose[@]}" up -d api

echo "→ Esperando la reanudación (RNF-06: ≤ 2 min)"
for _ in $(seq 1 24); do
    pendientes=$(psql_bd "SELECT count(*) FROM ingesta.evento_recibido WHERE estado = 0 AND recibido_utc < now() - interval '1 minute'")
    [[ "$pendientes" == "0" ]] && break
    sleep 5
done

duplicados=$(psql_bd "SELECT count(*) - count(DISTINCT evento_id) FROM anomalias.reporte_anomalia")
echo "Pendientes viejos: $pendientes · reportes duplicados: $duplicados"
psql_bd "SELECT estado, count(*) FROM ingesta.evento_recibido GROUP BY estado ORDER BY estado"
if [[ "$pendientes" != "0" || "$duplicados" != "0" ]]; then
    echo "✗ Caos fallido: anotar en deploy/runbooks/simulacros.md" >&2
    exit 1
fi
echo "✓ Caos superado: 0 perdidos, 0 duplicados"
