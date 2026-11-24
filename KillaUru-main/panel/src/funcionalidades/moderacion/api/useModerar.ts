// T066 — moderarAnomalia: POST /anomalies/{id}/moderacion + invalidaciones (FR-030)
import { useMutation, useQueryClient } from '@tanstack/react-query'
import type { AccionModeracion, DetalleAnomalia } from '@/api/dtos/003-anomalias'
import { pedirJson } from '@/api/http'
import type { ErrorApi } from '@/api/problema'

interface Params {
  id: string
  accion: AccionModeracion
}

/** Lo que muestra estados de anomalías y debe recargarse tras moderar. */
const LISTAS_AFECTADAS = [['mapa'], ['por-revisar'], ['pendientes'], ['transiciones']]

export function useModerar() {
  const qc = useQueryClient()
  return useMutation<DetalleAnomalia, ErrorApi, Params>({
    mutationFn: ({ id, accion }) => pedirJson<DetalleAnomalia>(`/api/v1/anomalies/${id}/moderacion`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ accion }),
    }),
    onSuccess: (detalle) => {
      qc.setQueryData(['anomalia', detalle.id], detalle)
      LISTAS_AFECTADAS.forEach((llave) => void qc.invalidateQueries({ queryKey: llave }))
    },
    // 409 transicion_no_permitida: otro moderador cambió el estado; se recarga el detalle para mostrar el actual
    onError: (_error, { id }) => void qc.invalidateQueries({ queryKey: ['anomalia', id] }),
  })
}
