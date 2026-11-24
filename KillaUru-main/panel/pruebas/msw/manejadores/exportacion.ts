import { http, HttpResponse } from 'msw'

const BASE = import.meta.env?.VITE_URL_API ?? 'https://api.killauru.pe'

const CSV_EJEMPLO = `id,tipo,severidad,estado,persistencia,km,latitud,longitud,reportes,primer_reporte,ultimo_reporte
b2b2b2b2-0000-0000-0000-000000000001,Bache,3,Activa,5,15.3,-13.5234,-72.6845,4,2026-10-01T10:00:00Z,2026-10-13T14:00:00Z
c3c3c3c3-0000-0000-0000-000000000002,Hundimiento,2,Reparada,3,8.1,-13.5890,-72.6901,2,2026-10-05T08:00:00Z,2026-10-12T09:00:00Z`

const GEOJSON_EJEMPLO = JSON.stringify({
  type: 'FeatureCollection',
  features: [{ type: 'Feature', geometry: { type: 'Point', coordinates: [-72.6845, -13.5234] }, properties: { id: 'b2b2b2b2-0000-0000-0000-000000000001', tipo: 'Bache', severidad: 3, estado: 'Activa' } }],
})

/** Como el backend: CSV o GeoJSON según el `formato` pedido, con su tipo de contenido y extensión. */
const ARCHIVOS = {
  CSV: { contenido: CSV_EJEMPLO, tipo: 'text/csv', nombre: 'killauru-anomalias.csv' },
  GeoJSON: { contenido: GEOJSON_EJEMPLO, tipo: 'application/geo+json', nombre: 'killauru-anomalias.geojson' },
}

export const manejadoresExportacion = [
  http.post(`${BASE}/api/v1/exportaciones`, async ({ request }) => {
    const { formato } = await request.json() as { formato: keyof typeof ARCHIVOS }
    const archivo = ARCHIVOS[formato] ?? ARCHIVOS.CSV
    return new HttpResponse(archivo.contenido, {
      status: 200,
      headers: { 'Content-Type': archivo.tipo, 'Content-Disposition': `attachment; filename="${archivo.nombre}"` },
    })
  }),
]
