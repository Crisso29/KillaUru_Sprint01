// T031 — formulario de login con Turnstile (US1, FR-005)
import { useRef, useState } from 'react'
import { useIniciarSesion } from '../api/useIniciarSesion'
import { mensajeDeError } from '@/compartido/textos/errores'
import type { ErrorApi } from '@/api/problema'
import { TEXTOS_LOGIN } from '../textos'
import { Turnstile } from './Turnstile'
import { AvisoBloqueo } from './AvisoBloqueo'

interface Props {
  onExito?: (expiraAccesoUtc: Date) => void
}

export function FormularioLogin({ onExito }: Props) {
  const [correo, setCorreo] = useState('')
  const [contrasena, setContrasena] = useState('')
  const [token, setToken] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [bloqueoHasta, setBloqueoHasta] = useState<Date | null>(null)
  const resetRef = useRef<(() => void) | null>(null)

  const { mutate, isPending } = useIniciarSesion({ onExito })

  function manejarToken(t: string) {
    setToken(t)
  }

  function limpiarYNuevoDesafio() {
    setToken(null)
    resetRef.current?.()
  }

  function manejarError(e: ErrorApi) {
    if (e.codigo === 'cuenta_bloqueada' && e.bloqueadoHastaUtc) {
      setBloqueoHasta(e.bloqueadoHastaUtc)
    } else {
      setError(mensajeDeError(e))
      limpiarYNuevoDesafio()
    }
  }

  function enviar(ev: React.FormEvent) {
    ev.preventDefault()
    if (!token) return
    setError(null)
    setBloqueoHasta(null)
    mutate(
      { correo, contrasena, turnstileToken: token },
      { onError: (e) => manejarError(e as ErrorApi) },
    )
  }

  if (bloqueoHasta) {
    return (
      <AvisoBloqueo
        bloqueadoHastaUtc={bloqueoHasta}
        onDesbloqueado={() => setBloqueoHasta(null)}
      />
    )
  }

  return (
    <form onSubmit={enviar}>
      <div>
        <label htmlFor="correo">{TEXTOS_LOGIN.labelCorreo}</label>
        <input
          id="correo"
          type="email"
          autoComplete="email"
          value={correo}
          onChange={(e) => setCorreo(e.target.value)}
          required
        />
      </div>
      <div>
        <label htmlFor="contrasena">{TEXTOS_LOGIN.labelContrasena}</label>
        <input
          id="contrasena"
          type="password"
          autoComplete="current-password"
          value={contrasena}
          onChange={(e) => setContrasena(e.target.value)}
          required
        />
      </div>
      <Turnstile onToken={manejarToken} resetRef={resetRef} />
      {error && <p role="alert">{error}</p>}
      <button type="submit" disabled={isPending || !token}>
        {isPending ? TEXTOS_LOGIN.cargando : TEXTOS_LOGIN.botonEntrar}
      </button>
    </form>
  )
}
