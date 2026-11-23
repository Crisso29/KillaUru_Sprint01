"""Endpoints del panel (solo administradores): se comprueba que están protegidos, no su respuesta correcta.

Se generan desde el contrato: un endpoint de administrador que se agregue a un OpenAPI aparece aquí solo. Por cada
uno, sin sesión → 401 `token_invalido` y con la sesión de un conductor → 403 `sin_permiso` (RF-15, regla 401/403 de
`convenciones-api.md`). La autorización va antes que la validación, así que el cuerpo vacío no cambia el resultado.
Las respuestas correctas de estos endpoints las cubren las pruebas de integración del backend: entrar como
administrador exige el captcha Turnstile, que en local no se simula (decisión del autor, 2026-11-23).
"""
from __future__ import annotations

import re

from contratos import operaciones_de_administrador
from peticion import Peticion, carpeta
from scripts_postman import codigo_de_error, estado

_CON_CUERPO = ("POST", "PUT", "PATCH")


def panel_protegido() -> dict:
    peticiones = [peticion for metodo, plantilla in operaciones_de_administrador()
                  for peticion in _par(metodo, plantilla)]
    return carpeta("06 Panel protegido", "Cada endpoint de administrador rechaza a quien no lo es.", peticiones)


def _par(metodo: str, plantilla: str) -> list[Peticion]:
    ruta = re.sub(r"\{[^/]+\}", "{{idAjeno}}", plantilla)   # un id cualquiera: no debe llegar a buscarse
    cuerpo = {} if metodo in _CON_CUERPO else None
    return [
        Peticion(f"{metodo} {plantilla} sin sesión → 401", metodo, ruta,
                 estado(401) + codigo_de_error("token_invalido"), cuerpo=cuerpo),
        Peticion(f"{metodo} {plantilla} con un conductor → 403", metodo, ruta,
                 estado(403) + codigo_de_error("sin_permiso"), cuerpo=cuerpo, token="tokenConductor"),
    ]
