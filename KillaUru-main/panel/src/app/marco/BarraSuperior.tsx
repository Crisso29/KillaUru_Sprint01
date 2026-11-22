import { useLocation } from 'react-router'
import { BusquedaRapida } from './BusquedaRapida'
import { migas } from './secciones'

interface Props {
  entorno: string
}

/** Barra superior del prototipo (`.ops-top`): migas, búsqueda rápida (Ctrl K) y la marca DEMO fuera de producción. */
export function BarraSuperior({ entorno }: Props) {
  const { pathname } = useLocation()
  const m = migas(pathname)
  return (
    <header className="ops-top">
      {m && <div className="crumb" aria-label="Estás en">{m.grupo} / <b>{m.seccion}</b></div>}
      <BusquedaRapida />
      {entorno !== 'produccion' && (
        <span className="env" title="Entorno de demostración con datos de ejemplo" style={{ marginLeft: 'auto' }}>DEMO</span>
      )}
    </header>
  )
}
