"""Genera `IconosKillaUru.kt`: los íconos de trazo del prototipo como datos para Compose.

Fuente: el diccionario `I` de `referencia/prototipo/killauru-diseno.html` (el mismo que usa el panel en `Icono.tsx`).
Compose dibuja trazados SVG (`PathParser`), pero no `<circle>` ni `<rect>`: este script los convierte en arcos y líneas.
Uso: `python herramientas/marca/generar_iconos_android.py` desde la raíz del repositorio.
"""
import re
from pathlib import Path

RAIZ = Path(__file__).resolve().parents[2]
DESTINO = RAIZ / "app/src/main/java/pe/killauru/app/comun/ui/IconosKillaUru.kt"
# Solo los que usa la app: cuantos menos, más chico el archivo (y bajo el límite de 200 líneas).
USADOS = ["map", "hist", "gear", "user", "back", "close", "chev", "pin", "bell", "battery", "wifi", "down", "shield",
          "check", "warn", "info", "target", "layers", "sound", "mail", "lock", "phone", "car", "truck", "moto",
          "clock", "refresh", "ext", "out", "compass", "plus", "minus"]


def num(v):
    return float(v)


def circulo(cx, cy, r):
    return f"M{cx - r} {cy}a{r} {r} 0 1 0 {2 * r} 0a{r} {r} 0 1 0 {-2 * r} 0Z"


def rect(x, y, w, h, rx=0.0):
    if rx == 0:
        return f"M{x} {y}h{w}v{h}h{-w}Z"
    return (f"M{x + rx} {y}h{w - 2 * rx}a{rx} {rx} 0 0 1 {rx} {rx}v{h - 2 * rx}a{rx} {rx} 0 0 1 {-rx} {rx}"
            f"h{-(w - 2 * rx)}a{rx} {rx} 0 0 1 {-rx} {-rx}v{-(h - 2 * rx)}a{rx} {rx} 0 0 1 {rx} {-rx}Z")


def a_trazado(svg):
    """Une todas las figuras de un ícono en un solo trazado SVG."""
    partes = []
    for etiqueta, attrs in re.findall(r"<(\w+)([^>]*)/>", svg):
        a = dict(re.findall(r'([\w-]+)="([^"]*)"', attrs))
        if etiqueta == "path":
            partes.append(a["d"])
        elif etiqueta == "circle":
            partes.append(circulo(num(a["cx"]), num(a["cy"]), num(a["r"])))
        elif etiqueta == "rect":
            partes.append(rect(num(a["x"]), num(a["y"]), num(a["width"]), num(a["height"]), num(a.get("rx", 0))))
    return " ".join(partes)


def main():
    html = (RAIZ / "referencia/prototipo/killauru-diseno.html").read_text(encoding="utf-8")
    bloque = html[html.index("const I={") + len("const I={"): html.index("};\nconst ic=")]
    iconos = dict(re.findall(r"(\w+):'([^']*)'", bloque))
    filas = [f'        "{n}" to "{a_trazado(iconos[n])}",' for n in USADOS]
    kt = f'''package pe.killauru.app.comun.ui

// Generado por herramientas/marca/generar_iconos_android.py desde el prototipo: no editar a mano.
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.StrokeJoin
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.graphics.vector.PathParser
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp

/**
 * Íconos del sistema de diseño (prototipo `ic()`, mismos trazos que el panel): línea de 1,8 sobre 24 × 24, redondeada.
 * Se tiñen con `Icon(tint = …)`, como cualquier ícono de Material.
 */
internal object IconosKillaUru {{
    private val TRAZOS = mapOf(
{chr(10).join(filas)}
    )

    private val cache = mutableMapOf<String, ImageVector>()

    /** El ícono por su nombre del prototipo («car», «shield»…); falla si no existe para detectarlo en pruebas. */
    fun de(nombre: String): ImageVector = cache.getOrPut(nombre) {{
        val datos = requireNotNull(TRAZOS[nombre]) {{ "Ícono desconocido: $nombre" }}
        ImageVector.Builder(nombre, 24.dp, 24.dp, 24f, 24f).addPath(
            pathData = PathParser().parsePathString(datos).toNodes(),
            fill = null,
            stroke = SolidColor(Color.Black),
            strokeLineWidth = 1.8f,
            strokeLineCap = StrokeCap.Round,
            strokeLineJoin = StrokeJoin.Round,
        ).build()
    }}

    val nombres: Set<String> get() = TRAZOS.keys
}}
'''
    DESTINO.write_text(kt, encoding="utf-8")
    print("Generado", DESTINO, len(filas), "íconos")


if __name__ == "__main__":
    main()
