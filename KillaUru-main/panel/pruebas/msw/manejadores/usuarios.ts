import { http, HttpResponse } from 'msw'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresUsuarios = [
  http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
    const url = new URL(request.url)
    const ids = url.searchParams.get('ids')?.split(',').filter(Boolean) ?? []
    const items = ids.map(id => ({ id, correo: `actor-${id.replace(/-/g, '').slice(0, 8)}@killauru.pe` }))
    return HttpResponse.json({ items, total: items.length, pagina: 1, tamano: 50 })
  }),
]
