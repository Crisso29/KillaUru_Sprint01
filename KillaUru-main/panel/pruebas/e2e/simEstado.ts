// createRequire: Playwright usa ESM nativo; los import JSON requieren «with { type: "json" }» en ESM.
import { createRequire } from 'node:module'
import { type Page, type Route } from '@playwright/test'

const _req = createRequire(import.meta.url)

interface AnomaliaPropiedades {
  id: string; kmVia: number; tipo: string; severidad: number
  persistencia: string; ultimaDeteccionUtc: string; venceUtc: string; estado: string
}
type Anomalia = { type: string; id: string; geometry: unknown; properties: AnomaliaPropiedades }
type AnomaliaData = { type: string; generadoUtc: string; features: Anomalia[] }
type PorRevisarItem = { id: string; [k: string]: unknown }
type AuditoriaItem = { accion: string; categoria: string; actorId: string; [k: string]: unknown }

const anomaliasJson = _req('../msw/datos/anomalias.json') as AnomaliaData
const detalleBaseJson = _req('../msw/datos/detalle_anomalia.json') as Record<string, unknown>
const porRevisarJson = _req('../msw/datos/porRevisar.json') as PorRevisarItem[]
const transicionesJson: unknown = _req('../msw/datos/transiciones.json')
const dashboardJson: unknown = _req('../msw/datos/dashboard.json')
const auditoriaJson = _req('../msw/datos/auditoria.json') as { elementos: AuditoriaItem[]; [k: string]: unknown }
const usuariosJson: unknown = _req('../msw/datos/usuarios.json')
const revisionJson: unknown = _req('../msw/datos/revision.json')
const pendientesJson: unknown = _req('../msw/datos/pendientes.json')

declare global { interface Window { __killauruSimulado?: unknown } }

const TRANSICIONES: Record<string, { desde: string[]; hacia: string }> = {
  MarcarReparada: { desde: ['Activa', 'PendienteConfirmacion'], hacia: 'Reparada' },
  Descartar: { desde: ['Activa', 'PendienteConfirmacion', 'Expirada'], hacia: 'Descartada' },
  Reabrir: { desde: ['Reparada', 'Descartada'], hacia: 'Activa' },
  MantenerDescartada: { desde: ['Descartada'], hacia: 'Descartada' },
}

function crearEstado() {
  const anomalias: Anomalia[] = structuredClone(anomaliasJson.features)
  const porRevisar = new Set(porRevisarJson.map((item) => item.id))
  const buscar = (id: string) => anomalias.find((feat) => feat.properties.id === id)
  const detalleDe = (id: string): Record<string, unknown> | null => {
    const feat = buscar(id)
    if (!feat) return null
    const { severidad, ...propiedades } = feat.properties
    return { ...detalleBaseJson, ...propiedades, severidadMostrada: severidad, porRevisar: porRevisar.has(id) }
  }
  return {
    sesionPanel: () => ({
      expiraAccesoUtc: new Date(Date.now() + 5 * 60_000).toISOString(),
      sesionVenceUtc: new Date(Date.now() + 8 * 3600_000).toISOString(),
      rol: 'Administrador',
    }),
    listar(params: URLSearchParams) {
      const col = (nombre: string) => params.get(nombre)?.split(',').filter(Boolean) ?? []
      const estados = col('estado')
      const tipos = col('tipo')
      const severidades = col('severidad').map(Number)
      return {
        ...anomaliasJson,
        features: anomalias.filter(
          (feat) =>
            (!estados.length || estados.includes(feat.properties.estado)) &&
            (!tipos.length || tipos.includes(feat.properties.tipo)) &&
            (!severidades.length || severidades.includes(feat.properties.severidad)),
        ),
      }
    },
    detalleDe,
    moderar(id: string, accion: string): { estado: number; cuerpo: Record<string, unknown> } {
      const feat = buscar(id)
      if (!feat) return { estado: 404, cuerpo: { codigo: 'no_encontrado' } }
      const regla = TRANSICIONES[accion]
      const permitida = regla?.desde.includes(feat.properties.estado) && (accion !== 'MantenerDescartada' || porRevisar.has(id))
      if (!regla || !permitida) return { estado: 409, cuerpo: { codigo: 'transicion_no_permitida' } }
      feat.properties.estado = regla.hacia as typeof feat.properties.estado
      porRevisar.delete(id)
      return { estado: 200, cuerpo: detalleDe(id)! }
    },
    porRevisarActual: (): PorRevisarItem[] => porRevisarJson.filter((item) => porRevisar.has(item.id)),
  }
}

const TURNSTILE_SCRIPT = `(() => {
  const widgets = {}; let cnt = 0
  const cb = (id) => setTimeout(() => widgets[id]?.callback('token-simulado-' + (++cnt)), 0)
  window.turnstile = {
    render: (el, opts) => { const id = 'w' + Object.keys(widgets).length; widgets[id] = opts; cb(id); return id },
    reset: (id) => cb(id),
  }
  if (typeof window.onTurnstileLoad === 'function') window.onTurnstileLoad()
})()`

export async function registrarRutas(page: Page) {
  const sim = crearEstado()
  await page.route('https://challenges.cloudflare.com/turnstile/v0/api.js', (route) =>
    route.fulfill({ body: TURNSTILE_SCRIPT, contentType: 'text/javascript; charset=utf-8' }),
  )
  await page.route('**/api/v1/**', async (route: Route) => {
    const req = route.request()
    const url = new URL(req.url())
    const mth = req.method().toUpperCase()
    const pathname = url.pathname
    if (mth === 'POST' && (pathname.endsWith('/auth/inicio-sesion') || pathname.endsWith('/auth/renovar')))
      return route.fulfill({ json: sim.sesionPanel() })
    if (mth === 'POST' && pathname.endsWith('/auth/cerrar-sesion')) return route.fulfill({ status: 204 })
    if (mth === 'POST' && (pathname.endsWith('/auth/verificar-correo') || pathname.endsWith('/auth/fijar-contrasena')))
      return route.fulfill({ status: 204 })
    if (mth === 'POST' && pathname.endsWith('/auth/recuperar')) return route.fulfill({ status: 202 })
    if (mth === 'GET' && pathname.endsWith('/me'))
      return route.fulfill({ json: { correo: 'admin.operaciones@killauru.pe', rol: 'Administrador', fechaAltaUtc: '2026-01-15T14:00:00Z', correoVerificado: true, aceptaciones: [] } })
    if (mth === 'GET' && pathname.endsWith('/anomalies/transiciones'))
      return route.fulfill({ json: transicionesJson })
    if (mth === 'POST' && /\/anomalies\/[^/]+\/moderacion$/.test(pathname)) {
      const id = pathname.split('/').at(-2)!
      const { accion } = JSON.parse((await req.postData()) ?? '{}') as { accion: string }
      const resultado = sim.moderar(id, accion)
      return route.fulfill({ status: resultado.estado, json: resultado.cuerpo })
    }
    if (mth === 'GET' && /\/anomalies\/[^/]+$/.test(pathname)) {
      const id = pathname.split('/').at(-1)!
      const detalle = sim.detalleDe(id)
      return detalle
        ? route.fulfill({ json: detalle })
        : route.fulfill({ status: 404, json: { codigo: 'no_encontrado' } })
    }
    if (mth === 'GET' && pathname.endsWith('/anomalies'))
      return route.fulfill({ json: sim.listar(url.searchParams) })
    if (mth === 'GET' && pathname.endsWith('/moderacion/por-revisar'))
      return route.fulfill({ json: sim.porRevisarActual() })
    if (mth === 'GET' && pathname.endsWith('/moderacion/pendientes'))
      return route.fulfill({ json: pendientesJson })
    if (mth === 'GET' && pathname.endsWith('/dashboard/indicadores'))
      return route.fulfill({ json: dashboardJson })
    if (mth === 'GET' && pathname.endsWith('/auditoria')) {
      const accion = url.searchParams.get('accion')
      const categoria = url.searchParams.get('categoria')
      const actorId = url.searchParams.get('actorId')
      let elementos = auditoriaJson.elementos
      if (accion) elementos = elementos.filter((elem) => elem.accion === accion)
      if (categoria) elementos = elementos.filter((elem) => elem.categoria === categoria)
      if (actorId) elementos = elementos.filter((elem) => elem.actorId === actorId)
      return route.fulfill({ json: { ...auditoriaJson, elementos, total: elementos.length } })
    }
    if (mth === 'GET' && pathname.endsWith('/usuarios/revision-privilegios'))
      return route.fulfill({ json: revisionJson })
    if (mth === 'POST' && pathname.endsWith('/usuarios/revision-privilegios'))
      return route.fulfill({ status: 201, json: revisionJson })
    if (mth === 'GET' && pathname.endsWith('/usuarios')) {
      const ids = url.searchParams.get('ids')?.split(',').filter(Boolean) ?? []
      if (ids.length > 0) {
        const elementos = ids.map((id) => ({
          id, correo: `actor-${id.replace(/-/g, '').slice(0, 8)}@killauru.pe`,
          rol: 'Administrador', estado: 'Activa', correoVerificado: true,
          fechaAltaUtc: '2026-01-01T00:00:00Z', ultimaSesionUtc: null,
        }))
        return route.fulfill({ json: { elementos, pagina: 1, tamano: 200, total: elementos.length } })
      }
      return route.fulfill({ json: usuariosJson })
    }
    if (mth === 'POST' && pathname.endsWith('/usuarios'))
      return route.fulfill({ status: 201, json: { id: 'nuevo-0000-0000-0000-000000000001', correo: 'nuevo@killauru.pe', rol: 'ConductorRegistrado', estado: 'InvitacionPendiente', correoVerificado: false, fechaAltaUtc: '2026-11-05T00:00:00Z', ultimaSesionUtc: null } })
    if (mth === 'PUT' && /\/usuarios\/[^/]+\/rol$/.test(pathname)) return route.fulfill({ status: 204 })
    if (mth === 'POST' && /\/usuarios\/[^/]+\/(desactivar|activar)$/.test(pathname)) return route.fulfill({ status: 204 })
    if (mth === 'POST' && /\/usuarios\/[^/]+\/reenviar-correo$/.test(pathname)) return route.fulfill({ status: 202 })
    if (mth === 'POST' && pathname.endsWith('/exportaciones')) {
      const { formato } = JSON.parse((await req.postData()) ?? '{}') as { formato: string }
      if (formato === 'GeoJSON')
        return route.fulfill({ status: 200, headers: { 'Content-Type': 'application/geo+json', 'Content-Disposition': 'attachment; filename="killauru-anomalias.geojson"' }, body: '{"type":"FeatureCollection","features":[]}' })
      return route.fulfill({ status: 200, headers: { 'Content-Type': 'text/csv', 'Content-Disposition': 'attachment; filename="killauru-anomalias.csv"' }, body: 'id,tipo,severidad,estado\n' })
    }
    return route.fulfill({ status: 404, json: { codigo: 'no_encontrado' } })
  })
}
