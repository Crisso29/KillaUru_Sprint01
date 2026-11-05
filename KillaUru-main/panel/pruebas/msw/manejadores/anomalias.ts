import { http, HttpResponse } from 'msw'
import datosAnomalias from '../datos/anomalias.json'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresAnomalias = [
  http.get(`${BASE}/api/v1/anomalias`, () =>
    HttpResponse.json(datosAnomalias),
  ),

  http.get(`${BASE}/api/v1/anomalias/:id`, ({ params }) =>
    HttpResponse.json(datosAnomalias.items?.find((a: Record<string, unknown>) => a['id'] === params['id']) ?? null),
  ),

  http.patch(`${BASE}/api/v1/anomalias/:id/estado`, () =>
    HttpResponse.json({ ok: true }),
  ),
]
