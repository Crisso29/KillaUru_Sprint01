// T061 — PaginaVerificar: lee token del hash, POST verificar-correo una sola vez (FR-014)
import { useEffect, useRef, useState } from 'react'
import { tokenDelFragmento } from '../modelo/tokenDelFragmento'
import { useVerificarCorreo } from '../api/useEnlaces'
import { PaginaAcceso } from '@/compartido/ui'
import { TEXTOS_CUENTA as textos } from '../textos'
import { ResultadoEnlace } from './ResultadoEnlace'

const TITULO = 'Verificar correo'

export function PaginaVerificar() {
  const verificar = useVerificarCorreo()
  const sinToken = useVerificarUnaVez(verificar.mutate)
  if (sinToken || verificar.isError) return <ResultadoEnlace titulo={TITULO} mensaje={textos.enlaceInvalido} exito={false} />
  if (verificar.isSuccess) return <ResultadoEnlace titulo={TITULO} mensaje={textos.verificadoOk} exito />
  return <PaginaAcceso titulo={TITULO}><p role="status" aria-busy="true">{textos.verificandoTitulo}</p></PaginaAcceso>
}

/** El enlace es de un solo uso: se envía una vez aunque React monte el efecto dos veces (modo estricto). */
function useVerificarUnaVez(verificar: (token: string) => void): boolean {
  const enviado = useRef(false)
  const [sinToken, setSinToken] = useState(false)
  useEffect(() => {
    if (enviado.current) return
    enviado.current = true
    const token = tokenDelFragmento()
    if (token) verificar(token)
    else setSinToken(true)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])
  return sinToken
}
