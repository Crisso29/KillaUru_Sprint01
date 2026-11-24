// AGENTS.md «Conservar Arrange / Act / Assert»: toda prueba del panel (Vitest y Playwright) marca sus tres fases con
// comentarios, en orden. Así quien revisa ve de un vistazo qué se prepara, qué se ejecuta y qué se comprueba.
import { describe, it, expect } from 'vitest'
import { readdirSync, readFileSync } from 'fs'
import { join, relative, sep } from 'path'
import { pruebasSinAaa, tieneAaa } from './soporte/aaa'

const RAIZ = join(__dirname, '../..')

function archivosDePrueba(carpeta: string): string[] {
  return readdirSync(carpeta, { withFileTypes: true }).flatMap((entrada) => {
    const ruta = join(carpeta, entrada.name)
    if (entrada.isDirectory()) return entrada.name === 'node_modules' ? [] : archivosDePrueba(ruta)
    // *.pruebas.ts: E2E contra el entorno desplegado (playwright.pruebas.config.ts)
    return /\.(test|spec|pruebas)\.(ts|tsx)$/.test(entrada.name) ? [ruta] : []
  })
}

describe('AAA: el detector', () => {
  it.each([
    ['las tres fases en orden', '{\n// Arrange\nconst a = 1\n// Act\nconst b = f(a)\n// Assert\nexpect(b).toBe(2)\n}', true],
    ['Arrange y Act juntos', '{\n// Arrange y Act\nconst b = f(1)\n// Assert — con detalle\nexpect(b).toBe(2)\n}', true],
    ['falta Arrange', '{\n// Act\nconst b = f(1)\n// Assert\nexpect(b).toBe(2)\n}', false],
    ['orden cambiado', '{\n// Arrange\n// Assert\n// Act\n}', false],
    ['dos ciclos Act/Assert (son dos pruebas)', '{\n// Arrange\n// Act\n// Assert\n// Act\n// Assert\n}', false],
    ['sin comentarios', '() => expect(f(1)).toBe(2)', false],
  ])('%s', (_caso, cuerpo, esperado) => {
    // Arrange y Act
    const resultado = tieneAaa(cuerpo)

    // Assert
    expect(resultado).toBe(esperado)
  })

  it('reconoce it, test, it.each(...)(...) y test.fixme, pero no describe ni beforeEach', () => {
    // Arrange
    const codigo = [
      "describe('d', () => { beforeEach(() => {}) })",
      "it('a', () => {})", "test('b', async () => {})", "it.each([1])('c %i', () => {})", "test.fixme('d', async () => {})",
    ].join('\n')

    // Act
    const faltan = pruebasSinAaa('x.test.ts', codigo)

    // Assert
    expect(faltan).toEqual(['a', 'b', 'c %i', 'd'])
  })
})

describe('AAA: las pruebas del panel', () => {
  it('cada prueba de src/ y pruebas/ marca Arrange, Act y Assert en orden', () => {
    // Arrange
    const archivos = [...archivosDePrueba(join(RAIZ, 'src')), ...archivosDePrueba(join(RAIZ, 'pruebas'))]

    // Act
    const infractores = archivos.flatMap((ruta) => pruebasSinAaa(ruta, readFileSync(ruta, 'utf-8'))
      .map((nombre) => `${relative(RAIZ, ruta).split(sep).join('/')} › ${nombre}`))

    // Assert
    expect(infractores, 'Marcar // Arrange, // Act y // Assert (o «// Arrange y Act»)').toEqual([])
  })
})
