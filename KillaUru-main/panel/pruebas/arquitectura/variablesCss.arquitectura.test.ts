import { describe, it, expect } from 'vitest'
import { readdirSync, readFileSync, statSync } from 'node:fs'
import { join, resolve } from 'node:path'

/**
 * Sistema de diseño: toda `var(--x)` usada en el panel debe estar definida en `src/compartido/estilos/*.css`. Una variable
 * inexistente no da error: el navegador la ignora y el borde, el color de error o el espaciado simplemente no aparecen
 * (así estaban `--border`, `--error`, `--accent`… en 20 componentes). Patrón AAA.
 */
const SRC = resolve(__dirname, '../../src')

function archivos(dir: string): string[] {
  return readdirSync(dir).flatMap((n) => {
    const ruta = join(dir, n)
    if (statSync(ruta).isDirectory()) return n === 'generado' ? [] : archivos(ruta)
    return /\.(tsx?|css)$/.test(n) && !n.includes('.test.') ? [ruta] : []
  })
}

describe('Variables CSS del sistema de diseño', () => {
  it('toda var(--x) usada está definida en las hojas de estilo', () => {
    // Arrange
    const estilos = join(SRC, 'compartido/estilos')
    const definidas = new Set(readdirSync(estilos).filter((f) => f.endsWith('.css'))
      .flatMap((f) => [...readFileSync(join(estilos, f), 'utf8').matchAll(/(--[\w-]+)\s*:/g)].map((m) => m[1]!)))
    // Act — `var(--s${n})` arma el nombre en tiempo de ejecución; esos se validan aparte (severidad 1 a 5 existe)
    const faltan = archivos(SRC).flatMap((f) => [...readFileSync(f, 'utf8').matchAll(/var\((--[\w-]+)(\$\{)?/g)]
      .filter((m) => !m[2] && !definidas.has(m[1]!)).map((m) => `${m[1]} en ${f.slice(SRC.length + 1)}`))
    // Assert
    expect([...new Set(faltan)]).toEqual([])
  })
})
