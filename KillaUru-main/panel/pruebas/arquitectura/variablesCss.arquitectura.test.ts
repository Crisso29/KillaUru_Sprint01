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
  return readdirSync(dir).flatMap((nombre) => {
    const ruta = join(dir, nombre)
    if (statSync(ruta).isDirectory()) return nombre === 'generado' ? [] : archivos(ruta)
    return /\.(tsx?|css)$/.test(nombre) && !nombre.includes('.test.') ? [ruta] : []
  })
}

describe('Variables CSS del sistema de diseño', () => {
  it('toda var(--x) usada está definida en las hojas de estilo', () => {
    // Arrange
    const estilos = join(SRC, 'compartido/estilos')
    const definidas = new Set(readdirSync(estilos).filter((archivo) => archivo.endsWith('.css'))
      .flatMap((archivo) => [...readFileSync(join(estilos, archivo), 'utf8').matchAll(/(--[\w-]+)\s*:/g)].map((coincidencia) => coincidencia[1]!)))
    // Act — `var(--s${n})` arma el nombre en tiempo de ejecución; esos se validan aparte (severidad 1 a 5 existe)
    const faltan = archivos(SRC).flatMap((archivo) => [...readFileSync(archivo, 'utf8').matchAll(/var\((--[\w-]+)(\$\{)?/g)]
      .filter((coincidencia) => !coincidencia[2] && !definidas.has(coincidencia[1]!)).map((coincidencia) => `${coincidencia[1]} en ${archivo.slice(SRC.length + 1)}`))
    // Assert
    expect([...new Set(faltan)]).toEqual([])
  })
})
