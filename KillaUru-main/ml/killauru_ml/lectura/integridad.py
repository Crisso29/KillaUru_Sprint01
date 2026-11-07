"""FR-007 — verificación de integridad de señal: frecuencia, huecos y GPS."""
from __future__ import annotations

import enum
from dataclasses import dataclass, field

import numpy as np

_HZ_MINIMO = 45.0
_HUECO_MIN_NS = 201 * 1_000_000      # 201 ms en nanosegundos (> 200 ms cuenta)
_HUECOS_MAX_PCT = 1.0                 # ≤ 1 % del tiempo en huecos
_GPS_PRECISION_MAX_M = 20.0           # > 20 m = mala precisión
_GPS_MALA_MAX_PCT = 0.20              # > 20 % mala → SinGps


class MotivoNoApta(str, enum.Enum):
    FRECUENCIA_BAJA = "FrecuenciaBaja"
    HUECOS = "Huecos"
    SIN_GPS = "SinGps"
    META_INCOMPLETA = "MetaIncompleta"
    SOPORTE_INESTABLE = "SoporteInestable"


class EstadoIntegridad(str, enum.Enum):
    APTA = "Apta"
    NO_APTA = "NoApta"


@dataclass
class ResultadoIntegridad:
    estado: EstadoIntegridad
    motivos: list[MotivoNoApta] = field(default_factory=list)
    hz_efectivo: float = 0.0
    huecos_pct: float = 0.0


def _calcular_hz(t_ns: np.ndarray) -> float:
    if len(t_ns) < 2:
        return 0.0
    duracion_s = (t_ns[-1] - t_ns[0]) / 1e9
    return (len(t_ns) - 1) / duracion_s if duracion_s > 0 else 0.0


def _calcular_huecos_pct(t_ns: np.ndarray) -> float:
    if len(t_ns) < 2:
        return 0.0
    diffs = np.diff(t_ns)
    duracion_total_ns = t_ns[-1] - t_ns[0]
    tiempo_en_huecos_ns = diffs[diffs > _HUECO_MIN_NS].sum()
    return 100.0 * float(tiempo_en_huecos_ns) / duracion_total_ns if duracion_total_ns > 0 else 0.0


def verificar_integridad(
    t_accel_ns: np.ndarray,
    t_giro_ns: np.ndarray,
    precision_m: np.ndarray | None = None,
    t_gps: np.ndarray | None = None,
) -> ResultadoIntegridad:
    """Verifica frecuencia, huecos y calidad GPS de una grabación."""
    motivos: list[MotivoNoApta] = []

    hz = _calcular_hz(t_accel_ns)
    huecos_pct = _calcular_huecos_pct(t_accel_ns)

    if hz < _HZ_MINIMO:
        motivos.append(MotivoNoApta.FRECUENCIA_BAJA)
    if huecos_pct > _HUECOS_MAX_PCT:
        motivos.append(MotivoNoApta.HUECOS)

    if precision_m is not None and len(precision_m) > 0:
        mala_pct = float(np.mean(precision_m > _GPS_PRECISION_MAX_M))
        if mala_pct > _GPS_MALA_MAX_PCT:
            motivos.append(MotivoNoApta.SIN_GPS)

    estado = EstadoIntegridad.NO_APTA if motivos else EstadoIntegridad.APTA
    return ResultadoIntegridad(
        estado=estado,
        motivos=motivos,
        hz_efectivo=hz,
        huecos_pct=huecos_pct,
    )
