// T011: validación de datos simulados contra esquemas OpenAPI
import Ajv from 'ajv'
import addFormats from 'ajv-formats'

export function crearValidador() {
  const ajv = new Ajv({ allErrors: true, strict: false })
  addFormats(ajv as unknown as Parameters<typeof addFormats>[0])
  return ajv
}

export type ValidadorEsquema = ReturnType<typeof crearValidador>
