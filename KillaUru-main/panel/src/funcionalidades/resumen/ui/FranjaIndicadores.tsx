// T075 — FranjaIndicadores: 6 tarjetas en fila (FR-015)
import type { IndicadoresDashboard } from '@/api/generado/004-dashboard'
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
    <section aria-label="Indicadores de negocio">
      <div role="list">
        {ORDEN.map((clave) => (
          <div key={clave} role="listitem">
            <TarjetaIndicador clave={clave} dato={indicadores[clave]} />
          </div>
        ))}
      </div>
    </section>
  )
}
