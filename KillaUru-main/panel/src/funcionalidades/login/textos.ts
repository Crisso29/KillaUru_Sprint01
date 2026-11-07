// T031 — textos de la pantalla de login
import { minutosRestantes, horaEnLima } from './modelo/bloqueo'

export const TEXTOS_LOGIN = {
  titulo: 'KillaUru Ops',
  subtitulo: 'Panel de administración',
  labelCorreo: 'Correo electrónico',
  labelContrasena: 'Contraseña',
  botonEntrar: 'Entrar',
  linkRecuperar: '¿Olvidaste tu contraseña?',
  cargando: 'Entrando…',
} as const

export const TEXTOS_RECUPERAR = {
  titulo: 'Recuperar contraseña',
  labelCorreo: 'Correo electrónico',
  botonEnviar: 'Enviar enlace',
  cargando: 'Enviando…',
  exito: 'Si tu correo está registrado y verificado, recibirás un enlace en unos minutos.',
  linkVolver: 'Volver al inicio de sesión',
} as const

export function mensajeBloqueo(bloqueadoHastaUtc: Date, ahora: Date): string {
  const mins = minutosRestantes(bloqueadoHastaUtc, ahora)
  const hora = horaEnLima(bloqueadoHastaUtc)
  return `Cuenta bloqueada ${mins} min por intentos fallidos. Vuelve a intentar a las ${hora}.`
}
