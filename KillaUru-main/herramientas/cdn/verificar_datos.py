#!/usr/bin/env python3
"""T075 / FR-072, FR-074, BD-19: verifica el corredor y el tramo demo antes de publicarlos.

- El corredor cumple el esquema `Corredor` del contrato y su km crece.
- Todos los vértices del km 268 al final están dentro del tramo demo (≤ 120 vértices, R-010).
- Los km coinciden (< 1 m) con `backend/db/datos/km_referencia.csv`, los mismos puntos que verifica el backend.

Uso: python verificar_datos.py <corredor_pe28a.vN.json> <tramo_demo.geojson> <km_referencia.csv>  (sale con 1 si falla)
"""
import argparse
import csv
import json
import sys
from pathlib import Path

from shapely.geometry import Point, shape

from contrato import errores

KM_INICIO_DEMO = 268.0
MAX_VERTICES_DEMO = 120
TOLERANCIA_KM = 0.001  # 1 m


def _demo(corredor, demo):
    poligono = shape(demo['geometry'])
    fallos = [] if len(poligono.exterior.coords) - 1 <= MAX_VERTICES_DEMO else ['el tramo demo pasa de 120 vértices']
    fuera = [km for lon, lat, km in corredor['vertices'] if km >= KM_INICIO_DEMO and not poligono.contains(Point(lon, lat))]
    return fallos + ([f'{len(fuera)} vértices fuera del tramo demo (primero en el km {fuera[0]:.3f})'] if fuera else [])


def _referencias(corredor, ruta_csv):
    with Path(ruta_csv).open(encoding='utf8') as f:
        filas = [fila for fila in csv.DictReader(f) if fila['referencia'].startswith('vertice_')]
    vertices = corredor['vertices']
    return [f"{fila['referencia']}: km {vertices[int(fila['referencia'][8:])][2]:.6f} en vez de {float(fila['km']):.6f}"
            for fila in filas if abs(vertices[int(fila['referencia'][8:])][2] - float(fila['km'])) > TOLERANCIA_KM]


def verificar(corredor, demo, ruta_km_referencia):
    """Lista de fallos; vacía si se puede publicar."""
    fallos = errores('Corredor', corredor)
    kms = [km for _, _, km in corredor['vertices']]
    if any(b < a for a, b in zip(kms, kms[1:])):
        fallos.append('el km no crece a lo largo del corredor')
    return fallos + _demo(corredor, demo) + _referencias(corredor, ruta_km_referencia)


def main():
    argumentos = argparse.ArgumentParser(description=__doc__)
    for nombre in ('corredor', 'demo', 'km_referencia'):
        argumentos.add_argument(nombre)
    opciones = argumentos.parse_args()
    leer = lambda ruta: json.loads(Path(ruta).read_text(encoding='utf8'))
    fallos = verificar(leer(opciones.corredor), leer(opciones.demo), opciones.km_referencia)
    for fallo in fallos:
        print(f'FALLA: {fallo}', file=sys.stderr)
    print('OK: corredor y tramo demo listos para publicar' if not fallos else f'{len(fallos)} fallos')
    sys.exit(1 if fallos else 0)


if __name__ == '__main__':
    main()
