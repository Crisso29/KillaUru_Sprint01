#!/usr/bin/env python3
"""T077 / FR-072, R-014: `mapa/manifiesto.json` (esquema `ManifiestoMapa` de cdn.openapi.yaml).

Lista los 4 archivos versionados de la carpeta con su URL, tamaño y SHA-256. Se publica **al final**, después de subir
los archivos: así la app nunca lee un manifiesto que apunte a algo que aún no está en la CDN.

Uso: python generar_manifiesto.py <carpeta> <version> --base https://cdn.killauru.pe/mapa
"""
import argparse
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

ATRIBUCION = '© Colaboradores de OpenStreetMap (ODbL)'
# tipo del contrato → nombre del archivo versionado (patrón de /mapa/{archivo})
ARCHIVOS = {
    'pmtiles': 'corredor-pe28a.v{v}.pmtiles',
    'corredor': 'corredor_pe28a.v{v}.json',
    'tramoDemo': 'tramo_demo.v{v}.geojson',
    'estilo': 'estilo.v{v}.json',
}


def _archivo(carpeta, tipo, plantilla, version, base):
    ruta = Path(carpeta) / plantilla.format(v=version)
    contenido = ruta.read_bytes()  # FileNotFoundError si falta: no hay manifiesto a medias
    return {'tipo': tipo, 'version': str(version), 'url': f'{base}/{ruta.name}', 'bytes': len(contenido),
            'sha256': hashlib.sha256(contenido).hexdigest()}


def construir(carpeta, version, base, ahora):
    """`ahora` se inyecta (reloj) para que la salida sea reproducible en las pruebas."""
    return {
        'esquema': 1,
        'version': str(version),
        'publicadoUtc': ahora.astimezone(timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ'),
        'atribucion': ATRIBUCION,
        'archivos': [_archivo(carpeta, tipo, plantilla, version, base.rstrip('/')) for tipo, plantilla in ARCHIVOS.items()],
    }


def main():
    argumentos = argparse.ArgumentParser(description=__doc__)
    argumentos.add_argument('carpeta')
    argumentos.add_argument('version')
    argumentos.add_argument('--base', default='https://cdn.killauru.pe/mapa')
    opciones = argumentos.parse_args()
    manifiesto = construir(opciones.carpeta, opciones.version, opciones.base, datetime.now(timezone.utc))
    salida = Path(opciones.carpeta) / 'manifiesto.json'
    salida.write_text(json.dumps(manifiesto, ensure_ascii=False, indent=2), encoding='utf8')
    print(f"{salida}: versión {manifiesto['version']}, {len(manifiesto['archivos'])} archivos")


if __name__ == '__main__':
    main()
