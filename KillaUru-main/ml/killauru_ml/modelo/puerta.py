"""FR-028, data-model §9 — puerta de publicación: la versión se aprueba solo si cumple las 7 condiciones.

RNF-35, RNF-04, equivalencia, tamaño, clases = RN-01, acuerdo del etiquetado y partición. Cada condición es una
función pura sobre la evidencia; sin evidencia, falla. «≥» para mínimos y «<» estricto para latencia y tamaño.
"""
from __future__ import annotations

import pathlib
from dataclasses import dataclass, field

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.comun.config import leer_yaml
from killauru_ml.modelo.evidencia_puerta import (  # noqa: F401 (se reexportan: la puerta es la entrada pública)
    Acuerdo,
    Equivalencia,
    EvidenciaPuerta,
    MedicionCelular,
    VerificacionParticion,
)


@dataclass(frozen=True)
class CheckResultado:
    nombre: str
    aprobado: bool
    detalle: str = ""


@dataclass(frozen=True)
class ResultadoPuerta:
    aprobado: bool
    checks: list[CheckResultado] = field(default_factory=list)

    def resumen(self) -> str:
        lineas = ["APROBADA" if self.aprobado else "RECHAZADA"]
        lineas += [f"  [{'OK' if check.aprobado else 'FALLA'}] {check.nombre} {check.detalle}".rstrip()
                   for check in self.checks]
        return "\n".join(lineas)


@dataclass(frozen=True)
class UmbralesPuerta:
    """Valores de `config/puerta.yaml`; los de por defecto son los de la spec."""
    precision_minima: float = 0.90
    recall_minimo: float = 0.90
    severidad_minima: float = 0.80
    latencia_p95_maxima_ms: float = 50.0
    celulares_benchmark: int = 3
    ram_maxima_gb: float = 4.0
    antiguedad_maxima_anios: int = 4
    tamano_maximo_mb: float = 5.0
    tipo_igual_minimo: float = 0.99
    delta_confidence_p99_maximo: float = 0.05
    severidad_igual_minima: float = 0.98
    kappa_minimo: float = 0.80
    acuerdo_severidad_minimo: float = 0.90

    @staticmethod
    def leer(ruta: pathlib.Path) -> UmbralesPuerta:
        datos = leer_yaml(ruta)
        claves = {"PrecisionMinimaPorClase": "precision_minima", "RecallMinimoPorClase": "recall_minimo",
                  "SeveridadMasMenosUnoMinima": "severidad_minima", "LatenciaP95MaximaMs": "latencia_p95_maxima_ms",
                  "CelularesBenchmarkMinimos": "celulares_benchmark", "RamMaximaGb": "ram_maxima_gb",
                  "AntiguedadMaximaAnios": "antiguedad_maxima_anios", "TamanoMaximoModeloMb": "tamano_maximo_mb",
                  "TipoIgualMinimo": "tipo_igual_minimo", "DeltaConfidenceP99Maximo": "delta_confidence_p99_maximo",
                  "SeveridadIgualMinima": "severidad_igual_minima", "KappaMinimo": "kappa_minimo",
                  "AcuerdoSeveridadMinimo": "acuerdo_severidad_minimo"}
        return UmbralesPuerta(**{campo: datos[clave] for clave, campo in claves.items() if clave in datos})


def verificar_puerta(evidencia: EvidenciaPuerta, umbrales: UmbralesPuerta) -> ResultadoPuerta:
    condiciones = (_rnf35, _rnf04, _equivalencia, _tamano, _clases, _acuerdo, _particion)
    checks = [condicion(evidencia, umbrales) for condicion in condiciones]
    return ResultadoPuerta(aprobado=all(check.aprobado for check in checks), checks=checks)


def celulares_validos(evidencia: EvidenciaPuerta, umbrales: UmbralesPuerta) -> list[MedicionCelular]:
    """Gama media: RAM ≤ 4 GB y lanzado hace ≤ 4 años (un celular de 5 GB o de hace 5 años no cuenta)."""
    return [medicion for medicion in evidencia.benchmark if medicion.ram_gb <= umbrales.ram_maxima_gb
            and evidencia.anio_medicion - medicion.anio_lanzamiento <= umbrales.antiguedad_maxima_anios]


def _rnf35(evidencia: EvidenciaPuerta, umbrales: UmbralesPuerta) -> CheckResultado:
    fallos = [f"precision {clase}={evidencia.precision.get(clase, 0):.3f}" for clase in CLASES
              if evidencia.precision.get(clase, 0) < umbrales.precision_minima]
    fallos += [f"recall {clase}={evidencia.recall.get(clase, 0):.3f}" for clase in CLASES
               if evidencia.recall.get(clase, 0) < umbrales.recall_minimo]
    if evidencia.severidad_mas_menos_1 < umbrales.severidad_minima:
        fallos.append(f"severidad ±1={evidencia.severidad_mas_menos_1:.3f}")
    return CheckResultado("rnf35", not fallos, "; ".join(fallos))


def _rnf04(evidencia: EvidenciaPuerta, umbrales: UmbralesPuerta) -> CheckResultado:
    validos = celulares_validos(evidencia, umbrales)
    lentos = [medicion.alias for medicion in validos if medicion.latencia_p95_ms >= umbrales.latencia_p95_maxima_ms]
    aprobado = len(validos) >= umbrales.celulares_benchmark and not lentos
    return CheckResultado("rnf04", aprobado, f"{len(validos)} celulares de gama media; p95 ≥ 50 ms: {lentos}")


def _equivalencia(evidencia: EvidenciaPuerta, umbrales: UmbralesPuerta) -> CheckResultado:
    medida = evidencia.equivalencia
    if medida is None:
        return CheckResultado("equivalencia", False, "sin evidencia")
    aprobado = (medida.tipo_igual >= umbrales.tipo_igual_minimo
                and medida.delta_confidence_p99 <= umbrales.delta_confidence_p99_maximo
                and medida.severidad_igual >= umbrales.severidad_igual_minima)
    return CheckResultado("equivalencia", aprobado, str(medida))


def _tamano(evidencia: EvidenciaPuerta, umbrales: UmbralesPuerta) -> CheckResultado:
    megas = pathlib.Path(evidencia.ruta_tflite).stat().st_size / (1024 * 1024)
    return CheckResultado("tamano", megas < umbrales.tamano_maximo_mb, f"{megas:.3f} MB")


def _clases(evidencia: EvidenciaPuerta, _umbrales: UmbralesPuerta) -> CheckResultado:
    return CheckResultado("clases", tuple(evidencia.clases) == CLASES, str(evidencia.clases))


def _acuerdo(evidencia: EvidenciaPuerta, umbrales: UmbralesPuerta) -> CheckResultado:
    medido = evidencia.acuerdo
    if medido is None:
        return CheckResultado("acuerdo", False, "sin evidencia")
    aprobado = medido.kappa >= umbrales.kappa_minimo and medido.severidad_mas_menos_1 >= umbrales.acuerdo_severidad_minimo
    return CheckResultado("acuerdo", aprobado, str(medido))


def _particion(evidencia: EvidenciaPuerta, _umbrales: UmbralesPuerta) -> CheckResultado:
    if evidencia.particion is None:
        return CheckResultado("particion", False, "sin evidencia")
    return CheckResultado("particion", evidencia.particion.aprobada, evidencia.particion.detalle)
