// T022 — ErrorApi desde la respuesta HTTP (data-model.md §2). Lee el `Problema` del contrato 001 (RFC 9457) y el
// `ProblemaBloqueo` de 003, con sus nombres en camelCase: `correlacionId` y `bloqueadoHastaUtc`.

export interface ErrorApi {
  status: number
  codigo: string
  correlacionId?: string
  bloqueadoHastaUtc?: Date
}

interface CuerpoProblema {
  codigo?: unknown
  correlacionId?: unknown
  bloqueadoHastaUtc?: unknown
}

const SIN_CONEXION = 'sin_conexion'

export async function armarError(respuesta: Response): Promise<ErrorApi> {
  try {
    const cuerpo = await respuesta.clone().json() as CuerpoProblema | null
    if (typeof cuerpo?.codigo !== 'string') throw new Error('sin codigo')
    return {
      status: respuesta.status,
      codigo: cuerpo.codigo,
      correlacionId: typeof cuerpo.correlacionId === 'string' ? cuerpo.correlacionId : undefined,
      bloqueadoHastaUtc: typeof cuerpo.bloqueadoHastaUtc === 'string' ? new Date(cuerpo.bloqueadoHastaUtc) : undefined,
    }
  } catch {
    // Sin cuerpo legible (red caída, proxy, HTML de error): el usuario ve «sin conexión», no un error técnico
    return { status: respuesta.status || 0, codigo: SIN_CONEXION }
  }
}
