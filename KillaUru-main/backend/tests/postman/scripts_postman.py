"""Fragmentos de JavaScript de Postman que reutilizan las peticiones: cada comprobación se escribe una sola vez.

Cada función devuelve líneas de script. Los nombres de las pruebas están en español porque son lo que se lee en el
informe de Newman.
"""
from __future__ import annotations

# Se ejecuta después de CADA petición (nivel de colección): reglas transversales de la API.
COMUNES = [
    "// FR-013: HSTS y nosniff en toda respuesta",
    "pm.test('cabeceras de seguridad (HSTS y nosniff)', () => {",
    "  pm.expect(pm.response.headers.get('X-Content-Type-Options')).to.eql('nosniff');",
    "  pm.expect(pm.response.headers.get('Strict-Transport-Security')).to.include('max-age=');",
    "});",
    "// Constitución IX: todo error es application/problem+json con código estable y correlacionId",
    "if (pm.response.code >= 400) {",
    "  pm.test('el error es un problema RFC 9457 con codigo y correlacionId', () => {",
    "    pm.expect(pm.response.headers.get('Content-Type')).to.include('application/problem+json');",
    "    const problema = pm.response.json();",
    "    pm.expect(problema).to.have.property('codigo').that.is.a('string');",
    "    pm.expect(problema).to.have.property('correlacionId').that.is.a('string');",
    "  });",
    "}",
    "// No es una prueba de carga (eso es k6): solo detecta una petición anormalmente lenta",
    "pm.test('responde en menos de 2 s', () => pm.expect(pm.response.responseTime).to.be.below(2000));",
]


def estado(codigo: int) -> list[str]:
    return [f"pm.test('responde {codigo}', () => pm.response.to.have.status({codigo}));"]


def codigo_de_error(codigo: str) -> list[str]:
    return [f"pm.test('código de error «{codigo}»', () => pm.expect(pm.response.json().codigo).to.eql('{codigo}'));"]


def campos(*nombres: str) -> list[str]:
    lista = ", ".join(f"'{nombre}'" for nombre in nombres)
    return [f"pm.test('trae {', '.join(nombres)}', () => pm.expect(pm.response.json()).to.include.keys({lista}));"]


def guardar(variable: str, expresion: str) -> list[str]:
    """Guarda un valor de la respuesta para las peticiones siguientes (solo en memoria de la ejecución)."""
    return [f"if (pm.response.code < 300) pm.collectionVariables.set('{variable}', {expresion});"]


def no_cachear() -> list[str]:
    """ADR-006, FR-013: lo de un usuario autenticado nunca queda en cachés intermedias."""
    return ["pm.test('no se guarda en caché (private, no-store)', () =>",
            "  pm.expect(pm.response.headers.get('Cache-Control')).to.include('no-store'));"]


def lote_aceptado(cantidad: int) -> list[str]:
    return [f"pm.test('los {cantidad} eventos aceptados, sin duplicados ni rechazos', () => {{",
            "  const r = pm.response.json();",
            f"  pm.expect(r.aceptados).to.have.lengthOf({cantidad});",
            "  pm.expect(r.duplicados).to.be.empty; pm.expect(r.rechazados).to.be.empty;",
            "});"]


def lote_duplicado(cantidad: int) -> list[str]:
    return [f"pm.test('reenviar el mismo lote: {cantidad} duplicados, nada nuevo', () => {{",
            "  const r = pm.response.json();",
            f"  pm.expect(r.duplicados).to.have.lengthOf({cantidad}); pm.expect(r.aceptados).to.be.empty;",
            "});"]
