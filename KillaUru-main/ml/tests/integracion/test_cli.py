"""T014 / FR-016 — la línea de órdenes `python -m killauru_ml <etapa>` y sus códigos de salida.

0 = correcto, 1 = los datos no pasan la verificación, 2 = etapa pendiente o que necesita datos reales. Se escribieron
antes de partir `main` en un despachador. Patrón AAA.
"""
import json
import pathlib

import pytest

from killauru_ml.__main__ import main
from tests.sinteticos import crear_grabacion


def _etiquetas(tmp_path: pathlib.Path, etiquetas: list[dict]) -> str:
    ruta = tmp_path / "etiquetas.json"
    ruta.write_text(json.dumps(etiquetas), encoding="utf-8")
    return str(ruta)


def test_etiquetas_validas_salen_con_0(tmp_path: pathlib.Path) -> None:
    # Arrange
    archivo = _etiquetas(tmp_path, [{"tipo": "Bache", "severidad": 3}, {"tipo": "Normal", "subtipo": "Junta"}])
    # Act
    codigo = main(["validar-etiquetas", archivo])
    # Assert
    assert codigo == 0


def test_etiqueta_invalida_sale_con_1_y_lo_dice(tmp_path: pathlib.Path, capsys: pytest.CaptureFixture) -> None:
    # Arrange — rompemuelles es subtipo de Normal, no un tipo
    archivo = _etiquetas(tmp_path, [{"tipo": "Rompemuelles"}])
    # Act
    codigo = main(["validar-etiquetas", archivo])
    # Assert
    assert codigo == 1
    assert "Rompemuelles" in capsys.readouterr().err


def test_ingerir_grabacion_apta_sale_con_0(tmp_path: pathlib.Path) -> None:
    # Arrange
    carpeta = crear_grabacion(tmp_path / "grabaciones")
    # Act
    codigo = main(["ingerir", str(carpeta), "--catalogo", str(tmp_path / "catalogo.csv")])
    # Assert
    assert codigo == 0


def test_particion_lee_la_version_congelada(capsys: pytest.CaptureFixture) -> None:
    # Arrange — se ejecuta desde ml/, donde está config/particion.yaml
    argumentos = ["particion"]
    # Act
    codigo = main(argumentos)
    # Assert
    assert codigo == 0
    assert "ds-1" in capsys.readouterr().out


@pytest.mark.parametrize("argumentos", [["todo", "--version", "1.0.0"], ["candidatos"], ["evaluar"]])
def test_etapas_pendientes_o_sin_datos_reales_salen_con_2(argumentos: list[str]) -> None:
    # Arrange — pendientes del plan (T032 datos reales, T037–T043 etapas)
    # Act
    codigo = main(argumentos)
    # Assert
    assert codigo == 2
