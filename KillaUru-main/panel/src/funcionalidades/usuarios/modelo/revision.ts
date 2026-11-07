// T055 — diasRestantes y armarRevision (data-model §5)

export function diasRestantes(venceUtc: string, ahora: Date): number {
  const ms = new Date(venceUtc).getTime() - ahora.getTime()
  return Math.floor(ms / (1000 * 60 * 60 * 24))
}

export type DecisionRevision = 'confirmar' | 'retirar'

export function armarRevision(
  decisiones: Record<string, DecisionRevision>,
): { confirmados: string[]; retirados: string[] } {
  const confirmados: string[] = []
  const retirados: string[] = []
  for (const [id, decision] of Object.entries(decisiones)) {
    if (decision === 'confirmar') confirmados.push(id)
    else retirados.push(id)
  }
  return { confirmados, retirados }
}
