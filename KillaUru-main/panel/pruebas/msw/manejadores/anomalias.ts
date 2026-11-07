import { http, HttpResponse } from 'msw'
import datosAnomalias from '../datos/anomalias.json'
import datosDetalle from '../datos/detalle_anomalia.json'
import datosTransiciones from '../datos/transiciones.json'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresAnomalias = [
  // Debe ir antes que /:id para que 'transiciones' no se capture como id
  http.get(`${BASE}/api/v1/anomalies/transiciones`, () =>
    HttpResponse.json(datosTransiciones),
  ),

  http.get(`${BASE}/api/v1/anomalies/:id`, ({ params }) => {
    if (params['id'] === datosDetalle.id) return HttpResponse.json(datosDetalle)
    const feature = datosAnomalias.features.find(f => f.properties.id === params['id'])
    if (!feature) return HttpResponse.json({ codigo: 'no_encontrado' }, { status: 404 })
    return HttpResponse.json(feature.properties)
  }),

  http.patch(`${BASE}/api/v1/anomalies/:id/estado`, () =>
    HttpResponse.json({ ok: true }),
  ),
]
