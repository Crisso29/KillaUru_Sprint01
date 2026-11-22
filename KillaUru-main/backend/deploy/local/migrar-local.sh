#!/usr/bin/env bash
# Entorno local desechable (killauru-local): V0000 solo la primera vez y luego el migrar.sh de la VM.
# El respaldo previo a R2 lo sustituye local/respaldo-omitido.sh (montado en docker-compose.local.yml).
set -euo pipefail

aplicada="$(PGPASSWORD="${KU_PG_SUPERUSUARIO:?}" psql -h "${KU_DB_HOST:-postgres}" -U postgres -d killauru -At \
    -c "SELECT to_regclass('migraciones.aplicada') IS NOT NULL")"

if [[ "$aplicada" != "t" ]]; then
    bash /killauru/deploy/migrar.sh --arranque
fi

exec bash /killauru/deploy/migrar.sh
