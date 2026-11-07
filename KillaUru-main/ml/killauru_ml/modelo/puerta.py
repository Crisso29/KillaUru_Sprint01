"""FR-028, puerta.yaml — verificaciones antes de publicar el modelo."""
from __future__ import annotations

import pathlib
from dataclasses import dataclass, field


@dataclass
class CheckResultado:
    nombre: str
    valor: float
    umbral: float
    aprobado: bool
    detalle: str = ""


@dataclass
class ResultadoPuerta:
    aprobado: bool
    checks: list[CheckResultado] = field(default_factory=list)

    def resumen(self) -> str:
        lineas = ["APROBADA" if self.aprobado else "RECHAZADA"]
        for c in self.checks:
            estado = "OK" if c.aprobado else "FALLA"
            lineas.append(f"  [{estado}] {c.nombre}: {c.valor:.4f} (umbral {c.umbral})")
        return "\n".join(lineas)


def verificar_tamano(ruta: pathlib.Path, max_mb: float = 5.0) -> CheckResultado:
    """Verifica que el .tflite no supere el tamaño máximo."""
    tamano_mb = ruta.stat().st_size / (1024 * 1024)
    return CheckResultado(
        nombre="tamano_mb",
        valor=tamano_mb,
        umbral=max_mb,
        aprobado=tamano_mb < max_mb,
        detalle="" if tamano_mb < max_mb else f"{tamano_mb:.2f} MB > {max_mb} MB",
    )


def verificar_metricas_puerta(
    metricas: dict[str, float],
    precision_min: float = 0.90,
    recall_min: float = 0.90,
) -> CheckResultado:
    """Verifica precisión y recall por clase."""
    fallos = []
    min_val = 1.0
    for clave, val in metricas.items():
        min_val = min(min_val, val)
        if clave.startswith("precision_") and val < precision_min:
            fallos.append(f"{clave}={val:.3f}<{precision_min}")
        elif clave.startswith("recall_") and val < recall_min:
            fallos.append(f"{clave}={val:.3f}<{recall_min}")
    return CheckResultado(
        nombre="metricas",
        valor=min_val,
        umbral=min(precision_min, recall_min),
        aprobado=len(fallos) == 0,
        detalle="; ".join(fallos),
    )


def verificar_severidad_puerta(
    sev_true: list[int],
    sev_pred: list[int],
    min_pct: float = 0.80,
) -> CheckResultado:
    """Verifica acuerdo de severidad ±1 (RNF-35)."""
    if not sev_true:
        return CheckResultado("severidad", 1.0, min_pct, True, "sin pares")
    dentro = sum(1 for t, p in zip(sev_true, sev_pred) if abs(t - p) <= 1)
    pct = dentro / len(sev_true)
    return CheckResultado(
        nombre="severidad_mas_menos_uno",
        valor=pct,
        umbral=min_pct,
        aprobado=pct >= min_pct,
        detalle="" if pct >= min_pct else f"{pct:.3f} < {min_pct}",
    )


def verificar_puerta_modelo(
    ruta_tflite: pathlib.Path,
    metricas: dict[str, float],
    sev_true: list[int] | None = None,
    sev_pred: list[int] | None = None,
    max_mb: float = 5.0,
    precision_min: float = 0.90,
    recall_min: float = 0.90,
    sev_min: float = 0.80,
) -> ResultadoPuerta:
    """Ejecuta todos los checks de publicación y retorna ResultadoPuerta."""
    checks = [
        verificar_tamano(ruta_tflite, max_mb),
        verificar_metricas_puerta(metricas, precision_min, recall_min),
    ]
    if sev_true and sev_pred:
        checks.append(verificar_severidad_puerta(sev_true, sev_pred, sev_min))
    return ResultadoPuerta(
        aprobado=all(c.aprobado for c in checks),
        checks=checks,
    )
