"""T033 (parte) / FR-013, puerta.yaml VentanasPruebaMinimasPorClase — mínimo de 200 ventanas por clase en prueba.

Con menos ventanas, una precisión de 0,90 no es estadísticamente creíble. Solo cuentan las ventanas reales: las
aumentadas (copias desplazadas ± 200 ms) no suman. Patrón AAA.
"""
import numpy as np
import pytest

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.particion.tamano import (
    TamanoError,
    resumen_tamano,
    verificar_requisitos,
    verificar_tamano,
)
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


# --- FR-014 completo: celulares, sentidos y mínimos de validación y entrenamiento ---

def _particion_valida() -> dict[str, list[VentanaDataset]]:
    """Cumple FR-014 justo en los límites: prueba 200 por clase con 3 celulares y los 2 sentidos."""
    prueba = [VentanaDataset(a_v=np.zeros(25, dtype=np.float32), t_inicio_ns=0, t_fin_ns=1, tipo=tipo,
                             celular=f"C{indice % 3 + 1}", sentido=("AyacuchoVinchos", "VinchosAyacucho")[indice % 2])
              for tipo in CLASES for indice in range(200)]
    validacion = [ventana for tipo in CLASES for ventana in _ventanas(tipo, 100)]
    entrenamiento = [ventana for tipo in CLASES for ventana in _ventanas(tipo, 400)]
    return {"entrenamiento": entrenamiento, "validacion": validacion, "prueba": prueba}


def test_particion_en_los_limites_de_fr014_pasa() -> None:
    # Arrange
    particion = _particion_valida()
    # Act
    verificar_requisitos(particion)
    # Assert — no lanzó
    assert len(particion["prueba"]) == 600


def test_dos_celulares_en_prueba_es_error() -> None:
    # Arrange
    particion = _particion_valida()
    for ventana in particion["prueba"]:
        ventana.celular = "C1" if ventana.celular == "C3" else ventana.celular
    # Act y Assert
    with pytest.raises(TamanoError, match="celulares"):
        verificar_requisitos(particion)


def test_un_solo_sentido_en_prueba_es_error() -> None:
    # Arrange
    particion = _particion_valida()
    for ventana in particion["prueba"]:
        ventana.sentido = "AyacuchoVinchos"
    # Act y Assert
    with pytest.raises(TamanoError, match="sentidos"):
        verificar_requisitos(particion)


@pytest.mark.parametrize(("conjunto", "clase", "quitar"), [("validacion", "Bache", 1), ("entrenamiento", "Hundimiento", 1)])
def test_una_ventana_menos_del_minimo_es_error(conjunto: str, clase: str, quitar: int) -> None:
    # Arrange — validación 99 de 100; entrenamiento 399 de 400
    particion = _particion_valida()
    indice = next(i for i, ventana in enumerate(particion[conjunto]) if ventana.tipo == clase)
    del particion[conjunto][indice:indice + quitar]
    # Act y Assert
    with pytest.raises(TamanoError, match=f"{conjunto}.*{clase}"):
        verificar_requisitos(particion)
