"""T025 / FR-009, FR-010 — validación de etiquetas: tipos, severidades y subtipos."""
import pytest
from killauru_ml.etiquetado.validar_etiquetas import validar_etiqueta, EtiquetaError


def _etiqueta(**kwargs):
    base = {"tipo": "Bache", "subtipo": "", "severidad": 3, "fuente": "video", "etiquetador": "E1", "bache_patron": ""}
    base.update(kwargs)
    return base


# --- tipo y severidad ---

def test_normal_sin_severidad_es_valido() -> None:
    # Arrange + Act + Assert
    validar_etiqueta(_etiqueta(tipo="Normal", severidad=None))


def test_normal_con_severidad_lanza_error() -> None:
    # Arrange + Act + Assert
    with pytest.raises(EtiquetaError, match="severidad"):
        validar_etiqueta(_etiqueta(tipo="Normal", severidad=3))


def test_bache_sin_severidad_lanza_error() -> None:
    # Arrange + Act + Assert
    with pytest.raises(EtiquetaError, match="severidad"):
        validar_etiqueta(_etiqueta(tipo="Bache", severidad=None))


def test_hundimiento_sin_severidad_lanza_error() -> None:
    # Arrange + Act + Assert
    with pytest.raises(EtiquetaError, match="severidad"):
        validar_etiqueta(_etiqueta(tipo="Hundimiento", severidad=None))


# --- límites de severidad ---

def test_severidad_0_lanza_error() -> None:
    # Arrange + Act + Assert
    with pytest.raises(EtiquetaError, match="severidad"):
        validar_etiqueta(_etiqueta(tipo="Bache", severidad=0))


def test_severidad_6_lanza_error() -> None:
    # Arrange + Act + Assert
    with pytest.raises(EtiquetaError, match="severidad"):
        validar_etiqueta(_etiqueta(tipo="Bache", severidad=6))


def test_severidad_1_es_valida() -> None:
    # Arrange + Act + Assert
    validar_etiqueta(_etiqueta(tipo="Bache", severidad=1))


def test_severidad_5_es_valida() -> None:
    # Arrange + Act + Assert
    validar_etiqueta(_etiqueta(tipo="Hundimiento", severidad=5))


# --- subtipos ---

def test_bache_con_subtipo_lanza_error() -> None:
    # Arrange + Act + Assert
    with pytest.raises(EtiquetaError, match="subtipo"):
        validar_etiqueta(_etiqueta(tipo="Bache", severidad=2, subtipo="Junta"))


def test_normal_con_subtipo_valido_es_correcto() -> None:
    # Arrange + Act + Assert
    validar_etiqueta(_etiqueta(tipo="Normal", severidad=None, subtipo="Rompemuelles"))


def test_tipo_rompemuelles_lanza_error() -> None:
    # Arrange — Rompemuelles es subtipo, no tipo (RN-01, RS-07)
    with pytest.raises(EtiquetaError, match="tipo"):
        validar_etiqueta(_etiqueta(tipo="Rompemuelles", severidad=None))


# --- fuente ---

def test_fuente_invalida_lanza_error() -> None:
    # Arrange + Act + Assert
    with pytest.raises(EtiquetaError, match="fuente"):
        validar_etiqueta(_etiqueta(tipo="Bache", severidad=2, fuente="desconocida"))


def test_fuente_patron_es_valida() -> None:
    # Arrange + Act + Assert
    validar_etiqueta(_etiqueta(tipo="Bache", severidad=3, fuente="patron"))
