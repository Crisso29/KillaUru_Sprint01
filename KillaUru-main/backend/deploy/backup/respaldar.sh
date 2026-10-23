#!/usr/bin/env bash
# Respaldo cifrado fuera de la VM (RNF-07, FR-027, R-011, D12).
#   respaldar.sh                    respaldo diario
#   respaldar.sh --antes-de-migrar  el mismo respaldo, con otro nombre, antes de cada migración (RNF-18)
# Pasos: roles + datos → se comprueba que el volcado se puede leer → se cifra con age (clave pública)
# → se sube a R2 (bucket privado; la regla de ciclo de vida borra a los 30 días) → marca de éxito para Alloy.
set -euo pipefail

: "${KU_PG_SUPERUSUARIO:?}" "${KU_BACKUP_CLAVE_PUBLICA:?}" "${KU_R2_BUCKET_RESPALDOS:?}"
: "${KU_R2_ACCESS_KEY_ID:?}" "${KU_R2_SECRET_ACCESS_KEY:?}" "${KU_R2_ENDPOINT:?}"
export PGHOST="${KU_DB_HOST:-postgres}" PGUSER=postgres PGPASSWORD="$KU_PG_SUPERUSUARIO"

motivo="diario"
[[ "${1:-}" == "--antes-de-migrar" ]] && motivo="antes-de-migrar"
marca="$(date -u +%Y%m%dT%H%M%SZ)-$motivo"
trabajo="$(mktemp -d)"
trap 'rm -rf "$trabajo"' EXIT

# pg_dump no guarda los roles; sin ellos una restauración falla (T-BD01)
pg_dumpall --roles-only > "$trabajo/roles.sql"
pg_dump -d killauru -Fc -f "$trabajo/killauru.dump"
pg_restore --list "$trabajo/killauru.dump" > /dev/null   # un volcado ilegible no se sube

tar -C "$trabajo" -cf - roles.sql killauru.dump | age -r "$KU_BACKUP_CLAVE_PUBLICA" > "$trabajo/respaldo.tar.age"

export RCLONE_CONFIG_R2_TYPE=s3 RCLONE_CONFIG_R2_PROVIDER=Cloudflare \
       RCLONE_CONFIG_R2_ACCESS_KEY_ID="$KU_R2_ACCESS_KEY_ID" RCLONE_CONFIG_R2_SECRET_ACCESS_KEY="$KU_R2_SECRET_ACCESS_KEY" \
       RCLONE_CONFIG_R2_ENDPOINT="$KU_R2_ENDPOINT"
rclone copyto "$trabajo/respaldo.tar.age" "r2:$KU_R2_BUCKET_RESPALDOS/killauru-$marca.tar.age"

if [[ "$motivo" == "diario" && -d /metricas ]]; then
    printf 'killauru_tarea_ultima_ok{tarea="backup"} %s\n' "$(date +%s)" > /metricas/backup.prom.tmp
    mv /metricas/backup.prom.tmp /metricas/backup.prom   # atómico: Alloy nunca lee un archivo a medias
fi
echo "Respaldo killauru-$marca subido"
