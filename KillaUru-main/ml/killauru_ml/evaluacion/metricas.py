"""FR-021, RNF-35, R-012 — métricas por clase y severidad ± 1. Los mínimos los aplica la puerta (`puerta.py`)."""
from __future__ import annotations

from sklearn.metrics import precision_score, recall_score


def calcular_metricas(
    y_true: list[str],
    y_pred: list[str],
    clases: list[str],
) -> dict[str, float]:
    """Calcula precisión y recall por clase.

    Returns:
        {
          "precision_Normal": ..., "recall_Normal": ...,
          "precision_Bache": ...,  "recall_Bache": ...,
          ...
        }
    """
    precision = precision_score(
        y_true, y_pred, labels=clases, average=None, zero_division=0
    )
    recall = recall_score(
        y_true, y_pred, labels=clases, average=None, zero_division=0
    )
    metricas: dict[str, float] = {}
    for i, cls in enumerate(clases):
        metricas[f"precision_{cls}"] = float(precision[i])
        metricas[f"recall_{cls}"] = float(recall[i])
    return metricas


def severidad_mas_menos_1(tipos_reales: list[str], tipos_predichos: list[str], severidad_real: list[float],
                          severidad_predicha: list[float]) -> float:
    """R-012: proporción con |severidad predicha redondeada − real| ≤ 1, solo en anomalías detectadas con el tipo
    correcto (las otras ya penalizan precisión y recall). Sin ninguna, 0: no hay evidencia de cumplir RNF-35."""
    pares = [(round(float(predicha)), float(real)) for tipo, detectado, real, predicha
             in zip(tipos_reales, tipos_predichos, severidad_real, severidad_predicha, strict=True)
             if tipo != "Normal" and detectado == tipo]
    return sum(abs(predicha - real) <= 1 for predicha, real in pares) / len(pares) if pares else 0.0
