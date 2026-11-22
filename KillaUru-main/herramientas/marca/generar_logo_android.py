"""Genera los VectorDrawable del logo «Señal» para la app Android.

Fuente: opción C de `referencia/prototipo/killauru-logos.html` (la misma figura que `panel/public/favicon.svg`).
Android no lee SVG: este script traduce cada figura (círculos y elipses a arcos, la línea discontinua a trazos,
las opacidades a `fillAlpha`) y escribe tres archivos en `app/src/main/res/drawable/`:

- `logo_senal.xml`: el logo completo con esquinas redondeadas, para usar dentro de la app.
- `ic_launcher_fondo.xml` y `ic_launcher_figura.xml`: capas del ícono adaptativo (fondo y figura en la zona segura).

Uso: `python herramientas/marca/generar_logo_android.py` desde la raíz del repositorio.
"""
from pathlib import Path

RES = Path(__file__).resolve().parents[2] / "app" / "src" / "main" / "res" / "drawable"
NS = ('xmlns:android="http://schemas.android.com/apk/res/android" '
      'xmlns:aapt="http://schemas.android.com/aapt"')


def circulo(cx, cy, r):
    return f"M{cx - r},{cy} a{r},{r} 0 1,0 {2 * r},0 a{r},{r} 0 1,0 {-2 * r},0 Z"


def elipse(cx, cy, rx, ry):
    return f"M{cx - rx},{cy} a{rx},{ry} 0 1,0 {2 * rx},0 a{rx},{ry} 0 1,0 {-2 * rx},0 Z"


def relleno(datos, color, alfa=1.0):
    a = f' android:fillAlpha="{alfa}"' if alfa < 1 else ""
    return f'<path android:pathData="{datos}" android:fillColor="{color}"{a}/>'


def trazo(datos, color, ancho, alfa=1.0, redondo=False):
    a = f' android:strokeAlpha="{alfa}"' if alfa < 1 else ""
    r = ' android:strokeLineCap="round"' if redondo else ""
    return f'<path android:pathData="{datos}" android:strokeColor="{color}" android:strokeWidth="{ancho}"{a}{r}/>'


def radial(datos, cx, cy, r, paradas):
    items = "".join(f'<item android:offset="{o}" android:color="{c}"/>' for o, c in paradas)
    return (f'<path android:pathData="{datos}"><aapt:attr name="android:fillColor">'
            f'<gradient android:type="radial" android:centerX="{cx}" android:centerY="{cy}" '
            f'android:gradientRadius="{r}">{items}</gradient></aapt:attr></path>')


def fondo_degradado(datos):
    return ('<path android:pathData="' + datos + '"><aapt:attr name="android:fillColor">'
            '<gradient android:type="linear" android:startX="0" android:startY="0" android:endX="0" android:endY="120">'
            '<item android:offset="0" android:color="#FF0B2433"/><item android:offset="1" android:color="#FF11475A"/>'
            '</gradient></aapt:attr></path>')


def figura():
    """Todo el dibujo salvo el fondo, en coordenadas del SVG (120 × 120)."""
    estrellas = [(16, 22, 1), (104, 20, 1.1), (96, 58, .8), (20, 60, .8)]
    ala = elipse(0, 0, 15, 7)
    return "\n".join([
        *[relleno(circulo(x, y, r), "#BFEDE3", .8) for x, y, r in estrellas],
        relleno("M0,98 L18,84 L32,92 L48,78 L60,88 L72,78 L88,92 L102,84 L120,98 V120 H0Z", "#0A3445"),
        relleno("M0,106 L22,96 L40,104 L60,96 L80,104 L98,96 L120,106 V120 H0Z", "#082834"),
        relleno("M42,120 L58,90 H62 L78,120Z", "#06202A"),
        trazo("M42,120 L58,90 M78,120 L62,90", "#F2C14E", 1.5),
        # Línea central discontinua (3,5 dibujado y 3,5 vacío) de y = 120 a 92.
        trazo("M60,120 V116.5 M60,113 V109.5 M60,106 V102.5 M60,99 V95.5", "#FFFFFF", 1.3, .85),
        relleno(elipse(60, 90, 24, 5), "#FFD45C", .35),
        radial(circulo(60, 44, 30), 60, 44, 30, [(0, "#8CFFD45C"), (1, "#00FFD45C")]),
        f'<group android:translateX="39" android:translateY="34" android:rotation="-28">'
        f'{relleno(ala, "#9FE6D6", .55)}</group>',
        f'<group android:translateX="81" android:translateY="34" android:rotation="28">'
        f'{relleno(ala, "#9FE6D6", .55)}</group>',
        trazo("M54,25 Q50,16 45,14 M66,25 Q70,16 75,14", "#F4EFE0", 2.2, redondo=True),
        relleno(circulo(45, 14, 2), "#F4EFE0"),
        relleno(circulo(75, 14, 2), "#F4EFE0"),
        relleno("M60,86 C52,74 42,64 42,46 A18,18 0 1,1 78,46 C78,64 68,74 60,86Z", "#F4EFE0"),
        radial(circulo(60, 46, 11), 60, 46, 11, [(0, "#FFFFF6CF"), (.55, "#FFFFD45C"), (1, "#FFF2A81E")]),
        relleno(circulo(56.5, 42.5, 2.6), "#FFFFFF", .85),
    ])


ESQUINAS = "M27,0 H93 A27,27 0 0,1 120,27 V93 A27,27 0 0,1 93,120 H27 A27,27 0 0,1 0,93 V27 A27,27 0 0,1 27,0 Z"
AVISO = "<!-- Generado por herramientas/marca/generar_logo_android.py: no editar a mano. -->\n"


def escribir(nombre, lado_dp, vista, cuerpo):
    xml = (f'<?xml version="1.0" encoding="utf-8"?>\n{AVISO}'
           f'<vector {NS} android:width="{lado_dp}dp" android:height="{lado_dp}dp" '
           f'android:viewportWidth="{vista}" android:viewportHeight="{vista}">\n{cuerpo}\n</vector>\n')
    (RES / nombre).write_text(xml, encoding="utf-8")


def main():
    escribir("logo_senal.xml", 120, 120,
             f'<group><clip-path android:pathData="{ESQUINAS}"/>{fondo_degradado("M0,0 H120 V120 H0Z")}\n{figura()}</group>')
    # Ícono adaptativo: lienzo de 108 dp; el sistema recorta un círculo o cuadrado de ~72 dp en el centro.
    # La figura de 120 se reduce a 72 (escala 0,6) y se centra (desplazamiento 18) para quedar en la zona segura.
    escribir("ic_launcher_figura.xml", 108, 108,
             f'<group android:translateX="18" android:translateY="18" android:scaleX="0.6" android:scaleY="0.6">'
             f'<clip-path android:pathData="M0,0 H120 V120 H0Z"/>\n{figura()}</group>')
    # Ícono temático de Android 13+: solo la silueta (luciérnaga-marcador, alas y antenas); el sistema le da el color.
    silueta = "".join([
        f'<group android:translateX="39" android:translateY="34" android:rotation="-28">{relleno(elipse(0, 0, 15, 7), "#FF000000", .6)}</group>',
        f'<group android:translateX="81" android:translateY="34" android:rotation="28">{relleno(elipse(0, 0, 15, 7), "#FF000000", .6)}</group>',
        trazo("M54,25 Q50,16 45,14 M66,25 Q70,16 75,14", "#FF000000", 2.2, redondo=True),
        relleno(circulo(45, 14, 2), "#FF000000"), relleno(circulo(75, 14, 2), "#FF000000"),
        relleno("M60,86 C52,74 42,64 42,46 A18,18 0 1,1 78,46 C78,64 68,74 60,86Z", "#FF000000"),
        relleno("M42,112 L58,90 H62 L78,112Z", "#FF000000", .6),
    ])
    escribir("ic_launcher_monocromo.xml", 108, 108,
             f'<group android:translateX="18" android:translateY="18" android:scaleX="0.6" android:scaleY="0.6">{silueta}</group>')
    escribir("ic_launcher_fondo.xml", 108, 108,
             '<group android:scaleX="0.9" android:scaleY="0.9">' + fondo_degradado("M0,0 H120 V120 H0Z") + '</group>')
    print("Generados en", RES)


if __name__ == "__main__":
    main()
