"""FR-014, RNF-35 — tamaño y variedad mínimos de cada conjunto de la partición.

Con menos datos, una precisión de 0,90 no es creíble. Solo cuentan las ventanas reales: las aumentadas (copias
desplazadas ± 200 ms, solo en entrenamiento) no suman.
"""
from __future__ import annotations

from collections import Counter

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.ventanas.ventaneo import VentanaDataset

# FR-014: mínimo de ventanas reales por clase en cada conjunto (en entrenamiento, solo para las anomalías)
_MINIMOS = {"prueba": (CLASES, 200), "validacion": (CLASES, 100), "entrenamiento": (("Bache", "Hundimiento"), 400)}
_CELULARES_PRUEBA = 3
_SENTIDOS = {"AyacuchoVinchos", "VinchosAyacucho"}


class TamanoError(ValueError):
    pass


def verificar_requisitos(particion: dict[str, list[VentanaDataset]]) -> None:
    """FR-014 completo; lanza TamanoError con el primer incumplimiento y el pipeline se detiene."""
    for conjunto, (clases, minimo) in _MINIMOS.items():
        verificar_tamano(particion, list(clases), minimo, conjunto)
    prueba = [ventana for ventana in particion.get("prueba", []) if not ventana.aumentada]
    celulares = {ventana.celular for ventana in prueba}
    if len(celulares) < _CELULARES_PRUEBA:
        raise TamanoError(f"Prueba con {len(celulares)} celulares; FR-014 exige {_CELULARES_PRUEBA}")
    faltan = _SENTIDOS - {ventana.sentido for ventana in prueba}
    if faltan:
        raise TamanoError(f"Prueba sin ambos sentidos: falta {', '.join(sorted(faltan))}")


def verificar_tamano(particion: dict[str, list[VentanaDataset]], clases: list[str], minimo: int,
                     conjunto: str = "prueba") -> None:
    """Lanza TamanoError si alguna clase tiene menos de `minimo` ventanas reales en `conjunto`."""
    conteo = Counter(ventana.tipo for ventana in particion.get(conjunto, []) if not ventana.aumentada)
    faltantes = [f"{clase}: {conteo.get(clase, 0)} < {minimo}" for clase in clases if conteo.get(clase, 0) < minimo]
    if faltantes:
        raise TamanoError(f"Conjunto '{conjunto}' sin mínimo por clase: {'; '.join(faltantes)}")


def resumen_tamano(particion: dict[str, list[VentanaDataset]]) -> dict[str, dict[str, int]]:
    """Devuelve {conjunto → {tipo → n}} para diagnóstico."""
    return {nombre: dict(Counter(ventana.tipo for ventana in ventanas if not ventana.aumentada))
            for nombre, ventanas in particion.items()}
