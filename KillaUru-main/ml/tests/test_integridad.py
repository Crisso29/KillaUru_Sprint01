"""T009 / FR-007 — pruebas de integridad de señal: frecuencia, huecos y GPS."""
import numpy as np
from killauru_ml.lectura.integridad import (
    EstadoIntegridad,
    MotivoNoApta,
    verificar_integridad,
)


def _crear_timestamps(hz: float, duracion_s: float) -> np.ndarray:
    """Timestamps uniformes en nanosegundos a la frecuencia dada."""
    dt_ns = int(1_000_000_000 / hz)
    n = int(duracion_s * hz)
    return np.arange(n) * dt_ns


def _crear_timestamps_con_hueco(hz: float, duracion_s: float, hueco_ms: float, pct: float) -> np.ndarray:
    """Timestamps con un hueco de hueco_ms ms en pct% del tiempo."""
    dt_ns = int(1_000_000_000 / hz)
    n = int(duracion_s * hz)
    t = np.arange(n) * dt_ns
    # insertar huecos periódicamente para alcanzar el porcentaje pedido
    hueco_ns = int(hueco_ms * 1_000_000)
    n_huecos = max(1, int((duracion_s * 1000 * pct / 100) / hueco_ms))
    paso = max(1, n // (n_huecos + 1))
    for k in range(1, n_huecos + 1):
        idx = k * paso
        if idx < n:
            t[idx:] += hueco_ns
    return t


# --- Frecuencia efectiva ---

def test_449_hz_es_no_apta() -> None:
    # Arrange
    t = _crear_timestamps(44.9, 10.0)
    # Act
    resultado = verificar_integridad(t, t.copy(), np.ones(len(t)) * 5.0)
    # Assert
    assert resultado.estado == EstadoIntegridad.NO_APTA
    assert MotivoNoApta.FRECUENCIA_BAJA in resultado.motivos


def test_450_hz_es_apta() -> None:
    # Arrange
    t = _crear_timestamps(45.0, 10.0)
    # Act
    resultado = verificar_integridad(t, t.copy(), np.ones(len(t)) * 5.0)
    # Assert
    assert resultado.estado == EstadoIntegridad.APTA


# --- Huecos ---

def test_hueco_de_200_ms_no_cuenta() -> None:
    # Arrange — hueco exacto de 200 ms (no supera el umbral de 201 ms)
    t = _crear_timestamps(50.0, 20.0)
    t[500:] += 200 * 1_000_000  # 200 ms exactos
    # Act
    resultado = verificar_integridad(t, t.copy(), np.ones(len(t)) * 5.0)
    # Assert — 200 ms no cuenta; no debe provocar rechazo
    assert resultado.estado == EstadoIntegridad.APTA


def test_hueco_de_201_ms_cuenta() -> None:
    # Arrange — hueco de 201 ms sí cuenta
    t = _crear_timestamps(50.0, 100.0)
    # 2 huecos de 201 ms sobre 100 s = 0.402 s / 100 s = 0.4 % (< 1 %)
    t[500:] += 201 * 1_000_000
    t[2500:] += 201 * 1_000_000
    # Act
    resultado = verificar_integridad(t, t.copy(), np.ones(len(t)) * 5.0)
    # Assert — huecos cuentan, aunque aún < 1 %
    assert resultado.huecos_pct > 0.0


def test_huecos_sobre_1_pct_es_no_apta() -> None:
    # Arrange — huecos > 1 % del tiempo
    t = _crear_timestamps_con_hueco(50.0, 60.0, 250.0, 1.01)
    # Act
    resultado = verificar_integridad(t, t.copy(), np.ones(len(t)) * 5.0)
    # Assert
    assert resultado.estado == EstadoIntegridad.NO_APTA
    assert MotivoNoApta.HUECOS in resultado.motivos


def test_huecos_en_1_pct_es_apta() -> None:
    # Arrange — exactamente 1.00 % pasa
    t = _crear_timestamps_con_hueco(50.0, 100.0, 250.0, 1.00)
    # Act
    resultado = verificar_integridad(t, t.copy(), np.ones(len(t)) * 5.0)
    # Assert — 1.00 % exacto pasa (≤ 1 %)
    assert resultado.estado == EstadoIntegridad.APTA


# --- GPS ---

def test_gps_mala_precision_en_mas_de_20_pct_es_sin_gps() -> None:
    # Arrange — 25 % con precisión > 20 m
    t_gps = np.arange(100) * 1_000_000_000
    precision = np.where(np.arange(100) < 75, 5.0, 25.0)  # 25 % mala
    # Act
    resultado = verificar_integridad(
        _crear_timestamps(50.0, 100.0),
        _crear_timestamps(50.0, 100.0),
        t_gps=t_gps,
        precision_m=precision,
    )
    # Assert
    assert resultado.estado == EstadoIntegridad.NO_APTA
    assert MotivoNoApta.SIN_GPS in resultado.motivos
