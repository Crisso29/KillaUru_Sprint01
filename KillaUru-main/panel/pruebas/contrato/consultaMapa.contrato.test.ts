// Contrato 006: todo parámetro que el panel envía a GET /api/v1/anomalies existe en `mapa.openapi.yaml`.
// La prueba de rutas solo mira la ruta; un parámetro con otro nombre (`severidad_min` en vez de `severidad`, hasta el
// 2026-11-23) pasaba todas las pruebas, y el backend lo ignoraba en silencio: el filtro no tenía efecto.
import { describe, it, expect } from 'vitest'
import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { filtrosAConsulta } from '@/funcionalidades/mapa/modelo/peticion'
import { filtrosDefecto } from '@/funcionalidades/mapa/modelo/filtros'

const CONTRATO = resolve(__dirname, '../../../specs/006-modulo-consulta-mapa/contracts/mapa.openapi.yaml')

/** Nombres de los parámetros `in: query` de GET /api/v1/anomalies en el contrato. */
function parametrosDelContrato(): string[] {
  const texto = readFileSync(CONTRATO, 'utf8')
  const inicio = texto.indexOf('  /api/v1/anomalies:')
  const bloque = texto.slice(inicio, texto.indexOf('\n  /api', inicio + 1))
  return [...bloque.matchAll(/name: (\w+)/g)].map((coincidencia) => coincidencia[1]!)
}

describe('Consulta del mapa frente al contrato 006', () => {
  it('cada parámetro que arma el panel, con cualquier filtro, está en el contrato', () => {
    // Arrange — los filtros por defecto y una combinación con todos los filtros activos
    const enContrato = parametrosDelContrato()
    const variantes = [filtrosDefecto(), { ...filtrosDefecto(), tipos: ['Bache'], severidades: [2, 5], periodo: '24h' as const }]

    // Act
    const enviados = new Set(variantes.flatMap((filtros) => [...filtrosAConsulta(filtros, new Date()).keys()]))

    // Assert
    expect(enContrato.length, 'no se leyeron los parámetros del contrato').toBeGreaterThan(3)
    expect([...enviados].filter((nombre) => !enContrato.includes(nombre))).toEqual([])
  })
})
