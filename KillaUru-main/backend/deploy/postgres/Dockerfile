# PostgreSQL 17 + PostGIS 3 para arm64 (T028, R-008, RS-02).
# Comprobado el 2026-10-22: la imagen postgis/postgis:17-3.5 solo se publica para amd64, y la VM es ARM.
# La imagen oficial de postgres sí es multiarquitectura y ya trae el repositorio de PGDG, que tiene PostGIS para arm64.
FROM postgres:17-bookworm
RUN apt-get update \
    && apt-get install -y --no-install-recommends postgresql-17-postgis-3 \
    && rm -rf /var/lib/apt/lists/*
