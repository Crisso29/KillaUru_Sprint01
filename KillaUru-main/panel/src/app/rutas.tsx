import { lazy, Suspense } from 'react'
import { Routes, Route } from 'react-router'
import { useSesion } from '@/sesion/useSesion'
import { RutaProtegida } from './RutaProtegida'
import { PaginaNoEncontrada } from './PaginaNoEncontrada'
import { MarcoConectado } from './marco/MarcoConectado'
import { Esqueleto } from '@/compartido/ui'

// FR-001: rutas perezosas — cada funcionalidad se carga solo cuando se navega
const PaginaLogin = lazy(() => import('@/funcionalidades/login').then(m => ({ default: m.PaginaLogin })))
const PaginaResumen = lazy(() => import('./ResumenConectado').then(m => ({ default: m.ResumenConectado })))
const PaginaMapa = lazy(() => import('@/funcionalidades/mapa/pagina').then(m => ({ default: m.PaginaMapa })))
const PaginaModeracion = lazy(() => import('@/funcionalidades/moderacion').then(m => ({ default: m.PaginaModeracion })))
const PaginaUsuarios = lazy(() => import('@/funcionalidades/usuarios').then(m => ({ default: m.PaginaUsuarios })))
const PaginaAuditoria = lazy(() => import('@/funcionalidades/auditoria').then(m => ({ default: m.PaginaAuditoria })))
const PaginaExportacion = lazy(() => import('@/funcionalidades/exportacion').then(m => ({ default: m.PaginaExportacion })))
const PaginaVerificar = lazy(() => import('@/funcionalidades/cuenta').then(m => ({ default: m.PaginaVerificar })))
const PaginaFijarContrasena = lazy(() => import('@/funcionalidades/cuenta').then(m => ({ default: m.PaginaFijarContrasena })))
const PaginaRecuperar = lazy(() => import('@/funcionalidades/login').then(m => ({ default: m.PaginaRecuperar })))

const Cargando = () => <Esqueleto filas={4} alto={40} />

export function Rutas() {
  const { sesion: estado } = useSesion()
  const correoActual = estado.tipo === 'activa' ? estado.correo ?? '' : ''
  return (
    <Suspense fallback={<Cargando />}>
      <Routes>
        <Route path="/login" element={<PaginaLogin />} />
        <Route path="/recuperar" element={<PaginaRecuperar />} />
        <Route path="/verificar" element={<PaginaVerificar />} />
        <Route path="/establecer-contrasena" element={<PaginaFijarContrasena />} />
        {/* Páginas protegidas dentro del armazón del prototipo (barra lateral y superior). */}
        <Route element={<RutaProtegida><MarcoConectado /></RutaProtegida>}>
          <Route path="/resumen" element={<PaginaResumen />} />
          <Route path="/mapa" element={<PaginaMapa />} />
          <Route path="/moderacion" element={<PaginaModeracion />} />
          <Route path="/usuarios" element={<PaginaUsuarios correoActual={correoActual} />} />
          <Route path="/auditoria" element={<PaginaAuditoria />} />
          <Route path="/exportacion" element={<PaginaExportacion correoActual={correoActual} />} />
          <Route path="/" element={<PaginaResumen />} />
        </Route>
        <Route path="*" element={<PaginaNoEncontrada />} />
      </Routes>
    </Suspense>
  )
}
