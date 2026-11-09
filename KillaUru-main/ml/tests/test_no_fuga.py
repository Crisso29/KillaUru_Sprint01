"""T034 / FR-013 — verificación de no fuga entre conjuntos."""
import numpy as np
import pytest

from killauru_ml.particion.no_fuga import FugaError, verificar_no_fuga
from killauru_ml.ventanas.ventaneo import VentanaDataset


def _ventana(recorrido: str) -> VentanaDataset:
    return VentanaDataset(
        a_v=np.zeros(25, dtype=np.float32),
        t_inicio_ns=0,
        t_fin_ns=500_000_000,
        tipo="Normal",
        recorrido=recorrido,
    )


# --- casos positivos ---

def test_sin_fuga_pasa() -> None:
    # Arrange — sets completamente disjuntos
    particion = {
        "entrenamiento": [_ventana("R01"), _ventana("R02")],
        "validacion": [_ventana("R03")],
        "prueba": [_ventana("R04")],
    }
    # Act + Assert — no debe lanzar
    verificar_no_fuga(particion)


def test_un_solo_conjunto_pasa() -> None:
    # Arrange
    particion = {"entrenamiento": [_ventana("R01"), _ventana("R02")]}
    # Act + Assert
    verificar_no_fuga(particion)


# --- casos negativos ---

def test_fuga_entrenamiento_prueba_lanza_error() -> None:
    # Arrange — R01 aparece en entrenamiento Y prueba
    particion = {
        "entrenamiento": [_ventana("R01"), _ventana("R02")],
        "validacion": [_ventana("R03")],
        "prueba": [_ventana("R01")],  # fuga!
    }
    # Act + Assert
    with pytest.raises(FugaError, match="R01"):
        verificar_no_fuga(particion)


def test_fuga_entrenamiento_validacion_lanza_error() -> None:
    # Arrange — R02 aparece en entrenamiento Y validacion
    particion = {
        "entrenamiento": [_ventana("R01"), _ventana("R02")],
        "validacion": [_ventana("R02")],
        "prueba": [_ventana("R03")],
    }
    # Act + Assert
    with pytest.raises(FugaError, match="R02"):
        verificar_no_fuga(particion)


# --- integración con partir ---

def test_partir_produce_particion_sin_fuga() -> None:
    # Arrange
    from killauru_ml.particion.particion import partir
    agrupado = {f"R{i:02d}": [_ventana(f"R{i:02d}") for _ in range(5)] for i in range(1, 11)}
    # Act
    particion = partir(agrupado)
    # Assert — partir siempre devuelve partición sin fuga
    verificar_no_fuga(particion)
