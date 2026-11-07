import { lazy, Suspense } from 'react'
import { Routes, Route } from 'react-router'
import { RutaProtegida } from './RutaProtegida'
import { PaginaNoEncontrada } from './PaginaNoEncontrada'
import { Esqueleto } from '@/compartido/ui'

// FR-001: rutas perezosas — cada funcionalidad se carga solo cuando se navega
const PaginaLogin = lazy(() => import('@/funcionalidades/login').then(m => ({ default: m.PaginaLogin })))
const PaginaResumen = lazy(() => import('@/funcionalidades/resumen').then(m => ({ default: m.PaginaResumen })))
const PaginaMapa = lazy(() => import('@/funcionalidades/mapa').then(m => ({ default: m.PaginaMapa })))
const PaginaModeracion = lazy(() => import('@/funcionalidades/moderacion').then(m => ({ default: m.PaginaModeracion })))
const PaginaUsuarios = lazy(() => import('@/funcionalidades/usuarios').then(m => ({ default: m.PaginaUsuarios })))
const PaginaAuditoria = lazy(() => import('@/funcionalidades/auditoria').then(m => ({ default: m.PaginaAuditoria })))
const PaginaExportacion = lazy(() => import('@/funcionalidades/exportacion').then(m => ({ default: m.PaginaExportacion })))
const PaginaVerificar = lazy(() => import('@/funcionalidades/cuenta').then(m => ({ default: m.PaginaVerificar })))
const PaginaFijarContrasena = lazy(() => import('@/funcionalidades/cuenta').then(m => ({ default: m.PaginaFijarContrasena })))
const PaginaRecuperar = lazy(() => import('@/funcionalidades/login').then(m => ({ default: m.PaginaRecuperar })))

const Cargando = () => <Esqueleto filas={4} alto={40} />

export function Rutas() {
  return (
    <Suspense fallback={<Cargando />}>
      <Routes>
        <Route path="/login" element={<PaginaLogin />} />
        <Route path="/recuperar" element={<PaginaRecuperar />} />
        <Route path="/verificar" element={<PaginaVerificar />} />
        <Route path="/establecer-contrasena" element={<PaginaFijarContrasena />} />
        <Route path="/resumen" element={<RutaProtegida><PaginaResumen /></RutaProtegida>} />
        <Route path="/mapa" element={<RutaProtegida><PaginaMapa /></RutaProtegida>} />
        <Route path="/moderacion" element={<RutaProtegida><PaginaModeracion /></RutaProtegida>} />
        <Route path="/usuarios" element={<RutaProtegida><PaginaUsuarios /></RutaProtegida>} />
        <Route path="/auditoria" element={<RutaProtegida><PaginaAuditoria /></RutaProtegida>} />
        <Route path="/exportacion" element={<RutaProtegida><PaginaExportacion /></RutaProtegida>} />
        <Route path="/" element={<RutaProtegida><PaginaResumen /></RutaProtegida>} />
        <Route path="*" element={<PaginaNoEncontrada />} />
      </Routes>
    </Suspense>
  )
}
