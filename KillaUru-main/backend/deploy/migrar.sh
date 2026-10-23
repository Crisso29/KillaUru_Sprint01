#!/usr/bin/env bash
# Aplica las migraciones SQL versionadas (RNF-18, T-BD01).
#   migrar.sh --arranque   V0000 como superusuario, una sola vez (crea roles, esquemas y migraciones.aplicada)
#   migrar.sh              respaldo previo + cada V*.sql pendiente como killauru_migrador, en orden
# Cada script corre con ON_ERROR_STOP en UNA transacción junto con su registro: si falla, no queda nada aplicado.
set -euo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../db/migraciones" && pwd)"
: "${KU_DB_HOST:=postgres}" "${KU_DB_NOMBRE:=killauru}"

psql_como() {   # $1 usuario, $2 contraseña, resto: argumentos de psql
    local usuario="$1" clave="$2"; shift 2
    PGPASSWORD="$clave" psql -h "$KU_DB_HOST" -d "$KU_DB_NOMBRE" -U "$usuario" -v ON_ERROR_STOP=1 -X -q "$@"
}

arranque() {
    psql_como postgres "${KU_PG_SUPERUSUARIO:?falta KU_PG_SUPERUSUARIO}" --single-transaction \
        -v pw_migrador="${KU_DB_MIGRADOR_PASSWORD:?}" -v pw_app="${KU_DB_APP_PASSWORD:?}" \
        -f "$DIR/V0000__arranque.sql"
    echo "V0000 aplicada"
}

pendientes() {
    local aplicadas
    aplicadas="$(psql_como killauru_migrador "$KU_DB_MIGRADOR_PASSWORD" -At -c 'SELECT version FROM migraciones.aplicada')"
    for archivo in "$DIR"/V*.sql; do
        local version; version="$(basename "$archivo" | cut -d_ -f1)"
        [[ "$version" == "V0000" ]] && continue
        grep -qx "$version" <<<"$aplicadas" || echo "$archivo"
    done
}

aplicar() {   # $1 archivo
    local archivo="$1" nombre version descripcion fase
    nombre="$(basename "$archivo" .sql)"
    version="${nombre%%__*}"
    descripcion="${nombre#*__}"
    fase="$(grep -oE '^-- .* · fase ([12])' "$archivo" | grep -oE '[12]$' || echo 1)"
    psql_como killauru_migrador "$KU_DB_MIGRADOR_PASSWORD" --single-transaction -f "$archivo" \
        -c "INSERT INTO migraciones.aplicada VALUES ('$version', '$descripcion', clock_timestamp(), $fase)"
    echo "$version aplicada (fase $fase)"
}

if [[ "${1:-}" == "--arranque" ]]; then
    arranque
    exit 0
fi

: "${KU_DB_MIGRADOR_PASSWORD:?falta KU_DB_MIGRADOR_PASSWORD}"
mapfile -t lista < <(pendientes)
if [[ ${#lista[@]} -eq 0 ]]; then
    echo "Sin migraciones pendientes"
    exit 0
fi

"$(dirname "${BASH_SOURCE[0]}")/backup/respaldar.sh" --antes-de-migrar
for archivo in "${lista[@]}"; do
    aplicar "$archivo"
done
