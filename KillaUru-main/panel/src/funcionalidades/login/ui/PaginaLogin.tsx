// T031 — pantalla de login (US1, FR-005) con el diseño del prototipo (tarjeta sobre el corredor)
import { Link } from 'react-router'
import { FormularioLogin } from './FormularioLogin'
import { TEXTOS_LOGIN } from '../textos'
import { useSesion } from '@/sesion/useSesion'
import { PaginaAcceso } from '@/compartido/ui'

export function PaginaLogin() {
  const { notificarLogin } = useSesion()
  return (
    <PaginaAcceso titulo={TEXTOS_LOGIN.encabezado}>
      <FormularioLogin onExito={notificarLogin} />
      <Link to="/recuperar" className="lnk">{TEXTOS_LOGIN.linkRecuperar}</Link>
      <p className="hint" style={{ fontSize: 12, borderTop: '1px solid var(--line)', paddingTop: 'var(--s-8)' }}>
        {TEXTOS_LOGIN.notaSesion}
      </p>
    </PaginaAcceso>
  )
}
