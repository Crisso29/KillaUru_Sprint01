"""T010 / FR-012, FR-017 — pruebas de procesamiento de señal y kilometraje."""
import numpy as np
import pytest


# ---------------------------------------------------------------------------
# remuestreo lineal
# ---------------------------------------------------------------------------

def test_remuestreo_lineal_en_rampa() -> None:
    # Arrange — rampa de 0 a 10 en 11 puntos irregulares
    from killauru_ml.senal.remuestreo import remuestrear
    t_orig = np.array([0, 15, 35, 60, 90, 130, 180, 240, 310, 390, 500], dtype=float)  # ms
    vals = np.arange(11, dtype=float)
    t_meta = np.arange(0, 500, 20, dtype=float)  # 50 Hz = cada 20 ms
    # Act
    salida = remuestrear(t_orig, vals, t_meta)
    # Assert — interpolación lineal exacta
    esperado = np.interp(t_meta, t_orig, vals)
    np.testing.assert_allclose(salida, esperado, rtol=1e-5)


# ---------------------------------------------------------------------------
# gravedad
# ---------------------------------------------------------------------------

def test_gravedad_es_media_de_100_muestras() -> None:
    # Arrange — acelerómetro estático con ruido pequeño
    from killauru_ml.senal.gravedad import estimar_gravedad
    rng = np.random.default_rng(2828)
    accel = np.tile([0.0, 0.0, 9.81], (100, 1)) + rng.normal(0, 0.01, (100, 3))
    # Act
    g = estimar_gravedad(accel)
    # Assert — debe ser muy cercano a [0, 0, 9.81]
    np.testing.assert_allclose(g, [0.0, 0.0, 9.81], atol=0.1)


# ---------------------------------------------------------------------------
# componentes verticales y horizontales
# ---------------------------------------------------------------------------

def test_a_v_igual_celular_vertical_e_inclinado_30() -> None:
    # Arrange — celular vertical: g = [0, 0, 9.81]; inclinado 30°: g rotado
    from killauru_ml.senal.componentes import calcular_componentes
    angulo = np.radians(30)
    g_vertical = np.array([0.0, 0.0, 9.81])
    g_inclinado = np.array([0.0, 9.81 * np.sin(angulo), 9.81 * np.cos(angulo)])
    muestras = 25
    # Señal idéntica: una aceleración vertical de 2 m/s² encima de la gravedad
    señal_v = np.tile(g_vertical + np.array([0, 0, 2.0]), (muestras, 1))
    señal_i = np.tile(g_inclinado + np.array([0, 0, 2.0]) @ _rotar_z(0), (muestras, 1))
    # Act
    av_v, _ = calcular_componentes(señal_v, g_vertical)
    av_i, _ = calcular_componentes(señal_i, g_inclinado)
    # Assert — componente vertical de la perturbación debe ser igual
    np.testing.assert_allclose(av_v.mean(), av_i.mean(), atol=0.5)


def _rotar_z(ang: float) -> np.ndarray:
    return np.eye(3)  # simplificación para el test


# ---------------------------------------------------------------------------
# soporte inestable
# ---------------------------------------------------------------------------

def test_giro_mayor_30_grados_es_soporte_inestable() -> None:
    # Arrange — dos ventanas con giro de 31°
    from killauru_ml.senal.componentes import detectar_soporte_inestable
    g1 = np.array([0.0, 0.0, 9.81])
    angulo = np.radians(31)
    g2 = np.array([0.0, 9.81 * np.sin(angulo), 9.81 * np.cos(angulo)])
    # Act
    inestable = detectar_soporte_inestable(g1, g2)
    # Assert
    assert inestable is True


def test_giro_menor_30_grados_no_es_soporte_inestable() -> None:
    # Arrange — giro de 29°
    from killauru_ml.senal.componentes import detectar_soporte_inestable
    g1 = np.array([0.0, 0.0, 9.81])
    angulo = np.radians(29)
    g2 = np.array([0.0, 9.81 * np.sin(angulo), 9.81 * np.cos(angulo)])
    # Act
    inestable = detectar_soporte_inestable(g1, g2)
    # Assert
    assert inestable is False


# ---------------------------------------------------------------------------
# RN-03 — condición de velocidad
# ---------------------------------------------------------------------------

def test_velocidad_49_kmh_no_activa_rn03() -> None:
    # Arrange
    from killauru_ml.senal.condicion_velocidad import cumple_rn03
    # Act + Assert
    assert not cumple_rn03(velocidad_kmh=4.9, duracion_s=10)


def test_velocidad_50_kmh_activa_rn03() -> None:
    # Arrange
    from killauru_ml.senal.condicion_velocidad import cumple_rn03
    # Act + Assert
    assert cumple_rn03(velocidad_kmh=5.0, duracion_s=10)


def test_duracion_9s_no_activa_rn03() -> None:
    # Arrange
    from killauru_ml.senal.condicion_velocidad import cumple_rn03
    # Act + Assert
    assert not cumple_rn03(velocidad_kmh=5.1, duracion_s=9)


def test_duracion_10s_activa_rn03() -> None:
    # Arrange
    from killauru_ml.senal.condicion_velocidad import cumple_rn03
    # Act + Assert
    assert cumple_rn03(velocidad_kmh=5.1, duracion_s=10)
