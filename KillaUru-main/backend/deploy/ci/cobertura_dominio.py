"""Puerta de cobertura del CI (constitución VII, FR-037): las clases de dominio deben estar cubiertas ≥ N %.

Lee todos los informes Cobertura que dejó `dotnet test -- --coverage` y suma las líneas de las clases cuyo
espacio de nombres tiene `.Domain` (kernel compartido y dominio de cada módulo). Sale con código 1 si no llega.
"""
import glob
import sys
import xml.etree.ElementTree as ET

minimo = float(sys.argv[1]) if len(sys.argv) > 1 else 90.0
cubiertas, totales = {}, {}

for informe in glob.glob('**/TestResults/**/*.cobertura.xml', recursive=True) + glob.glob('**/cobertura.xml', recursive=True):
    for clase in ET.parse(informe).getroot().iter('class'):
        nombre = clase.get('name', '')
        if '.Domain' not in nombre:
            continue
        for linea in clase.iter('line'):
            clave = (nombre, linea.get('number'))
            totales[clave] = True
            if int(linea.get('hits', '0')) > 0:
                cubiertas[clave] = True

if not totales:
    print('Todavía no hay clases de dominio que medir.')
    sys.exit(0)

porcentaje = 100.0 * len(cubiertas) / len(totales)
print(f'Cobertura del dominio: {porcentaje:.1f} % ({len(cubiertas)}/{len(totales)} líneas). Mínimo: {minimo:.0f} %')
sys.exit(0 if porcentaje >= minimo else 1)
