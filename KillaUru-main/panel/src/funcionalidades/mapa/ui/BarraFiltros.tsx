// T046 — BarraFiltros: menús con casillas para estado, tipo, severidad y período (FR-022)
import { MenuFiltro } from '@/compartido/ui'
import { TEXTOS_MAPA } from '../textos'
import type { FiltrosMapa, Periodo } from '../modelo/filtros'

const ESTADOS_OPTS = ['Activa', 'PendienteConfirmacion', 'Expirada', 'Reparada', 'Descartada'] as const
const TIPOS_OPTS = ['Bache', 'Hundimiento'] as const
const SEVERIDADES_OPTS = [1, 2, 3, 4, 5] as const
const PERIODOS_OPTS: Periodo[] = ['24h', '7d', '30d']
const ETIQ_PERIODO: Record<Periodo, string> = { '24h': '24 h', '7d': '7 d', '30d': '30 d' }

interface Props {
  filtros: FiltrosMapa
  onChange: (f: FiltrosMapa) => void
}

export function BarraFiltros({ filtros, onChange }: Props) {
  return (
    <div style={{ display: 'flex', gap: 'var(--s-13)', alignItems: 'center', padding: 'var(--s-5) var(--s-21)' }}>
      <MenuFiltro etiqueta={TEXTOS_MAPA.estadoLabel} opciones={ESTADOS_OPTS.map(e => ({ valor: e, etiqueta: e }))} seleccionadas={[...filtros.estados]} onChange={vals => onChange({ ...filtros, estados: ESTADOS_OPTS.filter(e => vals.includes(e)) })} />
      <MenuFiltro etiqueta={TEXTOS_MAPA.tipoLabel} opciones={TIPOS_OPTS.map(t => ({ valor: t, etiqueta: t }))} seleccionadas={[...filtros.tipos]} onChange={vals => onChange({ ...filtros, tipos: TIPOS_OPTS.filter(t => vals.includes(t)) })} />
      <MenuFiltro etiqueta={TEXTOS_MAPA.severidadLabel} opciones={SEVERIDADES_OPTS.map(s => ({ valor: String(s), etiqueta: String(s) }))} seleccionadas={filtros.severidades.map(String)} onChange={vals => onChange({ ...filtros, severidades: vals.map(Number) })} />
      <select
        aria-label={TEXTOS_MAPA.periodoLabel}
        value={filtros.periodo}
        onChange={e => onChange({ ...filtros, periodo: e.target.value as Periodo })}
        style={{ fontSize: 'var(--t-13)' }}
      >
        {PERIODOS_OPTS.map(p => <option key={p} value={p}>{ETIQ_PERIODO[p]}</option>)}
      </select>
    </div>
  )
}
