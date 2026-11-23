"""FR-013, R-008, data-model §6 — la partición que el autor declara a mano en `config/particion.yaml`.

La unidad es el recorrido: los celulares de un mismo recorrido ven el mismo bache a la vez y deben ir juntos. Una
vez `congelada: true`, cualquier cambio exige una versión nueva del dataset; la huella permite detectarlo.
"""
from __future__ import annotations

import hashlib
import json
import pathlib
from dataclasses import asdict, dataclass

from killauru_ml.comun.config import leer_yaml

CONJUNTOS = ("entrenamiento", "validacion", "prueba")


@dataclass(frozen=True)
class DeclaracionParticion:
    version: str
    congelada: bool
    entrenamiento: list[str]
    validacion: list[str]
    prueba: list[str]
    celular_no_visto: str | None = None   # R-008: sus grabaciones van solo a prueba

    def recorridos(self, conjunto: str) -> list[str]:
        return list(getattr(self, conjunto))

    def huella(self) -> str:
        """SHA-256 del contenido en forma canónica: el orden de los recorridos dentro de un conjunto no importa."""
        contenido = asdict(self) | {conjunto: sorted(self.recorridos(conjunto)) for conjunto in CONJUNTOS}
        return hashlib.sha256(json.dumps(contenido, sort_keys=True).encode("utf-8")).hexdigest()


def leer_declaracion(ruta: pathlib.Path) -> DeclaracionParticion:
    """Lee `particion.yaml` (formato del data-model §6); falla si falta una clave."""
    datos = leer_yaml(ruta, ["datasetVersion", "congelada", *CONJUNTOS])
    return DeclaracionParticion(
        version=str(datos["datasetVersion"]),
        congelada=bool(datos["congelada"]),
        entrenamiento=list(datos["entrenamiento"] or []),
        validacion=list(datos["validacion"] or []),
        prueba=list(datos["prueba"] or []),
        celular_no_visto=datos.get("celularNoVisto"),
    )
