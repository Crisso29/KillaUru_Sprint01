// DTO DE PRESENTACION — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/003-modulo-identidad-accesos/contracts/identidad.openapi.yaml

export type Rol = 'ConductorRegistrado' | 'Administrador'
export type EstadoUsuario = 'Activa' | 'Desactivada' | 'InvitacionPendiente'

export interface Usuario {
  id: string
  correo: string
  rol: Rol
  estado: EstadoUsuario
  correoVerificado: boolean
  fechaAltaUtc: string
  ultimaSesionUtc: string | null
}

export interface PaginaUsuarios {
  elementos: Usuario[]
  pagina: number
  tamano: number
  total: number
}

export interface EstadoRevision {
  ultimaRevisionUtc: string | null
  venceUtc: string
  administradores: Usuario[]
}

export interface paths {
  '/api/v1/auth/iniciar': {
    post: {
      requestBody: { content: { 'application/json': { correo: string; contrasena: string; canal: string; turnstileToken?: string } } }
      responses: { 200: { content: { 'application/json': { expiraAccesoUtc: string; sesionVenceUtc: string; rol: Rol } } } }
    }
  }
  '/api/v1/auth/renovar': {
    post: { responses: { 200: { content: { 'application/json': { expiraAccesoUtc: string; sesionVenceUtc: string; rol: Rol } } } } }
  }
  '/api/v1/auth/cerrar-sesion': {
    post: { responses: { 204: Record<string, never> } }
  }
  '/api/v1/me': {
    get: { responses: { 200: { content: { 'application/json': { correo: string; rol: Rol } } } } }
  }
  '/api/v1/usuarios': {
    get: { responses: { 200: { content: { 'application/json': PaginaUsuarios } } } }
    post: {
      requestBody: { content: { 'application/json': { correo: string; rol: Rol } } }
      responses: { 201: { content: { 'application/json': Usuario } } }
    }
  }
  '/api/v1/usuarios/{id}/rol': {
    put: {
      requestBody: { content: { 'application/json': { rol: Rol } } }
      responses: { 204: Record<string, never> }
    }
  }
  '/api/v1/usuarios/{id}/desactivar': {
    post: {
      requestBody: { content: { 'application/json': { motivo: string } } }
      responses: { 204: Record<string, never> }
    }
  }
  '/api/v1/usuarios/{id}/activar': {
    post: { responses: { 204: Record<string, never> } }
  }
  '/api/v1/usuarios/{id}/reenviar-correo': {
    post: { responses: { 202: Record<string, never> } }
  }
  '/api/v1/usuarios/revision-privilegios': {
    get: { responses: { 200: { content: { 'application/json': EstadoRevision } } } }
    post: {
      requestBody: { content: { 'application/json': { confirmados: string[]; retirados: string[] } } }
      responses: { 201: { content: { 'application/json': EstadoRevision } } }
    }
  }
}
