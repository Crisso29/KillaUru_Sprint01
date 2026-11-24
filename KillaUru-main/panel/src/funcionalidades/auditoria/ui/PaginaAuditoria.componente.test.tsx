// T079 — PaginaAuditoria: filtros, actor, retención
import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaAuditoria } from './PaginaAuditoria'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'
const ADMIN_ID = 'a1a1a1a1-0000-0000-0000-000000000001'

const DATOS_AUDITORIA = {
  elementos: [
    {
      id: 1001, ocurridoUtc: '2026-11-04T10:30:00Z', actorId: ADMIN_ID, actorRol: 'Administrador', categoria: 'Moderacion',
      accion: 'AnomaliaModerada', objetivo: 'b2b2b2b2-0000-0000-0000-000000000001', contexto: { accion: 'MarcarReparada' },
    },
    {
      id: 1002, ocurridoUtc: '2026-11-04T09:15:00Z', actorId: null, actorRol: 'Sistema', categoria: 'AccesoADatos',
      accion: 'AccesoAdministrativoDatos', objetivo: null, contexto: {},
    },
  ],
  pagina: 1,
  tamano: 50,
  total: 2,
}

const paginaDeUsuarios = (correos: string[]) => ({
  elementos: correos.map((correo) => ({
    id: ADMIN_ID, correo, rol: 'Administrador', estado: 'Activa', correoVerificado: true,
    fechaAltaUtc: '2026-01-01T00:00:00Z', ultimaSesionUtc: null,
  })),
  pagina: 1, tamano: 50, total: correos.length,
})

/** La auditoría responde DATOS_AUDITORIA y guarda los parámetros de la última consulta. */
function servirAuditoria() {
  const ultimaConsulta = { params: new URLSearchParams() }
  server.use(http.get(`${BASE}/api/v1/auditoria`, ({ request }) => {
    ultimaConsulta.params = new URL(request.url).searchParams
    return HttpResponse.json(DATOS_AUDITORIA)
  }))
  return ultimaConsulta
}

/** El directorio de usuarios: la búsqueda por correo devuelve `correosEncontrados`; cuenta cada pedido por tipo. */
function servirUsuarios(correosEncontrados: string[]) {
  const pedidos = { busqueda: 0, ids: 0 }
  server.use(http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
    const params = new URL(request.url).searchParams
    if (params.has('busqueda')) { pedidos.busqueda++; return HttpResponse.json(paginaDeUsuarios(correosEncontrados)) }
    if (params.has('ids')) { pedidos.ids++; return HttpResponse.json(paginaDeUsuarios(['admin@killauru.pe'])) }
    return HttpResponse.json(paginaDeUsuarios([]))
  }))
  return pedidos
}

describe('PaginaAuditoria', () => {
  it('muestra «N registros · retención permanente»', async () => {
    // Arrange
    servirAuditoria()

    // Act
    renderizar(<PaginaAuditoria />)

    // Assert
    expect(await screen.findByText(/retención permanente/i)).toBeInTheDocument()
  })

  it('actorId null se muestra como «sistema»', async () => {
    // Arrange
    servirAuditoria()

    // Act
    renderizar(<PaginaAuditoria />)

    // Assert
    expect((await screen.findAllByText('sistema')).length).toBeGreaterThan(0)
  })

  it('filtro categoría envía el parámetro correcto', async () => {
    // Arrange
    const consulta = servirAuditoria()
    renderizar(<PaginaAuditoria />)
    const user = userEvent.setup()
    const categoria = await screen.findByRole('combobox', { name: /categoría/i })

    // Act
    await user.selectOptions(categoria, 'Moderacion')

    // Assert
    await waitFor(() => expect(consulta.params.get('categoria')).toBe('Moderacion'))
  })

  it('actor por correo: busca la cuenta y consulta la auditoría con su actorId', async () => {
    // Arrange
    const consulta = servirAuditoria()
    const pedidos = servirUsuarios(['admin@killauru.pe'])
    renderizar(<PaginaAuditoria />)
    const user = userEvent.setup()
    const actor = await screen.findByRole('searchbox', { name: /actor/i })

    // Act
    await user.type(actor, 'admin@killauru.pe')

    // Assert
    await waitFor(() => expect(consulta.params.get('actorId')).toBe(ADMIN_ID))
    expect(pedidos.busqueda).toBeGreaterThanOrEqual(1)
  })

  it('actor por correo sin cuenta → aviso', async () => {
    // Arrange
    servirAuditoria()
    servirUsuarios([])
    renderizar(<PaginaAuditoria />)
    const user = userEvent.setup()
    const actor = await screen.findByRole('searchbox', { name: /actor/i })

    // Act
    await user.type(actor, 'nadie@killauru.pe')

    // Assert
    expect(await screen.findByRole('alert')).toBeInTheDocument()
  })

  it('una sola consulta ?ids= por página para resolver los correos de los actores', async () => {
    // Arrange
    servirAuditoria()
    const pedidos = servirUsuarios([])

    // Act
    renderizar(<PaginaAuditoria />)

    // Assert
    expect(await screen.findByText(/retención permanente/i)).toBeInTheDocument()
    await waitFor(() => expect(pedidos.ids).toBe(1))
  })

  it('la fila se expande con su botón (aria-expanded) y muestra el contexto en JSON', async () => {
    // Arrange
    servirAuditoria()
    renderizar(<PaginaAuditoria />)
    const user = userEvent.setup()
    const [boton] = await screen.findAllByRole('button', { name: /ver contexto/i })
    expect(boton).toHaveAttribute('aria-expanded', 'false') // condición previa: empieza cerrada

    // Act
    await user.click(boton!)

    // Assert
    expect(boton).toHaveAttribute('aria-expanded', 'true')
    expect(screen.getAllByText(/MarcarReparada/).length).toBeGreaterThan(0)
  })
})
