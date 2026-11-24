// T075 — geometría del gráfico de eventos: escala, guías, trazo del periodo anterior y rótulos (FR-016)
import type { EventoPorDia } from '@/api/dtos/004-dashboard'

/** Medidas del prototipo (OV.resumen): lienzo 640 × 233 con márgenes para los ejes. */
export const LIENZO = { ANCHO: 640, ALTO: 233, IZQ: 34, ABAJO: 21, ARRIBA: 13 } as const

const ROTULOS_MAXIMOS = 8

/** Escala «redonda» del eje: 350 si el máximo es 321, como en el prototipo (8 % de aire, múltiplos de 50). */
export const techo = (maximo: number) => Math.max(50, Math.ceil((maximo * 1.08) / 50) * 50)

export function escalaGrafico(datos: EventoPorDia[]) {
  const { ANCHO, ALTO, IZQ, ABAJO, ARRIBA } = LIENZO
  const maximo = techo(Math.max(...datos.flatMap((fila) => [fila.eventos ?? 0, fila.eventosPeriodoAnterior ?? 0]), 1))
  const ancho = (ANCHO - IZQ) / Math.max(datos.length, 1)
  const y = (valor: number) => ALTO - ABAJO - (valor / maximo) * (ALTO - ABAJO - ARRIBA)
  const centro = (indice: number) => IZQ + indice * ancho + ancho / 2
  const trazoAnterior = datos
    .map((fila, i) => `${i ? 'L' : 'M'}${centro(i).toFixed(1)} ${y(fila.eventosPeriodoAnterior ?? 0).toFixed(1)}`).join(' ')
  return {
    ancho, y, centro, trazoAnterior,
    guias: [0, 1, 2, 3].map((i) => Math.round(((maximo / 3.5) * i) / 50) * 50),
    paso: Math.ceil(datos.length / ROTULOS_MAXIMOS),
  }
}
