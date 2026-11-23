"""T039 / FR-018, RNF-15 — prefiltro de energía: pico a pico vertical bajo el umbral → Normal sin llamar al modelo.

El umbral se elige con las anomalías de entrenamiento para perder como mucho el 1 %. Patrón AAA.
"""
import numpy as np
import pytest

from killauru_ml.features.features import INDICE_PICO_A_PICO
from killauru_ml.features.prefiltro import (
    PrefiltroError,
    ajustar_prefiltro,
    clasificar_con_prefiltro,
    verificar_umbral,
)
from killauru_ml.particion.no_fuga import huella_ventana
from killauru_ml.ventanas.ventaneo import VentanaDataset


def _anomalia(pico_a_pico: float, tipo: str = "Bache") -> VentanaDataset:
    a_v = np.zeros(25, dtype=np.float32)
    a_v[12] = pico_a_pico
    return VentanaDataset(a_v=a_v, t_inicio_ns=int(pico_a_pico * 1000), t_fin_ns=0, tipo=tipo, recorrido="R01")


def _vector(pico_a_pico: float) -> np.ndarray:
    vector = np.zeros(20, dtype=np.float32)
    vector[INDICE_PICO_A_PICO] = pico_a_pico
    return vector


def test_el_umbral_elegido_deja_pasar_al_menos_el_99_por_ciento_de_las_anomalias() -> None:
    # Arrange — 1 000 anomalías con pico a pico 1, 2, …, 1 000, y Normales que no deben influir
    ventanas = [_anomalia(float(valor)) for valor in range(1, 1001)] + [_anomalia(0.1, "Normal")] * 50
    # Act
    umbral, _ = ajustar_prefiltro(ventanas)
    # Assert — pierde exactamente 10 (1 %): las de pico a pico 1…10
    pasan = sum(valor >= umbral for valor in range(1, 1001))
    assert (umbral, pasan) == (11.0, 990)


def test_el_ajuste_queda_registrado_con_las_huellas_usadas() -> None:
    # Arrange — FR-015: la prueba de no-fuga necesita saber con qué ventanas se ajustó
    ventanas = [_anomalia(float(valor)) for valor in range(1, 101)]
    # Act
    _, registro = ajustar_prefiltro(ventanas)
    # Assert
    assert registro.nombre == "prefiltro"
    assert registro.huellas == frozenset(huella_ventana(ventana) for ventana in ventanas)


@pytest.mark.parametrize(("perdidas", "aprobado"), [(110, False), (100, True)])
def test_paso_de_98_9_por_ciento_falla_y_99_pasa(perdidas: int, aprobado: bool) -> None:
    # Arrange — 10 000 anomalías; `perdidas` quedan bajo el umbral 5,0
    pico_a_pico = np.array([1.0] * perdidas + [9.0] * (10_000 - perdidas))
    # Act y Assert
    if aprobado:
        verificar_umbral(pico_a_pico, umbral=5.0)
    else:
        with pytest.raises(PrefiltroError, match="98,90 %"):
            verificar_umbral(pico_a_pico, umbral=5.0)


def test_ventana_bajo_el_umbral_es_normal_sin_llamar_al_modelo() -> None:
    # Arrange — un modelo espía que falla si lo llaman
    llamadas = []
    modelo = lambda vector: llamadas.append(vector) or ("Bache", 3, 0.99)  # noqa: E731
    # Act
    resultado = clasificar_con_prefiltro(_vector(0.5), umbral=1.2, modelo=modelo)
    # Assert
    assert (resultado, llamadas) == (("Normal", None, 1.0), [])


def test_ventana_en_el_umbral_si_pasa_al_modelo() -> None:
    # Arrange — «menor que el umbral» descarta: igual al umbral pasa
    modelo = lambda vector: ("Bache", 3, 0.99)  # noqa: E731
    # Act
    resultado = clasificar_con_prefiltro(_vector(1.2), umbral=1.2, modelo=modelo)
    # Assert
    assert resultado == ("Bache", 3, 0.99)
