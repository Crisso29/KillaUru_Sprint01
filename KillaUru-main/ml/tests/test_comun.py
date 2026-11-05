"""T005 / FR-016 — pruebas de los módulos comunes: config, semillas, huella, entorno."""
import pathlib
import hashlib
import tempfile
import pytest


# ---------------------------------------------------------------------------
# comun.config
# ---------------------------------------------------------------------------

def test_leer_yaml_devuelve_valores_esperados(tmp_path: pathlib.Path) -> None:
    # Arrange
    yaml_texto = "FrecuenciaHz: 50\nVentanaMs: 500\n"
    archivo = tmp_path / "captura.yaml"
    archivo.write_text(yaml_texto, encoding="utf-8")
    from killauru_ml.comun.config import leer_yaml
    # Act
    cfg = leer_yaml(archivo)
    # Assert
    assert cfg["FrecuenciaHz"] == 50
    assert cfg["VentanaMs"] == 500


def test_leer_yaml_falla_si_falta_clave(tmp_path: pathlib.Path) -> None:
    # Arrange
    archivo = tmp_path / "captura.yaml"
    archivo.write_text("FrecuenciaHz: 50\n", encoding="utf-8")
    from killauru_ml.comun.config import leer_yaml, ConfigError
    # Act + Assert
    with pytest.raises(ConfigError, match="VentanaMs"):
        leer_yaml(archivo, claves_requeridas=["FrecuenciaHz", "VentanaMs"])


def test_leer_yaml_falla_si_archivo_no_existe() -> None:
    # Arrange
    from killauru_ml.comun.config import leer_yaml
    # Act + Assert
    with pytest.raises(FileNotFoundError):
        leer_yaml(pathlib.Path("no_existe.yaml"))


# ---------------------------------------------------------------------------
# comun.semillas
# ---------------------------------------------------------------------------

def test_misma_semilla_produce_mismos_numeros() -> None:
    # Arrange
    from killauru_ml.comun.semillas import fijar_semillas
    import numpy as np
    # Act
    fijar_semillas(2828)
    a = np.random.rand(5).tolist()
    fijar_semillas(2828)
    b = np.random.rand(5).tolist()
    # Assert
    assert a == b


def test_semillas_distintas_producen_numeros_distintos() -> None:
    # Arrange
    from killauru_ml.comun.semillas import fijar_semillas
    import numpy as np
    # Act
    fijar_semillas(2828)
    a = np.random.rand(5).tolist()
    fijar_semillas(9999)
    b = np.random.rand(5).tolist()
    # Assert
    assert a != b


# ---------------------------------------------------------------------------
# comun.huella
# ---------------------------------------------------------------------------

def test_sha256_de_archivo_conocido(tmp_path: pathlib.Path) -> None:
    # Arrange
    contenido = b"killauru"
    archivo = tmp_path / "prueba.bin"
    archivo.write_bytes(contenido)
    esperado = hashlib.sha256(contenido).hexdigest()
    from killauru_ml.comun.huella import sha256_archivo
    # Act
    resultado = sha256_archivo(archivo)
    # Assert
    assert resultado == esperado


def test_sha256_archivo_vacio(tmp_path: pathlib.Path) -> None:
    # Arrange
    archivo = tmp_path / "vacio.bin"
    archivo.write_bytes(b"")
    esperado = hashlib.sha256(b"").hexdigest()
    from killauru_ml.comun.huella import sha256_archivo
    # Act
    resultado = sha256_archivo(archivo)
    # Assert
    assert resultado == esperado


# ---------------------------------------------------------------------------
# comun.entorno
# ---------------------------------------------------------------------------

def test_entorno_devuelve_versiones_de_python_y_tensorflow() -> None:
    # Arrange
    import sys
    from killauru_ml.comun.entorno import versiones
    # Act
    env = versiones()
    # Assert
    assert env["python"].startswith(f"{sys.version_info.major}.{sys.version_info.minor}")
    assert "tensorflow" in env
    assert len(env["tensorflow"]) > 0


def test_entorno_devuelve_numpy() -> None:
    # Arrange
    import numpy as np
    from killauru_ml.comun.entorno import versiones
    # Act
    env = versiones()
    # Assert
    assert env.get("numpy") == np.__version__
