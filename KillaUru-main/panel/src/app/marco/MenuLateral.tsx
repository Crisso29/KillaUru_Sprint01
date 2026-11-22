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
      {GRUPOS.map((g) => (
        <Fragment key={g.titulo}>
          <h5>{g.titulo}</h5>
          {g.secciones.map((s) => (
            <NavLink key={s.ruta} to={s.ruta}>
              <Icono nombre={s.icono} />
              {s.texto}
              {s.ruta === '/moderacion' && porRevisar > 0 && (
                <span className="cnt" aria-label={`${porRevisar} por revisar`}>{porRevisar}</span>
              )}
            </NavLink>
          ))}
        </Fragment>
      ))}
    </nav>
  )
}
