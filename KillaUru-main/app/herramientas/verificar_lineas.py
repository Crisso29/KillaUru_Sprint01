"""FR-076 / constitución VIII: ningún fuente mantenido alcanza 200 líneas."""

import sys
from pathlib import Path

EXTENSIONES = {".kt", ".kts", ".py", ".sh"}
GENERADOS = {"build", "schemas", ".gradle", ".kotlin", "__pycache__"}


def infracciones(raiz: Path) -> list[tuple[str, int]]:
    """Devuelve rutas relativas y tamaños; nunca oculta fuentes por su nombre."""
    resultado = []
    for archivo in sorted(raiz.rglob("*")):
        relativa = archivo.relative_to(raiz)
        if archivo.suffix not in EXTENSIONES or GENERADOS.intersection(relativa.parts):
            continue
        lineas = len(archivo.read_text(encoding="utf-8-sig").splitlines())
        if lineas >= 200:
            resultado.append((relativa.as_posix(), lineas))
    return resultado


def main() -> int:
    raiz = Path(__file__).resolve().parents[1]
    errores = infracciones(raiz)
    for nombre, lineas in errores:
        print(f"{nombre}: {lineas} líneas; máximo 199")
    print(f"FR-076: {len(errores)} infracciones")
    return int(bool(errores))


if __name__ == "__main__":
    sys.exit(main())
