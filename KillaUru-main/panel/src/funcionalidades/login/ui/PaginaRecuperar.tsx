// T031 — pantalla de recuperación de contraseña (US1, FR-013)
import { useState } from 'react'
import { Link } from 'react-router'
import { useRecuperar } from '../api/useRecuperar'
import { mensajeDeError } from '@/compartido/textos/errores'
import type { ErrorApi } from '@/api/problema'
import { TEXTOS_RECUPERAR } from '../textos'

export function PaginaRecuperar() {
  const [correo, setCorreo] = useState('')
  const [exito, setExito] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const { mutate, isPending } = useRecuperar()

  function enviar(ev: React.FormEvent) {
    ev.preventDefault()
    setError(null)
    mutate(correo, {
      onSuccess: () => setExito(true),
      onError: (e) => setError(mensajeDeError(e as ErrorApi)),
    })
  }

  return (
    <main id="contenido" className="pagina-recuperar">
      <h1>{TEXTOS_RECUPERAR.titulo}</h1>
      {exito ? (
        <p role="status">{TEXTOS_RECUPERAR.exito}</p>
      ) : (
        <form onSubmit={enviar}>
          <div>
            <label htmlFor="correo">{TEXTOS_RECUPERAR.labelCorreo}</label>
            <input
              id="correo"
              type="email"
              autoComplete="email"
              value={correo}
              onChange={(e) => setCorreo(e.target.value)}
              required
            />
          </div>
          {error && <p role="alert">{error}</p>}
          <button type="submit" disabled={isPending}>
            {isPending ? TEXTOS_RECUPERAR.cargando : TEXTOS_RECUPERAR.botonEnviar}
          </button>
        </form>
      )}
      <nav>
        <Link to="/login">{TEXTOS_RECUPERAR.linkVolver}</Link>
      </nav>
    </main>
  )
}
