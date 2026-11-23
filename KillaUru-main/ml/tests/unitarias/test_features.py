"""T038 / FR-017, data-model §4 — las 20 características `p1` que el modelo recibe y que la app calcula igual.

Cada prueba usa una señal cuyo resultado se puede calcular a mano. Patrón AAA.
"""
import numpy as np
import pytest

from killauru_ml.features.features import NOMBRES, extraer_features, velocidad_ultimo_fix

_CERO = np.zeros(25, dtype=np.float32)


def _features(a_v=_CERO, a_h=_CERO, w=_CERO, contexto=None, velocidad_kmh=0.0) -> dict[str, float]:
    contexto = np.concatenate([np.zeros(75, dtype=np.float32), a_v]) if contexto is None else contexto
    return dict(zip(NOMBRES, extraer_features(a_v, a_h, w, contexto, velocidad_kmh).tolist(), strict=True))


def test_son_20_valores_float32_en_el_orden_del_data_model() -> None:
    # Arrange
    orden = ["v_media", "v_desv", "v_min", "v_max", "v_pap", "v_rms", "v_curtosis", "v_cruces", "v_jerk", "v_razon",
             "v_orden", "h_desv", "h_max", "h_pap", "h_rms", "w_desv", "w_max", "w_rms", "v_energia_rel",
             "velocidad_kmh"]
    # Act
    vector = extraer_features(_CERO, _CERO, _CERO, np.zeros(100, dtype=np.float32), 0.0)
    # Assert
    assert (list(NOMBRES), vector.shape, vector.dtype) == (orden, (20,), np.float32)


def test_estadisticos_verticales_de_una_senal_conocida() -> None:
    # Arrange — 0, 1, 2, …, 24
    a_v = np.arange(25, dtype=np.float32)
    # Act
    valores = _features(a_v)
    # Assert
    assert (valores["v_media"], valores["v_min"], valores["v_max"], valores["v_pap"]) == (12.0, 0.0, 24.0, 24.0)
    assert valores["v_desv"] == pytest.approx(np.sqrt(52.0))          # desviación poblacional de 0..24
    assert valores["v_rms"] == pytest.approx(np.sqrt(np.mean(a_v.astype(np.float64) ** 2)))
    assert valores["v_jerk"] == pytest.approx(50.0)                   # salto máximo 1 m/s² en 20 ms


def test_curtosis_es_0_con_senal_constante() -> None:
    # Arrange — desviación 0: la fórmula dividiría por cero
    a_v = np.full(25, 3.0, dtype=np.float32)
    # Act
    valores = _features(a_v)
    # Assert
    assert valores["v_curtosis"] == 0.0


def test_cruces_por_cero_alrededor_de_la_media() -> None:
    # Arrange — +1, −1, +1, … : 24 cambios de signo respecto de la media 0,04
    a_v = np.array([1.0 if indice % 2 == 0 else -1.0 for indice in range(25)], dtype=np.float32)
    # Act
    valores = _features(a_v)
    # Assert
    assert valores["v_cruces"] == 24.0


@pytest.mark.parametrize(("pico", "valle", "orden"), [(3, 10, 1.0), (10, 3, -1.0)])
def test_orden_es_mas_1_si_el_maximo_va_antes_que_el_minimo(pico: int, valle: int, orden: float) -> None:
    # Arrange — un bache sube y luego cae (+1); un hundimiento cae y luego sube (−1)
    a_v = np.zeros(25, dtype=np.float32)
    a_v[pico], a_v[valle] = 8.0, -4.0
    # Act
    valores = _features(a_v)
    # Assert
    assert valores["v_orden"] == orden
    assert valores["v_razon"] == pytest.approx(4.0 / (8.0 + 1e-6))


def test_componentes_horizontal_y_de_giro() -> None:
    # Arrange
    a_h = np.full(25, 2.0, dtype=np.float32)
    giro = np.zeros(25, dtype=np.float32)   # norma del giroscopio |ω|: un solo golpe de 5 rad/s
    giro[0] = 5.0
    # Act
    valores = _features(a_h=a_h, w=giro)
    # Assert
    assert (valores["h_desv"], valores["h_max"], valores["h_pap"], valores["h_rms"]) == (0.0, 2.0, 0.0, 2.0)
    assert (valores["w_max"], valores["w_rms"]) == (5.0, pytest.approx(5.0 / 5.0))


def test_energia_relativa_compara_la_ventana_con_sus_2_s_de_contexto() -> None:
    # Arrange — ventana con rms 2 dentro de un contexto de 100 muestras con rms 1
    a_v = np.full(25, 2.0, dtype=np.float32)
    contexto = np.concatenate([np.zeros(75, dtype=np.float32), a_v])
    # Act
    valores = _features(a_v, contexto=contexto)
    # Assert — rms del contexto = sqrt(25·4 / 100) = 1
    assert valores["v_energia_rel"] == pytest.approx(2.0 / (1.0 + 1e-6))


def test_velocidad_es_la_del_ultimo_fix_anterior_al_fin_de_la_ventana() -> None:
    # Arrange — fixes cada segundo; la ventana termina en t = 2,5 s
    t_fix = np.array([0, 1, 2, 3], dtype=np.int64) * 1_000_000_000
    velocidad_ms = np.array([5.0, 6.0, 7.0, 8.0])
    # Act
    kmh = velocidad_ultimo_fix(t_fix, velocidad_ms, t_fin_ns=2_500_000_000)
    sin_fix = velocidad_ultimo_fix(t_fix, velocidad_ms, t_fin_ns=0)
    # Assert — el fix de 2 s (7 m/s = 25,2 km/h); el de 3 s todavía no llegó; sin fix previo, 0
    assert (kmh, sin_fix) == (pytest.approx(25.2), 0.0)
