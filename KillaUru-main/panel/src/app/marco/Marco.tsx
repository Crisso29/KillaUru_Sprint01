import type { EstadoSesion } from '@/sesion/estadoSesion'
import { Marca } from '@/compartido/ui'
import { MenuLateral } from './MenuLateral'
import { PieSesion } from './PieSesion'
import { BarraSuperior } from './BarraSuperior'

interface Props {
  sesion: Extract<EstadoSesion, { tipo: 'activa' }>
  porRevisar: number
  onSalir: () => void
  entorno: string
  /** URL pública de la página de estado (UptimeRobot, HU-17). */
  urlEstado?: string
  /** El mapa ocupa todo el contenido, sin el margen de página (prototipo `shell(…, full)`). */
  completo?: boolean
  children: React.ReactNode
}

/**
 * T019: armazón del panel según el prototipo (`.ops`): barra lateral con marca, menú y sesión; barra superior con
 * migas y búsqueda; y el contenido. «Saltar al contenido» es el primer elemento enfocable (WCAG 2.4.1).
 */
export function Marco({ sesion, porRevisar, onSalir, entorno, urlEstado = '#', completo = false, children }: Props) {
  return (
    <div className="ops">
      <a href="#contenido" className="saltar">Saltar al contenido</a>
      <aside className="ops-side">
        <div className="ops-logo"><Marca tamano="sm" conOps /></div>
        <MenuLateral porRevisar={porRevisar} />
        <PieSesion correo={sesion.correo ?? ''} sesionVenceUtc={sesion.sesionVenceUtc} urlEstado={urlEstado} onSalir={onSalir} />
      </aside>
      <BarraSuperior entorno={entorno} />
      <main id="contenido" className="ops-main" tabIndex={-1}>
        {completo ? children : <div className="ops-pg">{children}</div>}
      </main>
    </div>
  )
}
