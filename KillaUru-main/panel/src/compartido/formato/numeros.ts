// T021 — formato numérico en español peruano
// Usamos en-US (siempre disponible) + replace para obtener coma decimal
const MENOS = '−' // minus tipográfico

function conComa(n: number, maxDec: number): string {
  return n.toLocaleString('en-US', { maximumFractionDigits: maxDec, useGrouping: false })
    .replace('.', ',')
}

export function formatearNumero(n: number): string {
  return conComa(n, 1)
}

export function formatearPorcentaje(n: number): string {
  return `${conComa(n, 1)} %`
}

export function formatearDelta(delta: number): string {
  if (delta === 0) return '0'
  if (delta > 0) return `+${delta}`
  return `${MENOS}${Math.abs(delta)}`
}

export function formatearDeltaPts(delta: number): string {
  const abs = Math.abs(delta)
  const signo = delta > 0 ? '+' : delta < 0 ? MENOS : ''
  const sufijo = abs === 1 ? 'pt' : 'pts'
  if (delta === 0) return `0 ${sufijo}`
  return `${signo}${abs} ${sufijo}`
}
