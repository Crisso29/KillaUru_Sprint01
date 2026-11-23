"""FR-078: valida datos contra los esquemas de `specs/008-app-android/contracts/cdn.openapi.yaml`, la fuente única.

Lo usan las pruebas y `verificar_datos.py` antes de publicar: nada sale a la CDN si no cumple el contrato que lee la app.
"""
from functools import lru_cache
from pathlib import Path

import yaml
from jsonschema import Draft202012Validator

CONTRATO = Path(__file__).resolve().parents[2] / 'specs/008-app-android/contracts/cdn.openapi.yaml'
PREFIJO_REF = '#/components/schemas/'


@lru_cache(maxsize=1)
def _esquemas():
    return yaml.safe_load(CONTRATO.read_text(encoding='utf8'))['components']['schemas']


def _resolver(nodo):
    """Sustituye cada `$ref` interno por el esquema al que apunta (el contrato no tiene referencias circulares)."""
    if isinstance(nodo, dict):
        if '$ref' in nodo:
            return _resolver(_esquemas()[nodo['$ref'].removeprefix(PREFIJO_REF)])
        return {clave: _resolver(valor) for clave, valor in nodo.items()}
    if isinstance(nodo, list):
        return [_resolver(valor) for valor in nodo]
    return nodo


def esquema(nombre):
    return _resolver(_esquemas()[nombre])


def errores(nombre, datos):
    """Mensajes legibles de lo que no cumple; lista vacía si todo está bien."""
    validador = Draft202012Validator(esquema(nombre), format_checker=Draft202012Validator.FORMAT_CHECKER)
    return [f"{'/'.join(map(str, e.absolute_path)) or '(raíz)'}: {e.message}" for e in validador.iter_errors(datos)]
