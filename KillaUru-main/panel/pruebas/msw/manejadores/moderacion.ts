import { http, HttpResponse } from 'msw'
import datosPendientes from '../datos/pendientes.json'
import { simulador } from '../simulador/estado'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresMoederacion = [
  http.get(`${BASE}/api/v1/moderacion/por-revisar`, () => HttpResponse.json(simulador.porRevisar())),
  http.get(`${BASE}/api/v1/moderacion/pendientes`, () => HttpResponse.json(datosPendientes)),
  // RN-07 como el backend: una transición no permitida responde 409 transicion_no_permitida
  http.post(`${BASE}/api/v1/anomalies/:id/moderacion`, async ({ params, request }) => {
    const { accion } = await request.json() as { accion: string }
    const { estado, cuerpo } = simulador.moderar(String(params['id']), accion)
    return HttpResponse.json(cuerpo, { status: estado })
  }),
]
