import { http, HttpResponse } from 'msw'
import { simulador } from '../simulador/estado'
import datosTransiciones from '../datos/transiciones.json'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresAnomalias = [
  // Debe ir antes que /:id para que 'transiciones' no se capture como id
  http.get(`${BASE}/api/v1/anomalies/transiciones`, () =>
    HttpResponse.json(datosTransiciones),
  ),

  http.get(`${BASE}/api/v1/anomalies/:id`, ({ params }) => {
    const detalle = simulador.detalle(String(params['id']))
    return detalle ? HttpResponse.json(detalle) : HttpResponse.json({ codigo: 'no_encontrado' }, { status: 404 })
  }),
]
