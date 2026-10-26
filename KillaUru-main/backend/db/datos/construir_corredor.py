"""Reconstruye la PE-28A desde el JSON Overpass; no rellena huecos ni inventa segmentos."""
import argparse
import gzip
import heapq
import json
import math
from collections import defaultdict
from pathlib import Path

RADIO_TERRESTRE = 6371008.8  # IUGG; mismo valor en backend y contrato Android (008).

def distancia(a, b):
    """Metros sobre esfera media, coherentes con GeodesiaLocal; no son hitos oficiales."""
    lon1, lat1, lon2, lat2 = map(math.radians, (*a, *b))
    h = math.sin((lat2-lat1)/2)**2 + math.cos(lat1)*math.cos(lat2)*math.sin((lon2-lon1)/2)**2
    return 2 * RADIO_TERRESTRE * math.asin(min(1, math.sqrt(h)))


def grafo(datos):
    """Une exclusivamente nodos compartidos de los ways originales."""
    puntos, vecinos = {}, defaultdict(dict)
    for way in datos['elements']:
        for nodo, punto in zip(way['nodes'], way['geometry']):
            puntos[nodo] = [punto['lon'], punto['lat']]
        for a, b in zip(way['nodes'], way['nodes'][1:]):
            peso = distancia(puntos[a], puntos[b])
            vecinos[a][b] = vecinos[b][a] = peso
    return puntos, vecinos


def recorrido(vecinos, inicio, fin):
    """Dijkstra evita desvíos y vueltas en enlaces; falla si la vía es discontinua."""
    cola, costos, anterior = [(0, inicio)], {inicio: 0}, {}
    while cola:
        costo, nodo = heapq.heappop(cola)
        if nodo == fin:
            break
        if costo > costos[nodo]:
            continue
        for vecino, peso in vecinos[nodo].items():
            nuevo = costo + peso
            if nuevo < costos.get(vecino, math.inf):
                costos[vecino], anterior[vecino] = nuevo, nodo
                heapq.heappush(cola, (nuevo, vecino))
    if fin not in costos:
        raise ValueError('Extremos desconectados: no se permite inventar un enlace.')
    ruta = [fin]
    while ruta[-1] != inicio:
        ruta.append(anterior[ruta[-1]])
    return list(reversed(ruta)), costos[fin]


def generar(entrada, destino, inicio, fin):
    """Conserva todos los vértices: error de simplificación cero, menor al máximo de cinco metros."""
    contenido = gzip.decompress(entrada.read_bytes()) if entrada.suffix == '.gz' else entrada.read_bytes()
    datos = json.loads(contenido.decode('utf-8-sig'))
    puntos, vecinos = grafo(datos)
    ids, longitud = recorrido(vecinos, inicio, fin)
    salida = {'type': 'Feature', 'properties': {
        'ref': 'PE-28A', 'fuente': 'OpenStreetMap contributors', 'licencia': 'ODbL-1.0',
        'consulta': 'way["ref"="PE-28A"](-14,-77,-12,-73);out geom;',
        'snapshot': datos['osm3s']['timestamp_osm_base'], 'inicio_osm': inicio, 'fin_osm': fin,
        'longitud_km': longitud / 1000, 'radio_terrestre_m': RADIO_TERRESTRE,
        'simplificacion_m': 0, 'demo_km_inicio': 268.0, 'demo_km_fin': longitud / 1000},
        'geometry': {'type': 'LineString', 'coordinates': [puntos[n] for n in ids]}}
    destino.write_text(json.dumps(salida, ensure_ascii=False, separators=(',', ':')) + '\n', encoding='utf8')
    print(f'{len(ids)} vertices, {longitud/1000:.3f} km. Revisar la traza antes de publicar.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('entrada', type=Path)
    parser.add_argument('destino', type=Path)
    parser.add_argument('--inicio', type=int, required=True)
    parser.add_argument('--fin', type=int, required=True)
    args = parser.parse_args()
    generar(args.entrada, args.destino, args.inicio, args.fin)
