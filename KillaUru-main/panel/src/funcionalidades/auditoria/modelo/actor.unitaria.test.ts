// T078 — detectarTipoActor y elegirCuenta
import { describe, it, expect } from 'vitest'
import { detectarTipoActor, elegirCuenta } from './actor'
import type { Usuario } from '@/api/dtos/001-identidad'

const UUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'

const BASE_USUARIO: Usuario = {
  id: 'a1a1a1a1-0000-0000-0000-000000000001',
  correo: 'admin@killauru.pe',
  rol: 'Administrador',
  estado: 'Activa',
  correoVerificado: true,
  fechaAltaUtc: '2026-01-01T00:00:00Z',
  ultimaSesionUtc: null,
}

describe('detectarTipoActor', () => {
  it.each([
    ['UUID válido → id', UUID, { tipo: 'id', valor: UUID }],
    ['correo válido → correo', 'admin@killauru.pe', { tipo: 'correo', valor: 'admin@killauru.pe' }],
    ['texto sin formato → inválido', 'texto-sin-formato', { tipo: 'invalido' }],
    ['cadena vacía → inválido', '', { tipo: 'invalido' }],
    ['UUID parcial → inválido', 'a1b2c3d4-e5f6', { tipo: 'invalido' }],
  ])('%s', (_caso, texto, esperado) => {
    // Arrange y Act
    const tipo = detectarTipoActor(texto)

    // Assert
    expect(tipo).toEqual(esperado)
  })
})

describe('elegirCuenta', () => {
  const USUARIOS: Usuario[] = [
    { ...BASE_USUARIO, id: '1', correo: 'admin@killauru.pe' },
    { ...BASE_USUARIO, id: '2', correo: 'conductor@killauru.pe' },
    { ...BASE_USUARIO, id: '3', correo: 'otro.admin@killauru.pe' },
  ]

  it('coincidencia exacta (sin mayúsculas) → devuelve el usuario', () => {
    // Arrange y Act
    const resultado = elegirCuenta('ADMIN@killauru.pe', USUARIOS)

    // Assert
    expect(resultado?.id).toBe('1')
  })

  it('coincidencia exacta → devuelve el usuario', () => {
    // Arrange y Act
    const resultado = elegirCuenta('conductor@killauru.pe', USUARIOS)

    // Assert
    expect(resultado?.id).toBe('2')
  })

  it.each([
    ['sin coincidencia', 'nadie@killauru.pe', USUARIOS],
    ['varias coincidencias parciales (ambiguo)', 'admin', USUARIOS],
    ['lista vacía', 'admin@killauru.pe', []],
  ])('%s → null', (_caso, texto, usuarios) => {
    // Arrange y Act
    const resultado = elegirCuenta(texto, usuarios)

    // Assert
    expect(resultado).toBeNull()
  })
})
