import { http, HttpResponse } from 'msw'
import datosAnomalias from '../datos/anomalias.json'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresMapa = [
  http.get(`${BASE}/api/v1/mapa/anomalias`, () =>
    HttpResponse.json(datosAnomalias),
  ),
]
