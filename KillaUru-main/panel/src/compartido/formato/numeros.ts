// T021 — formato numérico en español peruano
const FMT_NUM = new Intl.NumberFormat('es-PE', { maximumFractionDigits: 1 })
const FMT_PCT = new Intl.NumberFormat('es-PE', { maximumFractionDigits: 1 })
const MENOS = '−' // minus tipográfico

export function formatearNumero(n: number): string {
  return FMT_NUM.format(n)
}

export function formatearPorcentaje(n: number): string {
  return `${FMT_PCT.format(n)} %`
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
