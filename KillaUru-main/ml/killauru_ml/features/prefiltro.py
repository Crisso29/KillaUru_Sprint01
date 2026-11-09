"""FR-016 — prefiltro: descarta ventanas con varianza inferior al umbral."""
from __future__ import annotations

import numpy as np

from killauru_ml.ventanas.ventaneo import VentanaDataset

_UMBRAL_VAR_DEFAULT = 0.01  # config/entrenamiento.yaml PerdidaMaximaPrefiltro


def filtrar_prefiltro(
    ventanas: list[VentanaDataset],
    umbral_var: float = _UMBRAL_VAR_DEFAULT,
) -> list[VentanaDataset]:
    """Retiene solo ventanas cuya varianza de a_v supera el umbral.

    Señales casi planas indican vehículo detenido o sensor tapado.
    """
    return [v for v in ventanas if float(np.var(v.a_v)) > umbral_var]
