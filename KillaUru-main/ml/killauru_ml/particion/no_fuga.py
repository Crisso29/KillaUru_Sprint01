"""FR-013 — verificación de no fuga entre conjuntos de la partición."""
from __future__ import annotations

from killauru_ml.ventanas.ventaneo import VentanaDataset


class FugaError(ValueError):
    pass


def verificar_no_fuga(particion: dict[str, list[VentanaDataset]]) -> None:
    """Lanza FugaError si algún recorrido aparece en más de un conjunto.

    Compara todos los pares de conjuntos; lanza en el primer cruce encontrado.
    """
    conjuntos = list(particion.items())
    for i in range(len(conjuntos)):
        nombre_i, ventanas_i = conjuntos[i]
        recs_i = {v.recorrido for v in ventanas_i}
        for j in range(i + 1, len(conjuntos)):
            nombre_j, ventanas_j = conjuntos[j]
            recs_j = {v.recorrido for v in ventanas_j}
            cruce = recs_i & recs_j
            if cruce:
                recorridos_str = ", ".join(sorted(cruce))
                raise FugaError(
                    f"Fuga detectada entre '{nombre_i}' y '{nombre_j}': "
                    f"recorridos {recorridos_str}"
                )
