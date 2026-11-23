"""T033 (parte) / FR-013, puerta.yaml VentanasPruebaMinimasPorClase — mínimo de 200 ventanas por clase en prueba.

Con menos ventanas, una precisión de 0,90 no es estadísticamente creíble. Solo cuentan las ventanas reales: las
aumentadas (copias desplazadas ± 200 ms) no suman. Patrón AAA.
"""
import numpy as np
import pytest

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.particion.tamano import TamanoError, resumen_tamano, verificar_tamano
from killauru_ml.ventanas.ventaneo import VentanaDataset

_MINIMO = 200


def _ventanas(tipo: str, cantidad: int, aumentada: bool = False) -> list[VentanaDataset]:
    return [VentanaDataset(a_v=np.zeros(25, dtype=np.float32), t_inicio_ns=0, t_fin_ns=500_000_000, tipo=tipo,
                           aumentada=aumentada) for _ in range(cantidad)]


def _prueba(hundimientos: int, hundimientos_aumentados: int = 0) -> dict[str, list[VentanaDataset]]:
    ventanas = _ventanas("Normal", _MINIMO) + _ventanas("Bache", _MINIMO) + _ventanas("Hundimiento", hundimientos)
    return {"prueba": ventanas + _ventanas("Hundimiento", hundimientos_aumentados, aumentada=True)}


def test_200_ventanas_por_clase_pasa() -> None:
    # Arrange
    particion = _prueba(hundimientos=200)
    # Act
    verificar_tamano(particion, list(CLASES), _MINIMO)
    # Assert — no lanzó: el límite es inclusivo
    assert resumen_tamano(particion)["prueba"]["Hundimiento"] == 200


def test_199_ventanas_de_hundimiento_falla_y_lo_dice() -> None:
    # Arrange
    particion = _prueba(hundimientos=199)
    # Act y Assert — la excepción es el resultado esperado
    with pytest.raises(TamanoError, match="Hundimiento: 199 < 200"):
        verificar_tamano(particion, list(CLASES), _MINIMO)


def test_las_ventanas_aumentadas_no_cuentan() -> None:
    # Arrange — 150 reales + 100 copias aumentadas = 250, pero solo 150 son reales
    particion = _prueba(hundimientos=150, hundimientos_aumentados=100)
    # Act y Assert
    with pytest.raises(TamanoError, match="Hundimiento: 150 < 200"):
        verificar_tamano(particion, list(CLASES), _MINIMO)
