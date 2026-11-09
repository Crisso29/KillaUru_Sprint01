#!/usr/bin/env bash
# Corre respaldar.sh todos los días a las 02:00 de Lima (calendario de T-OPS05). Sin cron: un bucle que
# duerme hasta la próxima hora es más simple de leer y de probar en un contenedor.
# Actualmente espera antes del primer respaldo; falta la ejecución al arrancar exigida por FR-042.
set -euo pipefail
HORA="${KU_BACKUP_HORA:-02:00}"

while true; do
    ahora=$(date +%s)
    proxima=$(date -d "today $HORA" +%s)
    if (( proxima <= ahora )); then
        proxima=$(date -d "tomorrow $HORA" +%s)
    fi
    sleep $(( proxima - ahora ))
    /killauru/deploy/backup/respaldar.sh || echo "Respaldo fallido: la alerta de 26 h de Grafana lo avisará" >&2
done
