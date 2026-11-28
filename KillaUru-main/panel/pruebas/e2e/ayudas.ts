// T012: ayudas reutilizables para pruebas e2e Playwright contra el build simulado.
// page.route() intercepta la API antes del Service Worker — funciona en todos los navegadores (incluido Firefox CI
// donde navigator.serviceWorker.controller es siempre null por el protocolo CDP de Playwright).
// createRequire: Playwright ejecuta test files como ESM nativo de Node.js; los import de JSON requieren
// «with { type: "json" }» en ESM, pero createRequire(CJS) los carga sin atributos en cualquier versión.
import { createRequire } from 'node:module'
import { expect, type Page, type Route } from '@playwright/test'

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

/** Lo que el modo simulado deja en `window` (pruebas/msw/navegador.ts → exponerParaPruebas). */
interface Simulado {
  worker: { use: (...manejadores: unknown[]) => void }
  http: Record<'get' | 'post' | 'put' | 'delete', (ruta: string, resolver: () => Response | undefined) => unknown>
  HttpResponse: { json: (cuerpo: unknown, init?: ResponseInit) => Response }
  base: string
}

declare global {
  interface Window { __killauruSimulado?: Simulado }
}

/** Reglas RN-07 replicadas en Node.js (el simulador del navegador usa la misma tabla). */
const TRANSICIONES: Record<string, { desde: string[]; hacia: string }> = {
  MarcarReparada: { desde: ['Activa', 'PendienteConfirmacion'], hacia: 'Reparada' },
  Descartar: { desde: ['Activa', 'PendienteConfirmacion', 'Expirada'], hacia: 'Descartada' },
  Reabrir: { desde: ['Reparada', 'Descartada'], hacia: 'Activa' },
  MantenerDescartada: { desde: ['Descartada'], hacia: 'Descartada' },
}

/** Estado en memoria de la sesión simulada: mismo comportamiento que anomaliasSimuladas.ts pero en Node.js. */
function crearEstado() {
  const anomalias: Anomalia[] = structuredClone(anomaliasJson.features)
  const porRevisar = new Set(porRevisarJson.map((e) => e.id))
  const buscar = (id: string) => anomalias.find((a) => a.properties.id === id)

  const detalleDe = (id: string): Record<string, unknown> | null => {
    const a = buscar(id)
    if (!a) return null
    const { severidad, ...propiedades } = a.properties
    return {
      ...detalleBaseJson,
      ...propiedades,
      severidadMostrada: severidad,
      porRevisar: porRevisar.has(id),
    }
  }

  return {
    sesionPanel: () => ({
      expiraAccesoUtc: new Date(Date.now() + 5 * 60_000).toISOString(),
      sesionVenceUtc: new Date(Date.now() + 8 * 3600_000).toISOString(),
      rol: 'Administrador',
    }),
    listar(params: URLSearchParams) {
      const col = (n: string) => params.get(n)?.split(',').filter(Boolean) ?? []
      const estados = col('estado')
      const tipos = col('tipo')
      const severidades = col('severidad').map(Number)
      return {
        ...anomaliasJson,
        features: anomalias.filter(
          (a) =>
            (!estados.length || estados.includes(a.properties.estado)) &&
            (!tipos.length || tipos.includes(a.properties.tipo)) &&
            (!severidades.length || severidades.includes(a.properties.severidad)),
        ),
      }
    },
    detalleDe,
    moderar(id: string, accion: string): { estado: number; cuerpo: Record<string, unknown> } {
      const a = buscar(id)
      if (!a) return { estado: 404, cuerpo: { codigo: 'no_encontrado' } }
      const regla = TRANSICIONES[accion]
      const permitida =
        regla?.desde.includes(a.properties.estado) &&
        (accion !== 'MantenerDescartada' || porRevisar.has(id))
      if (!regla || !permitida) return { estado: 409, cuerpo: { codigo: 'transicion_no_permitida' } }
      a.properties.estado = regla.hacia as typeof a.properties.estado
      porRevisar.delete(id)
      return { estado: 200, cuerpo: detalleDe(id)! }
    },
    porRevisarActual: (): PorRevisarItem[] =>
      porRevisarJson.filter((e) => porRevisar.has(e.id)),
  }
}

/** Script de Turnstile simulado — igual al del manejador MSW (sin red). */
const TURNSTILE_SCRIPT = `(() => {
  const w = {}; let n = 0
  const ok = (id) => setTimeout(() => w[id]?.callback('token-simulado-' + (++n)), 0)
  window.turnstile = {
    render: (c, o) => { const id = 'w' + Object.keys(w).length; w[id] = o; ok(id); return id },
    reset: (id) => ok(id),
  }
  if (typeof window.onTurnstileLoad === 'function') window.onTurnstileLoad()
})()`

/** Registra page.route() para toda la API simulada. Las rutas se evalúan de más reciente a más antigua (LIFO). */
async function registrarRutas(page: Page) {
  const sim = crearEstado()

  await page.route('https://challenges.cloudflare.com/turnstile/v0/api.js', (route) =>
    route.fulfill({ body: TURNSTILE_SCRIPT, contentType: 'text/javascript; charset=utf-8' }),
  )

  await page.route('**/api/v1/**', async (route: Route) => {
    const req = route.request()
    const url = new URL(req.url())
    const m = req.method().toUpperCase()
    const p = url.pathname

    // ── auth ──────────────────────────────────────────────────────────────────
    if (m === 'POST' && (p.endsWith('/auth/inicio-sesion') || p.endsWith('/auth/renovar')))
      return route.fulfill({ json: sim.sesionPanel() })
    if (m === 'POST' && p.endsWith('/auth/cerrar-sesion'))
      return route.fulfill({ status: 204 })
    if (m === 'POST' && (p.endsWith('/auth/verificar-correo') || p.endsWith('/auth/fijar-contrasena')))
      return route.fulfill({ status: 204 })
    if (m === 'POST' && p.endsWith('/auth/recuperar'))
      return route.fulfill({ status: 202 })
    if (m === 'GET' && p.endsWith('/me'))
      return route.fulfill({ json: { correo: 'admin.operaciones@killauru.pe', rol: 'Administrador', fechaAltaUtc: '2026-01-15T14:00:00Z', correoVerificado: true, aceptaciones: [] } })

    // ── anomalías — orden: transiciones → moderacion → detalle → lista ────────
    if (m === 'GET' && p.endsWith('/anomalies/transiciones'))
      return route.fulfill({ json: transicionesJson })
    if (m === 'POST' && /\/anomalies\/[^/]+\/moderacion$/.test(p)) {
      const id = p.split('/').at(-2)!
      const { accion } = JSON.parse((await req.postData()) ?? '{}') as { accion: string }
      const r = sim.moderar(id, accion)
      return route.fulfill({ status: r.estado, json: r.cuerpo })
    }
    if (m === 'GET' && /\/anomalies\/[^/]+$/.test(p)) {
      const id = p.split('/').at(-1)!
      const detalle = sim.detalleDe(id)
      return detalle
        ? route.fulfill({ json: detalle })
        : route.fulfill({ status: 404, json: { codigo: 'no_encontrado' } })
    }
    if (m === 'GET' && p.endsWith('/anomalies'))
      return route.fulfill({ json: sim.listar(url.searchParams) })

    // ── moderación ────────────────────────────────────────────────────────────
    if (m === 'GET' && p.endsWith('/moderacion/por-revisar'))
      return route.fulfill({ json: sim.porRevisarActual() })
    if (m === 'GET' && p.endsWith('/moderacion/pendientes'))
      return route.fulfill({ json: pendientesJson })

    // ── dashboard ─────────────────────────────────────────────────────────────
    if (m === 'GET' && p.endsWith('/dashboard/indicadores'))
      return route.fulfill({ json: dashboardJson })

    // ── auditoría ─────────────────────────────────────────────────────────────
    if (m === 'GET' && p.endsWith('/auditoria')) {
      const accion = url.searchParams.get('accion')
      const categoria = url.searchParams.get('categoria')
      const actorId = url.searchParams.get('actorId')
      let elementos = auditoriaJson.elementos
      if (accion) elementos = elementos.filter((e) => e.accion === accion)
      if (categoria) elementos = elementos.filter((e) => e.categoria === categoria)
      if (actorId) elementos = elementos.filter((e) => e.actorId === actorId)
      return route.fulfill({ json: { ...auditoriaJson, elementos, total: elementos.length } })
    }

    // ── usuarios ──────────────────────────────────────────────────────────────
    if (m === 'GET' && p.endsWith('/usuarios/revision-privilegios'))
      return route.fulfill({ json: revisionJson })
    if (m === 'POST' && p.endsWith('/usuarios/revision-privilegios'))
      return route.fulfill({ status: 201, json: revisionJson })
    if (m === 'GET' && p.endsWith('/usuarios')) {
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
    if (m === 'POST' && p.endsWith('/usuarios'))
      return route.fulfill({ status: 201, json: { id: 'nuevo-0000-0000-0000-000000000001', correo: 'nuevo@killauru.pe', rol: 'ConductorRegistrado', estado: 'InvitacionPendiente', correoVerificado: false, fechaAltaUtc: '2026-11-05T00:00:00Z', ultimaSesionUtc: null } })
    if (m === 'PUT' && /\/usuarios\/[^/]+\/rol$/.test(p))
      return route.fulfill({ status: 204 })
    if (m === 'POST' && /\/usuarios\/[^/]+\/(desactivar|activar)$/.test(p))
      return route.fulfill({ status: 204 })
    if (m === 'POST' && /\/usuarios\/[^/]+\/reenviar-correo$/.test(p))
      return route.fulfill({ status: 202 })

    // ── exportaciones ─────────────────────────────────────────────────────────
    if (m === 'POST' && p.endsWith('/exportaciones')) {
      const { formato } = JSON.parse((await req.postData()) ?? '{}') as { formato: string }
      if (formato === 'GeoJSON')
        return route.fulfill({ status: 200, headers: { 'Content-Type': 'application/geo+json', 'Content-Disposition': 'attachment; filename="killauru-anomalias.geojson"' }, body: '{"type":"FeatureCollection","features":[]}' })
      return route.fulfill({ status: 200, headers: { 'Content-Type': 'text/csv', 'Content-Disposition': 'attachment; filename="killauru-anomalias.csv"' }, body: 'id,tipo,severidad,estado\n' })
    }

    // ── fallback ──────────────────────────────────────────────────────────────
    return route.fulfill({ status: 404, json: { codigo: 'no_encontrado' } })
  })
}

/** Abre una ruta y espera a que el simulador (MSW) esté listo. */
export async function abrir(page: Page, ruta: string) {
  await registrarRutas(page)
  await page.goto(ruta)
  await page.waitForFunction(() => Boolean(window.__killauruSimulado))
}

export interface Escenario {
  metodo: 'get' | 'post' | 'put' | 'delete'
  /** Ruta de la API sin la base, p. ej. `/api/v1/auth/inicio-sesion`; admite `:id`. */
  ruta: string
  estado: number
  cuerpo?: unknown
  /** Cuántas veces responde así antes de volver a la respuesta normal; sin valor, siempre. */
  veces?: number
}

/**
 * Cambia la respuesta simulada de una ruta hasta recargar la página. El último escenario agregado tiene prioridad
 * (Playwright evalúa rutas en orden LIFO); agotadas sus `veces`, la petición pasa al anterior.
 */
export async function simularRespuesta(page: Page, escenario: Escenario) {
  const { metodo, ruta, estado, cuerpo, veces } = escenario
  const pattern = `**${ruta.replace(/:([^/]+)/g, '*')}`
  const handler = async (route: Route) => {
    if (cuerpo !== undefined) {
      await route.fulfill({ status: estado, json: cuerpo as Record<string, unknown> })
    } else {
      await route.fulfill({ status: estado })
    }
  }
  if (veces !== undefined) {
    await page.route(pattern, handler, { times: veces })
  } else {
    await page.route(pattern, handler)
  }
}

/** Login completo por la pantalla: el Turnstile simulado entrega el token y se espera el Resumen. */
export async function iniciarSesion(page: Page, correo = 'ops@killauru.pe', contrasena = 'Demo1234!') {
  await completarLogin(page, correo, contrasena)
  await page.getByRole('button', { name: 'Entrar' }).click()
  await page.waitForURL('**/resumen')
}

/** Escribe las credenciales y espera a que el desafío anti-bot habilite «Entrar» (sin pulsarlo). */
export async function completarLogin(page: Page, correo = 'ops@killauru.pe', contrasena = 'Demo1234!') {
  if (!page.url().includes('/login')) await abrir(page, '/login')
  await page.getByLabel('Correo').fill(correo)
  await page.getByLabel('Contraseña').fill(contrasena)
  await expect(page.getByRole('button', { name: 'Entrar' })).toBeEnabled()
}
