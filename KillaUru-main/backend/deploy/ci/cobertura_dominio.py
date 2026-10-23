"""Puerta de CI por dominio. Usar --informes con una carpeta limpia para evitar informes antiguos."""
import argparse
import re
import sys
from pathlib import Path
import xml.etree.ElementTree as ET


def dominios_existentes(raiz):
    """No exige cobertura a módulos que todavía no tienen fuentes Domain."""
    nombres = set()
    for archivo in (raiz / 'src').rglob('Domain/*.cs'):
        nombres.update(re.findall(r'namespace\s+([\w.]+\.Domain)\b', archivo.read_text(encoding='utf-8-sig')))
    return nombres


def leer_informes(carpeta):
    """Une líneas repetidas entre suites dentro de cada dominio."""
    lineas = {}
    for informe in carpeta.rglob('*.cobertura.xml'):
        for clase in ET.parse(informe).getroot().iter('class'):
            nombre = clase.get('name', '')
            if '.Domain.' not in nombre:
                continue
            grupo = lineas.setdefault(nombre.split('.Domain.')[0] + '.Domain', {})
            for linea in clase.iter('line'):
                clave = (clase.get('filename', nombre), linea.get('number'))
                grupo[clave] = grupo.get(clave, False) or int(linea.get('hits', '0')) > 0
    return lineas


def verificar(raiz, informes, minimo):
    """Falla si falta un dominio existente o su porcentaje es insuficiente."""
    esperados = dominios_existentes(raiz)
    medidos = leer_informes(informes)
    correcto = bool(esperados)
    for dominio in sorted(esperados):
        lineas = medidos.get(dominio, {})
        cubiertas = sum(lineas.values())
        porcentaje = 100 * cubiertas / len(lineas) if lineas else 0
        print(f'{dominio}: {porcentaje:.1f}% ({cubiertas}/{len(lineas)} líneas)')
        correcto = correcto and bool(lineas) and porcentaje >= minimo
    return 0 if correcto else 1


if __name__ == '__main__':
    argumentos = argparse.ArgumentParser(description=__doc__)
    argumentos.add_argument('minimo', nargs='?', type=float, default=90)
    argumentos.add_argument('--informes', type=Path, default=Path('.'))
    opciones = argumentos.parse_args()
    sys.exit(verificar(Path.cwd(), opciones.informes, opciones.minimo))
