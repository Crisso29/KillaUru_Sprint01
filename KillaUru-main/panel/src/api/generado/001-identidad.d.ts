// GENERADO — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/contratos/001-identidad.yaml

export interface paths {
  '/api/v1/auth/iniciar': {
    post: {
      requestBody: { content: { 'application/json': { correo: string; contrasena: string; token_turnstile: string } } }
      responses: { 200: { content: { 'application/json': { acceso_vence_utc: string } } } }
    }
  }
  '/api/v1/auth/renovar': {
    post: {
      responses: { 200: { content: { 'application/json': { acceso_vence_utc: string } } } }
    }
  }
  '/api/v1/auth/cerrar': {
    post: { responses: { 200: { content: { 'application/json': Record<string, never> } } } }
  }
  '/api/v1/cuenta/perfil': {
    get: { responses: { 200: { content: { 'application/json': { correo: string; nombre: string; rol: string } } } } }
  }
}
