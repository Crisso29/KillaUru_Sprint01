"""RN-03 — condición de velocidad: > 5 km/h durante ≥ 10 s continuos."""

_VELOCIDAD_MINIMA_KMH = 5.0
_DURACION_SOSTENIDA_S = 10


def cumple_rn03(velocidad_kmh: float, duracion_s: float) -> bool:
    """True si la velocidad supera el mínimo durante el tiempo sostenido requerido."""
    return velocidad_kmh > _VELOCIDAD_MINIMA_KMH and duracion_s >= _DURACION_SOSTENIDA_S
