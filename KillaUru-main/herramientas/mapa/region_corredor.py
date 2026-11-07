#!/usr/bin/env python3
"""T076 / FR-072, R-015: genera franja de 5km alrededor del corredor."""
import sys
import json
import math
import argparse
from pathlib import Path

RADIO_TIERRA_M = 6_371_008.8
FRANJA_M = 5000.0


def desplazar(lat, lon, distancia_m, rumbo_rad):
    lat_r = math.radians(lat)
    lon_r = math.radians(lon)
    d = distancia_m / RADIO_TIERRA_M
    lat2 = math.asin(math.sin(lat_r) * math.cos(d)
                     + math.cos(lat_r) * math.sin(d) * math.cos(rumbo_rad))
    lon2 = lon_r + math.atan2(
        math.sin(rumbo_rad) * math.sin(d) * math.cos(lat_r),
        math.cos(d) - math.sin(lat_r) * math.sin(lat2))
    return math.degrees(lat2), math.degrees(lon2)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("corredor_json", help="corredor_pe28a.vN.json")
    parser.add_argument("--salida", default="herramientas/mapa/region_corredor.geojson")
    args = parser.parse_args()

    with Path(args.corredor_json).open(encoding="utf-8") as f:
        datos = json.load(f)
    vertices = datos["vertices"]

    izquierda = []
    derecha = []
    for v in vertices:
        lat_i, lon_i = desplazar(v["lat"], v["lon"], FRANJA_M, math.pi / 2)
        lat_d, lon_d = desplazar(v["lat"], v["lon"], FRANJA_M, -math.pi / 2)
        izquierda.append([lon_i, lat_i])
        derecha.append([lon_d, lat_d])

    anillo = izquierda + list(reversed(derecha))
    anillo.append(anillo[0])

    gj = {
        "type": "FeatureCollection",
        "features": [{
            "type": "Feature",
            "geometry": {"type": "Polygon", "coordinates": [anillo]},
            "properties": {"franja_m": FRANJA_M},
        }]
    }

    salida = Path(args.salida)
    salida.parent.mkdir(parents=True, exist_ok=True)
    with salida.open("w", encoding="utf-8") as f:
        json.dump(gj, f, ensure_ascii=False)
    print(f"Region generada: {salida}")


if __name__ == "__main__":
    main()
