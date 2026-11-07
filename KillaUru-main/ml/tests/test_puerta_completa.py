"""T050 / FR-028, puerta.yaml — verificación completa antes de publicar."""
import numpy as np
import pytest
from killauru_ml.modelo.puerta import (
    verificar_tamano,
    verificar_metricas_puerta,
    verificar_severidad_puerta,
    ResultadoPuerta,
    CheckResultado,
)

_CLASES = ["Normal", "Bache", "Hundimiento"]


# --- tamaño del modelo (puerta.yaml TamanoMaximoModeloMb=5.0) ---

def test_tamano_ok_pasa(tmp_path) -> None:
    # Arrange — archivo de 1 KB
    ruta = tmp_path / "modelo.tflite"
    ruta.write_bytes(b"x" * 1024)
    # Act
    check = verificar_tamano(ruta, max_mb=5.0)
    # Assert
    assert check.aprobado


def test_tamano_excede_falla(tmp_path) -> None:
    # Arrange — archivo de 6 MB
    ruta = tmp_path / "modelo.tflite"
    ruta.write_bytes(b"x" * (6 * 1024 * 1024))
    # Act
    check = verificar_tamano(ruta, max_mb=5.0)
    # Assert
    assert not check.aprobado


# --- métricas (puerta.yaml Precision/RecallMinimaPorClase=0.90) ---

def _metricas_ok() -> dict:
    m = {f"precision_{c}": 0.90 for c in _CLASES}
    m.update({f"recall_{c}": 0.90 for c in _CLASES})
    return m


def test_metricas_ok_pasa() -> None:
    # Arrange + Act
    check = verificar_metricas_puerta(_metricas_ok(), precision_min=0.90, recall_min=0.90)
    # Assert
    assert check.aprobado


def test_metricas_precision_bache_falla() -> None:
    # Arrange — Bache precision = 0.89 < 0.90
    m = _metricas_ok()
    m["precision_Bache"] = 0.89
    # Act
    check = verificar_metricas_puerta(m, precision_min=0.90, recall_min=0.90)
    # Assert
    assert not check.aprobado
    assert "Bache" in check.detalle


def test_metricas_recall_hundimiento_falla() -> None:
    # Arrange
    m = _metricas_ok()
    m["recall_Hundimiento"] = 0.85
    # Act
    check = verificar_metricas_puerta(m, precision_min=0.90, recall_min=0.90)
    # Assert
    assert not check.aprobado


# --- severidad ±1 (puerta.yaml SeveridadMasMenosUnoMinima=0.80) ---

def test_severidad_90_pct_pasa() -> None:
    # Arrange — exactamente 90 % dentro de ±1
    sev_true = [3] * 90 + [3] * 10
    sev_pred = [3] * 90 + [5] * 10  # 10 fuera de ±1
    # Act
    check = verificar_severidad_puerta(sev_true, sev_pred, min_pct=0.90)
    # Assert
    assert check.aprobado


def test_severidad_89_pct_falla() -> None:
    # Arrange — 89 % dentro de ±1
    sev_true = [3] * 89 + [3] * 11
    sev_pred = [3] * 89 + [5] * 11  # 11 fuera de ±1
    # Act
    check = verificar_severidad_puerta(sev_true, sev_pred, min_pct=0.90)
    # Assert
    assert not check.aprobado


# --- ResultadoPuerta ---

def test_resultado_aprobado_si_todos_ok() -> None:
    # Arrange
    checks = [CheckResultado("a", 1.0, 0.9, True, ""), CheckResultado("b", 1.0, 0.9, True, "")]
    # Act
    resultado = ResultadoPuerta(aprobado=all(c.aprobado for c in checks), checks=checks)
    # Assert
    assert resultado.aprobado


def test_resultado_rechazado_si_alguno_falla() -> None:
    # Arrange
    checks = [CheckResultado("a", 1.0, 0.9, True, ""), CheckResultado("b", 0.4, 0.9, False, "fallo")]
    # Act
    resultado = ResultadoPuerta(aprobado=all(c.aprobado for c in checks), checks=checks)
    # Assert
    assert not resultado.aprobado
