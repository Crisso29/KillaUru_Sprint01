// Abstracción del reloj — permite inyectar un reloj falso en tests
export interface Reloj {
  ahora: () => Date
}

export const relojReal: Reloj = { ahora: () => new Date() }
