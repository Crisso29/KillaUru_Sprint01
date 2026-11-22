// T031 — textos de la pantalla de login
import { minutosRestantes, horaEnLima } from './modelo/bloqueo'

export const TEXTOS_LOGIN = {
  titulo: 'KillaUru Ops',
  subtitulo: 'Panel de operación · solo personal autorizado',
  encabezado: 'Iniciar sesión',
  labelCorreo: 'Correo electrónico',
  labelContrasena: 'Contraseña',
  botonEntrar: 'Entrar',
  linkRecuperar: 'Olvidé mi contraseña',
  cargando: 'Entrando…',
  notaSesion: 'La sesión dura como máximo 8 h. Los intentos fallidos se registran en la auditoría.',
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
