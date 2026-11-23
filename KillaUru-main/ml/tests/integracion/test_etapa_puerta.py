"""T060, T064 / FR-028, data-model §7 — la etapa `puerta`: código de salida y manifiesto solo si aprueba. Patrón AAA."""
import dataclasses
import json
import pathlib

import pytest

from killauru_ml.__main__ import main
from killauru_ml.modelo.puerta import (
    Acuerdo,
    Equivalencia,
    EvidenciaPuerta,
    MedicionCelular,
    VerificacionParticion,
)


def _preparar(tmp_path: pathlib.Path, **cambios) -> pathlib.Path:
    carpeta = tmp_path / "modelos" / "1.0.0"
    carpeta.mkdir(parents=True)
    tflite = carpeta / "killauru-1.0.0.tflite"
    tflite.write_bytes(b"x" * 2048)
    evidencia = EvidenciaPuerta(
        version="1.0.0", dataset_version="ds-1", version_minima_app="1.0.0", ruta_tflite=str(tflite),
        temperatura=1.3, umbral_prefiltro=1.2, clases=["Normal", "Bache", "Hundimiento"],
        precision={"Normal": 0.99, "Bache": 0.93, "Hundimiento": 0.91},
        recall={"Normal": 0.99, "Bache": 0.92, "Hundimiento": 0.90}, severidad_mas_menos_1=0.86, anio_medicion=2026,
        benchmark=[MedicionCelular(f"C{numero}", 4, 2024, 0.5) for numero in (1, 2, 3)],
        equivalencia=Equivalencia(0.997, 0.03, 0.99), acuerdo=Acuerdo(0.85, 0.93),
        particion=VerificacionParticion(True, ""))
    dataclasses.replace(evidencia, **cambios).guardar(carpeta / "evidencia-puerta.json")
    return carpeta


def test_aprobada_sale_con_0_y_escribe_un_manifiesto_valido(tmp_path: pathlib.Path, monkeypatch) -> None:
    # Arrange
    carpeta = _preparar(tmp_path)
    monkeypatch.chdir(tmp_path)
    # Act
    codigo = main(["puerta", "--version", "1.0.0", "--config", str(pathlib.Path(__file__).parents[2] / "config")])
    # Assert — el manifiesto trae la latencia de los 3 celulares y la huella del archivo
    manifiesto = json.loads((carpeta / "manifiesto.json").read_text(encoding="utf-8"))
    assert codigo == 0
    assert set(manifiesto["metricas"]["latenciaP95MsPorCelular"]) == {"C1", "C2", "C3"}


def test_rechazada_sale_con_1_y_no_escribe_manifiesto(tmp_path: pathlib.Path, monkeypatch,
                                                       capsys: pytest.CaptureFixture) -> None:
    # Arrange — el acuerdo del etiquetado no llega
    carpeta = _preparar(tmp_path, acuerdo=Acuerdo(0.79, 0.93))
    monkeypatch.chdir(tmp_path)
    # Act
    codigo = main(["puerta", "--version", "1.0.0", "--config", str(pathlib.Path(__file__).parents[2] / "config")])
    # Assert
    assert codigo == 1
    assert not (carpeta / "manifiesto.json").exists()
    assert "[FALLA] acuerdo" in capsys.readouterr().out
