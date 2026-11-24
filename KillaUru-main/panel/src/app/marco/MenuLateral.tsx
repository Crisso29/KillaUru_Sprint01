import { Fragment } from 'react'
import { NavLink } from 'react-router'
import { Icono } from '@/compartido/ui'
import { GRUPOS } from './secciones'

interface Props {
  /** Anomalías por revisar: se muestran junto a «Moderación» (prototipo `.cnt`). */
  porRevisar: number
}

/** Menú lateral del prototipo (`.ops-nav`): grupos Operación y Administración, con íconos y contador. */
export function MenuLateral({ porRevisar }: Props) {
  return (
    <nav className="ops-nav" aria-label="Menú principal">
      {GRUPOS.map((grupo) => (
        <Fragment key={grupo.titulo}>
          <h5>{grupo.titulo}</h5>
          {grupo.secciones.map((seccion) => (
            <NavLink key={seccion.ruta} to={seccion.ruta}>
              <Icono nombre={seccion.icono} />
              {seccion.texto}
              {seccion.ruta === '/moderacion' && porRevisar > 0 && (
                <span className="cnt" aria-label={`${porRevisar} por revisar`}>{porRevisar}</span>
              )}
            </NavLink>
          ))}
        </Fragment>
      ))}
    </nav>
  )
}
