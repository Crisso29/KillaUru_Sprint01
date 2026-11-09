"""FR-016 — lectura de YAMLs de configuración con validación de claves requeridas."""
from __future__ import annotations

import pathlib
from typing import Any

import yaml


class ConfigError(ValueError):
    """Clave requerida ausente en el YAML de configuración."""


def leer_yaml(
    ruta: pathlib.Path,
    claves_requeridas: list[str] | None = None,
) -> dict[str, Any]:
    """Lee un YAML y valida que estén todas las claves requeridas."""
    if not ruta.exists():
        raise FileNotFoundError(f"No se encontró el archivo de configuración: {ruta}")

    with ruta.open(encoding="utf-8") as f:
        datos: dict[str, Any] = yaml.safe_load(f) or {}

    if claves_requeridas:
        faltantes = [k for k in claves_requeridas if k not in datos]
        if faltantes:
            raise ConfigError(
                f"Claves requeridas ausentes en {ruta.name}: {', '.join(faltantes)}"
            )

    return datos
