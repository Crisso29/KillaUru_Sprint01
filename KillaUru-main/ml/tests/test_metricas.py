"""T048 / FR-019, RNF-35 — métricas por clase: precisión y recall ≥ 0.90."""
import pytest
from killauru_ml.evaluacion.metricas import (
    calcular_metricas,
    verificar_puerta_metricas,
    MetricaError,
)

_CLASES = ["Normal", "Bache", "Hundimiento"]


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

def test_metricas_ok_no_lanza() -> None:
    # Arrange — exactamente en el umbral mínimo (0.90)
    metricas = {f"precision_{c}": 0.90 for c in _CLASES}
    metricas.update({f"recall_{c}": 0.90 for c in _CLASES})
    metricas["severidad_mas_menos_uno"] = 0.80
    # Act + Assert
    verificar_puerta_metricas(metricas)


def test_precision_bajo_umbral_lanza_error() -> None:
    # Arrange — Bache precision = 0.89 < 0.90
    metricas = {f"precision_{c}": 0.90 for c in _CLASES}
    metricas.update({f"recall_{c}": 0.90 for c in _CLASES})
    metricas["precision_Bache"] = 0.89
    metricas["severidad_mas_menos_uno"] = 0.80
    # Act + Assert
    with pytest.raises(MetricaError, match="Bache"):
        verificar_puerta_metricas(metricas)


def test_severidad_bajo_umbral_lanza_error() -> None:
    # Arrange — severidad_mas_menos_uno = 0.79 < 0.80
    metricas = {f"precision_{c}": 0.90 for c in _CLASES}
    metricas.update({f"recall_{c}": 0.90 for c in _CLASES})
    metricas["severidad_mas_menos_uno"] = 0.79
    # Act + Assert
    with pytest.raises(MetricaError, match="severidad"):
        verificar_puerta_metricas(metricas)
