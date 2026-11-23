"""FR-017, data-model §4 — preprocesamiento `p1`: 20 características `float32` por ventana de 500 ms a 50 Hz.

Es un contrato con la app (008): el modelo espera estos 20 valores crudos en este orden y la app debe calcularlos
igual (la normalización va dentro del modelo, R-011). Cambiar una definición o el orden obliga a subir
`preprocesamientoVersion` (FR-027). Las definiciones ambiguas se fijan aquí y la app debe copiarlas:
- desviación = poblacional (divide entre n);
- cruces = cuántas veces cambia de signo estricto `a_v − v_media` entre muestras consecutivas (un 0 exacto no cruza);
- velocidad = la del último fix GPS **anterior** al fin de la ventana, en km/h.
"""
from __future__ import annotations

import numpy as np

NOMBRES: tuple[str, ...] = (
    "v_media", "v_desv", "v_min", "v_max", "v_pap", "v_rms", "v_curtosis", "v_cruces", "v_jerk", "v_razon",
    "v_orden", "h_desv", "h_max", "h_pap", "h_rms", "w_desv", "w_max", "w_rms", "v_energia_rel", "velocidad_kmh",
)
INDICE_PICO_A_PICO = NOMBRES.index("v_pap")   # lo usa el prefiltro (FR-018)
_HZ = 50
_EPSILON = 1e-6


def extraer_features(a_v: np.ndarray, a_h: np.ndarray, w: np.ndarray, contexto_a_v: np.ndarray,
                     velocidad_kmh: float) -> np.ndarray:
    """Las 20 características de una ventana.

    Args:
        a_v, a_h, w: las 25 muestras de la ventana: aceleración vertical sin gravedad, horizontal (m/s²) y norma del
            giro (rad/s), salidas de `senal.componentes`.
        contexto_a_v: los últimos 2 s de `a_v` (100 muestras) que terminan con la ventana.
        velocidad_kmh: velocidad del último fix anterior al fin de la ventana (`velocidad_ultimo_fix`).
    """
    vertical = np.asarray(a_v, dtype=np.float64)
    valores = (_verticales(vertical) + _resumen(a_h) + _resumen(w, con_pico_a_pico=False)
               + [_rms(vertical) / (_rms(np.asarray(contexto_a_v, dtype=np.float64)) + _EPSILON), float(velocidad_kmh)])
    return np.array(valores, dtype=np.float32)


def velocidad_ultimo_fix(t_fix_ns: np.ndarray, velocidad_ms: np.ndarray, t_fin_ns: int) -> float:
    """km/h del último fix con hora < `t_fin_ns`; 0 si todavía no hubo ninguno."""
    anteriores = np.flatnonzero(np.asarray(t_fix_ns) < t_fin_ns)
    return float(velocidad_ms[anteriores[-1]]) * 3.6 if len(anteriores) else 0.0


def _verticales(senal: np.ndarray) -> list[float]:
    """0–10: media, desviación, mínimo, máximo, pico a pico, rms, curtosis, cruces, jerk, razón y orden."""
    media, desviacion = float(np.mean(senal)), float(np.std(senal))
    minimo, maximo = float(np.min(senal)), float(np.max(senal))
    curtosis = float(np.mean(((senal - media) / desviacion) ** 4) - 3.0) if desviacion >= _EPSILON else 0.0
    centrada = senal - media
    cruces = float(np.count_nonzero(centrada[:-1] * centrada[1:] < 0))
    jerk = float(np.max(np.abs(np.diff(senal)))) * _HZ
    orden = 1.0 if int(np.argmax(senal)) < int(np.argmin(senal)) else -1.0
    return [media, desviacion, minimo, maximo, maximo - minimo, _rms(senal), curtosis, cruces, jerk,
            abs(minimo) / (maximo + _EPSILON), orden]


def _resumen(valores: np.ndarray, con_pico_a_pico: bool = True) -> list[float]:
    """Desviación, máximo, [pico a pico] y rms de una componente (horizontal 11–14; giro 15–17, sin pico a pico)."""
    senal = np.asarray(valores, dtype=np.float64)
    pico_a_pico = [float(np.max(senal) - np.min(senal))] if con_pico_a_pico else []
    return [float(np.std(senal)), float(np.max(senal)), *pico_a_pico, _rms(senal)]


def _rms(senal: np.ndarray) -> float:
    return float(np.sqrt(np.mean(senal ** 2)))
