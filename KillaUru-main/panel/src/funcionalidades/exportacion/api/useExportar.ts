// T084 — useExportar: POST /api/v1/exportaciones → blob → descarga
import { useMutation } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'
import type { SolicitudExportacion } from '../modelo/solicitud'
import { iniciarDescarga, nombreArchivo } from '../modelo/descarga'

export function useExportar() {
  return useMutation<void, { codigo: string }, SolicitudExportacion>({
    mutationFn: async (solicitud) => {
      const r = await fetch(`${entorno.urlApi}/api/v1/exportaciones`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(solicitud),
      })
      if (!r.ok) {
        const j = await r.json() as { codigo: string }
        throw j
      }
      const blob = await r.blob()
      const nombre = nombreArchivo(
        solicitud.formato,
        solicitud.desdeUtc?.slice(0, 10) ?? '',
        solicitud.hastaUtc ? solicitud.hastaUtc.slice(0, 10) : '',
      )
      iniciarDescarga(blob, nombre)
    },
  })
}
