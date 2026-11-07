// T074 — rótulos, formato de variación y valor de indicadores (data-model §5, FR-015)
type ClaveIndicador =
  | 'eventosDia'
  | 'conductoresActivos'
  | 'dispositivosDia'
  | 'coberturaPct'
  | 'confirmacionPct'
  | 'reaparicionPct'

interface MetaIndicador {
  rotulo: string
  ayuda: string
  subirEsMalo: boolean
  tipo: 'porcentaje' | 'absoluta' | 'puntos'
}

export const INDICADORES_META: Record<ClaveIndicador, MetaIndicador> = {
  eventosDia: {
    rotulo: 'Eventos válidos (ayer)',
    ayuda: 'Eventos válidos (reportes procesados) del último día cerrado.',
    subirEsMalo: false,
    tipo: 'porcentaje',
  },
  conductoresActivos: {
    rotulo: 'Conductores activos',
    ayuda: 'Cuentas que renovaron su sesión en los últimos 7 días.',
    subirEsMalo: false,
    tipo: 'absoluta',
  },
  dispositivosDia: {
    rotulo: 'Dispositivos 24 h',
    ayuda: 'Seudónimos distintos con eventos en 24 h. Solo se guarda el número.',
    subirEsMalo: false,
    tipo: 'absoluta',
  },
  coberturaPct: {
    rotulo: 'Cobertura',
    ayuda: 'Tramos de 1 km con al menos un evento válido en 7 días.',
    subirEsMalo: false,
    tipo: 'puntos',
  },
  confirmacionPct: {
    rotulo: 'Confirmación',
    ayuda: 'Pendientes que pasaron a activa antes de expirar.',
    subirEsMalo: false,
    tipo: 'puntos',
  },
  reaparicionPct: {
    rotulo: 'Reaparición',
    ayuda: 'Vuelven a activa tras expirar. Si sube, la caducidad es corta.',
    subirEsMalo: true,
    tipo: 'puntos',
  },
}

export type { ClaveIndicador }

function numComma(n: number, decimales: number): string {
  return n.toFixed(decimales).replace('.', ',')
}

export function formatearValor(valor: number | null, unidad?: string): string {
  if (valor === null) return '—'
  const dec = valor % 1 === 0 ? 0 : 1
  const num = numComma(valor, dec)
  return unidad === '%' ? `${num} %` : num
}

export function formatearVariacion(
  clave: ClaveIndicador,
  variacion: number | null,
): string {
  if (variacion === null) return '—'
  const meta = INDICADORES_META[clave]
  const signo = variacion >= 0 ? '+' : '−'
  const abs = Math.abs(variacion)
  if (meta.tipo === 'porcentaje') {
    return `${signo}${numComma(abs, abs % 1 === 0 ? 0 : 1)} %`
  }
  if (meta.tipo === 'absoluta') {
    return `${signo}${numComma(abs, 0)}`
  }
  const pt = abs === 1 ? 'pt' : 'pts'
  return `${signo}${numComma(abs, 0)} ${pt}`
}
