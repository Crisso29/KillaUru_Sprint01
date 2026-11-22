// T061 — PaginaFijarContrasena: formulario nueva contraseña (restablecer | invitacion, FR-014)
import { type FormEvent, useRef, useState } from 'react'
import { tokenDelFragmento } from '../modelo/tokenDelFragmento'
import { validarConfirmacion, validarContrasena } from '../modelo/contrasena'
import { useFijarContrasena } from '../api/useEnlaces'
import { Link } from 'react-router'
import { PaginaAcceso } from '@/compartido/ui'
import { TEXTOS_CUENTA as T } from '../textos'

interface Props {
  modo?: 'restablecer' | 'invitacion'
}

export function PaginaFijarContrasena({ modo = 'restablecer' }: Props) {
  const mutation = useFijarContrasena()
  const token = useRef(tokenDelFragmento())
  const [errorLocal, setErrorLocal] = useState<string | null>(null)

  function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    const fd = new FormData(e.currentTarget)
    const nueva = fd.get('nueva') as string
    const confirmar = fd.get('confirmar') as string

    const errConf = validarConfirmacion(nueva, confirmar)
    if (errConf) { setErrorLocal(errConf); return }

    const errPass = validarContrasena(nueva)
    if (errPass) { setErrorLocal(errPass); return }

    setErrorLocal(null)
    mutation.mutate({ token: token.current ?? '', nuevaContrasena: nueva })
  }

  if (mutation.isSuccess) {
    const msg = modo === 'restablecer' ? T.okRestablecer : T.okInvitacion
    return (
      <PaginaAcceso titulo={T.fijarContrasena}>
        <p role="status" className="note ok">{msg}</p>
        <Link to="/login" className="lnk">Ir al inicio de sesión</Link>
      </PaginaAcceso>
    )
  }

  const errorApi = mutation.isError
    ? T.enlaceInvalido
    : null

  return (
    <PaginaAcceso titulo={T.fijarContrasena}>
      <form onSubmit={handleSubmit} className="formulario-login">
        <div className="field">
          <label htmlFor="nueva">{T.fijarContrasena}</label>
          <input id="nueva" type="password" name="nueva" autoComplete="new-password" />
        </div>
        <div className="field">
          <label htmlFor="confirmar">{T.confirmarContrasena}</label>
          <input id="confirmar" type="password" name="confirmar" autoComplete="new-password" />
        </div>
        {(errorLocal ?? errorApi) && (
          <p role="alert" className="note bad">{errorLocal ?? errorApi}</p>
        )}
        <button type="submit" className="btn btn-primary btn-block" disabled={mutation.isPending} style={{ minHeight: 40, fontSize: 14 }}>
          {mutation.isPending ? T.guardando : T.guardar}
        </button>
      </form>
    </PaginaAcceso>
  )
}
