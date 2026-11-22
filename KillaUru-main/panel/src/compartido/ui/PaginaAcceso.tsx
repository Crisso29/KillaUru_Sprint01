import { FondoCorredor } from './FondoCorredor'
import { LogoSenal } from './LogoSenal'

interface Props {
  /** Título de la tarjeta («Iniciar sesión», «Recuperar contraseña»…). */
  titulo: string
  children: React.ReactNode
}

const VERSION = import.meta.env.VITE_VERSION ?? '0'

/**
 * Marco de las páginas públicas (login, recuperar, verificar, fijar contraseña) según el prototipo: tarjeta
 * `.login-card` con la marca, sobre el corredor (`.login`). Una sola pieza para que las cuatro se vean iguales.
 */
export function PaginaAcceso({ titulo, children }: Props) {
  return (
    <main id="contenido" className="login">
      <FondoCorredor />
      <div className="login-card">
        <header style={{ display: 'flex', alignItems: 'center', gap: 'var(--s-8)' }}>
          <LogoSenal tamano={55} />
          <div>
            <p style={{ fontFamily: 'var(--f-display)', fontWeight: 900, fontSize: 18 }}>KillaUru Ops</p>
            <p className="hint">Panel de operación · solo personal autorizado</p>
          </div>
        </header>
        <h1 style={{ fontSize: 21, fontWeight: 800 }}>{titulo}</h1>
        {children}
      </div>
      <footer className="login-foot">KillaUru Ops v{VERSION} · CRISSO DEV S.A.C.</footer>
    </main>
  )
}
