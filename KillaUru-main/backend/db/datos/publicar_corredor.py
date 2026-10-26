"""Deriva SQL y referencias de kilometraje del GeoJSON revisado, sin descargar ni alterar la traza."""
import csv
import json
from pathlib import Path
from construir_corredor import distancia, RADIO_TERRESTRE


def publicar(carpeta):
    """Un único eje alimenta la migración y las pruebas compartidas con Android."""
    fuente = carpeta / 'corredor_pe28a.geojson'
    datos = json.loads(fuente.read_text(encoding='utf8'))
    puntos = datos['geometry']['coordinates']
    assert datos['geometry']['type'] == 'LineString' and len(puntos) >= 2
    assert datos['properties']['radio_terrestre_m'] == RADIO_TERRESTRE
    acumulado = [0.0]
    for a, b in zip(puntos, puntos[1:]):
        acumulado.append(acumulado[-1] + distancia(a, b) / 1000)
    assert abs(acumulado[-1] - datos['properties']['longitud_km']) < 1e-9
    geometria = json.dumps(datos['geometry'], separators=(',', ':'))
    sql = ('-- Generado por db/datos/publicar_corredor.py desde corredor_pe28a.geojson.\n'
           '-- OpenStreetMap contributors, ODbL-1.0; procedencia y extremos en db/datos/README.md.\n'
           'INSERT INTO anomalias.corredor_via (id, traza) VALUES\n'
           f"(1, ST_SetSRID(ST_GeomFromGeoJSON('{geometria}'), 4326)::geography);\n")
    (carpeta.parent / 'migraciones/V0005__corredor_via_datos.sql').write_text(sql, encoding='utf8')
    escribir_referencias(carpeta, puntos, acumulado)


def escribir_referencias(carpeta, puntos, acumulado):
    """Veinte controles: extremos, curvas distribuidas y el inicio demo interpolado en el eje."""
    indices = sorted({round(i * (len(puntos) - 1) / 18) for i in range(19)})
    filas = [(f'vertice_{i}', *puntos[i], acumulado[i]) for i in indices]
    for i in range(1, len(puntos)):
        if acumulado[i - 1] <= 268 <= acumulado[i]:
            t = (268 - acumulado[i - 1]) / (acumulado[i] - acumulado[i - 1])
            p = [a + t * (b - a) for a, b in zip(puntos[i - 1], puntos[i])]
            filas.append(('inicio_demo', *p, 268.0))
            break
    with (carpeta / 'km_referencia.csv').open('w', newline='', encoding='utf8') as archivo:
        escritor = csv.writer(archivo, lineterminator='\n')
        escritor.writerow(['referencia', 'longitud', 'latitud', 'km'])
        escritor.writerows(sorted(filas, key=lambda f: f[3]))


if __name__ == '__main__':
    publicar(Path(__file__).resolve().parent)
