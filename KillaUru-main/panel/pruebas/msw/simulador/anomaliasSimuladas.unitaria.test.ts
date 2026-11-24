// El simulador de anomalías (modo simulado y E2E) se comporta como el backend: filtra el mapa, sirve el detalle de
// cualquier anomalía y modera con la tabla RN-07 de `TablaModeracion.cs` (copiada del backend, no del panel: así un
// error en la regla del panel no queda oculto por un simulador que piensa igual).
import { beforeEach, describe, it, expect } from 'vitest'
import { crearSimulador, type SimuladorAnomalias } from './anomaliasSimuladas'

const ACTIVA = 'a1b2c3d4-0000-0000-0000-000000000001'
const EXPIRADA = 'a1b2c3d4-0000-0000-0000-000000000003'
const REPARADA = 'a1b2c3d4-0000-0000-0000-000000000004'
const DESCARTADA_POR_REVISAR = 'd1d1d1d1-0000-0000-0000-000000000001'

let simulador: SimuladorAnomalias
beforeEach(() => { simulador = crearSimulador() })

describe('listar (GET /anomalies)', () => {
  it.each([
    ['estado=Activa,PendienteConfirmacion', ['Activa', 'PendienteConfirmacion']],
    ['estado=Expirada', ['Expirada']],
    ['', ['Activa', 'PendienteConfirmacion', 'Expirada', 'Reparada', 'Descartada']], // sin filtro: todas
  ])('«%s» → solo esos estados', (consulta, estados) => {
    // Arrange
    const params = new URLSearchParams(consulta)

    // Act
    const { features } = simulador.listar(params)

    // Assert
    expect(new Set(features.map((feature) => feature.properties.estado))).toEqual(new Set(estados))
  })

  it('filtra también por tipo y severidad', () => {
    // Arrange
    const params = new URLSearchParams('tipo=Hundimiento&severidad=5')

    // Act
    const { features } = simulador.listar(params)

    // Assert
    expect(features.every(({ properties }) => properties.tipo === 'Hundimiento' && properties.severidad === 5)).toBe(true)
    expect(features.length).toBeGreaterThan(0)
  })
})

describe('moderar (POST /anomalies/{id}/moderacion) con la tabla RN-07 del backend', () => {
  it.each([
    [ACTIVA, 'MarcarReparada', 'Reparada'],
    [ACTIVA, 'Descartar', 'Descartada'],
    [EXPIRADA, 'Descartar', 'Descartada'],
    [REPARADA, 'Reabrir', 'Activa'],
    [DESCARTADA_POR_REVISAR, 'MantenerDescartada', 'Descartada'],
  ])('%s + %s → %s, y el detalle lo recuerda', (id, accion, estadoNuevo) => {
    // Arrange y Act
    const respuesta = simulador.moderar(id, accion)

    // Assert
    expect(respuesta).toMatchObject({ estado: 200, cuerpo: { id, estado: estadoNuevo } })
    expect(simulador.detalle(id)?.estado).toBe(estadoNuevo)
  })

  it.each([
    [EXPIRADA, 'MarcarReparada'],
    [ACTIVA, 'Reabrir'],
    [REPARADA, 'MantenerDescartada'], // solo una descartada «por revisar»
  ])('%s + %s → 409 transicion_no_permitida, sin cambiar nada', (id, accion) => {
    // Arrange
    const antes = simulador.detalle(id)?.estado

    // Act
    const respuesta = simulador.moderar(id, accion)

    // Assert
    expect(respuesta).toEqual({ estado: 409, cuerpo: { codigo: 'transicion_no_permitida' } })
    expect(simulador.detalle(id)?.estado).toBe(antes)
  })

  it('mantener descartada la saca de «por revisar»: una segunda vez ya no se puede', () => {
    // Arrange
    simulador.moderar(DESCARTADA_POR_REVISAR, 'MantenerDescartada')

    // Act
    const segunda = simulador.moderar(DESCARTADA_POR_REVISAR, 'MantenerDescartada')

    // Assert
    expect(segunda.estado).toBe(409)
  })

  it('una anomalía que no existe → 404 no_encontrado', () => {
    // Arrange y Act
    const respuesta = simulador.moderar('ffffffff-0000-0000-0000-000000000000', 'Descartar')

    // Assert
    expect(respuesta).toEqual({ estado: 404, cuerpo: { codigo: 'no_encontrado' } })
  })
})
