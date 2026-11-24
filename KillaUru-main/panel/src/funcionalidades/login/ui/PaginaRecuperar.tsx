// T031 — pantalla de recuperación de contraseña (US1, FR-013)
import { useState } from 'react'
import { Link } from 'react-router'
import { useRecuperar } from '../api/useRecuperar'
import { mensajeDeError } from '@/compartido/textos/errores'
import type { ErrorApi } from '@/api/problema'
import { CampoAcceso, PaginaAcceso } from '@/compartido/ui'
import { TEXTOS_RECUPERAR } from '../textos'

export function PaginaRecuperar() {
  const [exito, setExito] = useState(false)
  return (
    <PaginaAcceso titulo={TEXTOS_RECUPERAR.titulo}>
      {exito ? <p role="status" className="note ok">{TEXTOS_RECUPERAR.exito}</p> : <FormularioRecuperar onEnviado={() => setExito(true)} />}
      <Link to="/login" className="lnk">{TEXTOS_RECUPERAR.linkVolver}</Link>
    </PaginaAcceso>
  )
}

/** RN-19: la respuesta es la misma exista o no la cuenta; por eso el éxito no menciona el correo. */
function FormularioRecuperar({ onEnviado }: { onEnviado: () => void }) {
  const [correo, setCorreo] = useState('')
  const [error, setError] = useState<string | null>(null)
  const { mutate, isPending } = useRecuperar()
  const enviar = (evento: React.FormEvent) => {
    evento.preventDefault()
    setError(null)
    mutate(correo, { onSuccess: onEnviado, onError: (fallo) => setError(mensajeDeError(fallo as ErrorApi)) })
  }
  return (
    <form onSubmit={enviar} className="formulario-login">
      <CampoAcceso nombre="correo" etiqueta={TEXTOS_RECUPERAR.labelCorreo} tipo="email" autocompletar="email" valor={correo} onCambiar={setCorreo} />
      {error && <p role="alert" className="note bad">{error}</p>}
      <button type="submit" className="btn btn-primary btn-block" disabled={isPending} style={{ minHeight: 40, fontSize: 14 }}>
        {isPending ? TEXTOS_RECUPERAR.cargando : TEXTOS_RECUPERAR.botonEnviar}
      </button>
    </form>
  )
}
