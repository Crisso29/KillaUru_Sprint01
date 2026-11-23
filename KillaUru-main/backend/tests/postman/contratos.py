"""Lee las operaciones de los contratos OpenAPI de la API (specs 001–007): la fuente de verdad de la colección.

La CDN (008) no es parte de esta API: tiene su propio contrato y sus pruebas en la app.
"""
from __future__ import annotations

import pathlib
import re

import yaml

RAIZ = pathlib.Path(__file__).resolve().parents[3]
_METODOS = ("get", "post", "put", "patch", "delete")
_ROL_ADMINISTRADOR = re.compile(r"^\s*Rol:\s*administrador\b", re.IGNORECASE)

Operacion = tuple[str, str]   # ("GET", "/api/v1/usuarios/{id}/rol")


def _contratos() -> list[dict]:
    archivos = sorted(RAIZ.glob("specs/00[1-7]-*/contracts/*.openapi.yaml"))
    return [yaml.safe_load(archivo.read_text(encoding="utf-8")) for archivo in archivos]


def _recorrer() -> list[tuple[str, str, dict]]:
    return [(metodo.upper(), ruta, operacion) for contrato in _contratos()
            for ruta, metodos in contrato["paths"].items()
            for metodo, operacion in metodos.items() if metodo in _METODOS]


def operaciones() -> list[Operacion]:
    """Todas las operaciones (método, plantilla de ruta) de la API."""
    return [(metodo, ruta) for metodo, ruta, _ in _recorrer()]


def operaciones_de_administrador() -> list[Operacion]:
    """Las que el contrato declara solo para administradores («Rol: administrador» al inicio de la descripción)."""
    return [(metodo, ruta) for metodo, ruta, operacion in _recorrer()
            if _ROL_ADMINISTRADOR.match(operacion.get("description", ""))]
