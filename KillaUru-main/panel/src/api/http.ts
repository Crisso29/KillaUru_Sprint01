// T022, FR-006, FR-007 — la ÚNICA puerta del panel hacia la API (lo exige llamadasHttp.arquitectura.test).
// - Siempre con la cookie de sesión (`credentials: 'include'`) y nunca con cabecera Authorization: el token es HttpOnly.
// - Ante un 401 `token_invalido` renueva una vez (colapsando renovaciones simultáneas, ver sesion/renovador) y reintenta;
//   si vuelve a dar 401, avisa que la sesión terminó. Los 401 del login (credenciales, bloqueo) no son tokens vencidos.
// La sesión se conecta con `configurarSesionHttp` desde ProveedorSesion: así esta capa no depende de React.
import { entorno } from '@/config/entorno'
import { armarError, type ErrorApi } from './problema'

export interface SesionHttp {
  renovar: () => Promise<unknown>
  onSesionTerminada: () => void
}

export interface OpcionesPedido {
  /** false solo para las llamadas de la propia sesión (renovar, cerrar): no pueden renovarse a sí mismas. */
  renovarAnte401?: boolean
}

const NO_RENUEVA = new Set(['credenciales_invalidas', 'cuenta_bloqueada', 'solo_administradores'])

let sesion: SesionHttp | null = null

export function configurarSesionHttp(nueva: SesionHttp | null): void {
  sesion = nueva
}

export async function pedirApi(ruta: string, init: RequestInit = {}, opciones: OpcionesPedido = {}): Promise<Response> {
  const pedir = () => fetch(`${entorno.urlApi}${ruta}`, { ...init, credentials: 'include' })
  const respuesta = await pedir()
  if (respuesta.status !== 401 || opciones.renovarAnte401 === false || !sesion) return respuesta
  if (NO_RENUEVA.has((await armarError(respuesta)).codigo)) return respuesta
  return reintentarTrasRenovar(pedir, sesion, respuesta)
}

/** Como `pedirApi`, pero devuelve el JSON o lanza el {@link ErrorApi} del `Problema` (contrato 001). */
export async function pedirJson<T>(ruta: string, init: RequestInit = {}): Promise<T> {
  const respuesta = await pedirApi(ruta, init)
  if (!respuesta.ok) throw await armarError(respuesta) satisfies ErrorApi
  return await respuesta.json() as T
}

async function reintentarTrasRenovar(pedir: () => Promise<Response>, actual: SesionHttp, original: Response): Promise<Response> {
  try {
    await actual.renovar()
  } catch {
    actual.onSesionTerminada()
    return original
  }
  const reintento = await pedir()
  if (reintento.status === 401) actual.onSesionTerminada()
  return reintento
}
