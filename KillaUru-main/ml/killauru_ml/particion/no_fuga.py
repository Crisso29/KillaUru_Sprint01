"""FR-013, FR-015, R-008 — prueba de no-fuga: el conjunto de prueba no influye en nada de lo entrenado.

Cuatro comprobaciones:
1. Ninguna ventana de prueba copiada en entrenamiento (por su huella).
2. Ningún recorrido en dos conjuntos (salvo las ventanas del «celular no visto», que van solo a prueba).
3. Ningún par (recorrido, celular) en dos conjuntos.
4. Normalización, prefiltro y temperatura ajustados sin ventanas de prueba: quien ajusta un parámetro guarda un
   `RegistroAjuste` con las huellas que usó, y aquí se cruzan con las de prueba.
"""
from __future__ import annotations

import hashlib
from collections.abc import Sequence
from dataclasses import dataclass
from itertools import combinations

import numpy as np

from killauru_ml.ventanas.ventaneo import VentanaDataset


class FugaError(ValueError):
    pass


@dataclass(frozen=True)
class RegistroAjuste:
    """Qué ventanas se usaron para ajustar un parámetro (`normalizacion`, `prefiltro`, `temperatura`…)."""
    nombre: str
    huellas: frozenset[str]


def huella_ventana(ventana: VentanaDataset) -> str:
    """data-model §5: SHA-256 de la señal + la grabación (recorrido y celular) + la posición (hora de inicio)."""
    sha = hashlib.sha256(np.ascontiguousarray(ventana.a_v, dtype=np.float32).tobytes())
    sha.update(f"|{ventana.recorrido}|{ventana.celular}|{ventana.t_inicio_ns}".encode())
    return sha.hexdigest()


def verificar_no_fuga(particion: dict[str, list[VentanaDataset]], ajustes: Sequence[RegistroAjuste] = (),
                      celular_no_visto: str | None = None) -> None:
    """Lanza FugaError con la primera fuga, de la más concreta (una ventana copiada) a la más general."""
    prueba = {huella_ventana(ventana): ventana.recorrido for ventana in particion.get("prueba", [])}
    huellas_prueba = set(prueba)
    copiadas = huellas_prueba & {huella_ventana(ventana) for ventana in particion.get("entrenamiento", [])}
    if copiadas:
        recorridos = ", ".join(sorted({prueba[huella] for huella in copiadas}))
        raise FugaError(f"{len(copiadas)} huella(s) de prueba también en entrenamiento (recorridos {recorridos})")
    for (nombre1, ventanas1), (nombre2, ventanas2) in combinations(particion.items(), 2):
        _sin_cruce(nombre1, nombre2, _recorridos(ventanas1, celular_no_visto), _recorridos(ventanas2, celular_no_visto),
                   "recorridos")
        _sin_cruce(nombre1, nombre2, _pares(ventanas1), _pares(ventanas2), "pares (recorrido, celular)")
    for ajuste in ajustes:
        if ajuste.huellas & huellas_prueba:
            raise FugaError(f"'{ajuste.nombre}' se ajustó con {len(ajuste.huellas & huellas_prueba)} ventana(s) de "
                            "prueba; FR-015 solo permite entrenamiento y validación")


def _recorridos(ventanas: list[VentanaDataset], celular_no_visto: str | None) -> set[str]:
    return {ventana.recorrido for ventana in ventanas if celular_no_visto is None or ventana.celular != celular_no_visto}


def _pares(ventanas: list[VentanaDataset]) -> set[str]:
    return {f"{ventana.recorrido}/{ventana.celular}" for ventana in ventanas}


def _sin_cruce(nombre1: str, nombre2: str, elementos1: set[str], elementos2: set[str], que: str) -> None:
    cruce = elementos1 & elementos2
    if cruce:
        raise FugaError(f"Fuga entre '{nombre1}' y '{nombre2}': {que} {', '.join(sorted(cruce))}")
