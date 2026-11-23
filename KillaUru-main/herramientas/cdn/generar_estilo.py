#!/usr/bin/env python3
"""T076 / FR-040, ADR-011: `estilo.vN.json`, estilo MapLibre del mapa base para la app y el panel.

Lee el PMTiles de la misma versión (esquema 4 de Protomaps: earth, water, landcover, landuse, roads, places) con los
colores del prototipo: las vías del mapa base en blanco con borde gris (`--line-2`) para que solo resalte la PE-28A, que
la app y el panel dibujan encima con `--route` y borde `--luna`; el agua en `--info`. El panel lo usa tal cual por HTTP; la app cambia la URL
del PMTiles y la de las letras por sus archivos locales para funcionar sin señal.

Uso: python generar_estilo.py <version> --base https://cdn.killauru.pe/mapa --salida estilo.vN.json
"""
import argparse
import json
from pathlib import Path

GLIFOS = 'https://protomaps.github.io/basemaps-assets/fonts/{fontstack}/{range}.pbf'
FUENTE = ['Noto Sans Regular']
ATRIBUCION = '<a href="https://openstreetmap.org/copyright">© Colaboradores de OpenStreetMap</a> · Protomaps'
COLORES = {
    'tierra': '#EEF1E7', 'bosque': '#DCE7D2', 'pasto': '#E6ECDA', 'roca': '#ECE7DC', 'ciudad': '#E4E5E1',
    'agua': '#A9CDE3', 'rio': '#86B6D4', 'via_menor': '#FFFFFF', 'borde_menor': '#DCE4E6',
    'via_mayor': '#FFFFFF', 'borde_mayor': '#C3CFD2', 'texto': '#3D4F57', 'halo': '#FFFFFF',
}


def _relleno(id_, capa, color, filtro=None):
    estilo = {'id': id_, 'type': 'fill', 'source': 'protomaps', 'source-layer': capa, 'paint': {'fill-color': color}}
    return estilo | ({'filter': filtro} if filtro else {})


def _linea(id_, capa, filtro, color, ancho):
    return {'id': id_, 'type': 'line', 'source': 'protomaps', 'source-layer': capa, 'filter': filtro,
            'layout': {'line-cap': 'round', 'line-join': 'round'},
            'paint': {'line-color': color, 'line-width': ['interpolate', ['exponential', 1.6], ['zoom'], 8, ancho[0], 16, ancho[1]]}}


def _suelo():
    c = COLORES
    return [
        {'id': 'fondo', 'type': 'background', 'paint': {'background-color': c['tierra']}},
        _relleno('tierra', 'earth', c['tierra']),
        _relleno('bosque', 'landcover', c['bosque'], ['==', ['get', 'kind'], 'forest']),
        _relleno('pasto', 'landcover', c['pasto'], ['in', ['get', 'kind'], ['literal', ['grassland', 'scrub', 'farmland']]]),
        _relleno('roca', 'landcover', c['roca'], ['==', ['get', 'kind'], 'barren']),
        _relleno('ciudad', 'landuse', c['ciudad'], ['in', ['get', 'kind'], ['literal', ['residential', 'urban_area']]]),
        _relleno('agua', 'water', c['agua'], ['==', ['geometry-type'], 'Polygon']),
        _linea('rios', 'water', ['==', ['geometry-type'], 'LineString'], c['rio'], (0.6, 3)),
    ]


def _vias():
    c, mayores = COLORES, ['in', ['get', 'kind'], ['literal', ['highway', 'major_road', 'medium_road']]]
    menores = ['in', ['get', 'kind'], ['literal', ['minor_road', 'other']]]
    return [
        _linea('vias-menores-borde', 'roads', menores, c['borde_menor'], (1, 9)),
        _linea('vias-menores', 'roads', menores, c['via_menor'], (0.5, 7)),
        _linea('vias-mayores-borde', 'roads', mayores, c['borde_mayor'], (2, 14)),
        _linea('vias-mayores', 'roads', mayores, c['via_mayor'], (1, 11)),
    ]


def _nombres():
    return [{'id': 'pueblos', 'type': 'symbol', 'source': 'protomaps', 'source-layer': 'places',
             'filter': ['in', ['get', 'kind'], ['literal', ['locality', 'neighbourhood']]],
             'layout': {'text-field': ['get', 'name'], 'text-font': FUENTE, 'text-size': 13},
             'paint': {'text-color': COLORES['texto'], 'text-halo-color': COLORES['halo'], 'text-halo-width': 1.5}}]


def construir(version, base):
    url = f"pmtiles://{base.rstrip('/')}/corredor-pe28a.v{version}.pmtiles"
    return {'version': 8, 'name': f'KillaUru v{version}', 'glyphs': GLIFOS,
            'sources': {'protomaps': {'type': 'vector', 'url': url, 'attribution': ATRIBUCION}},
            'layers': _suelo() + _vias() + _nombres()}


def main():
    argumentos = argparse.ArgumentParser(description=__doc__)
    argumentos.add_argument('version')
    argumentos.add_argument('--base', default='https://cdn.killauru.pe/mapa')
    argumentos.add_argument('--salida')
    a = argumentos.parse_args()
    salida = Path(a.salida or f'estilo.v{a.version}.json')
    salida.write_text(json.dumps(construir(a.version, a.base), ensure_ascii=False), encoding='utf8')
    print(f'{salida}: estilo de la versión {a.version}')


if __name__ == '__main__':
    main()
