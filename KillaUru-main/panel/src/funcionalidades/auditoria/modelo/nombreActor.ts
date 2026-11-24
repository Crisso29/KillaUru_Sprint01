// T078, R-015 — quién aparece en la columna «Actor» de la auditoría
import { TEXTOS_AUDITORIA } from '../textos'

/**
 * - Sin actorId: lo hizo el sistema (purgas, tareas programadas).
 * - Con actorId y su correo resuelto: el correo.
 * - Con actorId pero la cuenta ya no existe (p. ej. borrada): «Actor no encontrado en el directorio».
 * - Con actorId y la búsqueda de correos aún pendiente (`undefined`): el id, para no dejar la celda vacía.
 */
export function nombreDelActor(actorId: string | null, correo: string | null | undefined): string {
  if (actorId === null) return TEXTOS_AUDITORIA.sistema
  if (correo === undefined) return actorId
  return correo ?? TEXTOS_AUDITORIA.sinCuenta
}
