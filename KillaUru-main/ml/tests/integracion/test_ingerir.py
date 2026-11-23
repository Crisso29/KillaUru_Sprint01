"""T013 / FR-007 — ingesta: verifica la grabación, calcula su huella sin el video y la anota en el catálogo.

Se escribieron antes de partir `ingerir` en funciones cortas: fijan su comportamiento para que el refactor no lo
cambie. Patrón AAA.
"""
import pathlib

from killauru_ml.lectura.catalogo import leer_catalogo
from killauru_ml.lectura.ingerir import ingerir
from tests.sinteticos import crear_grabacion


def test_grabacion_buena_queda_apta_en_el_catalogo(tmp_path: pathlib.Path) -> None:
    # Arrange
    carpeta = crear_grabacion(tmp_path / "grabaciones", duracion_s=60)
    catalogo = tmp_path / "catalogo.csv"
    # Act
    fila = ingerir(carpeta, ruta_catalogo=catalogo)
    # Assert — 60 s a 8,33 m/s = 60 s en marcha; la fila escrita es la devuelta
    assert (fila.estado, fila.motivo, fila.celular, fila.recorrido) == ("Apta", "", "C1", "R01")
    assert abs(fila.horas - 60 / 3600) < 1e-9
    assert len(fila.sha256) == 64
    assert [guardada.grabacion_id for guardada in leer_catalogo(catalogo)] == [fila.grabacion_id]


def test_la_huella_no_depende_del_video(tmp_path: pathlib.Path) -> None:
    # Arrange — el video no se sube (privacidad, FR-007): cambiarlo no debe cambiar la huella
    carpeta = crear_grabacion(tmp_path / "grabaciones", es_video=True)
    huella_antes = ingerir(carpeta, ruta_catalogo=tmp_path / "catalogo.csv").sha256
    (carpeta / "video.mp4").write_bytes(b"otro contenido de video")
    # Act
    huella_despues = ingerir(carpeta, ruta_catalogo=tmp_path / "catalogo.csv").sha256
    # Assert
    assert huella_despues == huella_antes


def test_grabacion_sin_meta_queda_no_apta_con_motivo(tmp_path: pathlib.Path) -> None:
    # Arrange
    carpeta = crear_grabacion(tmp_path / "grabaciones")
    (carpeta / "meta.json").unlink()
    # Act
    fila = ingerir(carpeta, ruta_catalogo=tmp_path / "catalogo.csv")
    # Assert
    assert fila.estado == "NoApta"
    assert fila.motivo.startswith("MetaIncompleta")
    assert fila.grabacion_id == carpeta.name
