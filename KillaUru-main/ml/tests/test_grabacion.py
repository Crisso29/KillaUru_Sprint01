"""T008 / FR-003, FR-004, FR-005, FR-006 — lectura y validación de grabaciones."""
import json
import pathlib

import pytest

from tests.sinteticos import crear_grabacion


def test_lee_carpeta_sintetica_sin_error(tmp_path: pathlib.Path) -> None:
    # Arrange
    carpeta = crear_grabacion(tmp_path, patron="bache")
    from killauru_ml.lectura.grabacion import leer_grabacion
    # Act
    grabacion = leer_grabacion(carpeta)
    # Assert
    assert grabacion.meta["celular"]["alias"] == "C1"
    assert len(grabacion.acelerometro) > 0


def test_categoria_pesado_lanza_error(tmp_path: pathlib.Path) -> None:
    # Arrange
    carpeta = crear_grabacion(tmp_path, patron="liso")
    meta_path = carpeta / "meta.json"
    meta = json.loads(meta_path.read_text(encoding="utf-8"))
    meta["vehiculo"]["categoria"] = "Pesado"
    meta_path.write_text(json.dumps(meta), encoding="utf-8")
    from killauru_ml.lectura.grabacion import GrabacionError, leer_grabacion
    # Act + Assert — RN-18: solo Liviano detecta
    with pytest.raises(GrabacionError, match="categoria"):
        leer_grabacion(carpeta)


def test_meta_json_con_campo_imei_lanza_error(tmp_path: pathlib.Path) -> None:
    # Arrange
    carpeta = crear_grabacion(tmp_path, patron="liso")
    meta_path = carpeta / "meta.json"
    meta = json.loads(meta_path.read_text(encoding="utf-8"))
    meta["imei"] = "123456789012345"
    meta_path.write_text(json.dumps(meta), encoding="utf-8")
    from killauru_ml.lectura.grabacion import GrabacionError, leer_grabacion
    # Act + Assert — FR-005: ningún dato personal
    with pytest.raises(GrabacionError, match="imei"):
        leer_grabacion(carpeta)


def test_meta_json_con_campo_placa_lanza_error(tmp_path: pathlib.Path) -> None:
    # Arrange
    carpeta = crear_grabacion(tmp_path, patron="liso")
    meta_path = carpeta / "meta.json"
    meta = json.loads(meta_path.read_text(encoding="utf-8"))
    meta["placa"] = "ABC-123"
    meta_path.write_text(json.dumps(meta), encoding="utf-8")
    from killauru_ml.lectura.grabacion import GrabacionError, leer_grabacion
    # Act + Assert
    with pytest.raises(GrabacionError, match="placa"):
        leer_grabacion(carpeta)


def test_meta_json_con_campo_nombre_lanza_error(tmp_path: pathlib.Path) -> None:
    # Arrange
    carpeta = crear_grabacion(tmp_path, patron="liso")
    meta_path = carpeta / "meta.json"
    meta = json.loads(meta_path.read_text(encoding="utf-8"))
    meta["nombre"] = "Juan"
    meta_path.write_text(json.dumps(meta), encoding="utf-8")
    from killauru_ml.lectura.grabacion import GrabacionError, leer_grabacion
    # Act + Assert
    with pytest.raises(GrabacionError, match="nombre"):
        leer_grabacion(carpeta)


def test_archivo_acelerometro_faltante_lanza_error(tmp_path: pathlib.Path) -> None:
    # Arrange
    carpeta = crear_grabacion(tmp_path, patron="liso")
    (carpeta / "acelerometro.csv").unlink()
    from killauru_ml.lectura.grabacion import GrabacionError, leer_grabacion
    # Act + Assert
    with pytest.raises(GrabacionError, match="acelerometro"):
        leer_grabacion(carpeta)
