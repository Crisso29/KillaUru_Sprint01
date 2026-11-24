// T031 — pantalla de login (US1, FR-005) con el diseño del prototipo (tarjeta sobre el corredor)
import { Link, useSearchParams } from 'react-router'
import { FormularioLogin } from './FormularioLogin'
import { TEXTOS_LOGIN } from '../textos'
import { useSesion } from '@/sesion/useSesion'
import { PaginaAcceso } from '@/compartido/ui'
import { SESION_TERMINADA } from '@/compartido/textos/errores'

export function PaginaLogin() {
  const { notificarLogin } = useSesion()
  // RutaProtegida trae aquí con ?motivo=sesion cuando la renovación fue rechazada: se explica por qué hay que volver a entrar
  const [params] = useSearchParams()
  return (
    <PaginaAcceso titulo={TEXTOS_LOGIN.encabezado}>
      {params.get('motivo') === 'sesion' && <p role="status" className="note warn">{SESION_TERMINADA}</p>}
      <FormularioLogin onExito={notificarLogin} />
      <Link to="/recuperar" className="lnk">{TEXTOS_LOGIN.linkRecuperar}</Link>
      <p className="hint" style={{ fontSize: 12, borderTop: '1px solid var(--line)', paddingTop: 'var(--s-8)' }}>
        {TEXTOS_LOGIN.notaSesion}
      </p>
    </PaginaAcceso>
  )
}
