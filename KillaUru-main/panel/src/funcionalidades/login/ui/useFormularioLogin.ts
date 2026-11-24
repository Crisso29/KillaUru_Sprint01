// T031 — estado del login del panel: campos, token anti-bot, error y bloqueo (FR-005, R-020)
import { useRef, useState } from 'react'
import { useIniciarSesion } from '../api/useIniciarSesion'
import { mensajeDeError } from '@/compartido/textos/errores'
import type { ErrorApi } from '@/api/problema'
import type { DatosSesion } from '@/sesion/sesionPanel'

export function useFormularioLogin(onExito?: (sesion: DatosSesion) => void) {
  const campos = useCamposLogin()
  const [token, setToken] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [bloqueoHasta, setBloqueoHasta] = useState<Date | null>(null)
  const reiniciarDesafio = useRef<(() => void) | null>(null)
  const { mutate, isPending } = useIniciarSesion({ onExito })
  // Un bloqueo se muestra con su cuenta regresiva; cualquier otro error exige un desafío anti-bot nuevo
  const alFallar = (fallo: ErrorApi) => {
    if (fallo.codigo === 'cuenta_bloqueada' && fallo.bloqueadoHastaUtc) return setBloqueoHasta(fallo.bloqueadoHastaUtc)
    setError(mensajeDeError(fallo)); setToken(null); reiniciarDesafio.current?.()
  }
  const enviar = () => {
    if (!token) return
    setError(null); setBloqueoHasta(null)
    mutate({ correo: campos.correo, contrasena: campos.contrasena, turnstileToken: token }, { onError: (fallo) => alFallar(fallo as ErrorApi) })
  }
  const desbloquear = () => setBloqueoHasta(null)
  return { ...campos, token, setToken, error, bloqueoHasta, desbloquear, reiniciarDesafio, enviar, ocupado: isPending }
}

function useCamposLogin() {
  const [campos, setCampos] = useState({ correo: '', contrasena: '' })
  const poner = (campo: 'correo' | 'contrasena') => (valor: string) => setCampos((actuales) => ({ ...actuales, [campo]: valor }))
  return { ...campos, setCorreo: poner('correo'), setContrasena: poner('contrasena') }
}
