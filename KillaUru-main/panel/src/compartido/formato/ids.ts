// T021 — id corto de 8 caracteres (sin guiones) para mostrar en UI
export function idCorto(id: string): string {
  return id.replace(/-/g, '').slice(0, 8) || id.slice(0, 8)
}
