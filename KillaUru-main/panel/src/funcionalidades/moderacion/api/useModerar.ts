// T066 — moderarAnomalia: POST /anomalies/{id}/moderacion + invalidaciones (FR-030)
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'
import type { AccionModeracion, DetalleAnomalia } from '@/api/generado/003-anomalias'

interface Params {
  id: string
  accion: AccionModeracion
}

export function useModerar() {
  const qc = useQueryClient()
  return useMutation<DetalleAnomalia, { codigo: string; status?: number }, Params>({
    mutationFn: async ({ id, accion }) => {
      const r = await fetch(`${entorno.urlApi}/api/v1/anomalies/${id}/moderacion`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ accion }),
      })
      if (!r.ok) throw await r.json()
      return r.json() as Promise<DetalleAnomalia>
    },
    onSuccess: (data) => {
      qc.setQueryData(['anomalia', data.id], data)
      void qc.invalidateQueries({ queryKey: ['mapa'] })
      void qc.invalidateQueries({ queryKey: ['por-revisar'] })
      void qc.invalidateQueries({ queryKey: ['pendientes'] })
      void qc.invalidateQueries({ queryKey: ['transiciones'] })
    },
  })
}
