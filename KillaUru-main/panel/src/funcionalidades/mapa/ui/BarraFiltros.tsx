// T046 — BarraFiltros: menús con casillas para estado, tipo, severidad y período (FR-022)
import { MenuFiltro } from '@/compartido/ui'
import { textoEstado } from '@/compartido/textos/estados'
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
    <div className="mtb" role="group" aria-label="Filtros del mapa">
      <MenuFiltro etiqueta={TEXTOS_MAPA.tipoLabel} opciones={TIPOS_OPTS.map(tipo => ({ valor: tipo, etiqueta: tipo }))} seleccionadas={[...filtros.tipos]} onChange={vals => onChange({ ...filtros, tipos: TIPOS_OPTS.filter(tipo => vals.includes(tipo)) })} />
      <MenuFiltro etiqueta={TEXTOS_MAPA.severidadLabel} opciones={SEVERIDADES_OPTS.map(severidad => ({ valor: String(severidad), etiqueta: String(severidad) }))} seleccionadas={filtros.severidades.map(String)} onChange={vals => onChange({ ...filtros, severidades: vals.map(Number) })} />
      <MenuFiltro etiqueta={TEXTOS_MAPA.estadoLabel} opciones={ESTADOS_OPTS.map(evento => ({ valor: evento, etiqueta: textoEstado(evento) }))} seleccionadas={[...filtros.estados]} onChange={vals => onChange({ ...filtros, estados: ESTADOS_OPTS.filter(evento => vals.includes(evento)) })} />
      <label className="fbtn">
        {TEXTOS_MAPA.periodoLabel}:
        <select
          aria-label={TEXTOS_MAPA.periodoLabel}
          value={filtros.periodo}
          onChange={evento => onChange({ ...filtros, periodo: evento.target.value as Periodo })}
          style={{ border: 0, background: 'transparent', fontWeight: 600, color: 'var(--ink)', cursor: 'pointer' }}
        >
          {PERIODOS_OPTS.map(periodo => <option key={periodo} value={periodo}>{ETIQ_PERIODO[periodo]}</option>)}
        </select>
      </label>
    </div>
  )
}
