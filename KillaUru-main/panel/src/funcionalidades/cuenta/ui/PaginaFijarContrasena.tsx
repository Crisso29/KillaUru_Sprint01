// T061 — PaginaFijarContrasena: formulario nueva contraseña (restablecer | invitacion, FR-014)
import { type FormEvent, useRef, useState } from 'react'
import { tokenDelFragmento } from '../modelo/tokenDelFragmento'
import { validarConfirmacion, validarContrasena } from '../modelo/contrasena'
import { useFijarContrasena } from '../api/useEnlaces'
import { CampoAcceso, PaginaAcceso } from '@/compartido/ui'
import { TEXTOS_CUENTA as textos } from '../textos'
import { ResultadoEnlace } from './ResultadoEnlace'

interface Props {
  modo?: 'restablecer' | 'invitacion'
}

export function PaginaFijarContrasena({ modo = 'restablecer' }: Props) {
  const fijar = useFijarContrasena()
  const token = useRef(tokenDelFragmento())
  const [errorLocal, setErrorLocal] = useState<string | null>(null)
  const enviar = (evento: FormEvent<HTMLFormElement>) => {
    evento.preventDefault()
    const datos = new FormData(evento.currentTarget)
    const nueva = String(datos.get('nueva'))
    const error = validarConfirmacion(nueva, String(datos.get('confirmar'))) ?? validarContrasena(nueva)
    setErrorLocal(error)
    if (!error) fijar.mutate({ token: token.current ?? '', nuevaContrasena: nueva })
  }
  if (fijar.isSuccess) {
    return <ResultadoEnlace titulo={textos.fijarContrasena} mensaje={modo === 'restablecer' ? textos.okRestablecer : textos.okInvitacion} exito />
  }
  return <FormularioContrasena onEnviar={enviar} error={errorLocal ?? (fijar.isError ? textos.enlaceInvalido : null)} ocupado={fijar.isPending} />
}

interface PropsFormulario { onEnviar: (evento: FormEvent<HTMLFormElement>) => void; error: string | null; ocupado: boolean }

function FormularioContrasena({ onEnviar, error, ocupado }: PropsFormulario) {
  return (
    <PaginaAcceso titulo={textos.fijarContrasena}>
      <form onSubmit={onEnviar} className="formulario-login">
        <CampoAcceso nombre="nueva" etiqueta={textos.fijarContrasena} tipo="password" autocompletar="new-password" />
        <CampoAcceso nombre="confirmar" etiqueta={textos.confirmarContrasena} tipo="password" autocompletar="new-password" />
        {error && <p role="alert" className="note bad">{error}</p>}
        <button type="submit" className="btn btn-primary btn-block" disabled={ocupado} style={{ minHeight: 40, fontSize: 14 }}>
          {ocupado ? textos.guardando : textos.guardar}
        </button>
      </form>
    </PaginaAcceso>
  )
}
