"""FR-008 — detección de candidatos a etiquetado a partir de a_v."""
from __future__ import annotations

import numpy as np


def detectar_candidatos(
    t_ns: np.ndarray,
    a_v: np.ndarray,
    velocidad_ms: np.ndarray,
    km: np.ndarray,
    umbral_ms2: float = 2.0,
) -> list[dict]:
    """Devuelve candidatos donde |a_v| > umbral_ms2.

    Cada candidato incluye: t_pico_ns, km, velocidad_ms, segundo_video.
    segundo_video = tiempo desde el inicio de la grabación en segundos.
    """
    a_v = np.asarray(a_v, dtype=np.float64)
    magnitud = np.abs(a_v)
    indices = np.where(magnitud > umbral_ms2)[0]

    t0_ns = int(t_ns[0]) if len(t_ns) > 0 else 0
    candidatos = []
    for idx in indices:
        t_pico = int(t_ns[idx])
        candidatos.append(
            {
                "t_pico_ns": t_pico,
                "km": float(km[idx]),
                "velocidad_ms": float(velocidad_ms[idx]),
                "segundo_video": (t_pico - t0_ns) / 1_000_000_000.0,
            }
        )
    return candidatos
