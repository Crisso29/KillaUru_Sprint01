"""T057 — pipeline completo con datos sintéticos (integración end-to-end)."""
import os
import pathlib


def test_todo_sintetico_sale_sin_excepcion(tmp_path) -> None:
    # Arrange — ejecutar desde tmp para aislar archivos generados
    cwd_original = pathlib.Path.cwd()
    os.chdir(tmp_path)
    try:
        from killauru_ml.__main__ import main
        # Act — pipeline completo
        ret = main(["todo", "--version", "0.0.1-test", "--sintetico"])
    finally:
        os.chdir(cwd_original)
    # Assert — 0=éxito total, 2=puerta no aprobada (esperado en sintético)
    assert ret in (0, 2), f"Código de salida inesperado: {ret}"


def test_todo_sintetico_crea_archivo_tflite(tmp_path) -> None:
    # Arrange
    cwd_original = pathlib.Path.cwd()
    os.chdir(tmp_path)
    try:
        from killauru_ml.__main__ import main
        main(["todo", "--version", "0.0.2-test", "--sintetico"])
    finally:
        os.chdir(cwd_original)
    # Assert — al menos un .tflite debe existir en el directorio de trabajo
    tflites = list(tmp_path.rglob("*.tflite"))
    assert len(tflites) >= 1


def test_todo_sintetico_guarda_la_evidencia_y_sin_aprobar_no_publica_manifiesto(tmp_path) -> None:
    # Arrange — con datos aleatorios y sin benchmark ni acuerdo la puerta DEBE rechazar (data-model §7)
    cwd_original = pathlib.Path.cwd()
    os.chdir(tmp_path)
    try:
        from killauru_ml.__main__ import main
        # Act
        codigo = main(["todo", "--version", "0.0.3-test", "--sintetico"])
    finally:
        os.chdir(cwd_original)
    # Assert — queda la evidencia para la puerta; el manifiesto solo lo escribe una puerta aprobada
    carpeta = tmp_path / "modelos" / "0.0.3-test"
    assert codigo == 2
    assert (carpeta / "evidencia-puerta.json").is_file() and (carpeta / "registro.json").is_file()
    assert not (carpeta / "manifiesto.json").exists()
