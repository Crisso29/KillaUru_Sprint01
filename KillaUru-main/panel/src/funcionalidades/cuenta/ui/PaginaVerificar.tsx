// T061 — PaginaVerificar: lee token del hash, POST verificar-correo una sola vez (FR-014)
import { useEffect, useRef, useState } from 'react'
import { tokenDelFragmento } from '../modelo/tokenDelFragmento'
import { useVerificarCorreo } from '../api/useEnlaces'
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
      <div style={{ padding: 'var(--s-34)', textAlign: 'center' }}>
        <p role="alert">{T.enlaceInvalido}</p>
      </div>
    )
  }

  if (mutation.isSuccess) {
    return (
      <div style={{ padding: 'var(--s-34)', textAlign: 'center' }}>
        <p>{T.verificadoOk}</p>
      </div>
    )
  }

  return (
    <div style={{ padding: 'var(--s-34)', textAlign: 'center' }}>
      <p>{T.verificandoTitulo}</p>
    </div>
  )
}
