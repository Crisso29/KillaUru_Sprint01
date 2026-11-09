"""FR-017 — descomposición en componentes vertical y horizontal, y detección de soporte inestable."""
import numpy as np

_ANGULO_MAX_GIRO_RAD = np.radians(30)  # > 30° entre ventanas → SoporteInestable (RS-08)


def calcular_componentes(
    acelerometro: np.ndarray,
    g: np.ndarray,
) -> tuple[np.ndarray, np.ndarray]:
    """
    Calcula a_v y a_h para las muestras de la ventana (data-model §4, pasos 2–3).

    Returns:
        a_v: componente vertical ajustada (a·ĝ − |g|), shape (n,)
        a_h: magnitud del componente horizontal |a − (a·ĝ)ĝ|, shape (n,)
    """
    g_norm = float(np.linalg.norm(g))
    if g_norm < 1e-6:
        g_hat = np.array([0.0, 0.0, 1.0], dtype=np.float32)
        g_norm = 1.0
    else:
        g_hat = (g / g_norm).astype(np.float32)

    a_v_bruto = acelerometro @ g_hat
    a_v = (a_v_bruto - g_norm).astype(np.float32)

    a_v_proj = np.outer(a_v_bruto, g_hat)
    a_h = np.linalg.norm(acelerometro - a_v_proj, axis=1).astype(np.float32)

    return a_v, a_h


def detectar_soporte_inestable(g_ant: np.ndarray, g_act: np.ndarray) -> bool:
    """True si el ángulo entre la gravedad de dos ventanas consecutivas supera 30°."""
    norma_ant = np.linalg.norm(g_ant)
    norma_act = np.linalg.norm(g_act)
    if norma_ant < 1e-6 or norma_act < 1e-6:
        return False
    cos_ang = np.clip(np.dot(g_ant, g_act) / (norma_ant * norma_act), -1.0, 1.0)
    return bool(float(np.arccos(cos_ang)) > _ANGULO_MAX_GIRO_RAD)
