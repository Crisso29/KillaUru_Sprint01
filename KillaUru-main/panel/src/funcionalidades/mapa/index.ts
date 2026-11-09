// T048 — exports públicos de la funcionalidad mapa
import { lazy } from 'react'

export const PaginaMapa = lazy(() => import('./ui/PaginaMapa').then(m => ({ default: m.PaginaMapa })))
export { parsearBusqueda } from './modelo/busqueda'
