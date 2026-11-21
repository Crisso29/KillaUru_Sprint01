import { describe, it, expect } from 'vitest'
import { readFileSync } from 'fs'
import { join } from 'path'

// T020 — la plantilla genera las 7 cabeceras de FR-011
// CSP sin unsafe-inline/unsafe-eval, frame-ancestors 'none'

const PLANTILLA = join(__dirname, '../cabeceras/_headers.plantilla')

function generarCabeceras(urlApi: string, urlCdn: string, turnstileSitekey = 'KEY') {
  const plantilla = readFileSync(PLANTILLA, 'utf-8')
  return plantilla
    .replace(/\{\{VITE_URL_API\}\}/g, urlApi)
    .replace(/\{\{VITE_URL_CDN\}\}/g, urlCdn)
    .replace(/\{\{VITE_TURNSTILE_SITEKEY\}\}/g, turnstileSitekey)
}

describe('Plantilla de cabeceras de seguridad', () => {
  const cabeceras = generarCabeceras(
    'https://api.killauru.pe',
    'https://cdn.killauru.pe',
  )

  it('incluye Content-Security-Policy', () =>
    expect(cabeceras).toContain('Content-Security-Policy'))

  it('incluye X-Frame-Options', () =>
    expect(cabeceras).toContain('X-Frame-Options'))

  it('incluye X-Content-Type-Options', () =>
    expect(cabeceras).toContain('X-Content-Type-Options'))

  it('incluye Referrer-Policy', () =>
    expect(cabeceras).toContain('Referrer-Policy'))

  it('incluye Strict-Transport-Security', () =>
    expect(cabeceras).toContain('Strict-Transport-Security'))

  it('incluye Permissions-Policy', () =>
    expect(cabeceras).toContain('Permissions-Policy'))

  it('incluye Cross-Origin-Opener-Policy', () =>
    expect(cabeceras).toContain('Cross-Origin-Opener-Policy'))

  it('CSP no tiene unsafe-inline ni unsafe-eval', () => {
    const csp = cabeceras.match(/Content-Security-Policy:\s*(.+)/)?.[1] ?? ''
    expect(csp).not.toContain("'unsafe-inline'")
    expect(csp).not.toContain("'unsafe-eval'")
  })

  it('CSP tiene frame-ancestors none', () => {
    const csp = cabeceras.match(/Content-Security-Policy:\s*(.+)/)?.[1] ?? ''
    expect(csp).toContain("frame-ancestors 'none'")
  })

  it('API y CDN en connect-src', () => {
    expect(cabeceras).toContain('https://api.killauru.pe')
    expect(cabeceras).toContain('https://cdn.killauru.pe')
  })

  it('index.html tiene Cache-Control no-cache', () =>
    expect(cabeceras).toContain('no-cache'))

  it('/assets/ tiene immutable', () =>
    expect(cabeceras).toContain('immutable'))
})
