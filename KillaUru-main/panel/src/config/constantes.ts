// T007: configuración del modelo de datos §8 — data-model.md

// Estados que se muestran en el mapa por defecto (D6, RN-08)
export const ESTADOS_INICIALES_MAPA = ['Activa', 'PendienteConfirmacion'] as const

// Tramo demo del corredor PE-28A (corredor_pe28a.geojson)
export const TRAMO_DEMO_KM = { desde: 268.0, hasta: 335.073 } as const

// Sesión — data-model.md §8
export const SESION_MAXIMA_HORAS = 8
export const VIDA_ACCESO_MIN = 5
export const MARGEN_RENOVACION_S = 60

// Caché TanStack Query
export const REFRESCO_OPERATIVO_S = 60
export const CACHE_LECTURA_AUDITADA_MIN = 10

// Mapa — corredor PE-28A
export const TRAMO_FRANJA_KM = 10
export const LARGO_CORREDOR_KM = 335.073
export const PUEBLOS_CORREDOR = [
  [0, 'Inicio PE-1S'],
  [268, 'Inicio demo'],
  [335.073, 'Fin Ayacucho'],
] as const

// Gráfico de señales por día
export const DIAS_GRAFICO = [7, 14, 30] as const
export const DIAS_GRAFICO_INICIAL = 14

// Paginación
export const TAMANO_PAGINA = 50
