import { http, HttpResponse } from 'msw'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

/** `SesionPanel` del contrato 003: acceso de 5 min y sesión de 8 h desde el login (las cookies no se simulan). */
const inicio = Date.now()
const sesionPanel = () => ({
  expiraAccesoUtc: new Date(Date.now() + 5 * 60_000).toISOString(),
  sesionVenceUtc: new Date(inicio + 8 * 3600_000).toISOString(),
  rol: 'Administrador',
})

export const manejadoresIdentidad = [
  http.post(`${BASE}/api/v1/auth/inicio-sesion`, () => HttpResponse.json(sesionPanel())),
  http.post(`${BASE}/api/v1/auth/renovar`, () => HttpResponse.json(sesionPanel())),
  http.post(`${BASE}/api/v1/auth/cerrar-sesion`, () => new HttpResponse(null, { status: 204 })),
  // `MisDatos` del contrato 003.
  http.get(`${BASE}/api/v1/me`, () => HttpResponse.json({
    correo: 'admin.operaciones@killauru.pe', rol: 'Administrador', fechaAltaUtc: '2026-01-15T14:00:00Z',
    correoVerificado: true, aceptaciones: [],
  })),
]
