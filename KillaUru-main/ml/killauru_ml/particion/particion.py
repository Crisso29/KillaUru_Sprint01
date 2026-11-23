"""FR-013, R-008 — aplica la partición declarada a mano: cada recorrido entero a su conjunto.

No hay reparto al azar: el autor decide en `particion.yaml` (≈ 70/15/15) para asegurar prueba variada (ambos sentidos,
≥ 3 celulares). `partir()` se niega a seguir si la declaración repite un recorrido o deja uno del catálogo sin conjunto.
"""
from __future__ import annotations

import copy

from killauru_ml.particion.declaracion import CONJUNTOS, DeclaracionParticion
from killauru_ml.ventanas.ventaneo import VentanaDataset


class ParticionError(ValueError):
    pass


def partir(agrupado: dict[str, list[VentanaDataset]],
           declaracion: DeclaracionParticion) -> dict[str, list[VentanaDataset]]:
    """{recorrido → ventanas} → {conjunto → ventanas}, con `recorrido` asignado en cada ventana."""
    conjunto_de = _conjunto_por_recorrido(declaracion)
    sin_conjunto = sorted(set(agrupado) - set(conjunto_de))
    if sin_conjunto:
        raise ParticionError(f"Recorridos del catálogo sin conjunto en particion.yaml: {', '.join(sin_conjunto)}")
    particion: dict[str, list[VentanaDataset]] = {conjunto: [] for conjunto in CONJUNTOS}
    for recorrido, ventanas in agrupado.items():
        for ventana in ventanas:
            copia = copy.copy(ventana)
            copia.recorrido = recorrido
            no_visto = declaracion.celular_no_visto is not None and copia.celular == declaracion.celular_no_visto
            particion["prueba" if no_visto else conjunto_de[recorrido]].append(copia)
    return particion


def _conjunto_por_recorrido(declaracion: DeclaracionParticion) -> dict[str, str]:
    """Recorrido → conjunto; un recorrido declarado en dos conjuntos es fuga segura y se rechaza."""
    conjunto_de: dict[str, str] = {}
    for conjunto in CONJUNTOS:
        for recorrido in declaracion.recorridos(conjunto):
            if recorrido in conjunto_de:
                raise ParticionError(f"{recorrido} está en '{conjunto_de[recorrido]}' y en '{conjunto}'")
            conjunto_de[recorrido] = conjunto
    return conjunto_de
