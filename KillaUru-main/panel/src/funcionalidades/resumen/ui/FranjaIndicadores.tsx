// T075 — FranjaIndicadores: 6 tarjetas en una sola tira (FR-015; prototipo `.card.kstrip`)
import type { IndicadoresDashboard } from '@/api/dtos/004-dashboard'
import type { ClaveIndicador } from '../modelo/indicadores'
import { TarjetaIndicador } from './TarjetaIndicador'

const ORDEN: ClaveIndicador[] = [
  'eventosDia',
  'conductoresActivos',
  'dispositivosDia',
  'coberturaPct',
  'confirmacionPct',
  'reaparicionPct',
]

interface Props {
  indicadores: IndicadoresDashboard
}

export function FranjaIndicadores({ indicadores }: Props) {
  return (
    <section aria-label="Indicadores de negocio" className="card kstrip">
      {ORDEN.map((clave) => (
        <TarjetaIndicador key={clave} clave={clave} dato={indicadores[clave]} />
      ))}
    </section>
  )
}
