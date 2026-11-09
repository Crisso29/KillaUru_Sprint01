#!/usr/bin/env bash
# Parte de T077: mata la API con kill -9, espera pendientes viejos y consulta duplicados de reportes.
# Se corre EN la VM, contra «killauru-pruebas», mientras k6 (desde fuera) corre carga-combinada.k6.js.
# No registra los eventoId aceptados por HTTP ni los compara con la BD; todavía no prueba «0 perdidos».
# El mensaje final requiere esa conciliación adicional antes de acreditar RNF-06/EQ-09.
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
