// T056, FR-031 — la búsqueda de usuarios espera 400 ms desde la última tecla (o Enter): no se pide una página por letra.
// Se prueba el gancho con reloj falso; en la página completa React y MSW avanzan en ciclos reales y el tiempo no se
// puede medir con precisión. El mínimo de 3 caracteres se prueba en `aConsulta` (consulta.unitaria.test.ts).
import { afterEach, describe, it, expect, vi } from 'vitest'
import { act, renderHook } from '@testing-library/react'
import { useBusquedaDiferida } from './useBusquedaDiferida'

afterEach(() => { vi.useRealTimers() })

function montar() {
  vi.useFakeTimers()
  const alBuscar = vi.fn()
  const { result } = renderHook(() => useBusquedaDiferida(alBuscar))
  return { gancho: result, alBuscar }
}

describe('useBusquedaDiferida', () => {
  it('el texto se ve al instante, pero no se busca antes de 400 ms', () => {
    // Arrange
    const { gancho, alBuscar } = montar()

    // Act
    act(() => { gancho.current.escribir('jos'); vi.advanceTimersByTime(399) })

    // Assert
    expect({ texto: gancho.current.texto, buscado: gancho.current.buscado }).toEqual({ texto: 'jos', buscado: '' })
    expect(alBuscar).not.toHaveBeenCalled()
  })

  it('a los 400 ms de la última tecla busca el texto escrito', () => {
    // Arrange
    const { gancho, alBuscar } = montar()

    // Act
    act(() => { gancho.current.escribir('jos'); vi.advanceTimersByTime(400) })

    // Assert
    expect(gancho.current.buscado).toBe('jos')
    expect(alBuscar).toHaveBeenCalledOnce()
  })

  it('cada tecla reinicia la espera: escribir seguido busca una sola vez, al final', () => {
    // Arrange
    const { gancho, alBuscar } = montar()

    // Act
    act(() => {
      gancho.current.escribir('j')
      vi.advanceTimersByTime(300)
      gancho.current.escribir('jo')
      vi.advanceTimersByTime(300)
      gancho.current.escribir('jos')
      vi.advanceTimersByTime(400)
    })

    // Assert
    expect(gancho.current.buscado).toBe('jos')
    expect(alBuscar).toHaveBeenCalledOnce()
  })

  it('Enter busca ya, sin esperar', () => {
    // Arrange
    const { gancho } = montar()
    act(() => { gancho.current.escribir('jose') })

    // Act
    act(() => { gancho.current.buscarYa() })

    // Assert
    expect(gancho.current.buscado).toBe('jose')
  })
})
