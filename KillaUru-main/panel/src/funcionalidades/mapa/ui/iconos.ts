// T045 — iconos: dibuja las 25 combinaciones (5 estados × 5 severidades) según FR-023 y el prototipo (`.sev`)
import { ESTADOS_MAPA, SEVERIDADES, claveIcono, marcaParaEstado, tieneAnillo } from '../modelo/simbolos'

const LADO = 30 // px lógicos; se dibuja al doble para pantallas de alta densidad
const ESCALA = 2

/** Lee un color del sistema de diseño (tokens.css): así los íconos siguen la paleta y el modo oscuro. */
function token(nombre: string, respaldo: string): string {
  const v = typeof document === 'undefined' ? '' : getComputedStyle(document.documentElement).getPropertyValue(nombre).trim()
  return v || respaldo
}

function circulo(ctx: CanvasRenderingContext2D, r: number) {
  ctx.beginPath()
  ctx.arc(LADO / 2, LADO / 2, r, 0, Math.PI * 2)
}

/** Insignia pequeña abajo a la derecha con ✓ (Reparada) o ✕ (Descartada). */
function insignia(ctx: CanvasRenderingContext2D, texto: string, color: string) {
  ctx.beginPath()
  ctx.arc(LADO - 7, LADO - 7, 6, 0, Math.PI * 2)
  ctx.fillStyle = token('--surface', '#FFFFFF')
  ctx.fill()
  ctx.fillStyle = color
  ctx.font = 'bold 10px sans-serif'
  ctx.fillText(texto, LADO - 7, LADO - 6.5)
}

function crearIcono(estado: string, severidad: number): ImageData {
  const canvas = document.createElement('canvas')
  canvas.width = LADO * ESCALA
  canvas.height = LADO * ESCALA
  const ctx = canvas.getContext('2d')!
  ctx.scale(ESCALA, ESCALA)
  ctx.textAlign = 'center'
  ctx.textBaseline = 'middle'
  const sev = token(`--s${severidad}`, '#E0642A')
  const superficie = token('--surface', '#FFFFFF')
  const marca = marcaParaEstado(estado)
  const r = 10

  if (tieneAnillo(estado, severidad)) { // prototipo: 2 px de superficie y un anillo del color de la severidad
    circulo(ctx, r + 4); ctx.fillStyle = sev; ctx.fill()
    circulo(ctx, r + 2); ctx.fillStyle = superficie; ctx.fill()
  }
  circulo(ctx, r)
  const relleno = { lleno: sev, punteado: superficie, anillo: superficie, check: token('--info', '#2763A6'), cruz: token('--ink-3', '#63767E') }[marca]
  ctx.fillStyle = relleno
  ctx.fill()
  if (marca === 'punteado' || marca === 'anillo') {
    ctx.setLineDash(marca === 'punteado' ? [3, 2] : [])
    ctx.lineWidth = 2.5
    ctx.strokeStyle = marca === 'punteado' ? sev : token('--ink-3', '#63767E')
    ctx.stroke()
    ctx.setLineDash([])
  }
  const textoOscuro = marca === 'lleno' && severidad <= 3
  ctx.fillStyle = marca === 'lleno' ? (textoOscuro ? token('--on-s13', '#121829') : token('--on-s45', '#FFFFFF'))
    : marca === 'check' || marca === 'cruz' ? '#FFFFFF' : token('--ink', '#0F1E24')
  ctx.font = 'bold 12px Overpass, sans-serif'
  ctx.fillText(String(severidad), LADO / 2, LADO / 2 + 1)
  if (marca === 'check') insignia(ctx, '✓', token('--info', '#2763A6'))
  if (marca === 'cruz') insignia(ctx, '✕', token('--ink-3', '#63767E'))

  return ctx.getImageData(0, 0, canvas.width, canvas.height)
}

/** Las 25 imágenes para `map.addImage(clave, datos, { pixelRatio: 2 })`. */
export function obtenerIconos(): Array<{ clave: string; datos: ImageData }> {
  return ESTADOS_MAPA.flatMap(estado =>
    SEVERIDADES.map(sev => ({ clave: claveIcono(estado, sev), datos: crearIcono(estado, sev) })),
  )
}

export const RELACION_PIXELES = ESCALA
