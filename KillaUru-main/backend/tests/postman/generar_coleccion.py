"""Genera la colección de Postman de la API de KillaUru y su entorno local.

La colección es un archivo generado: no se edita a mano en Postman, porque `test_coleccion.py` exige que coincida con
lo que produce este script (así no se desincroniza de los contratos). Para cambiarla, editar los `escenarios_*.py` y
ejecutar `python generar_coleccion.py` desde esta carpeta.
"""
from __future__ import annotations

import json
import pathlib

from escenarios_datos import ingesta, mapa, punto_del_corredor
from escenarios_identidad import cierre, identidad_publica, mi_cuenta, plataforma
from escenarios_protegidos import panel_protegido
from scripts_postman import COMUNES

CARPETA = pathlib.Path(__file__).resolve().parent
_ESQUEMA = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
# Variables que se llenan al correr y solo viven en memoria (tokens, correo, contraseña, lote…)
_EN_MEMORIA = ("correo", "contrasena", "tokenConductor", "tokenRenovacion", "versionPolitica", "versionTerminos",
               "seudonimo", "lote", "loteId", "etagMapa")


def coleccion() -> dict:
    variables = [{"key": nombre, "value": ""} for nombre in _EN_MEMORIA]
    variables += [{"key": "puntoCorredor", "value": json.dumps(punto_del_corredor())},
                  {"key": "idAjeno", "value": "00000000-0000-7000-8000-000000000001"}]
    return {
        "info": {"name": "KillaUru API", "schema": _ESQUEMA,
                 "description": "Recorrido de un conductor por la API y protección del panel. Generada desde los "
                                "contratos OpenAPI de las specs 001–007 (ver README.md de esta carpeta)."},
        "event": [{"listen": "test", "script": {"type": "text/javascript", "exec": COMUNES}}],
        "variable": variables,
        "item": [plataforma(), identidad_publica(), mi_cuenta(), ingesta(), mapa(), panel_protegido(), cierre()],
    }


def entorno_local() -> dict:
    """Solo la URL: lo demás se genera al correr. Para la VM de pruebas, otro entorno con su URL."""
    return {"name": "KillaUru local", "values": [{"key": "baseUrl", "value": "http://localhost:18080", "enabled": True}]}


def _escribir(nombre: str, contenido: dict) -> None:
    texto = json.dumps(contenido, ensure_ascii=False, indent=2) + "\n"
    (CARPETA / nombre).write_text(texto, encoding="utf-8", newline="\n")


if __name__ == "__main__":
    _escribir("KillaUru.postman_collection.json", coleccion())
    _escribir("local.postman_environment.json", entorno_local())
    print("Colección y entorno generados en", CARPETA)
