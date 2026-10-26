# Corredor PE-28A: fuente geométrica única

Revisión: 2026-10-24. La longitud y los límites se ajustaron a la geometría por indicación del usuario. Los 333 km anteriores eran una aproximación del prototipo; no se escala ni se recorta la traza para obtenerlos.

| Dato | Valor |
|---|---|
| Fuente canónica | `corredor_pe28a.geojson` (GeoJSON WGS84, orden longitud/latitud) |
| Longitud acumulada | 335.0729634613575 km; presentación 335,073 km |
| Radio esférico común con Android | 6 371 008,8 m |
| Extremo inicial | nodo OSM 254346289, lon −76.1597883 / lat −13.6822487, enlace PE-1S |
| Extremo final | nodo OSM 3372734708, lon −74.2112089 / lat −13.1475212, Ayacucho |
| Vértices | 6689, sin simplificación; error de simplificación 0 m |
| Demo | km 268 → longitud completa: 67.0729634613575 km |
| Tramos de cobertura | `ceil(longitud_km)` = 336, incluido el último tramo parcial |
| SHA-256 del GeoJSON | `6f039964aa9ec64785940cd446588fdef144f908cd940183a20db62b103a5828` |

Los kilómetros son distancias acumuladas del eje seleccionado, no hitos oficiales ni distancias entre centros urbanos. El nombre previo «Ayacucho–Vinchos» y las posiciones de pueblos del prototipo no verifican el km 268: su coordenada interpolada está en `km_referencia.csv`. Se conserva ese inicio como límite funcional del demo y se corrige su fin al extremo geométrico, sin atribuirle un centro poblado. RN-08 documenta esta decisión.

## Procedencia y revisión

Datos © [OpenStreetMap contributors](https://www.openstreetmap.org/copyright), bajo [ODbL 1.0](https://opendatacommons.org/licenses/odbl/1-0/). Conservar esta atribución al reutilizar el eje en Android o distribuirlo. La consulta se obtuvo de `https://overpass.kumi.systems/api/interpreter`; la respuesta indica **snapshot 2026-07-28T02:16:18Z**. Esa es la fecha de los datos, no la de revisión.

Consulta: `[out:json][timeout:45];way["ref"="PE-28A"](-14,-77,-12,-73);out geom;`. `pe28a_overpass.json.gz` conserva la respuesta original, comprimida reproduciblemente, para regenerar sin depender de cambios de OSM.

Se revisaron visualmente el eje completo, sus extremos, continuidad y curvas; se comprobó que todos los segmentos proceden de nodos consecutivos de la fuente. La extracción usa el camino más corto sobre nodos compartidos, conserva una calzada en enlaces y no incorpora puentes artificiales entre vías desconectadas. El grafo es bidireccional porque representa un eje de validación, no un motor de navegación. La revisión no certifica la precisión topográfica de OSM ni sustituye una inspección de campo. Como referencia territorial adicional existe el [mapa vial de Vinchos del MTC](https://www.portal.mtc.gob.pe/transportes/caminos/normas_carreteras/Mapas%20Distritales/Ayacucho/AY_050114%20VINCHOS.pdf); no se dedujo de él ninguna progresiva.

## Reproducción y pruebas

Desde `backend`, con Python 3 (biblioteca estándar):

```powershell
python db/datos/construir_corredor.py db/datos/pe28a_overpass.json.gz db/datos/corredor_pe28a.geojson --inicio 254346289 --fin 3372734708
python db/datos/publicar_corredor.py
```

El segundo comando deriva `V0005__corredor_via_datos.sql` y los 20 puntos de `km_referencia.csv`. `CorredorRealTests` exige concordancia inferior a 1 m entre esos puntos y el backend; 008 debe reutilizar el mismo CSV. `CorredorMigracionTests` verifica los 6689 vértices y la carga desde PostGIS. El generador usa Haversine esférico como el backend; `ST_Length(geography)` usa por defecto un elipsoide y no sustituye este kilometraje compartido.

El polígono demo de margen 200 m y ≤ 60 vértices queda en T002 de **006**. Al generarlo, leer `demo_km_inicio` y `demo_km_fin` completos del GeoJSON, sin usar el valor redondeado 335,073 como corte. Validar su cobertura y forma antes de compartirlo con la app; esta entrega incluye el eje, no ese polígono.
