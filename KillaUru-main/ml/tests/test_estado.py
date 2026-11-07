"""T055 / FR-034 — estado del pipeline: grabaciones, aptas, partición."""
import pathlib
import pytest
from killauru_ml.estado import obtener_estado

_CABECERA = "grabacion_id,recorrido,celular,vehiculo,sentido,horas,hz_efectivo,huecos_pct,sha256,estado,motivo\n"


def test_estado_tiene_claves_requeridas() -> None:
    # Arrange + Act — sin catálogo en disco
    estado = obtener_estado()
    # Assert
    assert "grabaciones" in estado
    assert "grabaciones_aptas" in estado
    assert "version_particion" in estado


def test_catalogo_vacio_devuelve_ceros(tmp_path) -> None:
    # Arrange — solo cabecera, sin filas
    cat = tmp_path / "catalogo.csv"
    cat.write_text(_CABECERA)
    # Act
    estado = obtener_estado(ruta_catalogo=cat)
    # Assert
    assert estado["grabaciones"] == 0
    assert estado["grabaciones_aptas"] == 0


def test_catalogo_cuenta_grabaciones(tmp_path) -> None:
    # Arrange
    cat = tmp_path / "catalogo.csv"
    cat.write_text(
        _CABECERA
        + "g001,R01,C1,auto,A→V,0.5,49.8,0.2,abc,Apta,\n"
        + "g002,R01,C2,auto,A→V,0.5,47.0,1.5,def,NoApta,FRECUENCIA_BAJA\n"
        + "g003,R02,C1,auto,A→V,0.5,49.9,0.1,ghi,Apta,\n"
    )
    # Act
    estado = obtener_estado(ruta_catalogo=cat)
    # Assert
    assert estado["grabaciones"] == 3
    assert estado["grabaciones_aptas"] == 2


def test_sin_catalogo_grabaciones_cero() -> None:
    # Arrange — ruta que no existe
    # Act
    estado = obtener_estado(ruta_catalogo=pathlib.Path("no_existe_xyzxyz.csv"))
    # Assert
    assert estado["grabaciones"] == 0


def test_particion_borrador_en_estado(tmp_path) -> None:
    # Arrange — crear particion.yaml mínimo
    import yaml  # noqa: PLC0415
    cfg = tmp_path / "particion.yaml"
    cfg.write_text("Version: ds-1\nEstado: borrador\n")
    # Act
    estado = obtener_estado(ruta_particion=cfg)
    # Assert
    assert estado["version_particion"] == "ds-1"
    assert estado["estado_particion"] == "borrador"
