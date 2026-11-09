// T043 — tramoDemo: límites del corredor piloto (R-012, data-model §5)
export const TRAMO_DEMO_KM = { desde: 268, hasta: 335.073 } as const

export function dentroDelTramo(kmVia: number): boolean {
  return kmVia >= TRAMO_DEMO_KM.desde && kmVia <= TRAMO_DEMO_KM.hasta
}
