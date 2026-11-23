"""T042 / FR-016, FR-019, R-011 — entrenamiento reproducible con dos salidas. Patrón AAA."""
import numpy as np

from killauru_ml.comun.semillas import fijar_semillas
from killauru_ml.modelo.entrenar import Conjunto, entrenar, huella_pesos


def _conjunto(cantidad: int, semilla: int, severidad_normal: float = 0.0) -> Conjunto:
    """Clases 0, 1, 2 repetidas; severidad 1–5 en anomalías y `severidad_normal` en las Normales."""
    generador = np.random.default_rng(semilla)
    tipos = np.arange(cantidad) % 3
    severidad = np.where(tipos == 0, severidad_normal, generador.integers(1, 6, cantidad)).astype(np.float32)
    return Conjunto(generador.random((cantidad, 20)).astype(np.float32), tipos, severidad)


def _entrenar(entrenamiento: Conjunto, epocas: int = 3):
    fijar_semillas(2828)
    return entrenar(entrenamiento, _conjunto(30, semilla=8), epocas=epocas, lote=16)


def test_entrenar_devuelve_modelo_e_historia_con_las_dos_perdidas() -> None:
    # Arrange
    entrenamiento = _conjunto(90, semilla=7)
    # Act
    modelo, historia = _entrenar(entrenamiento)
    # Assert
    assert {"loss", "val_loss", "tipo_logits_loss", "severidad_loss"} <= set(historia.history)
    assert 1 <= len(historia.history["loss"]) <= 3
    assert set(modelo.predict(entrenamiento.X[:2], verbose=0)) == {"tipo_logits", "severidad"}


def test_dos_corridas_con_la_semilla_2828_dan_la_misma_huella_de_pesos() -> None:
    # Arrange — SC-003
    entrenamiento = _conjunto(90, semilla=7)
    # Act
    primera, _ = _entrenar(entrenamiento)
    segunda, _ = _entrenar(entrenamiento)
    # Assert
    assert huella_pesos(primera) == huella_pesos(segunda)


def test_la_perdida_de_severidad_ignora_las_ventanas_normales() -> None:
    # Arrange — mismas ventanas; solo cambia la severidad de las Normales (0 frente a 99, absurda)
    con_cero = _conjunto(90, semilla=7, severidad_normal=0.0)
    con_99 = _conjunto(90, semilla=7, severidad_normal=99.0)
    # Act
    modelo_cero, _ = _entrenar(con_cero)
    modelo_99, _ = _entrenar(con_99)
    # Assert — la máscara hace que esa etiqueta no influya en absoluto
    assert huella_pesos(modelo_cero) == huella_pesos(modelo_99)
