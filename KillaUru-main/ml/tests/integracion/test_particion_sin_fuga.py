"""T034 / FR-013: partición y verificador de no fuga colaboran correctamente."""
import numpy as np

from killauru_ml.particion.no_fuga import verificar_no_fuga
from killauru_ml.ventanas.ventaneo import VentanaDataset


def _ventana(recorrido: str) -> VentanaDataset:
    return VentanaDataset(
        a_v=np.zeros(25, dtype=np.float32),
        t_inicio_ns=0,
        t_fin_ns=500_000_000,
        tipo="Normal",
        recorrido=recorrido,
    )


def test_partir_produce_particion_sin_fuga() -> None:
    # Arrange
    from killauru_ml.particion.declaracion import DeclaracionParticion
    from killauru_ml.particion.particion import partir
    agrupado = {f"R{i:02d}": [_ventana(f"R{i:02d}") for _ in range(5)] for i in range(1, 11)}
    declaracion = DeclaracionParticion("ds-1", True, [f"R{i:02d}" for i in range(1, 8)], ["R08"], ["R09", "R10"])
    # Act
    particion = partir(agrupado, declaracion)
    # Assert — partir siempre devuelve partición sin fuga
    verificar_no_fuga(particion)
