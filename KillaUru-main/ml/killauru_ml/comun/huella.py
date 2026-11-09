"""FR-007, FR-016 — cálculo de huellas SHA-256 para archivos y objetos."""
import hashlib
import pathlib

_BLOQUE = 65536  # 64 KiB por lectura


def sha256_archivo(ruta: pathlib.Path) -> str:
    """Devuelve el SHA-256 hexadecimal de un archivo, en bloques."""
    h = hashlib.sha256()
    with ruta.open("rb") as f:
        while bloque := f.read(_BLOQUE):
            h.update(bloque)
    return h.hexdigest()
