// T059 — tokenDelFragmento: lee #token=, llama replaceState, sin token → null
import { describe, it, expect, vi, afterEach } from 'vitest'
import { tokenDelFragmento } from './tokenDelFragmento'

afterEach(() => {
  window.location.hash = ''
})

describe('tokenDelFragmento', () => {
  it('extrae el token del fragmento', () => {
    window.location.hash = '#token=abc123'
    const token = tokenDelFragmento()
    expect(token).toBe('abc123')
  })

  it('llama a history.replaceState para quitar el fragmento', () => {
    window.location.hash = '#token=abc123'
    const spy = vi.spyOn(history, 'replaceState')
    tokenDelFragmento()
    expect(spy).toHaveBeenCalled()
    spy.mockRestore()
  })

  it('sin token (#token= vacío) → null', () => {
    window.location.hash = '#token='
    expect(tokenDelFragmento()).toBeNull()
  })

  it('sin fragmento → null', () => {
    window.location.hash = ''
    expect(tokenDelFragmento()).toBeNull()
  })

  it('fragmento sin #token= → null', () => {
    window.location.hash = '#otro=valor'
    expect(tokenDelFragmento()).toBeNull()
  })
})
