// T031 — aviso de cuenta bloqueada con contador (R-020)
import { useEffect, useState, useRef } from 'react'
import { relojReal } from '@/compartido/reloj'
import type { Reloj } from '@/compartido/reloj'
import { estaDesbloqueado } from '../modelo/bloqueo'
import { mensajeBloqueo } from '../textos'

interface Props {
  bloqueadoHastaUtc: Date
  onDesbloqueado: () => void
  reloj?: Reloj
}

export function AvisoBloqueo({ bloqueadoHastaUtc, onDesbloqueado, reloj = relojReal }: Props) {
  const [ahora, setAhora] = useState(() => reloj.ahora())
  const timerId = useRef<ReturnType<typeof setInterval> | null>(null)

  useEffect(() => {
    timerId.current = setInterval(() => {
      const instante = reloj.ahora()
      setAhora(instante)
      if (estaDesbloqueado(bloqueadoHastaUtc, instante)) {
        if (timerId.current) clearInterval(timerId.current)
        onDesbloqueado()
      }
    }, 1000)
    return () => { if (timerId.current) clearInterval(timerId.current) }
  }, [bloqueadoHastaUtc, onDesbloqueado, reloj])

  return <p role="alert" className="note bad">{mensajeBloqueo(bloqueadoHastaUtc, ahora)}</p>
}
