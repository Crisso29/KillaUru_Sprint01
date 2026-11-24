// T066 — flujo de una acción de moderación: elegirla, confirmarla en el diálogo y avisar si el estado ya cambió (409)
import { useState } from 'react'
import { useModerar } from '../api/useModerar'
import { TEXTOS_MODERACION as textos } from '../textos'
import type { DetalleAnomalia, AccionModeracion } from '@/api/dtos/003-anomalias'
import type { ErrorApi } from '@/api/problema'

export function useConfirmarModeracion(detalle: DetalleAnomalia, onModerado: (actualizado: DetalleAnomalia) => void) {
  const [accionPendiente, setAccionPendiente] = useState<AccionModeracion | null>(null)
  const [errorMensaje, setErrorMensaje] = useState<string | null>(null)
  const moderar = useModerar()
  const confirmar = async () => {
    if (!accionPendiente) return
    setErrorMensaje(null)
    try {
      onModerado(await moderar.mutateAsync({ id: detalle.id, accion: accionPendiente }))
    } catch (error) {
      if ((error as ErrorApi).codigo === 'transicion_no_permitida') setErrorMensaje(textos.transicionNoPermitida)
    } finally {
      setAccionPendiente(null)
    }
  }
  return { accionPendiente, elegir: setAccionPendiente, cancelar: () => setAccionPendiente(null), confirmar, errorMensaje, cargando: moderar.isPending }
}
