"""T014 / FR-016 — dispatcher de etapas: python -m killauru_ml <etapa> [opciones]."""
from __future__ import annotations

import argparse
import sys


def _parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        prog="python -m killauru_ml",
        description="Pipeline KillaUru ML — etapas del dataset y el modelo",
    )
    sub = p.add_subparsers(dest="etapa", required=True)

    # --- recolección ---
    ing = sub.add_parser("ingerir", help="Verifica una grabación y la registra en el catálogo")
    ing.add_argument("carpeta", help="Ruta a la carpeta de grabación")
    ing.add_argument("--subir", action="store_true", help="Subir a R2 privado tras ingerir")
    ing.add_argument("--catalogo", default=None, help="Ruta alternativa del catálogo")

    sub.add_parser("candidatos", help="Lista picos candidatos para etiquetar")

    # --- etiquetado ---
    ve = sub.add_parser("validar-etiquetas", help="Valida archivos de etiquetas")
    ve.add_argument("archivo", help="Ruta al archivo JSON de etiquetas")
    sub.add_parser("acuerdo", help="Calcula κ de Cohen del doble etiquetado")

    # --- ventanas y partición ---
    sub.add_parser("ventanas", help="Genera ventanas de 500 ms (data-model §5)")
    sub.add_parser("particion", help="Verifica la partición declarada en config/particion.yaml")

    # --- modelo ---
    ent = sub.add_parser("entrenar", help="Entrena el MLP con los datos de entrenamiento")
    ent.add_argument("--version", required=True, help="Versión candidata (ej. 1.0.0)")

    sub.add_parser("evaluar", help="Evalúa el modelo cuantizado sobre prueba")
    sub.add_parser("cuantizar", help="Exporta a TFLite int8 con TFLiteConverter")
    sub.add_parser("puerta", help="Verifica todas las condiciones de publicación (FR-028)")

    # --- seguimiento ---
    sub.add_parser("estado", help="Muestra horas, ventanas y métricas de validación (FR-034)")

    # --- punta a punta ---
    todo = sub.add_parser("todo", help="Ventanas → partición → entrenar → cuantizar → evaluar → puerta")
    todo.add_argument("--version", required=True, help="Versión candidata")
    todo.add_argument("--sintetico", action="store_true", help="Usar dataset sintético (CI/tests)")
    todo.add_argument("--verificar-huella", action="store_true", help="Falla si la huella cambia")

    return p


def main(argv: list[str] | None = None) -> int:
    args = _parser().parse_args(argv)

    if args.etapa == "ingerir":
        import pathlib
        from killauru_ml.lectura.ingerir import ingerir
        ruta_cat = pathlib.Path(args.catalogo) if args.catalogo else None
        fila = ingerir(pathlib.Path(args.carpeta), ruta_catalogo=ruta_cat, subir=args.subir)
        print(f"{fila.grabacion_id}: {fila.estado} {fila.motivo}")
        return 0 if fila.estado == "Apta" else 1

    if args.etapa == "particion":
        import pathlib
        from killauru_ml.comun.config import leer_yaml
        from killauru_ml.particion.no_fuga import verificar_no_fuga, FugaError
        cfg = leer_yaml(pathlib.Path("config/particion.yaml"), ["Version", "Estado"])
        print(f"Dataset {cfg['Version']} — estado: {cfg['Estado']}")
        return 0

    if args.etapa == "validar-etiquetas":
        import json
        import pathlib
        from killauru_ml.etiquetado.validar_etiquetas import validar_etiqueta, EtiquetaError
        ruta = pathlib.Path(vars(args).get("archivo", ""))
        etiquetas = json.loads(ruta.read_text()) if ruta.exists() else []
        errores = 0
        for etq in etiquetas:
            try:
                validar_etiqueta(etq)
            except EtiquetaError as exc:
                print(f"ERROR: {exc}", file=sys.stderr)
                errores += 1
        return 0 if errores == 0 else 1

    # El resto de etapas se implementan en sus fases correspondientes
    print(f"Etapa '{args.etapa}' aún no implementada en esta fase.", file=sys.stderr)
    return 2


if __name__ == "__main__":
    sys.exit(main())
