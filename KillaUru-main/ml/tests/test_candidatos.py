"""T024 / FR-008 — candidatos: ninguna pasada sobre un patrón queda fuera."""
import numpy as np
import pytest
from tests.sinteticos import crear_grabacion
from killauru_ml.lectura.grabacion import leer_grabacion


def _accel_con_pico(n: int, pico_idx: int, magnitud: float = 15.0) -> np.ndarray:
    """Señal vertical uniforme con un pico en pico_idx (en m/s²)."""
    señal = np.zeros(n)
    señal[pico_idx] = magnitud
    return señal


def test_pico_sobre_umbral_es_candidato(tmp_path) -> None:
    # Arrange
    from killauru_ml.etiquetado.candidatos import detectar_candidatos
    t_ns = np.arange(100) * 20_000_000  # 50 Hz
    a_v = _accel_con_pico(100, 50, magnitud=15.0)
    velocidad_ms = np.full(100, 10.0)
    km = np.arange(100) * 0.001
    # Act
    candidatos = detectar_candidatos(t_ns, a_v, velocidad_ms, km, umbral_ms2=2.0)
    # Assert — el pico en idx 50 debe ser candidato
    assert len(candidatos) >= 1
    assert any(c["t_pico_ns"] == t_ns[50] for c in candidatos)


def test_pico_bajo_umbral_no_es_candidato(tmp_path) -> None:
    # Arrange
    from killauru_ml.etiquetado.candidatos import detectar_candidatos
    t_ns = np.arange(100) * 20_000_000
    a_v = _accel_con_pico(100, 50, magnitud=1.0)  # < umbral 2.0
    velocidad_ms = np.full(100, 10.0)
    km = np.arange(100) * 0.001
    # Act
    candidatos = detectar_candidatos(t_ns, a_v, velocidad_ms, km, umbral_ms2=2.0)
    # Assert
    assert len(candidatos) == 0


def test_candidato_trae_hora_km_velocidad(tmp_path) -> None:
    # Arrange
    from killauru_ml.etiquetado.candidatos import detectar_candidatos
    t_ns = np.arange(200) * 20_000_000
    a_v = _accel_con_pico(200, 100, magnitud=12.0)
    velocidad_ms = np.full(200, 8.33)
    km = np.arange(200) * 0.001
    # Act
    candidatos = detectar_candidatos(t_ns, a_v, velocidad_ms, km, umbral_ms2=2.0)
    # Assert — cada fila debe tener los cuatro campos requeridos (FR-008)
    assert len(candidatos) >= 1
    campo = candidatos[0]
    assert "t_pico_ns" in campo
    assert "km" in campo
    assert "velocidad_ms" in campo
    assert "segundo_video" in campo


def test_multiples_picos_detecta_todos(tmp_path) -> None:
    # Arrange
    from killauru_ml.etiquetado.candidatos import detectar_candidatos
    t_ns = np.arange(300) * 20_000_000
    a_v = np.zeros(300)
    a_v[50] = 10.0
    a_v[150] = 12.0
    a_v[250] = 8.0
    velocidad_ms = np.full(300, 10.0)
    km = np.arange(300) * 0.001
    # Act
    candidatos = detectar_candidatos(t_ns, a_v, velocidad_ms, km, umbral_ms2=2.0)
    # Assert — los tres picos
    assert len(candidatos) == 3
