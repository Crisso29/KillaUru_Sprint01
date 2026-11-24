import { describe, it, expect } from 'vitest'
import { readdirSync, readFileSync, statSync } from 'node:fs'
import { join, resolve } from 'node:path'

/**
 * Coherencia con los contratos: toda ruta `/api/v1/...` que llama el panel, o que simula MSW, debe existir en algún
 * `specs/<módulo>/contracts/*.yaml`. Sin esta prueba, el panel y su API simulada podían inventar la misma ruta
 * (`/auth/iniciar` en vez de `/auth/inicio-sesion`) y todas las pruebas pasaban, pero el backend real respondía 404.
 */
const RAIZ = resolve(__dirname, '../../..')

function archivos(dir: string, filtro: (f: string) => boolean): string[] {
  return readdirSync(dir).flatMap((nombre) => {
    const ruta = join(dir, nombre)
    if (statSync(ruta).isDirectory()) return nombre === 'generado' || nombre === 'dtos' ? [] : archivos(ruta, filtro)
    return filtro(nombre) ? [ruta] : []
  })
}

/** `{id}` (OpenAPI), `${id}` (código) y `:id` (MSW) se comparan igual: `/api/v1/anomalies/{}/estado`. */
const normalizar = (ruta: string) => ruta.replace(/\$\{[^}]+\}|\{[^}]+\}|:\w+/g, '{}').replace(/\/$/, '')

function rutasDelContrato(): Set<string> {
  const specs = join(RAIZ, 'specs')
  const yamls = readdirSync(specs).flatMap((carpeta) => {
    const dir = join(specs, carpeta, 'contracts')
    try { return archivos(dir, (archivo) => archivo.endsWith('.yaml')) } catch { return [] }
  })
  return new Set(yamls.flatMap((y) => [...readFileSync(y, 'utf8').matchAll(/^ {2}(\/api\/v1\/\S+):/gm)].map((coincidencia) => normalizar(coincidencia[1]!))))
}

function rutasUsadas(): Map<string, string> {
  const fuentes = [
    ...archivos(join(RAIZ, 'panel/src'), (archivo) => /\.tsx?$/.test(archivo) && !archivo.includes('.test.')),
    ...archivos(join(RAIZ, 'panel/pruebas/msw'), (archivo) => archivo.endsWith('.ts')),
  ]
  const usadas = new Map<string, string>()
  for (const fuente of fuentes) {
    for (const coincidencia of readFileSync(fuente, 'utf8').matchAll(/\$\{(?:BASE|entorno\.urlApi)\}(\/api\/v1\/[^`?'"]*)/g)) {
      usadas.set(normalizar(coincidencia[1]!), fuente.slice(RAIZ.length + 1))
    }
  }
  return usadas
}

describe('Rutas del panel frente a los contratos OpenAPI', () => {
  it('lee los contratos y encuentra las rutas del panel', () => {
    // Arrange y Act
    const contrato = rutasDelContrato()
    const usadas = rutasUsadas()
    // Assert — si alguna lista sale vacía, la prueba no estaría mirando nada
    expect(contrato.size).toBeGreaterThan(20)
    expect(usadas.size).toBeGreaterThan(10)
  })

  it('toda ruta usada por el panel o por MSW existe en el contrato', () => {
    // Arrange
    const contrato = rutasDelContrato()
    // Act
    const fuera = [...rutasUsadas()].filter(([ruta]) => !contrato.has(ruta)).map(([ruta, archivo]) => `${ruta} (${archivo})`)
    // Assert
    expect(fuera).toEqual([])
  })
})
