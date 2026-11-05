// T023 — colapsa llamadas concurrentes en una sola renovación (FR-007)

type RenovarFn = () => Promise<Date>

export function crearRenovador(renovar: RenovarFn) {
  let promesaActiva: Promise<Date> | null = null

  async function renovarSesion(): Promise<Date> {
    if (promesaActiva) return promesaActiva
    promesaActiva = renovar().finally(() => { promesaActiva = null })
    return promesaActiva
  }

  return { renovarSesion }
}
