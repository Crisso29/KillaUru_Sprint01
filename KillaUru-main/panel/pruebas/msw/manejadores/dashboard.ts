import { http, HttpResponse } from 'msw'
import datosDashboard from '../datos/dashboard.json'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresDashboard = [
  http.get(`${BASE}/api/v1/dashboard/resumen`, () =>
    HttpResponse.json(datosDashboard),
  ),

  http.get(`${BASE}/api/v1/dashboard/senales-por-dia`, () =>
    HttpResponse.json(datosDashboard.senales_por_dia ?? []),
  ),
]
