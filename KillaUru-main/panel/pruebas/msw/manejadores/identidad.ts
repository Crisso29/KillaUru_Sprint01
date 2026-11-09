import { http, HttpResponse } from 'msw'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresIdentidad = [
  http.post(`${BASE}/api/v1/auth/iniciar`, () =>
    HttpResponse.json({ acceso_vence_utc: new Date(Date.now() + 5 * 60 * 1000).toISOString() }),
  ),

  http.post(`${BASE}/api/v1/auth/renovar`, () =>
    HttpResponse.json({ acceso_vence_utc: new Date(Date.now() + 5 * 60 * 1000).toISOString() }),
  ),

  http.post(`${BASE}/api/v1/auth/cerrar`, () =>
    HttpResponse.json({}),
  ),

  http.get(`${BASE}/api/v1/cuenta/perfil`, () =>
    HttpResponse.json({ correo: 'ops@killauru.pe', nombre: 'Operador Demo', rol: 'admin' }),
  ),
]
