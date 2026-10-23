# Runbook · Volver atrás (< 5 min, RNF-18)

1. Ver la versión anterior: `docker image ls ghcr.io/<cuenta>/killauru-api` o el historial de `cd.yml` en GitHub.
2. `cd /opt/killauru/KillaUru-main/backend/deploy && ./desplegar.sh <sha-anterior>`.
3. Confirmar `/health/ready` = 200 y la página de estado de UptimeRobot en verde.

No se revierten migraciones: las de fase 1 solo agregan, así que la versión anterior funciona con el esquema nuevo. Si una migración de fase 1 rompió algo, se corrige con **otra** migración hacia adelante.

Tiempo medido en el último simulacro: anotar en `simulacros.md`.
