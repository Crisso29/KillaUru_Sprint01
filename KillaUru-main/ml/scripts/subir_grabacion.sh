#!/usr/bin/env bash
# T013 / FR-005, FR-007 — sube una grabación al bucket R2 privado con rclone.
# Uso: subir_grabacion.sh <ruta-carpeta>
# Requiere: rclone configurado con el remote "killauru-r2:" y las credenciales de R2.
# El grabador no tiene permiso INTERNET (FR-005); la copia se hace por cable USB.
set -euo pipefail

CARPETA="${1:?Uso: $0 <ruta-carpeta>}"
NOMBRE=$(basename "$CARPETA")
REMOTE="killauru-r2:killauru-datos/${NOMBRE}"

if [ ! -d "$CARPETA" ]; then
  echo "ERROR: La carpeta no existe: $CARPETA" >&2
  exit 1
fi

echo "Subiendo $NOMBRE → $REMOTE ..."
# --immutable evita sobreescribir si ya existe (FR-031 analogía)
rclone copy "$CARPETA" "$REMOTE" \
  --exclude "video.mp4" \
  --checksum \
  --progress

echo "Subida completada: $REMOTE"
