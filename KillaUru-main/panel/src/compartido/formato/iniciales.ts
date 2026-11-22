/** «AO» para admin.operaciones@killauru.pe: dos letras del nombre de usuario del correo (prototipo `.av`). */
export function iniciales(correo: string): string {
  const partes = (correo.split('@')[0] ?? '').split(/[._-]+/).filter(Boolean)
  const letras = partes.length > 1 ? partes[0]![0]! + partes[1]![0]! : (partes[0] ?? '?').slice(0, 2)
  return letras.toUpperCase()
}
