import { describe, it, expect, vi, beforeEach } from 'vitest'
import { crearRenovador } from './renovador'

// T015 — 3 llamadas simultáneas → 1 renovación; renovación 401 → sesión terminada

const EXPIRA = new Date('2026-11-05T10:05:00Z')

describe('crearRenovador', () => {
  beforeEach(() => vi.clearAllMocks())

  it('3 llamadas simultáneas → solo 1 fetch', async () => {
    // Arrange
    const llamadas: number[] = []
    const renovar = vi.fn(async () => {
      llamadas.push(1)
      return EXPIRA
    })
    const { renovarSesion } = crearRenovador(renovar)


    // Act
    const [primera, segunda, tercera] = await Promise.all([
      renovarSesion(),
      renovarSesion(),
      renovarSesion(),
    ])

    // Assert
    expect(llamadas.length).toBe(1)
    expect(primera).toEqual(segunda)
    expect(segunda).toEqual(tercera)
  })

  it('renovación completada → siguiente llamada hace nuevo fetch', async () => {
    // Arrange
    const renovar = vi.fn(async () => EXPIRA)
    const { renovarSesion } = crearRenovador(renovar)
    await renovarSesion()

    // Act
    await renovarSesion()

    // Assert
    expect(renovar).toHaveBeenCalledTimes(2)
  })

  it('renovación 401 lanza ErrorRenovacion401', async () => {
    // Arrange
    const renovar = vi.fn(async () => { throw { codigo: 'token_invalido', status: 401 } })

    // Act
    const { renovarSesion } = crearRenovador(renovar)

    // Assert
    await expect(renovarSesion()).rejects.toMatchObject({ codigo: 'token_invalido' })
  })
})
