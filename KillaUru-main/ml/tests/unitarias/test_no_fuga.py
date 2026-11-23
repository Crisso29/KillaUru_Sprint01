"""T034 / FR-013 — verificación de no fuga entre conjuntos."""
import numpy as np
import pytest

from killauru_ml.particion.no_fuga import (
    FugaError,
    RegistroAjuste,
    huella_ventana,
    verificar_no_fuga,
)
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


# --- T034 / FR-015: pares (recorrido, celular), huellas y parámetros ajustados ---

def _de(recorrido: str, celular: str, inicio_ns: int = 0, senal: float = 0.0) -> VentanaDataset:
    return VentanaDataset(a_v=np.full(25, senal, dtype=np.float32), t_inicio_ns=inicio_ns, t_fin_ns=inicio_ns + 1,
                          recorrido=recorrido, celular=celular)


def _limpia() -> dict[str, list[VentanaDataset]]:
    return {"entrenamiento": [_de("R01", "C1"), _de("R01", "C2", 500)], "validacion": [_de("R02", "C1")],
            "prueba": [_de("R03", "C1"), _de("R03", "C2", 500)]}


@pytest.mark.parametrize("parametro", ["normalizacion", "prefiltro", "temperatura"])
def test_parametro_ajustado_con_una_ventana_de_prueba_es_fuga(parametro: str) -> None:
    # Arrange — el ajuste usó entrenamiento y, por error, una ventana de prueba
    particion = _limpia()
    usadas = [huella_ventana(ventana) for ventana in particion["entrenamiento"] + particion["prueba"][:1]]
    ajuste = RegistroAjuste(parametro, frozenset(usadas))
    # Act y Assert
    with pytest.raises(FugaError, match=parametro):
        verificar_no_fuga(particion, ajustes=[ajuste])


def test_parametros_ajustados_con_entrenamiento_y_validacion_pasan() -> None:
    # Arrange — FR-015 permite entrenamiento y validación
    particion = _limpia()
    usadas = frozenset(huella_ventana(ventana) for ventana in particion["entrenamiento"] + particion["validacion"])
    # Act
    verificar_no_fuga(particion, ajustes=[RegistroAjuste(nombre, usadas) for nombre in ("normalizacion", "temperatura")])
    # Assert — no lanzó
    assert usadas


def test_la_misma_ventana_en_entrenamiento_y_prueba_es_fuga() -> None:
    # Arrange — copia exacta de una ventana de prueba colada en entrenamiento (p. ej. un archivo duplicado)
    particion = _limpia()
    particion["entrenamiento"].append(particion["prueba"][0])
    # Act y Assert
    with pytest.raises(FugaError, match="huella"):
        verificar_no_fuga(particion)


def test_celular_no_visto_comparte_recorrido_pero_no_el_par_recorrido_celular() -> None:
    # Arrange — R-008: C4 graba R01 pero todo lo suyo va a prueba; el par (R01, C4) solo está en prueba
    particion = _limpia()
    particion["prueba"].append(_de("R01", "C4"))
    # Act
    verificar_no_fuga(particion, celular_no_visto="C4")
    # Assert — y si el mismo par aparece en entrenamiento, sí es fuga
    particion["entrenamiento"].append(_de("R01", "C4", 900))
    with pytest.raises(FugaError, match="R01.*C4"):
        verificar_no_fuga(particion, celular_no_visto="C4")


def test_la_huella_cambia_con_la_senal_la_grabacion_y_la_hora() -> None:
    # Arrange
    base = _de("R01", "C1")
    variantes = [_de("R01", "C1", senal=0.1), _de("R01", "C2"), _de("R02", "C1"), _de("R01", "C1", inicio_ns=20)]
    # Act
    huellas = {huella_ventana(ventana) for ventana in [base, *variantes]}
    # Assert — data-model §5: SHA-256 de la señal + grabación + posición
    assert len(huellas) == 5 and huella_ventana(base) == huella_ventana(_de("R01", "C1"))
