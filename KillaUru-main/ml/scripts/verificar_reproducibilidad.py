"""Dos procesos y directorios aislados: mismas semillas, mismos bytes TFLite."""
import hashlib
import pathlib
import subprocess
import sys
import tempfile


def ejecutar(raiz: pathlib.Path) -> str:
    resultado = subprocess.run(
        [sys.executable, "-m", "killauru_ml", "todo", "--version", "0.0.1-ci", "--sintetico"],
        cwd=raiz,
        check=False,
    )
    # La calidad puede reprobar con datos aleatorios; los artefactos deben existir.
    if resultado.returncode not in (0, 2):
        raise RuntimeError(f"El pipeline fallo: {resultado.returncode}")
    carpeta = raiz / "modelos" / "0.0.1-ci"
    if not (carpeta / "registro.json").is_file():
        raise RuntimeError("Falta el registro de entrenamiento")
    return hashlib.sha256((carpeta / "killauru-0.0.1-ci.tflite").read_bytes()).hexdigest()


def main() -> None:
    with tempfile.TemporaryDirectory() as primera, tempfile.TemporaryDirectory() as segunda:
        huella = ejecutar(pathlib.Path(primera))
        if ejecutar(pathlib.Path(segunda)) != huella:
            raise RuntimeError("El modelo no es reproducible entre procesos")
        print(f"SHA-256 reproducible: {huella}")


if __name__ == "__main__":
    main()
