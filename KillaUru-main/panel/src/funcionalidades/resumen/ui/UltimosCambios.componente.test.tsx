// T076 — «Últimos cambios»: quién lo hizo se ve igual que en el Historial y la Auditoría (correo, no un id recortado)
import { describe, it, expect } from 'vitest'
import { screen } from '@testing-library/react'
import { renderizar } from '@/../pruebas/renderizar'
import { UltimosCambios } from './UltimosCambios'
import type { TransicionReciente } from '@/api/dtos/003-anomalias'

const ADMIN_ID = 'b2c3d4e5-0000-0000-0000-000000000001'

const reparada: TransicionReciente = {
  anomaliaId: 'a1b2c3d4-0000-0000-0000-000000000001', tipo: 'Bache', kmVia: 287.45,
  desde: 'Activa', hacia: 'Reparada', ocurridaUtc: '2026-11-20T15:00:00Z', actorId: ADMIN_ID,
}

describe('UltimosCambios', () => {
  it('muestra el correo del administrador que moderó (el manejador MSW lo resuelve por id)', async () => {
    // Arrange
    const transiciones = [reparada]

    // Act
    renderizar(<UltimosCambios transiciones={transiciones} />)

    // Assert
    expect(await screen.findByText('actor-b2c3d4e5@killauru.pe')).toBeInTheDocument()
    expect(screen.queryByText('b2c3d4e5')).not.toBeInTheDocument()
  })

  it('el km usa el formato común del panel (coma decimal, 1 decimal)', () => {
    // Arrange
    const transiciones = [reparada]

    // Act
    renderizar(<UltimosCambios transiciones={transiciones} />)

    // Assert
    expect(screen.getByRole('link', { name: 'Bache · km 287,5' })).toBeInTheDocument()
  })
})
