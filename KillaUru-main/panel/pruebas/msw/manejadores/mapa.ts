import { http, HttpResponse } from 'msw'
import { simulador } from '../simulador/estado'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

// Filtra por estado, tipo y severidad como el backend (contrato 006)
export const manejadoresMapa = [
  http.get(`${BASE}/api/v1/anomalies`, ({ request }) =>
    HttpResponse.json(simulador.listar(new URL(request.url).searchParams)),
  ),
]
