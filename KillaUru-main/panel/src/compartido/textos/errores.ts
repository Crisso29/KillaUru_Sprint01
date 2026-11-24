// T021 — mensajes de error por código (data-model.md §2)
import type { ErrorApi } from '@/api/problema'

/** También lo muestra el login al llegar por una sesión perdida (`/login?motivo=sesion`, data-model §1). */
export const SESION_TERMINADA = 'Tu sesión terminó. Vuelve a entrar.'

/** Un mensaje por código del contrato; un código nuevo sin texto cae en el mensaje genérico con código de soporte. */
const MENSAJES: Record<string, string> = {
  credenciales_invalidas: 'Correo o contraseña incorrectos.',
  sesion_invalida: SESION_TERMINADA,
  cuenta_desactivada: SESION_TERMINADA,
  sin_permiso: 'No tienes permiso para esto.',
  solo_administradores: 'Esta cuenta no tiene acceso al panel.',
  antibot_fallido: 'No pudimos comprobar que eres una persona. Vuelve a intentarlo.',
  no_encontrado: 'Esta anomalía ya no existe.',
  transicion_no_permitida: 'Ya no se puede: la anomalía cambió de estado. Actualizamos el detalle.',
  correo_en_uso: 'Ese correo ya tiene una cuenta.',
  accion_sobre_si_mismo: 'No puedes cambiar tu propio rol ni desactivar tu propia cuenta.',
  ultimo_administrador: 'Debe quedar al menos un administrador activo.',
  revision_incompleta: 'La lista de administradores cambió. La cargamos de nuevo.',
  enlace_invalido: 'El enlace no es válido o ya venció. Pide uno nuevo desde la app.',
  datos_invalidos: 'Revisa los datos marcados.',
  limite_excedido: 'Hiciste muchas consultas seguidas. Espera un momento y vuelve a intentar.',
  sin_conexion: 'No pudimos conectar con el servidor. Revisa tu conexión y vuelve a intentar.',
}

export function mensajeDeError(error: ErrorApi): string {
  // `Object.hasOwn`: un código como «constructor» no debe leer propiedades heredadas del objeto
  const mensaje = Object.hasOwn(MENSAJES, error.codigo) ? MENSAJES[error.codigo] : undefined
  if (mensaje) return mensaje
  if (error.correlacionId) return `Algo falló de nuestro lado. Código de soporte: ${error.correlacionId}.`
  return 'Algo falló de nuestro lado. Vuelve a intentarlo.'
}
