"""T040 / FR-016 — prefiltro de ventanas de baja varianza."""
import numpy as np

from killauru_ml.features.prefiltro import filtrar_prefiltro
from killauru_ml.ventanas.ventaneo import VentanaDataset


def _ventana(a_v: np.ndarray) -> VentanaDataset:
    return VentanaDataset(a_v=a_v, t_inicio_ns=0, t_fin_ns=500_000_000)


def _plana() -> VentanaDataset:
    return _ventana(np.zeros(25, dtype=np.float32))


def _activa(magnitud: float = 5.0) -> VentanaDataset:
    a_v = np.zeros(25, dtype=np.float32)
    a_v[12] = magnitud
    return _ventana(a_v)


# --- excluir señales planas ---

def test_ventana_plana_excluida() -> None:
    # Arrange
    ventanas = [_plana()]
    # Act
    resultado = filtrar_prefiltro(ventanas, umbral_var=0.01)
    # Assert
    assert len(resultado) == 0


def test_ventana_activa_incluida() -> None:
    # Arrange
    ventanas = [_activa(5.0)]
    # Act
    resultado = filtrar_prefiltro(ventanas, umbral_var=0.01)
    # Assert
    assert len(resultado) == 1


def test_umbral_estricto_mayor_excluye() -> None:
    # Arrange — var(a_v) ≈ 0.01 con pico pequeño
    a_v = np.zeros(25, dtype=np.float32)
    a_v[0] = 0.5  # var ≈ 0.5² / 25 = 0.01
    ventana = _ventana(a_v)
    # Act — umbral ligeramente mayor → excluida
    resultado = filtrar_prefiltro([ventana], umbral_var=float(np.var(a_v)) + 0.001)
    # Assert
    assert len(resultado) == 0


def test_umbral_estricto_menor_incluye() -> None:
    # Arrange
    a_v = np.zeros(25, dtype=np.float32)
    a_v[0] = 0.5
    ventana = _ventana(a_v)
    # Act — umbral ligeramente menor → incluida
    resultado = filtrar_prefiltro([ventana], umbral_var=float(np.var(a_v)) - 0.001)
    # Assert
    assert len(resultado) == 1


def test_mezcla_incluye_activas_excluye_planas() -> None:
    # Arrange
    ventanas = [_plana(), _activa(), _plana(), _activa(3.0)]
    # Act
    resultado = filtrar_prefiltro(ventanas, umbral_var=0.01)
    # Assert
    assert len(resultado) == 2


def test_lista_vacia_devuelve_vacia() -> None:
    # Arrange + Act + Assert
    assert filtrar_prefiltro([], umbral_var=0.01) == []
