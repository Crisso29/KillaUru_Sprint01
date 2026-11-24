// T075 — geometría del gráfico de eventos (prototipo OV.resumen: lienzo 640 × 233)
import { describe, it, expect } from 'vitest'
import { escalaGrafico, LIENZO, techo } from './escalaGrafico'
import type { EventoPorDia } from '@/api/dtos/004-dashboard'

const dia = (numero: number, eventos: number | null, anterior: number | null): EventoPorDia =>
  ({ dia: `2026-11-${String(numero).padStart(2, '0')}`, eventos, eventosPeriodoAnterior: anterior })

describe('techo', () => {
  it.each([[321, 350], [0, 50], [46, 50], [47, 100]])('máximo %i → eje hasta %i (redondo, con 8 % de aire)', (maximo, esperado) => {
    // Arrange y Act
    const valor = techo(maximo)

    // Assert
    expect(valor).toBe(esperado)
  })
})

describe('escalaGrafico', () => {
  it('el 0 cae sobre el eje inferior y el techo en el margen superior', () => {
    // Arrange
    const datos = [dia(1, 321, 100)]

    // Act
    const escala = escalaGrafico(datos)

    // Assert
    expect(escala.y(0)).toBe(LIENZO.ALTO - LIENZO.ABAJO)
    expect(escala.y(350)).toBe(LIENZO.ARRIBA)
  })

  it('las guías son múltiplos de 50 repartidos en el eje', () => {
    // Arrange
    const datos = [dia(1, 321, 100)]

    // Act
    const { guias } = escalaGrafico(datos)

    // Assert
    expect(guias).toEqual([0, 100, 200, 300])
  })

  it('la línea del periodo anterior pasa por el centro de cada barra; un día sin dato cuenta como 0', () => {
    // Arrange
    const datos = [dia(1, 10, 50), dia(2, 20, null)]

    // Act
    const { trazoAnterior, centro, y } = escalaGrafico(datos)

    // Assert
    expect(trazoAnterior).toBe(`M${centro(0).toFixed(1)} ${y(50).toFixed(1)} L${centro(1).toFixed(1)} ${y(0).toFixed(1)}`)
  })

  it('con 30 días se rotula uno de cada 4 para que los números no se encimen', () => {
    // Arrange
    const datos = Array.from({ length: 30 }, (_, i) => dia(i + 1, i, i))

    // Act
    const { paso } = escalaGrafico(datos)

    // Assert
    expect(paso).toBe(4)
  })

  it('sin datos no divide entre cero', () => {
    // Arrange y Act
    const escala = escalaGrafico([])

    // Assert
    expect(escala.ancho).toBe(LIENZO.ANCHO - LIENZO.IZQ)
    expect(escala.trazoAnterior).toBe('')
  })
})
