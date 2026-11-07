"""FR-019, RNF-35 — métricas por clase y puerta de calidad del modelo."""
from __future__ import annotations

from sklearn.metrics import precision_score, recall_score

_PRECISION_MIN = 0.90   # puerta.yaml PrecisionMinimaPorClase
_RECALL_MIN = 0.90      # puerta.yaml RecallMinimoPorClase
_SEV_MIN = 0.80         # puerta.yaml SeveridadMasMenosUnoMinima


class MetricaError(ValueError):
    pass


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


def verificar_puerta_metricas(
    metricas: dict[str, float],
    precision_min: float = _PRECISION_MIN,
    recall_min: float = _RECALL_MIN,
    sev_min: float = _SEV_MIN,
) -> None:
    """Lanza MetricaError si alguna métrica no supera el umbral mínimo (RNF-35).

    Args:
        metricas: dict producido por calcular_metricas + "severidad_mas_menos_uno"
    """
    errores = []
    for clave, valor in metricas.items():
        if clave.startswith("precision_") and valor < precision_min:
            cls = clave.replace("precision_", "")
            errores.append(f"{cls}: precision={valor:.3f} < {precision_min}")
        elif clave.startswith("recall_") and valor < recall_min:
            cls = clave.replace("recall_", "")
            errores.append(f"{cls}: recall={valor:.3f} < {recall_min}")

    sev = metricas.get("severidad_mas_menos_uno")
    if sev is not None and sev < sev_min:
        errores.append(f"severidad_mas_menos_uno={sev:.3f} < {sev_min}")

    if errores:
        raise MetricaError("; ".join(errores))
