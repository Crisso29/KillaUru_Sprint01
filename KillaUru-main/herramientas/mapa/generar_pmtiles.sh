#!/usr/bin/env bash
# T076 / FR-072, R-015, RNF-15: extrae PMTiles del corredor (maxzoom 14, max 80MB).
set -euo pipefail

REGION_GEOJSON="${1:-herramientas/mapa/region_corredor.geojson}"
FUENTE_MBTILES="${2:-herramientas/mapa/planeta.mbtiles}"
SALIDA="${3:-herramientas/mapa/corredor.pmtiles}"
MAX_MB=80

if ! command -v pmtiles &>/dev/null; then
  echo "ERROR: pmtiles no encontrado. Instala con: go install github.com/protomaps/go-pmtiles@latest" >&2
  exit 1
fi

if [ ! -f "$FUENTE_MBTILES" ]; then
  echo "ERROR: no existe $FUENTE_MBTILES" >&2
  exit 1
fi

pmtiles extract "$FUENTE_MBTILES" "$SALIDA" \
  --region="$REGION_GEOJSON" \
  --maxzoom=14

SIZE_MB=$(du -m "$SALIDA" | cut -f1)
if [ "$SIZE_MB" -gt "$MAX_MB" ]; then
  echo "ERROR: $SALIDA tiene ${SIZE_MB}MB > ${MAX_MB}MB" >&2
  rm -f "$SALIDA"
  exit 1
fi

echo "OK: $SALIDA (${SIZE_MB}MB)"
