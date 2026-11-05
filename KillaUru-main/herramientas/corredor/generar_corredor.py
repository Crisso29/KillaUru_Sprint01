#!/usr/bin/env python3
"""T074 / FR-072, FR-074: genera corredor_pe28a.vN.json desde GeoJSON."""
import sys
import json
import math
import hashlib
import argparse
from pathlib import Path

RADIO_TIERRA_M = 6_371_008.8


def haversine(lat1, lon1, lat2, lon2):
    d_lat = math.radians(lat2 - lat1)
    d_lon = math.radians(lon2 - lon1)
    a = (math.sin(d_lat / 2) ** 2
         + math.cos(math.radians(lat1)) * math.cos(math.radians(lat2))
         * math.sin(d_lon / 2) ** 2)
    return RADIO_TIERRA_M * 2 * math.asin(math.sqrt(a))


def cargar_geojson(ruta: Path):
    with ruta.open(encoding="utf-8") as f:
        gj = json.load(f)
    coordenadas = []
    for feat in gj.get("features", []):
        geom = feat.get("geometry", {})
        tipo = geom.get("type", "")
        if tipo == "LineString":
            coordenadas.extend(geom["coordinates"])
        elif tipo == "MultiLineString":
            for linea in geom["coordinates"]:
                coordenadas.extend(linea)
    return coordenadas  # cada elemento: [lon, lat]


def calcular_km_acumulado(coordenadas):
    vertices = []
    km = 0.0
    for i, (lon, lat) in enumerate(coordenadas):
        if i > 0:
            prev_lon, prev_lat = coordenadas[i - 1]
            km += haversine(prev_lat, prev_lon, lat, lon) / 1000.0
        vertices.append({"lat": round(lat, 6), "lon": round(lon, 6), "km": round(km, 3)})
    return vertices


def sha256_de_archivo(ruta: Path) -> str:
    h = hashlib.sha256()
    with ruta.open("rb") as f:
        for bloque in iter(lambda: f.read(65536), b""):
            h.update(bloque)
    return h.hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("geojson", help="Ruta al archivo corredor_pe28a.geojson")
    parser.add_argument("version", help="Número de versión (entero)")
    parser.add_argument("--salida", default=".", help="Directorio de salida")
    args = parser.parse_args()

    geojson_path = Path(args.geojson)
    if not geojson_path.exists():
        print(f"ERROR: no existe {geojson_path}", file=sys.stderr)
        sys.exit(1)

    coordenadas = cargar_geojson(geojson_path)
    if len(coordenadas) < 2:
        print("ERROR: GeoJSON con menos de 2 vértices", file=sys.stderr)
        sys.exit(1)

    vertices = calcular_km_acumulado(coordenadas)
    fuente_sha = sha256_de_archivo(geojson_path)

    datos = {
        "version": int(args.version),
        "fuenteSha256": fuente_sha,
        "vertices": vertices,
    }

    salida = Path(args.salida) / f"corredor_pe28a.v{args.version}.json"
    with salida.open("w", encoding="utf-8") as f:
        json.dump(datos, f, ensure_ascii=False, separators=(",", ":"))

    print(f"Generado: {salida} ({len(vertices)} vértices, {vertices[-1]['km']:.3f} km)")


if __name__ == "__main__":
    main()
