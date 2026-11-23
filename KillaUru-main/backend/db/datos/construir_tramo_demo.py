"""006 T002 / RN-08, R-010: arma el polígono del tramo demo desde el eje canónico de la PE-28A.

Tramo entre `demo_km_inicio` y `demo_km_fin` del GeoJSON (km 268 → fin), margen de 200 m a cada lado y ≤ 120 vértices
(con 200 m la vía de montaña necesita ~111; ver R-010). Escribe la fuente única `tramo_demo.geojson`, que publica la CDN
para la app, y `Mapa:PoligonoDemo` en `appsettings.json` para el backend. Requiere shapely (solo para generar).

Uso, desde `backend`: python db/datos/construir_tramo_demo.py
"""
import hashlib
import json
import math
import re
from pathlib import Path

from shapely.geometry import LineString, Polygon
from shapely.geometry.polygon import orient

from construir_corredor import RADIO_TERRESTRE, distancia

MARGEN_M = 200.0
MAX_VERTICES = 120
LATITUD_REFERENCIA = -13.2  # centro del demo: la proyección local deforma < 0,1 % en 67 km
PASO_TOLERANCIA_M = 5.0
DECIMALES = 6  # ~0,1 m


def leer_eje(ruta):
    """Los puntos del eje y los límites del demo, con sus valores completos (sin redondear)."""
    datos = json.loads(Path(ruta).read_text(encoding='utf8'))
    propiedades = datos['properties']
    return {'puntos': datos['geometry']['coordinates'], 'km_inicio': propiedades['demo_km_inicio'],
            'km_fin': propiedades['demo_km_fin'], 'sha256': hashlib.sha256(Path(ruta).read_bytes()).hexdigest()}


def acumulados(puntos):
    """Km acumulado de cada vértice, con la misma Haversine que el backend y la app."""
    km = [0.0]
    for a, b in zip(puntos, puntos[1:]):
        km.append(km[-1] + distancia(a, b) / 1000)
    return km


def tramo(puntos, km, km_inicio, km_fin):
    """Vértices entre ambos km; el inicio se interpola sobre el segmento que lo contiene."""
    i = max(j for j, k in enumerate(km) if k <= km_inicio)
    t = (km_inicio - km[i]) / (km[i + 1] - km[i])
    inicio = [a + t * (b - a) for a, b in zip(puntos[i], puntos[i + 1])]
    return [inicio] + [p for p, k in zip(puntos[i + 1:], km[i + 1:]) if k <= km_fin + 1e-9]


def proyectar(coordenadas):
    """[lon, lat] → metros (equirrectangular local): basta para un margen de 200 m."""
    escala = math.cos(math.radians(LATITUD_REFERENCIA))
    return [(math.radians(lon) * RADIO_TERRESTRE * escala, math.radians(lat) * RADIO_TERRESTRE)
            for lon, lat, *_ in coordenadas]


def desproyectar(xy):
    escala = math.cos(math.radians(LATITUD_REFERENCIA))
    return [[round(math.degrees(x / (RADIO_TERRESTRE * escala)), DECIMALES),
             round(math.degrees(y / RADIO_TERRESTRE), DECIMALES)] for x, y in xy]


def poligono_demo(demo):
    """Margen fijo y la menor simplificación que deja ≤ 120 vértices con toda la vía dentro (tolerancia < margen).

    Los huecos (lazos cerrados de la vía) se rellenan: dentro no hay carretera y simplifican el recorte.
    """
    linea = LineString(proyectar(demo))
    franja = Polygon(linea.buffer(MARGEN_M).exterior)
    tolerancia = 0.0
    while tolerancia < MARGEN_M:
        anillo = desproyectar(orient(franja.simplify(tolerancia, preserve_topology=True)).exterior.coords)
        if len(anillo) - 1 <= MAX_VERTICES and Polygon(proyectar(anillo)).contains(linea):
            return anillo
        tolerancia += PASO_TOLERANCIA_M
    raise ValueError(f'No cabe en {MAX_VERTICES} vértices con {MARGEN_M} m de margen')


def escribir(carpeta, eje, anillo):
    """GeoJSON con procedencia (lo publica la CDN, 008) y la misma geometría en appsettings (backend)."""
    geojson = {'type': 'Feature', 'geometry': {'type': 'Polygon', 'coordinates': [anillo]}, 'properties': {
        'nombre': 'tramo_demo', 'km_inicio': eje['km_inicio'], 'km_fin': eje['km_fin'], 'margen_m': MARGEN_M,
        'vertices': len(anillo) - 1, 'fuente': 'corredor_pe28a.geojson', 'fuente_sha256': eje['sha256'],
        'atribucion': 'OpenStreetMap contributors', 'licencia': 'ODbL-1.0'}}
    (carpeta / 'tramo_demo.geojson').write_text(json.dumps(geojson, ensure_ascii=False, indent=1) + '\n', encoding='utf8')
    ajustes = carpeta.parent.parent / 'src/KillaUru.Api/appsettings.json'
    texto = ajustes.read_text(encoding='utf8')
    nuevo, cambios = re.subn(r'"PoligonoDemo": \[.*?\]\]', f'"PoligonoDemo": {json.dumps(anillo, separators=(", ", ", "))}',
                             texto, flags=re.S)
    assert cambios == 1, 'appsettings.json debe tener exactamente un Mapa:PoligonoDemo'
    ajustes.write_text(nuevo, encoding='utf8')


def main():
    carpeta = Path(__file__).parent
    eje = leer_eje(carpeta / 'corredor_pe28a.geojson')
    anillo = poligono_demo(tramo(eje['puntos'], acumulados(eje['puntos']), eje['km_inicio'], eje['km_fin']))
    escribir(carpeta, eje, anillo)
    print(f'tramo_demo.geojson: {len(anillo) - 1} vértices, margen {MARGEN_M:.0f} m')


if __name__ == '__main__':
    main()
