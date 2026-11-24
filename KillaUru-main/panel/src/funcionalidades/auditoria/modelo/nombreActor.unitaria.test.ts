// T078 — la columna «Actor» de la auditoría en sus cuatro casos
import { describe, it, expect } from 'vitest'
import { nombreDelActor } from './nombreActor'
import { TEXTOS_AUDITORIA } from '../textos'

const ID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'

describe('nombreDelActor', () => {
  it.each([
    ['sin actor: lo hizo el sistema', null, undefined, TEXTOS_AUDITORIA.sistema],
    ['con correo resuelto: el correo', ID, 'ops@killauru.pe', 'ops@killauru.pe'],
    ['cuenta que ya no existe: aviso', ID, null, TEXTOS_AUDITORIA.sinCuenta],
    ['búsqueda pendiente: el id', ID, undefined, ID],
  ])('%s', (_caso, actorId, correo, esperado) => {
    // Arrange y Act
    const nombre = nombreDelActor(actorId, correo)

    // Assert
    expect(nombre).toBe(esperado)
  })
})
