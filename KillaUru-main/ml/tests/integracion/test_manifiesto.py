"""T052 / FR-029, RN-04 — manifiesto del modelo: campos y restricciones."""
import json
import pathlib

import pytest

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.modelo.manifiesto import generar_manifiesto, guardar_manifiesto

_CLASES = list(CLASES)  # orden del tensor: catálogo único


def test_manifiesto_tiene_campos_requeridos() -> None:
    # Arrange + Act
    manifiesto = generar_manifiesto("1.0.0", "abc123", _CLASES)
    # Assert
    assert "version" in manifiesto
    assert "sha256" in manifiesto
    assert "clases" in manifiesto
    assert "fecha_utc" in manifiesto


def test_manifiesto_sin_confidence_minimo() -> None:
    # Arrange + Act — RN-04: ConfidenceMinimo vive en la app, no en el modelo
    manifiesto = generar_manifiesto("1.0.0", "abc123", _CLASES)
    # Assert
    assert "confidence_minimo" not in manifiesto
    assert "ConfidenceMinimo" not in manifiesto


def test_manifiesto_version_y_sha256_correctos() -> None:
    # Arrange + Act
    manifiesto = generar_manifiesto("2.3.1", "deadbeef", _CLASES)
    # Assert
    assert manifiesto["version"] == "2.3.1"
    assert manifiesto["sha256"] == "deadbeef"
    assert manifiesto["clases"] == _CLASES


def test_manifiesto_serializable_json() -> None:
    # Arrange + Act + Assert — no debe lanzar
    manifiesto = generar_manifiesto("1.0.0", "abc", _CLASES)
    json.dumps(manifiesto)


def test_guardar_manifiesto_crea_archivo(tmp_path) -> None:
    # Arrange
    manifiesto = generar_manifiesto("1.0.0", "abc", _CLASES)
    ruta = tmp_path / "manifiesto.json"
    # Act
    guardar_manifiesto(manifiesto, ruta)
    # Assert
    assert ruta.exists()
    datos = json.loads(ruta.read_text())
    assert datos["version"] == "1.0.0"


def test_guardar_manifiesto_no_sobrescribe(tmp_path) -> None:
    # Arrange — FR-031: versiones publicadas no se sobreescriben jamás
    manifiesto = generar_manifiesto("1.0.0", "abc", _CLASES)
    ruta = tmp_path / "m.json"
    guardar_manifiesto(manifiesto, ruta)
    # Act + Assert
    with pytest.raises(FileExistsError):
        guardar_manifiesto(manifiesto, ruta)


@pytest.mark.xfail(strict=True, reason="Deuda T061: generar_manifiesto produce 4 campos y el contrato exige 21 "
                                       "(dos salidas, calibración, prefiltro, latencias). Ver evidencias 2026-11-23-ml.")
def test_manifiesto_cumple_el_contrato_que_lee_la_app() -> None:
    # Arrange
    import jsonschema
    raiz = pathlib.Path(__file__).resolve().parents[3]
    esquema = json.loads((raiz / "specs/009-dataset-y-modelo/contracts/manifiesto-modelo.schema.json").read_text("utf-8"))
    # Act
    manifiesto = generar_manifiesto("1.0.0", "a" * 64, _CLASES)
    # Assert
    jsonschema.validate(manifiesto, esquema)
