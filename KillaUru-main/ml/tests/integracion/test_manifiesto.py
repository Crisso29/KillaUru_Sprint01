"""T061 / FR-029, RN-04, R-017 — manifiesto del modelo según el contrato que lee la app (008 FR-071). Patrón AAA."""
import dataclasses
import hashlib
import json
import pathlib

import jsonschema
import pytest

from killauru_ml.modelo.manifiesto import DatosManifiesto, generar_manifiesto, guardar_manifiesto

_RAIZ = pathlib.Path(__file__).resolve().parents[3]
_ESQUEMA = json.loads((_RAIZ / "specs/009-dataset-y-modelo/contracts/manifiesto-modelo.schema.json").read_text("utf-8"))
_DATOS = DatosManifiesto(
    version="1.0.0", dataset_version="ds-1", version_minima_app="1.0.0", temperatura=1.35, umbral_prefiltro=1.2,
    precision={"Normal": 0.998, "Bache": 0.93, "Hundimiento": 0.91},
    recall={"Normal": 0.999, "Bache": 0.92, "Hundimiento": 0.90},
    severidad_mas_menos_1=0.86, latencias_p95_ms={"C1": 0.41, "C2": 0.38, "C3": 0.62},
)


@pytest.fixture
def tflite(tmp_path: pathlib.Path) -> pathlib.Path:
    ruta = tmp_path / "killauru-1.0.0.tflite"
    ruta.write_bytes(b"modelo de prueba" * 64)
    return ruta


def test_cumple_el_contrato_que_lee_la_app(tflite: pathlib.Path) -> None:
    # Arrange y Act
    manifiesto = generar_manifiesto(_DATOS, tflite, fecha_utc="2026-11-23T15:00:00Z")
    # Assert — lanza ValidationError con el campo exacto si algo no cumple
    jsonschema.validate(manifiesto, _ESQUEMA)


def test_sha256_y_tamano_son_los_del_archivo(tflite: pathlib.Path) -> None:
    # Arrange
    contenido = tflite.read_bytes()
    # Act
    manifiesto = generar_manifiesto(_DATOS, tflite)
    # Assert
    assert (manifiesto["sha256"], manifiesto["tamanoBytes"]) == (hashlib.sha256(contenido).hexdigest(), len(contenido))


def test_no_trae_confidence_minimo(tflite: pathlib.Path) -> None:
    # Arrange — RN-04: el umbral de confidence vive en la app y el servidor, no en el modelo
    manifiesto = generar_manifiesto(_DATOS, tflite)
    # Act
    texto = json.dumps(manifiesto).lower()
    # Assert
    assert "confidence" not in texto


def test_las_urls_apuntan_a_la_carpeta_de_su_version(tflite: pathlib.Path) -> None:
    # Arrange
    manifiesto = generar_manifiesto(_DATOS, tflite)
    # Act
    urls = [manifiesto["url"], manifiesto["matrizConfusionUrl"], manifiesto["informeUrl"]]
    # Assert — R-017: cada versión en su carpeta inmutable
    assert all(url.startswith("https://cdn.killauru.pe/modelo/1.0.0/") for url in urls)
    assert manifiesto["url"].endswith("/killauru-1.0.0.tflite")


def test_una_metrica_bajo_rnf35_no_cumple_el_contrato(tflite: pathlib.Path) -> None:
    # Arrange — el contrato también protege: un modelo con recall 0,89 no se puede publicar
    datos = dataclasses.replace(_DATOS, recall={"Normal": 0.99, "Bache": 0.89, "Hundimiento": 0.90})
    # Act
    manifiesto = generar_manifiesto(datos, tflite)
    # Assert
    with pytest.raises(jsonschema.ValidationError, match="0.89"):
        jsonschema.validate(manifiesto, _ESQUEMA)


def test_guardar_no_sobrescribe_una_version_publicada(tflite: pathlib.Path, tmp_path: pathlib.Path) -> None:
    # Arrange — FR-031: los archivos de una versión publicada nunca se reemplazan
    ruta = tmp_path / "manifiesto.json"
    guardar_manifiesto(generar_manifiesto(_DATOS, tflite), ruta)
    # Act y Assert
    with pytest.raises(FileExistsError):
        guardar_manifiesto(generar_manifiesto(_DATOS, tflite), ruta)
