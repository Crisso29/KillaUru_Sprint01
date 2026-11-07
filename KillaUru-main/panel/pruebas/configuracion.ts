// T009: configuración global de pruebas
import '@testing-library/jest-dom'
import { expect, afterEach, beforeAll, afterAll } from 'vitest'
import { cleanup } from '@testing-library/react'
import { server } from './msw/servidor'

// Extender expect con jest-dom y vitest-axe
import * as matchers from '@testing-library/jest-dom/matchers'
expect.extend(matchers)

// MSW: arrancar servidor antes de las pruebas
beforeAll(() => server.listen({ onUnhandledRequest: 'warn' }))
afterEach(() => {
  cleanup()
  server.resetHandlers()
})
afterAll(() => server.close())
