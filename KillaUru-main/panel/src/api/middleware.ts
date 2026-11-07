// T022 — middleware reutilizable (FR-006: sin Authorization; FR-007: renovar+reintentar)
// La lógica principal está en clientes.ts; aquí se expone el tipo para pruebas
export type { OpcionesCliente } from './clientes'
export { crearCliente } from './clientes'
