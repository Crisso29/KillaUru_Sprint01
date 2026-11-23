#!/usr/bin/env python3
"""T076 / FR-072, R-015: región del mapa base, una franja de 5 km alrededor del corredor, para `pmtiles extract`.

Usa el buffer de shapely en metros (no desplazamientos punto a punto, que se cruzan en las curvas cerradas de la vía) y
la simplifica sin dejar fuera ningún punto de la vía. Rellena los huecos: lo que queda dentro de un lazo también se ve.

Uso: python region_corredor.py <corredor_pe28a.vN.json> --salida region_corredor.geojson
"""
import argparse
import json
import math
from pathlib import Path

from shapely.geometry import LineString, Polygon

MARGEN_M = 5000.0
TOLERANCIA_M = 250.0
RADIO_TIERRA_M = 6371008.8


def construir(corredor, margen_m=MARGEN_M):
    """GeoJSON Feature con el polígono de la región (lon/lat), proyectando a metros en una equirrectangular local."""
    puntos = [(lon, lat) for lon, lat, _ in corredor['vertices']]
    escala = math.cos(math.radians(sum(lat for _, lat in puntos) / len(puntos)))
    metros = [(math.radians(lon) * RADIO_TIERRA_M * escala, math.radians(lat) * RADIO_TIERRA_M) for lon, lat in puntos]
    franja = Polygon(LineString(metros).buffer(margen_m).exterior).simplify(TOLERANCIA_M, preserve_topology=True)
    anillo = [[math.degrees(x / (RADIO_TIERRA_M * escala)), math.degrees(y / RADIO_TIERRA_M)] for x, y in franja.exterior.coords]
    return {'type': 'Feature', 'properties': {'margen_m': margen_m}, 'geometry': {'type': 'Polygon', 'coordinates': [anillo]}}


def main():
    argumentos = argparse.ArgumentParser(description=__doc__)
    argumentos.add_argument('corredor')
    argumentos.add_argument('--salida', default='region_corredor.geojson')
    a = argumentos.parse_args()
    region = construir(json.loads(Path(a.corredor).read_text(encoding='utf8')))
    Path(a.salida).write_text(json.dumps(region), encoding='utf8')
    print(f"{a.salida}: {len(region['geometry']['coordinates'][0])} vértices, margen {MARGEN_M:.0f} m")


if __name__ == '__main__':
    main()
