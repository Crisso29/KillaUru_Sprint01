"""T048 / FR-019, RNF-35 — métricas por clase: precisión y recall ≥ 0.90."""
import pytest

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.evaluacion.metricas import (
    calcular_metricas,
    severidad_mas_menos_1,
)

_CLASES = list(CLASES)  # orden del tensor: catálogo único


def test_prediccion_perfecta_precision_recall_1() -> None:
    # Arrange
    y_true = ["Normal", "Bache", "Hundimiento"] * 10
    y_pred = y_true.copy()
    # Act
    metricas = calcular_metricas(y_true, y_pred, _CLASES)
    # Assert
    for cls in _CLASES:
        assert metricas[f"precision_{cls}"] == pytest.approx(1.0)
        assert metricas[f"recall_{cls}"] == pytest.approx(1.0)


def test_predictor_trivial_recall_bache_cero() -> None:
    # Arrange — predictor siempre dice "Normal"
    y_true = ["Normal"] * 10 + ["Bache"] * 10 + ["Hundimiento"] * 10
    y_pred = ["Normal"] * 30
    # Act
    metricas = calcular_metricas(y_true, y_pred, _CLASES)
    # Assert
    assert metricas["recall_Bache"] == pytest.approx(0.0)
    assert metricas["recall_Hundimiento"] == pytest.approx(0.0)


def test_metricas_contiene_todas_las_claves() -> None:
    # Arrange
    y_true = ["Normal", "Bache", "Hundimiento"] * 5
    y_pred = y_true.copy()
    # Act
    metricas = calcular_metricas(y_true, y_pred, _CLASES)
    # Assert — 2 métricas × 3 clases
    for cls in _CLASES:
        assert f"precision_{cls}" in metricas
        assert f"recall_{cls}" in metricas


# --- puerta de calidad ---

# --- R-012: severidad ± 1 solo sobre las anomalías detectadas con el tipo correcto ---

@pytest.mark.parametrize(("aciertos", "esperado"), [(79, 0.79), (80, 0.80)])
def test_severidad_mas_menos_1_sobre_anomalias_bien_detectadas(aciertos: int, esperado: float) -> None:
    # Arrange — 100 baches bien detectados: `aciertos` a ± 1 y el resto a 3 niveles; además, Normales y un bache
    # confundido con hundimiento que NO deben contar (ya penalizan la precisión y el recall)
    reales = ["Bache"] * 100 + ["Normal"] * 50 + ["Bache"]
    predichos = ["Bache"] * 100 + ["Normal"] * 50 + ["Hundimiento"]
    severidad_real = [3] * 100 + [0] * 50 + [5]
    severidad_predicha = [4] * aciertos + [1] * (100 - aciertos) + [0] * 50 + [1]
    # Act
    valor = severidad_mas_menos_1(reales, predichos, severidad_real, severidad_predicha)
    # Assert
    assert valor == pytest.approx(esperado)
