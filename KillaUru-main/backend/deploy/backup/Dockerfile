# Respaldo diario y herramienta de migraciones (T051, FR-027). Misma versión de cliente que el servidor (17).
FROM postgres:17-bookworm
RUN apt-get update \
    && apt-get install -y --no-install-recommends age rclone ca-certificates \
    && rm -rf /var/lib/apt/lists/*
ENV TZ=America/Lima
ENTRYPOINT ["/killauru/deploy/backup/programar.sh"]
