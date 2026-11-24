// T045 — iconos: dibuja las 25 combinaciones (5 estados × 5 severidades) según FR-023 y el prototipo (`.sev`)
import { ESTADOS_MAPA, SEVERIDADES, claveIcono, marcaParaEstado, tieneAnillo, type MarcaEstado as Marca } from '../modelo/simbolos'

const LADO = 30 // px lógicos; se dibuja al doble para pantallas de alta densidad
const ESCALA = 2
const RADIO = 10

type Lienzo = CanvasRenderingContext2D

/** Lee un color del sistema de diseño (tokens.css): así los íconos siguen la paleta y el modo oscuro. */
function token(nombre: string, respaldo: string): string {
  const valorCss = typeof document === 'undefined' ? '' : getComputedStyle(document.documentElement).getPropertyValue(nombre).trim()
  return valorCss || respaldo
}

const colorSeveridad = (severidad: number) => token(`--s${severidad}`, '#E0642A')
const superficie = () => token('--surface', '#FFFFFF')
const tinta3 = () => token('--ink-3', '#56686F')
const info = () => token('--info', '#2763A6')

function circulo(ctx: Lienzo, radio: number, relleno: string) {
  ctx.beginPath()
  ctx.arc(LADO / 2, LADO / 2, radio, 0, Math.PI * 2)
  ctx.fillStyle = relleno
  ctx.fill()
}

function nuevoLienzo(): { canvas: HTMLCanvasElement; ctx: Lienzo } {
  const canvas = document.createElement('canvas')
  canvas.width = LADO * ESCALA
  canvas.height = LADO * ESCALA
  const ctx = canvas.getContext('2d')!
  ctx.scale(ESCALA, ESCALA)
  ctx.textAlign = 'center'
  ctx.textBaseline = 'middle'
  return { canvas, ctx }
}

/** Círculo central: lleno (Activa), punteado (Pendiente), con borde (Expirada) o de color plano (Reparada/Descartada). */
function cuerpo(ctx: Lienzo, marca: Marca, severidad: number) {
  const relleno = { lleno: colorSeveridad(severidad), punteado: superficie(), anillo: superficie(), check: info(), cruz: tinta3() }[marca]
  circulo(ctx, RADIO, relleno)
  if (marca !== 'punteado' && marca !== 'anillo') return
  ctx.setLineDash(marca === 'punteado' ? [3, 2] : [])
  ctx.lineWidth = 2.5
  ctx.strokeStyle = marca === 'punteado' ? colorSeveridad(severidad) : tinta3()
  ctx.stroke()
  ctx.setLineDash([])
}

/** El número de la severidad con contraste AA sobre su fondo (oscuro sobre S1–S3 llenos, blanco sobre S4–S5). */
function numero(ctx: Lienzo, marca: Marca, severidad: number) {
  const sobreLleno = severidad <= 3 ? token('--on-s13', '#121829') : token('--on-s45', '#FFFFFF')
  const colores: Record<Marca, string> = { lleno: sobreLleno, check: '#FFFFFF', cruz: '#FFFFFF', punteado: token('--ink', '#0F1E24'), anillo: token('--ink', '#0F1E24') }
  ctx.fillStyle = colores[marca]
  ctx.font = 'bold 12px Overpass, sans-serif'
  ctx.fillText(String(severidad), LADO / 2, LADO / 2 + 1)
}

/** Insignia pequeña abajo a la derecha con ✓ (Reparada) o ✕ (Descartada). */
function insignia(ctx: Lienzo, marca: Marca) {
  if (marca !== 'check' && marca !== 'cruz') return
  ctx.beginPath()
  ctx.arc(LADO - 7, LADO - 7, 6, 0, Math.PI * 2)
  ctx.fillStyle = superficie()
  ctx.fill()
  ctx.fillStyle = marca === 'check' ? info() : tinta3()
  ctx.font = 'bold 10px sans-serif'
  ctx.fillText(marca === 'check' ? '✓' : '✕', LADO - 7, LADO - 6.5)
}

function crearIcono(estado: string, severidad: number): ImageData {
  const { canvas, ctx } = nuevoLienzo()
  const marca = marcaParaEstado(estado)
  if (tieneAnillo(estado, severidad)) { // prototipo: 2 px de superficie y un anillo del color de la severidad
    circulo(ctx, RADIO + 4, colorSeveridad(severidad))
    circulo(ctx, RADIO + 2, superficie())
  }
  cuerpo(ctx, marca, severidad)
  numero(ctx, marca, severidad)
  insignia(ctx, marca)
  return ctx.getImageData(0, 0, canvas.width, canvas.height)
}

/** Las 25 imágenes para `map.addImage(clave, datos, { pixelRatio: 2 })`. */
export function obtenerIconos(): Array<{ clave: string; datos: ImageData }> {
  return ESTADOS_MAPA.flatMap((estado) =>
    SEVERIDADES.map((severidad) => ({ clave: claveIcono(estado, severidad), datos: crearIcono(estado, severidad) })),
  )
}

export const RELACION_PIXELES = ESCALA
