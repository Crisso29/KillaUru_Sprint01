import { http, HttpResponse } from 'msw'
import datosPorRevisar from '../datos/porRevisar.json'
import datosPendientes from '../datos/pendientes.json'
import datosDetalle from '../datos/detalle_anomalia.json'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresMoederacion = [
  http.get(`${BASE}/api/v1/moderacion/por-revisar`, () =>
    HttpResponse.json(datosPorRevisar),
  ),
  http.get(`${BASE}/api/v1/moderacion/pendientes`, () =>
    HttpResponse.json(datosPendientes),
  ),
  http.post(`${BASE}/api/v1/anomalies/:id/moderacion`, () =>
    HttpResponse.json({ ...datosDetalle, estado: 'Reparada' }),
  ),
]
