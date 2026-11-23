"""008 US8 (T074–T077) / FR-072, FR-074, RNF-15: los archivos que publica la CDN cumplen `cdn.openapi.yaml`.

Se generan desde el eje canónico real (`backend/db/datos`) en una carpeta temporal. Patrón AAA.
Ejecutar: python -m unittest discover -s herramientas/cdn   (requisitos.txt)
"""
import csv
import hashlib
import json
import re
import tempfile
import unittest
from datetime import datetime, timezone
from pathlib import Path

from shapely.geometry import Point, shape

import generar_corredor
import generar_estilo
import generar_manifiesto
import region_corredor
import verificar_datos
from contrato import errores

DATOS = Path(__file__).resolve().parents[2] / 'backend/db/datos'
BASE = 'https://cdn.killauru.pe/mapa'
NOMBRE_ARCHIVO = re.compile(r'^[a-z0-9_.-]+\.v[0-9]+\.(pmtiles|json|geojson)$')  # patrón de /mapa/{archivo}


class DatosCdnTest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.tmp = tempfile.TemporaryDirectory()
        cls.carpeta = Path(cls.tmp.name)
        cls.corredor = generar_corredor.construir(DATOS / 'corredor_pe28a.geojson', '3')
        generar_corredor.escribir(cls.corredor, cls.carpeta)
        (cls.carpeta / 'tramo_demo.v3.geojson').write_bytes((DATOS / 'tramo_demo.geojson').read_bytes())
        (cls.carpeta / 'estilo.v3.json').write_text(json.dumps(generar_estilo.construir('3', BASE)), encoding='utf8')
        (cls.carpeta / 'corredor-pe28a.v3.pmtiles').write_bytes(b'PMTiles de prueba')
        cls.ahora = datetime(2026, 11, 22, 15, 0, tzinfo=timezone.utc)

    @classmethod
    def tearDownClass(cls):
        cls.tmp.cleanup()

    def test_el_corredor_cumple_el_esquema_y_conserva_el_kilometraje_del_backend(self):
        # Arrange
        with (DATOS / 'km_referencia.csv').open(encoding='utf8') as f:
            referencias = [fila for fila in csv.DictReader(f) if fila['referencia'].startswith('vertice_')]
        # Act
        vertices = self.corredor['vertices']
        # Assert — mismos vértices y km que CorredorVia (< 1 m, R-016)
        self.assertEqual([], errores('Corredor', self.corredor))
        self.assertEqual(6689, len(vertices))
        for fila in referencias:
            lon, lat, km = vertices[int(fila['referencia'].split('_')[1])]
            self.assertAlmostEqual(float(fila['km']), km, delta=0.001)
            self.assertAlmostEqual(float(fila['longitud']), lon, places=7)

    def test_el_estilo_lee_el_pmtiles_de_la_misma_version_y_atribuye_a_openstreetmap(self):
        # Arrange + Act
        estilo = generar_estilo.construir('3', BASE)
        # Assert
        fuente = estilo['sources']['protomaps']
        self.assertEqual(8, estilo['version'])
        self.assertEqual(f'pmtiles://{BASE}/corredor-pe28a.v3.pmtiles', fuente['url'])
        self.assertIn('OpenStreetMap', fuente['attribution'])
        ids = [capa['id'] for capa in estilo['layers']]
        self.assertEqual(len(ids), len(set(ids)))
        capas_v4 = {'earth', 'water', 'landuse', 'landcover', 'roads', 'boundaries', 'places', 'natural', 'buildings', 'pois'}
        self.assertTrue({c['source-layer'] for c in estilo['layers'] if 'source-layer' in c} <= capas_v4)

    def test_el_manifiesto_cumple_el_esquema_y_cada_archivo_su_tamano_y_huella(self):
        # Arrange + Act
        manifiesto = generar_manifiesto.construir(self.carpeta, '3', BASE, self.ahora)
        # Assert
        self.assertEqual([], errores('ManifiestoMapa', manifiesto))
        self.assertEqual('2026-11-22T15:00:00Z', manifiesto['publicadoUtc'])
        self.assertEqual({'pmtiles', 'corredor', 'tramoDemo', 'estilo'}, {a['tipo'] for a in manifiesto['archivos']})
        for archivo in manifiesto['archivos']:
            nombre = archivo['url'].removeprefix(BASE + '/')
            contenido = (self.carpeta / nombre).read_bytes()
            self.assertRegex(nombre, NOMBRE_ARCHIVO)
            self.assertEqual(len(contenido), archivo['bytes'])
            self.assertEqual(hashlib.sha256(contenido).hexdigest(), archivo['sha256'])

    def test_el_manifiesto_se_niega_si_falta_un_archivo(self):
        # Arrange
        with tempfile.TemporaryDirectory() as vacia:
            # Act + Assert — nunca se publica un manifiesto que apunte a algo que no se subió
            with self.assertRaises(FileNotFoundError):
                generar_manifiesto.construir(Path(vacia), '3', BASE, self.ahora)

    def test_la_verificacion_acepta_los_datos_reales_y_detecta_un_demo_que_no_cubre_la_via(self):
        # Arrange
        demo = json.loads((DATOS / 'tramo_demo.geojson').read_text(encoding='utf8'))
        recortado = json.loads(json.dumps(demo))
        recortado['geometry']['coordinates'][0] = recortado['geometry']['coordinates'][0][:10] + [
            recortado['geometry']['coordinates'][0][0]]
        # Act
        reales = verificar_datos.verificar(self.corredor, demo, DATOS / 'km_referencia.csv')
        malos = verificar_datos.verificar(self.corredor, recortado, DATOS / 'km_referencia.csv')
        # Assert
        self.assertEqual([], reales)
        self.assertTrue(any('fuera del tramo demo' in e for e in malos))

    def test_la_region_del_pmtiles_cubre_todo_el_corredor(self):
        # Arrange + Act
        region = shape(region_corredor.construir(self.corredor)['geometry'])
        # Assert — el mapa base alcanza la vía entera con margen para mirar alrededor
        self.assertTrue(all(region.contains(Point(lon, lat)) for lon, lat, _ in self.corredor['vertices']))
        self.assertLessEqual(len(region.exterior.coords), 500)


if __name__ == '__main__':
    unittest.main()
