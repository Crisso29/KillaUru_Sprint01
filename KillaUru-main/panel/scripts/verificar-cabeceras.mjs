// Verifica cabeceras de seguridad en https://panel.killauru.pe
const URL_PANEL = process.env.PANEL_URL ?? 'https://panel.killauru.pe'

const r = await fetch(URL_PANEL, { method: 'HEAD' })

const requeridas = [
  'strict-transport-security',
  'content-security-policy',
  'x-frame-options',
  'x-content-type-options',
  'referrer-policy',
]

const faltantes = requeridas.filter(h => !r.headers.has(h))

if (faltantes.length > 0) {
  console.error('Faltan cabeceras:', faltantes)
  process.exit(1)
}

console.log('✓ Cabeceras de seguridad correctas')
