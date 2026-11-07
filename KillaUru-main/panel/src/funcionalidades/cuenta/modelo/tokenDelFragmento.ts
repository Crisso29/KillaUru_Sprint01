// T061 — tokenDelFragmento: lee #token= y limpia la barra de direcciones (FR-014)
export function tokenDelFragmento(): string | null {
  const hash = window.location.hash
  if (!hash.startsWith('#token=')) return null
  const token = hash.slice('#token='.length)
  if (!token) return null
  history.replaceState(null, '', window.location.pathname + window.location.search)
  return token
}
