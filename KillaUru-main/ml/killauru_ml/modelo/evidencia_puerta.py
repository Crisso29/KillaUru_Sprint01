"""FR-028, data-model §7 — lo que la puerta necesita saber de una versión candidata.

Cada etapa del pipeline aporta su parte (métricas, equivalencia, benchmark, acuerdo, partición). `todo` la guarda en
`modelos/<versión>/evidencia-puerta.json` y la etapa `puerta` la lee. Un campo en `None` (o un benchmark vacío)
significa «aún no medido», y la puerta lo trata como fallo.
"""
from __future__ import annotations

import json
import pathlib
from dataclasses import asdict, dataclass, field


@dataclass(frozen=True)
class MedicionCelular:
    """Benchmark de un celular (RNF-04): solo cuenta si es de gama media."""
    alias: str
    ram_gb: float
    anio_lanzamiento: int
    latencia_p95_ms: float


@dataclass(frozen=True)
class Equivalencia:
    """R-014: el `.tflite` frente al modelo de Python sobre prueba."""
    tipo_igual: float
    delta_confidence_p99: float
    severidad_igual: float


@dataclass(frozen=True)
class Acuerdo:
    """FR-011: doble etiquetado del 10 %."""
    kappa: float
    severidad_mas_menos_1: float


@dataclass(frozen=True)
class VerificacionParticion:
    """FR-014 y no-fuga: resultado de `verificar_requisitos` y `verificar_no_fuga`."""
    aprobada: bool
    detalle: str


@dataclass(frozen=True)
class EvidenciaPuerta:
    version: str
    dataset_version: str
    version_minima_app: str
    ruta_tflite: str
    temperatura: float
    umbral_prefiltro: float
    clases: list[str]
    precision: dict[str, float]
    recall: dict[str, float]
    severidad_mas_menos_1: float
    anio_medicion: int
    benchmark: list[MedicionCelular] = field(default_factory=list)
    equivalencia: Equivalencia | None = None
    acuerdo: Acuerdo | None = None
    particion: VerificacionParticion | None = None

    def guardar(self, ruta: pathlib.Path) -> None:
        ruta.parent.mkdir(parents=True, exist_ok=True)
        ruta.write_text(json.dumps(asdict(self), indent=2, ensure_ascii=False), encoding="utf-8")

    @staticmethod
    def leer(ruta: pathlib.Path) -> EvidenciaPuerta:
        datos = json.loads(ruta.read_text(encoding="utf-8"))
        anidados = {"equivalencia": Equivalencia, "acuerdo": Acuerdo, "particion": VerificacionParticion}
        for clave, tipo in anidados.items():
            datos[clave] = tipo(**datos[clave]) if datos.get(clave) else None
        datos["benchmark"] = [MedicionCelular(**medicion) for medicion in datos.get("benchmark", [])]
        return EvidenciaPuerta(**datos)
