// T031 — aviso de cuenta bloqueada con contador (R-020)
import { useEffect, useState, useRef } from 'react'
import { relojReal } from '@/compartido/reloj'
import type { Reloj } from '@/compartido/reloj'
import { estaDesbloqueado, minutosRestantes, horaEnLima } from '../modelo/bloqueo'

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
      const t = reloj.ahora()
      setAhora(t)
      if (estaDesbloqueado(bloqueadoHastaUtc, t)) {
        if (timerId.current) clearInterval(timerId.current)
        onDesbloqueado()
      }
    }, 1000)
    return () => { if (timerId.current) clearInterval(timerId.current) }
  }, [bloqueadoHastaUtc, onDesbloqueado, reloj])

  const mins = minutosRestantes(bloqueadoHastaUtc, ahora)
  const hora = horaEnLima(bloqueadoHastaUtc)

  return (
    <p role="alert" className="aviso-bloqueo">
      {`Cuenta bloqueada ${mins} min por intentos fallidos. Vuelve a intentar a las ${hora}.`}
    </p>
  )
}
