import { http, HttpResponse } from 'msw'
import datosUsuarios from '../datos/usuarios.json'
import datosRevision from '../datos/revision.json'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

const USUARIO_NUEVO = {
  id: 'nuevo-0000-0000-0000-000000000001',
  correo: 'nuevo@killauru.pe',
  rol: 'ConductorRegistrado',
  estado: 'InvitacionPendiente',
  correoVerificado: false,
  fechaAltaUtc: '2026-11-05T00:00:00Z',
  ultimaSesionUtc: null,
}

export const manejadoresUsuarios = [
  http.get(`${BASE}/api/v1/usuarios/revision-privilegios`, () =>
    HttpResponse.json(datosRevision),
  ),
  http.post(`${BASE}/api/v1/usuarios/revision-privilegios`, () =>
    HttpResponse.json(datosRevision, { status: 201 }),
  ),
  http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
    const url = new URL(request.url)
    const ids = url.searchParams.get('ids')?.split(',').filter(Boolean) ?? []
    if (ids.length > 0) {
      const elementos = ids.map(id => ({
        id,
        correo: `actor-${id.replace(/-/g, '').slice(0, 8)}@killauru.pe`,
        rol: 'Administrador',
        estado: 'Activa',
        correoVerificado: true,
        fechaAltaUtc: '2026-01-01T00:00:00Z',
        ultimaSesionUtc: null,
      }))
      return HttpResponse.json({ elementos, pagina: 1, tamano: 200, total: elementos.length })
    }
    return HttpResponse.json(datosUsuarios)
  }),
  http.post(`${BASE}/api/v1/usuarios`, () =>
    HttpResponse.json(USUARIO_NUEVO, { status: 201 }),
  ),
  http.put(`${BASE}/api/v1/usuarios/:id/rol`, () =>
    new HttpResponse(null, { status: 204 }),
  ),
  http.post(`${BASE}/api/v1/usuarios/:id/desactivar`, () =>
    new HttpResponse(null, { status: 204 }),
  ),
  http.post(`${BASE}/api/v1/usuarios/:id/activar`, () =>
    new HttpResponse(null, { status: 204 }),
  ),
  http.post(`${BASE}/api/v1/usuarios/:id/reenviar-correo`, () =>
    new HttpResponse(null, { status: 202 }),
  ),
]
