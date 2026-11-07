// T055 — API pública del módulo usuarios
export { useActores } from './api/useActores'
export { useUsuarios } from './api/useUsuarios'
export { useRevision, useRegistrarRevision } from './api/useRevision'
export { useCambiarRol, useDesactivarUsuario, useActivarUsuario, useReenviarCorreo, useCrearUsuario } from './api/useAccionesUsuario'
export { aConsulta } from './modelo/consulta'
export { diasRestantes, armarRevision } from './modelo/revision'
export { PaginaUsuarios } from './ui/PaginaUsuarios'
