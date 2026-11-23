#!/usr/bin/env bash
# T076 / FR-072, R-015, RNF-15: extrae el mapa base del corredor como PMTiles (zoom ≤ 14, ≤ 80 MB).
#
# Lee solo las teselas de la región desde una compilación diaria de Protomaps por HTTP (no descarga el planeta).
# La fecha de la compilación se fija para que el resultado sea reproducible.
#
# Uso: generar_pmtiles.sh <region_corredor.geojson> <salida.pmtiles> [AAAAMMDD de la compilación]
set -euo pipefail

REGION="${1:?región GeoJSON (region_corredor.py)}"
SALIDA="${2:?archivo .pmtiles de salida}"
COMPILACION="${3:-20261120}"
FUENTE="https://build.protomaps.com/${COMPILACION}.pmtiles"
MAX_BYTES=83886080 # 80 MB (RNF-15; contrato ArchivoMapa)

command -v pmtiles >/dev/null || { echo "ERROR: falta la herramienta pmtiles (github.com/protomaps/go-pmtiles)" >&2; exit 1; }

pmtiles extract "$FUENTE" "$SALIDA" --region="$REGION" --maxzoom=14

BYTES=$(wc -c < "$SALIDA")
if [ "$BYTES" -gt "$MAX_BYTES" ]; then
  echo "ERROR: $SALIDA ocupa $BYTES bytes (> 80 MB)" >&2
  rm -f "$SALIDA"
  exit 1
fi
echo "OK: $SALIDA ($BYTES bytes, compilación $COMPILACION)"
