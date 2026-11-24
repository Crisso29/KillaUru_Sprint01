// T083 — PaginaExportacion: columnas, privacidad, POST, descarga, recientes, "tú"
import { describe, it, expect, vi, beforeAll, afterAll } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaExportacion } from './PaginaExportacion'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'
const CORREO_ACTUAL = 'admin@killauru.pe'

let createObjectURLSpy: import('vitest').MockInstance<typeof URL.createObjectURL>
let revokeObjectURLSpy: import('vitest').MockInstance<typeof URL.revokeObjectURL>

beforeAll(() => {
  if (!URL.createObjectURL) {
    Object.defineProperty(URL, 'createObjectURL', { value: vi.fn(), writable: true, configurable: true })
    Object.defineProperty(URL, 'revokeObjectURL', { value: vi.fn(), writable: true, configurable: true })
  }
  createObjectURLSpy = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:fake')
  revokeObjectURLSpy = vi.spyOn(URL, 'revokeObjectURL').mockReturnValue(undefined)
})

afterAll(() => {
  createObjectURLSpy?.mockRestore()
  revokeObjectURLSpy?.mockRestore()
})

describe('PaginaExportacion', () => {
  it('muestra las 11 columnas del dataset', () => {
    // Arrange
    const columnas = ['id', 'tipo', 'severidad', 'estado', 'persistencia', 'km', 'latitud', 'longitud', 'reportes', 'primer_reporte', 'ultimo_reporte']

    // Act
    renderizar(<PaginaExportacion correoActual={CORREO_ACTUAL} />)

    // Assert
    const faltan = columnas.filter((columna) => !screen.queryByText(columna, { exact: true }))
    expect(faltan).toEqual([])
  })

  it('"seudónimo" aparece tachado (no incluido en export)', () => {
    // Arrange y Act
    renderizar(<PaginaExportacion correoActual={CORREO_ACTUAL} />)

    // Assert
    const elem = screen.getByText('seudónimo', { exact: true })
    expect(elem).toBeTruthy()
    expect(elem.getAttribute('data-tachado')).toBe('true')
  })

  it('envía el POST correcto al hacer clic en Exportar', async () => {
    // Arrange
    let cuerpoRecibido: unknown = null
    server.use(
      http.post(`${BASE}/api/v1/exportaciones`, async ({ request }) => {
        cuerpoRecibido = await request.json()
        return new HttpResponse('id,tipo\n1,Bache', {
          status: 200,
          headers: { 'Content-Type': 'text/csv', 'Content-Disposition': 'attachment' },
        })
      }),
    )
    renderizar(<PaginaExportacion correoActual={CORREO_ACTUAL} />)
    const user = userEvent.setup()

    // Act
    await user.click(screen.getByRole('button', { name: /exportar/i }))

    // Assert
    await waitFor(() => expect(cuerpoRecibido).not.toBeNull())
    expect((cuerpoRecibido as { formato: string }).formato).toBeDefined()
  })

  it('llama a URL.createObjectURL al descargar', async () => {
    // Arrange
    createObjectURLSpy.mockClear()
    renderizar(<PaginaExportacion correoActual={CORREO_ACTUAL} />)
    const user = userEvent.setup()

    // Act
    await user.click(screen.getByRole('button', { name: /exportar/i }))

    // Assert
    await waitFor(() => expect(createObjectURLSpy).toHaveBeenCalled())
  })

  it('sección Recientes está visible', () => {
    // Arrange y Act
    renderizar(<PaginaExportacion correoActual={CORREO_ACTUAL} />)

    // Assert
    expect(screen.getByRole('heading', { name: /recientes/i })).toBeTruthy()
  })
})
