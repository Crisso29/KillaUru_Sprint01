// T031 — pantalla de recuperación de contraseña (US1, FR-013)
import { useState } from 'react'
import { Link } from 'react-router'
import { useRecuperar } from '../api/useRecuperar'
import { mensajeDeError } from '@/compartido/textos/errores'
import type { ErrorApi } from '@/api/problema'
import { PaginaAcceso } from '@/compartido/ui'
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
    <PaginaAcceso titulo={TEXTOS_RECUPERAR.titulo}>
      {exito ? (
        <p role="status" className="note ok">{TEXTOS_RECUPERAR.exito}</p>
      ) : (
        <form onSubmit={enviar} className="formulario-login">
          <div className="field">
            <label htmlFor="correo">{TEXTOS_RECUPERAR.labelCorreo}</label>
            <input
              id="correo"
              name="correo"
              type="email"
              autoComplete="email"
              value={correo}
              onChange={(e) => setCorreo(e.target.value)}
              required
            />
          </div>
          {error && <p role="alert" className="note bad">{error}</p>}
          <button type="submit" className="btn btn-primary btn-block" disabled={isPending} style={{ minHeight: 40, fontSize: 14 }}>
            {isPending ? TEXTOS_RECUPERAR.cargando : TEXTOS_RECUPERAR.botonEnviar}
          </button>
        </form>
      )}
      <Link to="/login" className="lnk">{TEXTOS_RECUPERAR.linkVolver}</Link>
    </PaginaAcceso>
  )
}
