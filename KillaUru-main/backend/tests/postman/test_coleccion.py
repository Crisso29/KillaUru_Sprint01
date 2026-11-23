"""La colección de Postman es coherente con los contratos OpenAPI de la API (specs 001–007). Patrón AAA.

Prueba de contrato: no llama a la API. Comprueba que cada operación del contrato tiene su petición, que la colección
no inventa rutas, que cada petición verifica algo, que cada endpoint de administrador se prueba protegido (401 sin
sesión, 403 con un conductor), que el archivo es el que produce el generador y que no guarda secretos.
Ejecutar desde `backend/tests/postman`: `python -m unittest -v`.
"""
import json
import pathlib
import re
import unittest

from contratos import operaciones, operaciones_de_administrador
from generar_coleccion import CARPETA, coleccion, entorno_local

COLECCION = json.loads((CARPETA / "KillaUru.postman_collection.json").read_text(encoding="utf-8"))
ENTORNO = json.loads((CARPETA / "local.postman_environment.json").read_text(encoding="utf-8"))


def _peticiones(nodo: dict) -> list[dict]:
    """Todas las peticiones de la colección, recorriendo las carpetas."""
    if "request" in nodo:
        return [nodo]
    return [peticion for hijo in nodo.get("item", []) for peticion in _peticiones(hijo)]


def _ruta(peticion: dict) -> str:
    """`{{baseUrl}}/api/v1/usuarios/{{idAjeno}}/rol?x=1` → `/api/v1/usuarios/{{idAjeno}}/rol`."""
    return peticion["request"]["url"]["raw"].replace("{{baseUrl}}", "").split("?")[0]


def _coincide(ruta: str, plantilla: str) -> bool:
    """`/api/v1/usuarios/{id}/rol` acepta cualquier segmento (variable de Postman o valor) en `{id}`."""
    patron = re.sub(r"\\\{[^/]+\\\}", "[^/]+", re.escape(plantilla))
    return re.fullmatch(patron, ruta) is not None


def _pruebas(peticion: dict) -> str:
    return "\n".join(linea for evento in peticion.get("event", []) if evento["listen"] == "test"
                     for linea in evento["script"]["exec"])


PETICIONES = _peticiones(COLECCION)


class CoberturaDelContrato(unittest.TestCase):
    def test_cada_operacion_del_contrato_tiene_al_menos_una_peticion(self) -> None:
        # Arrange
        contrato = operaciones()
        # Act
        sin_peticion = [f"{metodo} {plantilla}" for metodo, plantilla in contrato
                        if not any(p["request"]["method"] == metodo and _coincide(_ruta(p), plantilla) for p in PETICIONES)]
        # Assert
        self.assertGreaterEqual(len(contrato), 32)
        self.assertEqual(sin_peticion, [])

    def test_ninguna_peticion_usa_una_ruta_que_no_este_en_el_contrato(self) -> None:
        # Arrange
        contrato = operaciones()
        # Act
        inventadas = [f"{p['request']['method']} {_ruta(p)}" for p in PETICIONES
                      if not any(p["request"]["method"] == metodo and _coincide(_ruta(p), plantilla)
                                 for metodo, plantilla in contrato)]
        # Assert
        self.assertEqual(inventadas, [])

    def test_cada_endpoint_de_administrador_se_prueba_sin_sesion_y_con_un_conductor(self) -> None:
        # Arrange — RF-15: un conductor no debe poder usar nada del panel
        protegidas = operaciones_de_administrador()
        # Act
        faltan = [f"{metodo} {plantilla} → {estado}" for metodo, plantilla in protegidas for estado in ("401", "403")
                  if not any(p["request"]["method"] == metodo and _coincide(_ruta(p), plantilla)
                             and f"pm.response.to.have.status({estado})" in _pruebas(p) for p in PETICIONES)]
        # Assert
        self.assertEqual(len(protegidas), 16)
        self.assertEqual(faltan, [])


class CalidadDeLaColeccion(unittest.TestCase):
    def test_cada_peticion_comprueba_al_menos_su_codigo_de_estado(self) -> None:
        # Arrange y Act
        sin_prueba = [p["name"] for p in PETICIONES if "pm.response.to.have.status(" not in _pruebas(p)]
        # Assert
        self.assertEqual(sin_prueba, [])

    def test_el_archivo_es_el_que_produce_el_generador(self) -> None:
        # Arrange — la colección no se edita a mano: se regenera con `python generar_coleccion.py`
        generada = coleccion()
        # Act y Assert
        self.assertEqual(COLECCION, generada)
        self.assertEqual(ENTORNO, entorno_local())

    def test_no_guarda_tokens_ni_contrasenas(self) -> None:
        # Arrange
        texto = json.dumps(COLECCION) + json.dumps(ENTORNO)
        # Act
        con_valor = [variable["key"] for variable in ENTORNO["values"]
                     if variable["key"] != "baseUrl" and variable["value"]]
        # Assert — los tokens y la contraseña se generan al correr y viven solo en memoria
        self.assertNotRegex(texto, r"Bearer ey|\"contrasena\": \"[^{]")
        self.assertEqual(con_valor, [])


if __name__ == "__main__":
    unittest.main()
