import { NavLink } from 'react-router'
import type { EstadoSesion } from '@/sesion/estadoSesion'
import { fraccionRestante } from '@/sesion/planificador'
import { Boton, Marca } from '@/compartido/ui'
import { formatearHoraLima } from '@/compartido/formato/fechas'

interface Props {
  sesion: Extract<EstadoSesion, { tipo: 'activa' }>
  porRevisar: number
  onSalir: () => void
  entorno: string
  children: React.ReactNode
}

export function Marco({ sesion, porRevisar, onSalir, entorno, children }: Props) {
  const sesionHora = formatearHoraLima(sesion.sesionVenceUtc.toISOString())
  const fraccion = fraccionRestante(sesion.sesionVenceUtc, new Date())

  return (
    <>
      <a href="#contenido" style={{ position: 'absolute', left: -9999, top: 0 }} onFocus={e => { e.currentTarget.style.left = '0' }} onBlur={e => { e.currentTarget.style.left = '-9999px' }}>
        Saltar al contenido
      </a>
      <div className="marco">
        <header className="marco__cabecera">
          <Marca tamano="sm" />
          {entorno !== 'produccion' && (
            <span style={{ background: 'var(--luna)', color: '#000', fontSize: 'var(--t-13)', fontWeight: 700, padding: '1px 6px', borderRadius: 'var(--s-3)' }}>DEMO</span>
          )}
          <span style={{ marginLeft: 'auto', fontSize: 'var(--t-13)', color: 'var(--ink-3)' }}>Sesión hasta {sesionHora}</span>
          <progress value={fraccion} max={1} aria-label={`Sesión ${Math.round(fraccion * 100)} %`} style={{ width: 60 }} />
          <Boton variante="secundario" onClick={onSalir} style={{ fontSize: 'var(--t-13)', padding: 'var(--s-5) var(--s-13)' }}>Salir</Boton>
        </header>
        <aside className="marco__lateral">
          <nav aria-label="Menú principal">
            <p style={{ padding: 'var(--s-5) var(--s-21)', fontSize: 'var(--t-13)', fontWeight: 700, color: 'var(--ink-3)', textTransform: 'uppercase' }}>Operación</p>
            <NavLink to="/resumen" className="nav-enlace">Resumen</NavLink>
            <NavLink to="/mapa" className="nav-enlace">Mapa</NavLink>
            <NavLink to="/moderacion" className="nav-enlace">
              Moderación{porRevisar > 0 && <span style={{ marginLeft: 'var(--s-5)', background: 'var(--bad)', color: '#fff', borderRadius: 10, fontSize: 11, padding: '1px 5px' }}>{porRevisar}</span>}
            </NavLink>
            <p style={{ padding: 'var(--s-5) var(--s-21)', marginTop: 'var(--s-13)', fontSize: 'var(--t-13)', fontWeight: 700, color: 'var(--ink-3)', textTransform: 'uppercase' }}>Administración</p>
            <NavLink to="/usuarios" className="nav-enlace">Usuarios</NavLink>
            <NavLink to="/auditoria" className="nav-enlace">Auditoría</NavLink>
            <NavLink to="/exportacion" className="nav-enlace">Exportación</NavLink>
          </nav>
        </aside>
        <main id="contenido" className="marco__contenido">
          {children}
        </main>
      </div>
    </>
  )
}
