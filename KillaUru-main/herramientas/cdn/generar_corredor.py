#!/usr/bin/env python3
"""T074 / FR-072, FR-074, R-016: `corredor_pe28a.vN.json` (esquema `Corredor` de cdn.openapi.yaml).

Lee el eje canónico `backend/db/datos/corredor_pe28a.geojson` y acumula los km con la misma Haversine esférica que
el backend (`CorredorVia`), así la app y el servidor dan el mismo km para el mismo punto. Vértices `[lon, lat, km]`.

Uso: python generar_corredor.py <corredor_pe28a.geojson> <version> --salida <carpeta>
"""
import argparse
import hashlib
import json
import math
from pathlib import Path

RADIO_TIERRA_M = 6371008.8
DECIMALES_KM = 6  # 1 mm


def distancia_m(a, b):
    """Haversine esférica entre [lon, lat] y [lon, lat], en metros (igual que `construir_corredor.distancia`)."""
    lon1, lat1, lon2, lat2 = map(math.radians, (a[0], a[1], b[0], b[1]))
    h = math.sin((lat2 - lat1) / 2) ** 2 + math.cos(lat1) * math.cos(lat2) * math.sin((lon2 - lon1) / 2) ** 2
    return 2 * RADIO_TIERRA_M * math.asin(math.sqrt(h))


def construir(ruta_geojson, version):
    """El corredor del contrato: versión, huella de la fuente, radio y los vértices con su km acumulado."""
    fuente = Path(ruta_geojson).read_bytes()
    puntos = json.loads(fuente)['geometry']['coordinates']
    km, vertices = 0.0, []
    for i, punto in enumerate(puntos):
        if i:
            km += distancia_m(puntos[i - 1], punto) / 1000
        vertices.append([punto[0], punto[1], round(km, DECIMALES_KM)])
    return {'version': str(version), 'fuenteSha256': hashlib.sha256(fuente).hexdigest(),
            'radioTierraM': RADIO_TIERRA_M, 'vertices': vertices}


def escribir(corredor, carpeta):
    salida = Path(carpeta) / f"corredor_pe28a.v{corredor['version']}.json"
    salida.write_text(json.dumps(corredor, separators=(',', ':')), encoding='utf8')
    return salida


def main():
    argumentos = argparse.ArgumentParser(description=__doc__)
    argumentos.add_argument('geojson')
    argumentos.add_argument('version')
    argumentos.add_argument('--salida', default='.')
    a = argumentos.parse_args()
    corredor = construir(a.geojson, a.version)
    ruta = escribir(corredor, a.salida)
    print(f"{ruta.name}: {len(corredor['vertices'])} vértices, {corredor['vertices'][-1][2]:.3f} km")


if __name__ == '__main__':
    main()
