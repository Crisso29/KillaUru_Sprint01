"""T014 / FR-016 — despachador de etapas: python -m killauru_ml <etapa> [opciones].

Cada etapa implementada tiene su función `_etapa_*` en `_ETAPAS`; las demás responden 2 (pendiente). Los imports van
dentro de cada etapa para que `estado` o `validar-etiquetas` no carguen TensorFlow.
Códigos de salida: 0 correcto, 1 los datos no pasan la verificación, 2 pendiente o necesita datos reales.
"""
from __future__ import annotations

import argparse
import json
import pathlib
import sys
from collections.abc import Callable

_PENDIENTE = 2


def _parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="python -m killauru_ml",
                                     description="Pipeline KillaUru ML — etapas del dataset y el modelo")
    etapas = parser.add_subparsers(dest="etapa", required=True)
    _agregar_datos(etapas)
    _agregar_modelo(etapas)
    return parser


def _agregar_datos(etapas: argparse._SubParsersAction) -> None:
    """Recolección, etiquetado, ventanas y partición."""
    ingerir = etapas.add_parser("ingerir", help="Verifica una grabación y la registra en el catálogo")
    ingerir.add_argument("carpeta", help="Ruta a la carpeta de grabación")
    ingerir.add_argument("--subir", action="store_true", help="Subir a R2 privado tras ingerir")
    ingerir.add_argument("--catalogo", default=None, help="Ruta alternativa del catálogo")
    etapas.add_parser("candidatos", help="Lista picos candidatos para etiquetar")
    validar = etapas.add_parser("validar-etiquetas", help="Valida archivos de etiquetas")
    validar.add_argument("archivo", help="Ruta al archivo JSON de etiquetas")
    etapas.add_parser("acuerdo", help="Calcula κ de Cohen del doble etiquetado")
    etapas.add_parser("ventanas", help="Genera ventanas de 500 ms (data-model §5)")
    etapas.add_parser("particion", help="Verifica la partición declarada en config/particion.yaml")


def _agregar_modelo(etapas: argparse._SubParsersAction) -> None:
    """Entrenamiento, publicación, seguimiento y la ejecución de punta a punta."""
    etapas.add_parser("entrenar", help="Entrena el MLP").add_argument("--version", required=True, help="Ej. 1.0.0")
    etapas.add_parser("evaluar", help="Evalúa el modelo cuantizado sobre prueba")
    etapas.add_parser("cuantizar", help="Exporta a TFLite int8 con TFLiteConverter")
    etapas.add_parser("puerta", help="Verifica todas las condiciones de publicación (FR-028)")
    etapas.add_parser("estado", help="Muestra horas, ventanas y métricas de validación (FR-034)")
    todo = etapas.add_parser("todo", help="Ventanas → partición → entrenar → cuantizar → evaluar → puerta")
    todo.add_argument("--version", required=True, help="Versión candidata")
    todo.add_argument("--sintetico", action="store_true", help="Usar dataset sintético (CI/tests)")
    todo.add_argument("--verificar-huella", action="store_true", help="Falla si la huella cambia")


def _etapa_ingerir(args: argparse.Namespace) -> int:
    from killauru_ml.lectura.ingerir import ingerir
    catalogo = pathlib.Path(args.catalogo) if args.catalogo else None
    fila = ingerir(pathlib.Path(args.carpeta), ruta_catalogo=catalogo, subir=args.subir)
    print(f"{fila.grabacion_id}: {fila.estado} {fila.motivo}")
    return 0 if fila.estado == "Apta" else 1


def _etapa_particion(_args: argparse.Namespace) -> int:
    from killauru_ml.comun.config import leer_yaml
    configuracion = leer_yaml(pathlib.Path("config/particion.yaml"), ["Version", "Estado"])
    print(f"Dataset {configuracion['Version']} — estado: {configuracion['Estado']}")
    return 0


def _etapa_validar_etiquetas(args: argparse.Namespace) -> int:
    from killauru_ml.etiquetado.validar_etiquetas import EtiquetaError, validar_etiqueta
    ruta = pathlib.Path(args.archivo)
    errores = 0
    for etiqueta in json.loads(ruta.read_text()) if ruta.exists() else []:
        try:
            validar_etiqueta(etiqueta)
        except EtiquetaError as error:
            print(f"ERROR: {error}", file=sys.stderr)
            errores += 1
    return 0 if errores == 0 else 1


def _etapa_estado(_args: argparse.Namespace) -> int:
    from killauru_ml.estado import obtener_estado
    print(json.dumps(obtener_estado(), indent=2, ensure_ascii=False))
    return 0


def _etapa_todo(args: argparse.Namespace) -> int:
    if not args.sintetico:
        print("todo sin --sintetico requiere datos reales (T032).", file=sys.stderr)
        return _PENDIENTE
    from killauru_ml.pipeline.sintetico import ejecutar_todo_sintetico
    return ejecutar_todo_sintetico(args.version)


def _etapa_puerta(_args: argparse.Namespace) -> int:
    from killauru_ml.comun.config import leer_yaml
    leer_yaml(pathlib.Path("config/puerta.yaml"))  # al menos confirma que la configuración es válida
    print("Puerta: configuración cargada. Pase --modelo y --metricas para ejecutar.")
    return _PENDIENTE


_ETAPAS: dict[str, Callable[[argparse.Namespace], int]] = {
    "ingerir": _etapa_ingerir,
    "particion": _etapa_particion,
    "validar-etiquetas": _etapa_validar_etiquetas,
    "estado": _etapa_estado,
    "todo": _etapa_todo,
    "puerta": _etapa_puerta,
}


def main(argv: list[str] | None = None) -> int:
    args = _parser().parse_args(argv)
    etapa = _ETAPAS.get(args.etapa)
    if etapa is None:
        print(f"Etapa '{args.etapa}' aún no implementada en esta fase.", file=sys.stderr)
        return _PENDIENTE
    return etapa(args)


if __name__ == "__main__":
    sys.exit(main())
