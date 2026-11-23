"""T003 / FR-033 — ningún .py ni .kt de ml/ supera 199 líneas (constitución VIII)."""
import pathlib

# La suite vive en tests/arquitectura; inspeccionar todo ml/, no solo tests/.
_RAIZ_ML = pathlib.Path(__file__).resolve().parents[2]
_EXTENSIONES = {".py", ".kt", ".kts"}
_LIMITE = 199


def _contar_lineas(ruta: pathlib.Path) -> int:
    return len(ruta.read_text(encoding="utf-8").splitlines())


def _archivos_fuente() -> list[pathlib.Path]:
    return [
        archivo for ext in _EXTENSIONES
        for archivo in _RAIZ_ML.rglob(f"*{ext}")
        if not any(parte in archivo.parts for parte in (".cache", ".venv", "build", ".gradle"))
    ]


def test_ningún_archivo_supera_el_límite() -> None:
    # Arrange
    archivos = _archivos_fuente()
    # Act
    violaciones = [
        (archivo.relative_to(_RAIZ_ML), _contar_lineas(archivo))
        for archivo in archivos
        if _contar_lineas(archivo) > _LIMITE
    ]
    # Assert
    assert not violaciones, (
        "Archivos que superan 199 líneas:\n"
        + "\n".join(f"  {ruta}: {lineas} líneas" for ruta, lineas in violaciones)
    )
