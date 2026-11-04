#!/usr/bin/env python3
"""T075 / FR-072, FR-074, BD-19: verifica integridad del corredor generado."""
import sys
import json
import math
import csv
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


def point_in_polygon(lat, lon, poligono):
    dentro = False
    j = len(poligono) - 1
    for i, (vlat, vlon) in enumerate(poligono):
        jlat, jlon = poligono[j]
        if ((vlat > lat) != (jlat > lat) and
                lon < (jlon - vlon) * (lat - vlat) / (jlat - vlat) + vlon):
            dentro = not dentro
        j = i
    return dentro


def verificar_km_creciente(vertices):
    for i in range(1, len(vertices)):
        if vertices[i]["km"] <= vertices[i - 1]["km"]:
            return False, f"km no creciente en índice {i}: {vertices[i]['km']}"
    return True, ""


def cargar_demo_polygon(ruta):
    with ruta.open(encoding="utf-8") as f:
        gj = json.load(f)
    for feat in gj.get("features", []):
        geom = feat.get("geometry", {})
        if geom.get("type") == "Polygon":
            coords = geom["coordinates"][0]
            return [(c[1], c[0]) for c in coords]
    return []


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("corredor_json", help="corredor_pe28a.vN.json")
    parser.add_argument("demo_geojson", help="tramo_demo.geojson")
    parser.add_argument("km_referencia", help="km_referencia.csv")
    args = parser.parse_args()

    with Path(args.corredor_json).open(encoding="utf-8") as f:
        datos = json.load(f)
    vertices = datos["vertices"]

    ok, msg = verificar_km_creciente(vertices)
    if not ok:
        print(f"FALLA km creciente: {msg}", file=sys.stderr)
        sys.exit(1)

    poligono = cargar_demo_polygon(Path(args.demo_geojson))
    if poligono:
        km_inicio, km_fin = 268.0, 335.073
        for v in vertices:
            if km_inicio <= v["km"] <= km_fin:
                if not point_in_polygon(v["lat"], v["lon"], poligono):
                    print(f"FALLA: vértice km={v['km']} fuera del polígono demo", file=sys.stderr)
                    sys.exit(1)

    errores = []
    with Path(args.km_referencia).open(newline="", encoding="utf-8") as f:
        for fila in csv.DictReader(f):
            lat_ref, lon_ref, km_esp = float(fila["lat"]), float(fila["lon"]), float(fila["km_esperado"])
            mejor_dist = min(
                haversine(lat_ref, lon_ref, v["lat"], v["lon"])
                for v in vertices
                if abs(v["km"] - km_esp) < 2.0
            ) if any(abs(v["km"] - km_esp) < 2.0 for v in vertices) else 9999.0
            if mejor_dist > 1.0:
                errores.append(f"km_ref={km_esp}: distancia {mejor_dist:.2f}m > 1m")

    if errores:
        for e in errores:
            print(f"FALLA: {e}", file=sys.stderr)
        sys.exit(1)

    print(f"OK: {len(vertices)} vértices, km_max={vertices[-1]['km']:.3f}")


if __name__ == "__main__":
    main()
