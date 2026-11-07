"""FR-013 — verificación de tamaño mínimo por clase en cada conjunto."""
from __future__ import annotations

from collections import Counter

from killauru_ml.ventanas.ventaneo import VentanaDataset


class TamanoError(ValueError):
    pass


def verificar_tamano(
    particion: dict[str, list[VentanaDataset]],
    clases: list[str],
    minimo: int,
    conjunto: str = "prueba",
) -> None:
    """Lanza TamanoError si alguna clase tiene menos de `minimo` ventanas en `conjunto`.

    Solo verifica ventanas no aumentadas (zona gris incluida por defecto).
    """
    ventanas = particion.get(conjunto, [])
    conteo = Counter(
        v.tipo for v in ventanas if not v.aumentada
    )
    faltantes = [
        f"{cls}: {conteo.get(cls, 0)} < {minimo}"
        for cls in clases
        if conteo.get(cls, 0) < minimo
    ]
    if faltantes:
        raise TamanoError(
            f"Conjunto '{conjunto}' sin mínimo por clase: {'; '.join(faltantes)}"
        )


def resumen_tamano(
    particion: dict[str, list[VentanaDataset]],
) -> dict[str, dict[str, int]]:
    """Devuelve {conjunto → {tipo → n}} para diagnóstico."""
    resultado: dict[str, dict[str, int]] = {}
    for nombre, ventanas in particion.items():
        resultado[nombre] = dict(Counter(v.tipo for v in ventanas if not v.aumentada))
    return resultado
