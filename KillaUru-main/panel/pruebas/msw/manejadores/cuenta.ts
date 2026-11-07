import { http, HttpResponse } from 'msw'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresCuenta = [
  http.post(`${BASE}/api/v1/auth/verificar-correo`, () =>
    new HttpResponse(null, { status: 204 }),
  ),
  http.post(`${BASE}/api/v1/auth/fijar-contrasena`, () =>
    new HttpResponse(null, { status: 204 }),
  ),
  http.post(`${BASE}/api/v1/auth/recuperar`, () =>
    new HttpResponse(null, { status: 202 }),
  ),
]
