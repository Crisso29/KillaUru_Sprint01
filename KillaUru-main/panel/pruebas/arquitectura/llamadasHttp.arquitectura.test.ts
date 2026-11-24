// T022 — una sola puerta hacia la API: solo `src/api/http.ts` llama a `fetch`. Así todas las llamadas llevan la
// cookie, renuevan ante un 401 y arman el mismo ErrorApi. Hasta el 2026-11-23, 20 archivos llamaban a fetch directo y
// ninguno renovaba la sesión ante un 401.
import { describe, it, expect } from 'vitest'
import { readdirSync, readFileSync } from 'fs'
import { join, relative, sep } from 'path'

const RAIZ = join(__dirname, '../..')
const UNICO_PERMITIDO = join('src', 'api', 'http.ts')

function fuentes(carpeta: string): string[] {
  return readdirSync(carpeta, { withFileTypes: true }).flatMap((entrada) => {
    const ruta = join(carpeta, entrada.name)
    if (entrada.isDirectory()) return entrada.name === 'generado' ? [] : fuentes(ruta)
    const esFuente = /\.(ts|tsx)$/.test(entrada.name) && !/\.test\.(ts|tsx)$/.test(entrada.name)
    return esFuente ? [ruta] : []
  })
}

describe('T022: llamadas HTTP', () => {
  it('solo src/api/http.ts llama a fetch', () => {
    // Arrange
    const archivos = fuentes(join(RAIZ, 'src')).map((ruta) => relative(RAIZ, ruta))

    // Act — `fetch(` como llamada, no `refetch(` ni `prefetch(`
    const infractores = archivos.filter((ruta) =>
      ruta !== UNICO_PERMITIDO && /(^|[^\w.])fetch\(/m.test(readFileSync(join(RAIZ, ruta), 'utf-8')))

    // Assert
    expect(infractores.map((ruta) => ruta.split(sep).join('/')), 'Usar pedirApi / pedirJson de @/api/http').toEqual([])
  })
})
