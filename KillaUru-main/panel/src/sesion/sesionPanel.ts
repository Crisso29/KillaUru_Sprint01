import type { components } from '@/api/generado/identidad'

/** Respuesta de `inicio-sesion` y `renovar` con `canal = panel` (contrato 003, tipo generado del YAML). */
type SesionPanelDto = components['schemas']['SesionPanel']

export interface DatosSesion {
  expiraAccesoUtc: Date
  sesionVenceUtc: Date
}

const esFecha = (v: unknown): v is string => typeof v === 'string' && !Number.isNaN(Date.parse(v))

/**
 * Lee la sesión del panel tal como la manda el servidor. Falla si faltan las horas: es mejor un error claro que una
 * sesión con «Invalid Date» que nunca se renueva. Las cookies `ku_acceso` y `ku_renovacion` las guarda el navegador.
 */
export function leerSesionPanel(json: unknown): DatosSesion {
  const dto = json as Partial<SesionPanelDto> | null
  if (!esFecha(dto?.expiraAccesoUtc) || !esFecha(dto?.sesionVenceUtc)) {
    throw new Error('Respuesta sin expiraAccesoUtc o sesionVenceUtc: no sigue SesionPanel (contrato 003)')
  }
  return { expiraAccesoUtc: new Date(dto.expiraAccesoUtc), sesionVenceUtc: new Date(dto.sesionVenceUtc) }
}
