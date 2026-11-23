"""006 T002 / RN-08, R-010: polígono del tramo demo (km 268 → fin del eje, margen 200 m, ≤ 120 vértices).

Patrón AAA. Se ejecuta con `python -m unittest discover -s db/datos -p "test_*.py"` desde `backend` (requiere shapely).
"""
import json
import unittest
from pathlib import Path

from shapely.geometry import LineString, Point, Polygon

from construir_tramo_demo import MARGEN_M, MAX_VERTICES, acumulados, leer_eje, poligono_demo, proyectar, tramo

CARPETA = Path(__file__).parent


class TramoDemoTest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.eje = leer_eje(CARPETA / 'corredor_pe28a.geojson')
        cls.km = acumulados(cls.eje['puntos'])
        cls.demo = tramo(cls.eje['puntos'], cls.km, cls.eje['km_inicio'], cls.eje['km_fin'])
        cls.anillo = poligono_demo(cls.demo)

    def test_el_tramo_empieza_exactamente_en_el_km_268_y_termina_en_el_fin_del_eje(self):
        # Arrange
        puntos, km = self.eje['puntos'], self.km
        # Act
        inicio, fin = self.demo[0], self.demo[-1]
        # Assert — el inicio se interpola en el eje (no se toma el vértice más cercano)
        antes = max(i for i, k in enumerate(km) if k <= 268)
        self.assertTrue(km[antes] <= 268 <= km[antes + 1])
        self.assertNotEqual(inicio, puntos[antes])
        self.assertEqual(fin, puntos[-1])

    def test_el_poligono_cierra_gira_antihorario_y_no_pasa_de_120_vertices(self):
        # Arrange + Act
        anillo = self.anillo
        # Assert — RFC 7946: anillo exterior cerrado y en sentido antihorario
        self.assertEqual(anillo[0], anillo[-1])
        self.assertLessEqual(len(anillo) - 1, MAX_VERTICES)
        self.assertTrue(Polygon(anillo).exterior.is_ccw)
        self.assertTrue(Polygon(anillo).is_valid)

    def test_toda_la_via_demo_queda_dentro_y_el_borde_no_se_aleja_mas_del_doble_del_margen(self):
        # Arrange
        poligono, linea = Polygon(proyectar(self.anillo)), LineString(proyectar(self.demo))
        # Act
        lejania = max(linea.distance(Point(p)) for p in poligono.exterior.coords)
        # Assert
        self.assertTrue(poligono.contains(linea))
        self.assertLessEqual(lejania, 2 * MARGEN_M)

    def test_lo_que_esta_antes_del_km_268_queda_fuera(self):
        # Arrange — un vértice del eje 10 km antes del inicio del demo
        i = max(i for i, k in enumerate(self.km) if k <= 258)
        # Act
        punto = Point(proyectar([self.eje['puntos'][i]])[0])
        # Assert
        self.assertFalse(Polygon(proyectar(self.anillo)).contains(punto))

    def test_los_archivos_publicados_son_los_que_genera_el_script(self):
        # Arrange
        geojson = json.loads((CARPETA / 'tramo_demo.geojson').read_text(encoding='utf8'))
        ajustes = json.loads((CARPETA.parent.parent / 'src/KillaUru.Api/appsettings.json').read_text(encoding='utf8'))
        # Act
        publicado = geojson['geometry']['coordinates'][0]
        # Assert — misma fuente para el backend (appsettings) y para la app (CDN)
        self.assertEqual(self.anillo, publicado)
        self.assertEqual(self.anillo, ajustes['Mapa']['PoligonoDemo'])
        self.assertEqual('ODbL-1.0', geojson['properties']['licencia'])


if __name__ == '__main__':
    unittest.main()
