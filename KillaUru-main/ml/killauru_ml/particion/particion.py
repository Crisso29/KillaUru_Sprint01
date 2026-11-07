"""FR-013 — partición por recorrido para evitar fuga de datos."""
from __future__ import annotations

import copy
import random

from killauru_ml.ventanas.ventaneo import VentanaDataset

_FRAC_PRUEBA = 0.15
_FRAC_VAL = 0.15


def partir(
    agrupado: dict[str, list[VentanaDataset]],
    semilla: int = 2828,
    frac_prueba: float = _FRAC_PRUEBA,
    frac_val: float = _FRAC_VAL,
) -> dict[str, list[VentanaDataset]]:
    """Particiona ventanas por recorrido (no por ventana) para evitar fuga.

    Args:
        agrupado: {recorrido_id → lista de ventanas}
        semilla: semilla para reproducibilidad
        frac_prueba: fracción de recorridos destinada a prueba
        frac_val: fracción de recorridos destinada a validación

    Returns:
        {"entrenamiento": [...], "validacion": [...], "prueba": [...]}
        Cada ventana tiene v.recorrido asignado.
    """
    ids = sorted(agrupado.keys())
    rng = random.Random(semilla)
    rng.shuffle(ids)

    n = len(ids)
    n_prueba = max(1, round(n * frac_prueba))
    n_val = max(1, round(n * frac_val))
    # Garantizar que entrenamiento tenga al menos 1 recorrido
    n_prueba = min(n_prueba, n - 2)
    n_val = min(n_val, n - n_prueba - 1)

    ids_prueba = ids[:n_prueba]
    ids_val = ids[n_prueba: n_prueba + n_val]
    ids_ent = ids[n_prueba + n_val:]

    return {
        "entrenamiento": _aplanar(agrupado, ids_ent),
        "validacion": _aplanar(agrupado, ids_val),
        "prueba": _aplanar(agrupado, ids_prueba),
    }


def _aplanar(
    agrupado: dict[str, list[VentanaDataset]],
    ids: list[str],
) -> list[VentanaDataset]:
    """Aplana las ventanas de los recorridos dados, asignando v.recorrido."""
    ventanas = []
    for rec_id in ids:
        for v in agrupado[rec_id]:
            copia = copy.copy(v)
            copia.recorrido = rec_id
            ventanas.append(copia)
    return ventanas
