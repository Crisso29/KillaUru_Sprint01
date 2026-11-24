// T009: configuración global de pruebas
import '@testing-library/jest-dom'
import { expect, afterEach, beforeAll, afterAll } from 'vitest'
import { cleanup } from '@testing-library/react'
import { server } from './msw/servidor'
import { reiniciarSimulador } from './msw/simulador/estado'

// Extender expect con jest-dom y vitest-axe
import * as matchers from '@testing-library/jest-dom/matchers'
expect.extend(matchers)

// MSW: arrancar servidor antes de las pruebas
beforeAll(() => server.listen({ onUnhandledRequest: 'warn' }))
afterEach(() => {
  cleanup()
  server.resetHandlers()
  reiniciarSimulador() // la moderación de una prueba no cambia los datos de la siguiente
})
afterAll(() => server.close())
