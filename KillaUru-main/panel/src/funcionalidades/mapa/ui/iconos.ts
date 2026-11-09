// T045 — iconos: dibuja las 25 combinaciones (5 estados × 5 severidades) en canvas
import { ESTADOS_MAPA, SEVERIDADES, formaParaEstado, tieneAnillo, claveIcono } from '../modelo/simbolos'

const TAMA = 24

function colorParaEstado(estado: string): string {
  switch (estado) {
    case 'Activa': return '#e63946'
    case 'PendienteConfirmacion': return '#f4a261'
    case 'Expirada': return '#adb5bd'
    case 'Reparada': return '#2a9d8f'
    case 'Descartada': return '#6c757d'
    default: return '#999'
  }
}

function dibujarForma(ctx: CanvasRenderingContext2D, estado: string, x: number, y: number, r: number) {
  const forma = formaParaEstado(estado)
  ctx.beginPath()
  if (forma === 'circulo') {
    ctx.arc(x, y, r, 0, Math.PI * 2)
  } else if (forma === 'rombo') {
    ctx.moveTo(x, y - r)
    ctx.lineTo(x + r, y)
    ctx.lineTo(x, y + r)
    ctx.lineTo(x - r, y)
    ctx.closePath()
  } else if (forma === 'triangulo') {
    ctx.moveTo(x, y - r)
    ctx.lineTo(x + r, y + r * 0.7)
    ctx.lineTo(x - r, y + r * 0.7)
    ctx.closePath()
  } else if (forma === 'cuadrado') {
    ctx.rect(x - r * 0.8, y - r * 0.8, r * 1.6, r * 1.6)
  } else {
    for (let i = 0; i < 6; i++) {
      const a = (Math.PI / 3) * i - Math.PI / 6
      if (i === 0) ctx.moveTo(x + r * Math.cos(a), y + r * Math.sin(a))
      else ctx.lineTo(x + r * Math.cos(a), y + r * Math.sin(a))
    }
    ctx.closePath()
  }
}

function crearIcono(estado: string, severidad: number): ImageData {
  const canvas = document.createElement('canvas')
  canvas.width = TAMA
  canvas.height = TAMA
  const ctx = canvas.getContext('2d')!
  const c = TAMA / 2
  const r = TAMA / 2 - 2

  if (estado === 'PendienteConfirmacion') {
    ctx.setLineDash([2, 2])
    ctx.strokeStyle = colorParaEstado(estado)
    ctx.lineWidth = 2
    dibujarForma(ctx, estado, c, c, r)
    ctx.stroke()
    ctx.setLineDash([])
  } else {
    ctx.fillStyle = colorParaEstado(estado)
    dibujarForma(ctx, estado, c, c, r)
    ctx.fill()
  }

  if (tieneAnillo(estado, severidad)) {
    ctx.strokeStyle = '#fff'
    ctx.lineWidth = 3
    ctx.beginPath()
    ctx.arc(c, c, r + 2, 0, Math.PI * 2)
    ctx.stroke()
  }

  ctx.fillStyle = '#fff'
  ctx.font = `bold ${TAMA * 0.42}px sans-serif`
  ctx.textAlign = 'center'
  ctx.textBaseline = 'middle'
  ctx.fillText(String(severidad), c, c)

  return ctx.getImageData(0, 0, TAMA, TAMA)
}

export function obtenerIconos(): Array<{ clave: string; datos: ImageData }> {
  return ESTADOS_MAPA.flatMap(estado =>
    SEVERIDADES.map(sev => ({ clave: claveIcono(estado, sev), datos: crearIcono(estado, sev) })),
  )
}
