// T023 — cálculos de tiempo para renovación de sesión (data-model.md §1, R-006)
import {
  VIDA_ACCESO_MIN,
  MARGEN_RENOVACION_S,
  SESION_MAXIMA_HORAS,
} from '@/config/constantes'

const S = 1000
const MIN = 60 * S

export function calcularDesfase(expiraAccesoUtc: Date, ahora: Date): number {
  return expiraAccesoUtc.getTime() - VIDA_ACCESO_MIN * MIN - ahora.getTime()
}

export function debeRenovar(expiraAccesoUtc: Date, ahora: Date): boolean {
  return ahora.getTime() >= expiraAccesoUtc.getTime() - MARGEN_RENOVACION_S * S
}

export function fraccionRestante(sesionVenceUtc: Date, ahora: Date): number {
  const restante = sesionVenceUtc.getTime() - ahora.getTime()
  const maximo = SESION_MAXIMA_HORAS * 3600 * S
  return Math.max(0, Math.min(1, restante / maximo))
}

export function msDeltaRenovacion(expiraAccesoUtc: Date, ahora: Date): number {
  return Math.max(0, expiraAccesoUtc.getTime() - MARGEN_RENOVACION_S * S - ahora.getTime())
}
