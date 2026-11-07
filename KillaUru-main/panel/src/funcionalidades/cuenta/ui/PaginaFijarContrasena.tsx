// T061 — PaginaFijarContrasena: formulario nueva contraseña (restablecer | invitacion, FR-014)
import { type FormEvent, useRef, useState } from 'react'
import { tokenDelFragmento } from '../modelo/tokenDelFragmento'
import { validarConfirmacion, validarContrasena } from '../modelo/contrasena'
import { useFijarContrasena } from '../api/useEnlaces'
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
      <div style={{ padding: 'var(--s-34)', textAlign: 'center' }}>
        <p>{msg}</p>
      </div>
    )
  }

  const errorApi = mutation.isError
    ? T.enlaceInvalido
    : null

  return (
    <div style={{ padding: 'var(--s-34)' }}>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="nueva">{T.fijarContrasena}</label>
          <input id="nueva" type="password" name="nueva" />
        </div>
        <div>
          <label htmlFor="confirmar">{T.confirmarContrasena}</label>
          <input id="confirmar" type="password" name="confirmar" />
        </div>
        {(errorLocal ?? errorApi) && (
          <p role="alert">{errorLocal ?? errorApi}</p>
        )}
        <button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? T.guardando : T.guardar}
        </button>
      </form>
    </div>
  )
}
