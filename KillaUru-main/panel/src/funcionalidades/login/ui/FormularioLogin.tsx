// T031 — formulario de login con Turnstile (US1, FR-005)
import { TEXTOS_LOGIN } from '../textos'
import { CampoAcceso } from '@/compartido/ui'
import { Turnstile } from './Turnstile'
import type { DatosSesion } from '@/sesion/sesionPanel'
import { AvisoBloqueo } from './AvisoBloqueo'
import { useFormularioLogin } from './useFormularioLogin'

interface Props {
  onExito?: (sesion: DatosSesion) => void
}

export function FormularioLogin({ onExito }: Props) {
  const login = useFormularioLogin(onExito)
  if (login.bloqueoHasta) return <AvisoBloqueo bloqueadoHastaUtc={login.bloqueoHasta} onDesbloqueado={login.desbloquear} />
  const enviar = (evento: React.FormEvent) => { evento.preventDefault(); login.enviar() }
  return (
    <form onSubmit={enviar} className="formulario-login">
      <CampoAcceso nombre="correo" etiqueta={TEXTOS_LOGIN.labelCorreo} tipo="email" autocompletar="email" valor={login.correo} onCambiar={login.setCorreo} />
      <CampoAcceso nombre="contrasena" etiqueta={TEXTOS_LOGIN.labelContrasena} tipo="password" autocompletar="current-password"
        valor={login.contrasena} onCambiar={login.setContrasena} />
      <Turnstile onToken={login.setToken} resetRef={login.reiniciarDesafio} />
      {login.error && <p role="alert" className="note bad">{login.error}</p>}
      <button type="submit" className="btn btn-primary btn-block" disabled={login.ocupado || !login.token} style={{ minHeight: 40, fontSize: 14 }}>
        {login.ocupado ? TEXTOS_LOGIN.cargando : TEXTOS_LOGIN.botonEntrar}
      </button>
    </form>
  )
}
