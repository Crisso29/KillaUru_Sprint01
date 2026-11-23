"""Constitución VIII — en ml/ ninguna función de Python pasa de 20 líneas (como detekt en la app y la suite del backend).

Cuenta el cuerpo sin la docstring, de la primera a la última instrucción. Funciones cortas = una sola responsabilidad
y fáciles de probar.
"""
import ast
import pathlib

_RAIZ_ML = pathlib.Path(__file__).resolve().parents[2]
_LIMITE = 20


def _cuerpo_sin_docstring(funcion: ast.FunctionDef | ast.AsyncFunctionDef) -> list[ast.stmt]:
    primera = funcion.body[0]
    tiene_docstring = isinstance(primera, ast.Expr) and isinstance(primera.value, ast.Constant)
    return funcion.body[1:] if tiene_docstring else funcion.body


def _largas(ruta: pathlib.Path) -> list[str]:
    arbol = ast.parse(ruta.read_text(encoding="utf-8"))
    resultado = []
    for nodo in ast.walk(arbol):
        if isinstance(nodo, (ast.FunctionDef, ast.AsyncFunctionDef)) and (cuerpo := _cuerpo_sin_docstring(nodo)):
            lineas = cuerpo[-1].end_lineno - cuerpo[0].lineno + 1
            if lineas > _LIMITE:
                resultado.append(f"  {ruta.relative_to(_RAIZ_ML)}:{nodo.lineno} {nodo.name}() — {lineas} líneas")
    return resultado


def test_ninguna_funcion_pasa_de_20_lineas() -> None:
    # Arrange
    excluidas = {".venv", "build", "killauru_ml.egg-info"}
    fuentes = [archivo for archivo in _RAIZ_ML.rglob("*.py") if excluidas.isdisjoint(archivo.parts)]
    # Act
    violaciones = [linea for ruta in fuentes for linea in _largas(ruta)]
    # Assert
    assert not violaciones, "Funciones de más de 20 líneas:\n" + "\n".join(violaciones)
