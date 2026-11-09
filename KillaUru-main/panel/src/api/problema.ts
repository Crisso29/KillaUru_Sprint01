// T022 — ErrorApi desde la respuesta HTTP (data-model.md §2)

export interface ErrorApi {
  status: number
  codigo: string
  correlacionId?: string
  bloqueadoHastaUtc?: Date
}

export async function armarError(r: Response): Promise<ErrorApi> {
  try {
    const body = await r.clone().json()
    if (!body?.codigo) throw new Error('sin codigo')
    return {
      status: r.status,
      codigo: body.codigo as string,
      correlacionId: body.correlacion_id as string | undefined,
      bloqueadoHastaUtc: body.bloqueado_hasta_utc
        ? new Date(body.bloqueado_hasta_utc as string)
        : undefined,
    }
  } catch {
    return { status: r.status || 0, codigo: 'sin_conexion' }
  }
}
