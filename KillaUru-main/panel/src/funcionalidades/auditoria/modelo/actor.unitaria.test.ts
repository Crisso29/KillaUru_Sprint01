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
  it('UUID válido → tipo id', () => {
    expect(detectarTipoActor(UUID)).toEqual({ tipo: 'id', valor: UUID })
  })

  it('correo válido → tipo correo', () => {
    expect(detectarTipoActor('admin@killauru.pe')).toEqual({ tipo: 'correo', valor: 'admin@killauru.pe' })
  })

  it('texto inválido → tipo invalido', () => {
    expect(detectarTipoActor('texto-sin-formato')).toEqual({ tipo: 'invalido' })
  })

  it('cadena vacía → tipo invalido', () => {
    expect(detectarTipoActor('')).toEqual({ tipo: 'invalido' })
  })

  it('UUID parcial → tipo invalido', () => {
    expect(detectarTipoActor('a1b2c3d4-e5f6')).toEqual({ tipo: 'invalido' })
  })
})

describe('elegirCuenta', () => {
  const USUARIOS: Usuario[] = [
    { ...BASE_USUARIO, id: '1', correo: 'admin@killauru.pe' },
    { ...BASE_USUARIO, id: '2', correo: 'conductor@killauru.pe' },
    { ...BASE_USUARIO, id: '3', correo: 'otro.admin@killauru.pe' },
  ]

  it('coincidencia exacta (sin mayúsculas) → devuelve el usuario', () => {
    const resultado = elegirCuenta('ADMIN@killauru.pe', USUARIOS)
    expect(resultado?.id).toBe('1')
  })

  it('coincidencia exacta → devuelve el usuario', () => {
    const resultado = elegirCuenta('conductor@killauru.pe', USUARIOS)
    expect(resultado?.id).toBe('2')
  })

  it('sin coincidencia → null', () => {
    expect(elegirCuenta('nadie@killauru.pe', USUARIOS)).toBeNull()
  })

  it('múltiples coincidencias parciales → null', () => {
    expect(elegirCuenta('admin', USUARIOS)).toBeNull()
  })

  it('lista vacía → null', () => {
    expect(elegirCuenta('admin@killauru.pe', [])).toBeNull()
  })
})
