// T031 — pantalla de login (US1, FR-005)
import { Link } from 'react-router'
import { FormularioLogin } from './FormularioLogin'
import { TEXTOS_LOGIN } from '../textos'
import { useSesion } from '@/sesion/useSesion'

const VERSION = import.meta.env.VITE_VERSION ?? '0'

export function PaginaLogin() {
  const { notificarLogin } = useSesion()

  return (
    <main id="contenido" className="pagina-login">
      <header>
        <h1>{TEXTOS_LOGIN.titulo}</h1>
        <p>{TEXTOS_LOGIN.subtitulo}</p>
      </header>
      <FormularioLogin onExito={notificarLogin} />
      <nav>
        <Link to="/login/recuperar">{TEXTOS_LOGIN.linkRecuperar}</Link>
      </nav>
      <footer>
        <small>KillaUru Ops v{VERSION} · CRISSO DEV S.A.C.</small>
      </footer>
    </main>
  )
}
