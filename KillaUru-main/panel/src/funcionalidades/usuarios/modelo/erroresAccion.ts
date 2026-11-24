// T057 — mensaje para cada código de error de las acciones sobre cuentas (contrato 003: 409 de rol y desactivación)
import { TEXTOS_USUARIOS as textos } from '../textos'

const MENSAJES: Record<string, string> = {
  accion_sobre_si_mismo: textos.errorAccionSobreSiMismo,
  ultimo_administrador: textos.errorUltimoAdministrador,
  correo_en_uso: textos.errorCorreoEnUso,
  revision_incompleta: textos.errorRevisionIncompleta,
}

export const ERROR_INESPERADO = 'Error inesperado.'

export function mensajeDeErrorAccion(codigo: string): string {
  return MENSAJES[codigo] ?? ERROR_INESPERADO
}
