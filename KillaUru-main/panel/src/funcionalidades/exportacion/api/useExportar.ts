// T084 — useExportar: POST /api/v1/exportaciones → blob → descarga
import { useMutation } from '@tanstack/react-query'
import type { SolicitudExportacion } from '../modelo/solicitud'
import { iniciarDescarga, nombreArchivo } from '../modelo/descarga'
import { pedirApi } from '@/api/http'
import { armarError, type ErrorApi } from '@/api/problema'

export function useExportar() {
  return useMutation<void, ErrorApi, SolicitudExportacion>({
    mutationFn: async (solicitud) => {
      const respuesta = await pedirApi('/api/v1/exportaciones', {
        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(solicitud),
      })
      if (!respuesta.ok) throw await armarError(respuesta)
      iniciarDescarga(await respuesta.blob(), nombreDeLaSolicitud(solicitud))
    },
  })
}

/** «killauru-anomalias-20261101-20261130.csv»: el nombre dice qué rango de fechas contiene el archivo. */
const nombreDeLaSolicitud = (solicitud: SolicitudExportacion) =>
  nombreArchivo(solicitud.formato, solicitud.desdeUtc?.slice(0, 10) ?? '', solicitud.hastaUtc?.slice(0, 10) ?? '')
