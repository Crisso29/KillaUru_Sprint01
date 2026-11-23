"""T033 / FR-013, R-008 — partición declarada a mano por recorrido. Patrón AAA.

El autor escribe en `particion.yaml` qué recorrido va a cada conjunto; `partir()` solo la aplica y se niega a
seguir si la declaración no cubre el catálogo o repite un recorrido.
"""
import numpy as np
import pytest

from killauru_ml.particion.declaracion import DeclaracionParticion
from killauru_ml.particion.particion import ParticionError, partir
from killauru_ml.ventanas.ventaneo import VentanaDataset


def _ventana(celular: str = "C1") -> VentanaDataset:
    return VentanaDataset(a_v=np.zeros(25, dtype=np.float32), t_inicio_ns=0, t_fin_ns=500_000_000, celular=celular)


def _agrupado(*recorridos: str) -> dict[str, list[VentanaDataset]]:
    """Cada recorrido grabado a la vez por tres celulares (2 ventanas por celular)."""
    return {recorrido: [_ventana(celular) for celular in ("C1", "C2", "C3") for _ in range(2)] for recorrido in recorridos}


def _declaracion(**cambios) -> DeclaracionParticion:
    valores = {"version": "ds-1", "congelada": True, "entrenamiento": ["R01", "R02", "R03"], "validacion": ["R04"],
               "prueba": ["R05"], "celular_no_visto": None}
    return DeclaracionParticion(**(valores | cambios))


def test_cada_recorrido_va_entero_al_conjunto_declarado() -> None:
    # Arrange
    agrupado = _agrupado("R01", "R02", "R03", "R04", "R05")
    # Act
    particion = partir(agrupado, _declaracion())
    # Assert — los tres celulares de R05 van juntos a prueba (R-008)
    assert {ventana.recorrido for ventana in particion["prueba"]} == {"R05"}
    assert {ventana.celular for ventana in particion["prueba"]} == {"C1", "C2", "C3"}
    assert [len(particion[conjunto]) for conjunto in ("entrenamiento", "validacion", "prueba")] == [18, 6, 6]


def test_recorrido_en_dos_conjuntos_es_error() -> None:
    # Arrange
    declaracion = _declaracion(validacion=["R04", "R01"])
    # Act y Assert
    with pytest.raises(ParticionError, match="R01.*entrenamiento.*validacion"):
        partir(_agrupado("R01", "R02", "R03", "R04", "R05"), declaracion)


def test_recorrido_del_catalogo_sin_conjunto_es_error() -> None:
    # Arrange — R06 se grabó pero nadie decidió a qué conjunto va
    agrupado = _agrupado("R01", "R02", "R03", "R04", "R05", "R06")
    # Act y Assert
    with pytest.raises(ParticionError, match="R06"):
        partir(agrupado, _declaracion())


def test_celular_no_visto_solo_aparece_en_prueba() -> None:
    # Arrange — C3 es el celular «no visto»: sus grabaciones de cualquier recorrido van a prueba
    declaracion = _declaracion(celular_no_visto="C3")
    # Act
    particion = partir(_agrupado("R01", "R02", "R03", "R04", "R05"), declaracion)
    # Assert
    assert all(ventana.celular != "C3" for conjunto in ("entrenamiento", "validacion") for ventana in particion[conjunto])
    assert sum(ventana.celular == "C3" for ventana in particion["prueba"]) == 10


def test_cambiar_la_particion_cambia_la_huella() -> None:
    # Arrange
    original, igual, cambiada = _declaracion(), _declaracion(), _declaracion(validacion=["R05"], prueba=["R04"])
    # Act
    huellas = [declaracion.huella() for declaracion in (original, igual, cambiada)]
    # Assert — la misma declaración da la misma huella; mover un recorrido la cambia
    assert huellas[0] == huellas[1] != huellas[2]
