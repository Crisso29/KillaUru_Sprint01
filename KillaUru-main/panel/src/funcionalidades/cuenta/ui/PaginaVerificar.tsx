// T061 — PaginaVerificar: lee token del hash, POST verificar-correo una sola vez (FR-014)
import { useEffect, useRef, useState } from 'react'
import { tokenDelFragmento } from '../modelo/tokenDelFragmento'
import { useVerificarCorreo } from '../api/useEnlaces'
import { Link } from 'react-router'
import { PaginaAcceso } from '@/compartido/ui'
import { TEXTOS_CUENTA as T } from '../textos'

export function PaginaVerificar() {
  const mutation = useVerificarCorreo()
  const sent = useRef(false)
  const [sinToken, setSinToken] = useState(false)

  useEffect(() => {
    if (sent.current) return
    sent.current = true
    const token = tokenDelFragmento()
    if (!token) {
      setSinToken(true)
      return
    }
    mutation.mutate(token)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  if (sinToken || mutation.isError) {
    return (
      <PaginaAcceso titulo="Verificar correo">
        <p role="alert" className="note bad">{T.enlaceInvalido}</p>
        <Link to="/login" className="lnk">Ir al inicio de sesión</Link>
      </PaginaAcceso>
    )
  }

  if (mutation.isSuccess) {
    return (
      <PaginaAcceso titulo="Verificar correo">
        <p role="status" className="note ok">{T.verificadoOk}</p>
        <Link to="/login" className="lnk">Ir al inicio de sesión</Link>
      </PaginaAcceso>
    )
  }

  return (
    <PaginaAcceso titulo="Verificar correo">
      <p role="status" aria-busy="true">{T.verificandoTitulo}</p>
    </PaginaAcceso>
  )
}
