"""T003 / FR-033 — ningún .py ni .kt de ml/ supera 199 líneas (constitución VIII)."""
import pathlib

_RAIZ_ML = pathlib.Path(__file__).parent.parent
_EXTENSIONES = {".py", ".kt", ".kts"}
_LIMITE = 199


def _contar_lineas(ruta: pathlib.Path) -> int:
    return len(ruta.read_text(encoding="utf-8").splitlines())


def _archivos_fuente() -> list[pathlib.Path]:
    return [
        p for ext in _EXTENSIONES
        for p in _RAIZ_ML.rglob(f"*{ext}")
        if not any(parte in p.parts for parte in (".cache", ".venv", "build", ".gradle"))
    ]


def test_ningún_archivo_supera_el_límite() -> None:
    # Arrange
    archivos = _archivos_fuente()
    # Act
    violaciones = [
        (p.relative_to(_RAIZ_ML), _contar_lineas(p))
        for p in archivos
        if _contar_lineas(p) > _LIMITE
    ]
    # Assert
    assert not violaciones, (
        "Archivos que superan 199 líneas:\n"
        + "\n".join(f"  {ruta}: {n} líneas" for ruta, n in violaciones)
    )
