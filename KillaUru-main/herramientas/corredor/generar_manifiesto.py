#!/usr/bin/env python3
"""T077 / FR-072, R-014: genera mapa/manifiesto.json con version, bytes, sha256."""
import sys
import json
import hashlib
import argparse
from pathlib import Path


def sha256_archivo(ruta: Path) -> str:
    h = hashlib.sha256()
    with ruta.open("rb") as f:
        for bloque in iter(lambda: f.read(65536), b""):
            h.update(bloque)
    return h.hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("pmtiles", help="Archivo .pmtiles del mapa")
    parser.add_argument("version", help="Número de versión")
    parser.add_argument("--salida", default="mapa/manifiesto.json")
    args = parser.parse_args()

    ruta = Path(args.pmtiles)
    if not ruta.exists():
        print(f"ERROR: no existe {ruta}", file=sys.stderr)
        sys.exit(1)

    manifiesto = {
        "version": int(args.version),
        "bytes": ruta.stat().st_size,
        "sha256": sha256_archivo(ruta),
        "atribucion": "© OpenStreetMap contributors",
        "archivo": ruta.name,
    }

    salida = Path(args.salida)
    salida.parent.mkdir(parents=True, exist_ok=True)
    with salida.open("w", encoding="utf-8") as f:
        json.dump(manifiesto, f, ensure_ascii=False, indent=2)

    print(f"Manifiesto generado: {salida}")
    print(f"  version={manifiesto['version']}, bytes={manifiesto['bytes']}, sha256={manifiesto['sha256'][:16]}...")


if __name__ == "__main__":
    main()
