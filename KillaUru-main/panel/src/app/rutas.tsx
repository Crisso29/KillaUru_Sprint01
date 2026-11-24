import { lazy, Suspense, type ReactNode } from 'react'
import { Routes, Route } from 'react-router'
import { useSesion } from '@/sesion/useSesion'
import { RutaProtegida } from './RutaProtegida'
import { PaginaNoEncontrada } from './PaginaNoEncontrada'
import { MarcoConectado } from './marco/MarcoConectado'
import { Esqueleto } from '@/compartido/ui'

// FR-001: rutas perezosas — cada funcionalidad se carga solo cuando se navega
const PaginaLogin = lazy(() => import('@/funcionalidades/login').then(modulo => ({ default: modulo.PaginaLogin })))
const PaginaResumen = lazy(() => import('./ResumenConectado').then(modulo => ({ default: modulo.ResumenConectado })))
const PaginaMapa = lazy(() => import('@/funcionalidades/mapa/pagina').then(modulo => ({ default: modulo.PaginaMapa })))
const PaginaModeracion = lazy(() => import('@/funcionalidades/moderacion').then(modulo => ({ default: modulo.PaginaModeracion })))
const PaginaUsuarios = lazy(() => import('@/funcionalidades/usuarios').then(modulo => ({ default: modulo.PaginaUsuarios })))
const PaginaAuditoria = lazy(() => import('@/funcionalidades/auditoria').then(modulo => ({ default: modulo.PaginaAuditoria })))
const PaginaExportacion = lazy(() => import('@/funcionalidades/exportacion').then(modulo => ({ default: modulo.PaginaExportacion })))
const PaginaVerificar = lazy(() => import('@/funcionalidades/cuenta').then(modulo => ({ default: modulo.PaginaVerificar })))
const PaginaFijarContrasena = lazy(() => import('@/funcionalidades/cuenta').then(modulo => ({ default: modulo.PaginaFijarContrasena })))
const PaginaRecuperar = lazy(() => import('@/funcionalidades/login').then(modulo => ({ default: modulo.PaginaRecuperar })))

const Cargando = () => <Esqueleto filas={4} alto={40} />

/**
 * Pantallas sin sesión: entrar y los enlaces de los correos. Estos últimos deben coincidir con lo que envía el backend
 * (`UrlBaseEnlaces` + `EmitirEnlaceCorreo.Ruta`, FR-014); lo comprueba `pruebas/contrato/enlacesCorreo.contrato.test.ts`.
 */
const PUBLICAS: Array<[string, ReactNode]> = [
  ['/login', <PaginaLogin />],
  ['/recuperar', <PaginaRecuperar />],
  ['/cuenta/verificar', <PaginaVerificar />],
  ['/cuenta/restablecer', <PaginaFijarContrasena modo="restablecer" />],
  ['/cuenta/invitacion', <PaginaFijarContrasena modo="invitacion" />],
]

/** Pantallas del administrador; usuarios y exportación necesitan saber quién está para mostrar «tú». */
const protegidas = (correoActual: string): Array<[string, ReactNode]> => [
  ['/resumen', <PaginaResumen />], ['/mapa', <PaginaMapa />], ['/moderacion', <PaginaModeracion />],
  ['/usuarios', <PaginaUsuarios correoActual={correoActual} />], ['/auditoria', <PaginaAuditoria />],
  ['/exportacion', <PaginaExportacion correoActual={correoActual} />], ['/', <PaginaResumen />],
]

export function Rutas() {
  const { sesion } = useSesion()
  const correoActual = sesion.tipo === 'activa' ? sesion.correo ?? '' : ''
  return (
    <Suspense fallback={<Cargando />}>
      <Routes>
        {PUBLICAS.map(([ruta, pagina]) => <Route key={ruta} path={ruta} element={pagina} />)}
        {/* Páginas protegidas dentro del armazón del prototipo (barra lateral y superior). */}
        <Route element={<RutaProtegida><MarcoConectado /></RutaProtegida>}>
          {protegidas(correoActual).map(([ruta, pagina]) => <Route key={ruta} path={ruta} element={pagina} />)}
        </Route>
        <Route path="*" element={<PaginaNoEncontrada />} />
      </Routes>
    </Suspense>
  )
}
