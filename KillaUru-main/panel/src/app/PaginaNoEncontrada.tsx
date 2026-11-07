import { Link } from 'react-router'

export function PaginaNoEncontrada() {
  return (
    <main id="contenido" style={{ padding: 'var(--s-55)', textAlign: 'center' }}>
      <h1 style={{ fontSize: 'var(--t-34)', marginBottom: 'var(--s-13)' }}>404</h1>
      <p style={{ color: 'var(--ink-2)' }}>Esta página no existe.</p>
      <Link to="/resumen" style={{ marginTop: 'var(--s-21)', display: 'inline-block' }}>Ir al resumen</Link>
    </main>
  )
}
