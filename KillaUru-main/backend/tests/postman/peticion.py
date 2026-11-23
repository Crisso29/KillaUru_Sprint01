"""Construye una petición de Postman (formato v2.1) con sus pruebas. Todas las peticiones pasan por aquí.

`token` elige qué sesión manda la petición: el nombre de una variable (`tokenConductor`) o None para ir sin sesión.
Los valores dinámicos (correo, contraseña, ids) son variables `{{…}}` que se llenan al correr, nunca en el archivo.
"""
from __future__ import annotations

import json
from dataclasses import dataclass, field


@dataclass(frozen=True)
class Peticion:
    nombre: str
    metodo: str
    ruta: str                                  # con variables de Postman: /api/v1/usuarios/{{idAjeno}}/rol
    pruebas: list[str]
    cuerpo: dict | str | None = None           # str = JSON ya armado en el script previo (p. ej. «{{lote}}»)
    token: str | None = None
    cabeceras: dict[str, str] = field(default_factory=dict)
    antes: list[str] = field(default_factory=list)   # script previo (pre-request)
    descripcion: str = ""

    def a_postman(self) -> dict:
        item = {"name": self.nombre, "request": self._request(), "event": [_evento("test", self.pruebas)]}
        if self.antes:
            item["event"].insert(0, _evento("prerequest", self.antes))
        return item

    def _request(self) -> dict:
        cabeceras = dict(self.cabeceras)
        if self.token:
            cabeceras["Authorization"] = f"Bearer {{{{{self.token}}}}}"
        request = {"method": self.metodo, "header": [{"key": clave, "value": valor} for clave, valor in cabeceras.items()],
                   "url": _url(self.ruta), "description": self.descripcion}
        if self.cuerpo is not None:
            texto = self.cuerpo if isinstance(self.cuerpo, str) else json.dumps(self.cuerpo, ensure_ascii=False, indent=2)
            request["body"] = {"mode": "raw", "raw": texto, "options": {"raw": {"language": "json"}}}
            request["header"].append({"key": "Content-Type", "value": "application/json"})
        return request


def carpeta(nombre: str, descripcion: str, peticiones: list[Peticion]) -> dict:
    return {"name": nombre, "description": descripcion, "item": [peticion.a_postman() for peticion in peticiones]}


def _evento(tipo: str, lineas: list[str]) -> dict:
    return {"listen": tipo, "script": {"type": "text/javascript", "exec": list(lineas)}}


def _url(ruta: str) -> dict:
    """Postman guarda la URL desarmada; `raw` es lo que se ve en la aplicación."""
    camino, _, consulta = ruta.partition("?")
    url = {"raw": "{{baseUrl}}" + ruta, "host": ["{{baseUrl}}"], "path": [parte for parte in camino.split("/") if parte]}
    if consulta:
        url["query"] = [dict(zip(("key", "value"), par.split("=", 1), strict=True)) for par in consulta.split("&")]
    return url
