// T021 — mensajes de error por código (data-model.md §2)
import type { ErrorApi } from '@/api/problema'

export function mensajeDeError(e: ErrorApi): string {
  switch (e.codigo) {
    case 'credenciales_invalidas': return 'Correo o contraseña incorrectos.'
    case 'sesion_invalida':
    case 'cuenta_desactivada': return 'Tu sesión terminó. Vuelve a entrar.'
    case 'sin_permiso': return 'No tienes permiso para esto.'
    case 'solo_administradores': return 'Esta cuenta no tiene acceso al panel.'
    case 'antibot_fallido': return 'No pudimos comprobar que eres una persona. Vuelve a intentarlo.'
    case 'no_encontrado': return 'Esta anomalía ya no existe.'
    case 'transicion_no_permitida': return 'Ya no se puede: la anomalía cambió de estado. Actualizamos el detalle.'
    case 'correo_en_uso': return 'Ese correo ya tiene una cuenta.'
    case 'accion_sobre_si_mismo': return 'No puedes cambiar tu propio rol ni desactivar tu propia cuenta.'
    case 'ultimo_administrador': return 'Debe quedar al menos un administrador activo.'
    case 'revision_incompleta': return 'La lista de administradores cambió. La cargamos de nuevo.'
    case 'enlace_invalido': return 'El enlace no es válido o ya venció. Pide uno nuevo desde la app.'
    case 'datos_invalidos': return 'Revisa los datos marcados.'
    case 'limite_excedido': return 'Hiciste muchas consultas seguidas. Espera un momento y vuelve a intentar.'
    case 'sin_conexion': return 'No pudimos conectar con el servidor. Revisa tu conexión y vuelve a intentar.'
    default:
      if (e.correlacionId) return `Algo falló de nuestro lado. Código de soporte: ${e.correlacionId}.`
      return 'Algo falló de nuestro lado. Vuelve a intentarlo.'
  }
}
