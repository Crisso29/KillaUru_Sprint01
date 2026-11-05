import { http, HttpResponse } from 'msw'
import datosAuditoria from '../datos/auditoria.json'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

export const manejadoresAuditoria = [
  http.get(`${BASE}/api/v1/auditoria`, () =>
    HttpResponse.json(datosAuditoria),
  ),

  http.get(`${BASE}/api/v1/auditoria/exportar`, () =>
    new HttpResponse('fecha,accion,correo\n2026-01-01,login,ops@killauru.pe', {
      headers: { 'Content-Type': 'text/csv' },
    }),
  ),
]
