import { http, HttpResponse } from 'msw'
import datosAuditoria from '../datos/auditoria.json'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresAuditoria = [
  http.get(`${BASE}/api/v1/auditoria`, ({ request }) => {
    const url = new URL(request.url)
    const accion = url.searchParams.get('accion')
    const categoria = url.searchParams.get('categoria')
    const actorId = url.searchParams.get('actorId')

    let elementos = datosAuditoria.elementos as typeof datosAuditoria.elementos
    if (accion) elementos = elementos.filter((e) => e.accion === accion)
    if (categoria) elementos = elementos.filter((e) => e.categoria === categoria)
    if (actorId) elementos = elementos.filter((e) => e.actorId === actorId)

    return HttpResponse.json({ ...datosAuditoria, elementos, total: elementos.length })
  }),
]
