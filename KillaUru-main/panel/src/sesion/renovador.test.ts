import { describe, it, expect, vi, beforeEach } from 'vitest'
import { crearRenovador } from './renovador'

// T015 — 3 llamadas simultáneas → 1 renovación; renovación 401 → sesión terminada

describe('crearRenovador', () => {
  beforeEach(() => vi.clearAllMocks())

  it('3 llamadas simultáneas → solo 1 fetch', async () => {
    const llamadas: number[] = []
    const renovar = vi.fn(async () => {
      llamadas.push(1)
      return new Date()
    })
    const { renovarSesion } = crearRenovador(renovar)

    const [a, b, c] = await Promise.all([
      renovarSesion(),
      renovarSesion(),
      renovarSesion(),
    ])
    expect(llamadas.length).toBe(1)
    expect(a).toEqual(b)
    expect(b).toEqual(c)
  })

  it('renovación completada → siguiente llamada hace nuevo fetch', async () => {
    const renovar = vi.fn(async () => new Date())
    const { renovarSesion } = crearRenovador(renovar)
    await renovarSesion()
    await renovarSesion()
    expect(renovar).toHaveBeenCalledTimes(2)
  })

  it('renovación 401 lanza ErrorRenovacion401', async () => {
    const renovar = vi.fn(async () => { throw { codigo: 'token_invalido', status: 401 } })
    const { renovarSesion } = crearRenovador(renovar)
    await expect(renovarSesion()).rejects.toMatchObject({ codigo: 'token_invalido' })
  })
})
