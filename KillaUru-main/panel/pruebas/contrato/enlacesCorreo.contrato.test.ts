// FR-014, 003 R-006 — los enlaces que el backend pone en los correos abren una página del panel.
// Hasta el 2026-11-23 el backend enviaba `/cuenta/verificar#token=…` (como pide la spec) y el panel solo tenía
// `/verificar` y `/establecer-contrasena`: todo enlace de verificación, recuperación e invitación abría «404».
import { describe, it, expect } from 'vitest'
import { readFileSync } from 'node:fs'
import { join, resolve } from 'node:path'

const RAIZ = resolve(__dirname, '../../..')
const EMISOR = join(RAIZ, 'backend/src/Modules/KillaUru.Identidad/Application/Cuenta/EmitirEnlaceCorreo.cs')
const AJUSTES = join(RAIZ, 'backend/src/KillaUru.Api/appsettings.json')
const RUTAS_PANEL = join(RAIZ, 'panel/src/app/rutas.tsx')

/** Rutas del panel que arma el backend: ruta de `UrlBaseEnlaces` + cada propósito del `switch` de `Ruta(...)`. */
function rutasDeLosCorreos(): string[] {
  const base = new URL(JSON.parse(readFileSync(AJUSTES, 'utf8')).Identidad.UrlBaseEnlaces as string).pathname.replace(/\/$/, '')
  const metodo = /string Ruta\(PropositoToken proposito\) => proposito switch\s*\{([\s\S]*?)\};/.exec(readFileSync(EMISOR, 'utf8'))?.[1] ?? ''
  return [...metodo.matchAll(/=>\s*"([^"]+)"/g)].map((coincidencia) => `${base}/${coincidencia[1]}`)
}

/** Rutas públicas declaradas en `rutas.tsx` (lista PUBLICAS). */
function rutasPublicasDelPanel(): string[] {
  const lista = /const PUBLICAS[^=]*=\s*\[([\s\S]*?)\n\]/.exec(readFileSync(RUTAS_PANEL, 'utf8'))?.[1] ?? ''
  return [...lista.matchAll(/\['([^']+)'/g)].map((coincidencia) => coincidencia[1]!)
}

describe('Enlaces de los correos del backend', () => {
  it('cada ruta que el backend pone en un correo existe en el panel', () => {
    // Arrange
    const enCorreos = rutasDeLosCorreos()
    expect(enCorreos.length, 'no se encontraron las rutas del backend: ¿cambió EmitirEnlaceCorreo.cs?').toBe(3)

    // Act
    const faltan = enCorreos.filter((ruta) => !rutasPublicasDelPanel().includes(ruta))

    // Assert
    expect(faltan).toEqual([])
  })
})
