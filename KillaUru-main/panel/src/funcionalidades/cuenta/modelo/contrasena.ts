// T061 — validaciones de contraseña para fijar-contrasena (10–128 chars, FR-014)
export function validarContrasena(contrasena: string): string | null {
  if (contrasena.length < 10) return 'La contraseña debe tener al menos 10 caracteres.'
  if (contrasena.length > 128) return 'La contraseña no puede superar 128 caracteres.'
  return null
}

export function validarConfirmacion(contrasena: string, confirmacion: string): string | null {
  if (contrasena !== confirmacion) return 'Las contraseñas no coinciden.'
  return null
}
