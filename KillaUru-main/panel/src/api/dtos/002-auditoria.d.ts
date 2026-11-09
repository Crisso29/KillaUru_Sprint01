// DTO DE PRESENTACION — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/002-modulo-auditoria/contracts/auditoria.openapi.yaml

export type CategoriaAuditoria =
  | 'Usuarios'
  | 'Sesion'
  | 'CuentaDelTitular'
  | 'Moderacion'
  | 'Exportacion'
  | 'AccesoADatos'

export type AccionAuditada =
  | 'UsuarioCreado'
  | 'RolCambiado'
  | 'CuentaDesactivada'
  | 'CuentaActivada'
  | 'CuentaEliminada'
  | 'InicioSesionFallido'
  | 'CuentaBloqueada'
  | 'InicioSesionAdministrador'
  | 'RecuperacionSolicitada'
  | 'ContrasenaCambiada'
  | 'RevisionPrivilegios'
  | 'AnomaliaModerada'
  | 'DatasetExportado'
  | 'AccesoAdministrativoDatos'

export type ActorRol = 'ConductorRegistrado' | 'Administrador' | 'Sistema'

export interface RegistroAuditoria {
  id: number
  ocurridoUtc: string
  actorId: string | null
  actorRol: ActorRol
  categoria: CategoriaAuditoria
  accion: AccionAuditada
  objetivo: string | null
  contexto: Record<string, unknown>
}

export interface PaginaAuditoria {
  elementos: RegistroAuditoria[]
  pagina: number
  tamano: number
  total: number
}

export interface paths {
  '/api/v1/auditoria': {
    get: {
      parameters: {
        query?: {
          desde?: string
          hasta?: string
          categoria?: CategoriaAuditoria
          accion?: AccionAuditada
          actorId?: string
          pagina?: number
          tamano?: number
        }
      }
      responses: { 200: { content: { 'application/json': PaginaAuditoria } } }
    }
  }
}
