"""T052 / FR-029, RN-04 — manifiesto del modelo: campos y restricciones."""
import json

import pytest

from killauru_ml.modelo.manifiesto import generar_manifiesto, guardar_manifiesto

_CLASES = ["Normal", "Bache", "Hundimiento"]


def test_manifiesto_tiene_campos_requeridos() -> None:
    # Arrange + Act
    m = generar_manifiesto("1.0.0", "abc123", _CLASES)
    # Assert
    assert "version" in m
    assert "sha256" in m
    assert "clases" in m
    assert "fecha_utc" in m


def test_manifiesto_sin_confidence_minimo() -> None:
    # Arrange + Act — RN-04: ConfidenceMinimo vive en la app, no en el modelo
    m = generar_manifiesto("1.0.0", "abc123", _CLASES)
    # Assert
    assert "confidence_minimo" not in m
    assert "ConfidenceMinimo" not in m


def test_manifiesto_version_y_sha256_correctos() -> None:
    # Arrange + Act
    m = generar_manifiesto("2.3.1", "deadbeef", _CLASES)
    # Assert
    assert m["version"] == "2.3.1"
    assert m["sha256"] == "deadbeef"
    assert m["clases"] == _CLASES


def test_manifiesto_serializable_json() -> None:
    # Arrange + Act + Assert — no debe lanzar
    m = generar_manifiesto("1.0.0", "abc", _CLASES)
    json.dumps(m)


def test_guardar_manifiesto_crea_archivo(tmp_path) -> None:
    # Arrange
    m = generar_manifiesto("1.0.0", "abc", _CLASES)
    ruta = tmp_path / "manifiesto.json"
    # Act
    guardar_manifiesto(m, ruta)
    # Assert
    assert ruta.exists()
    datos = json.loads(ruta.read_text())
    assert datos["version"] == "1.0.0"


def test_guardar_manifiesto_no_sobrescribe(tmp_path) -> None:
    # Arrange — FR-031: versiones publicadas no se sobreescriben jamás
    m = generar_manifiesto("1.0.0", "abc", _CLASES)
    ruta = tmp_path / "m.json"
    guardar_manifiesto(m, ruta)
    # Act + Assert
    with pytest.raises(FileExistsError):
        guardar_manifiesto(m, ruta)
