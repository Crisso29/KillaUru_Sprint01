// Detecta pruebas sin las tres fases Arrange / Act / Assert marcadas con comentarios (AGENTS.md: «Conservar AAA»).
// Se analiza con el compilador de TypeScript, no con expresiones regulares, para ubicar bien el cuerpo de cada prueba.
import ts from 'typescript'

const FASES = ['Arrange', 'Act', 'Assert'] as const
/** Variantes de `it`/`test` que declaran una prueba (`describe`, `beforeEach`, etc. no lo son). */
const VARIANTES = new Set(['skip', 'only', 'fixme', 'fail', 'concurrent', 'each', 'todo'])

/** Fases que marca un comentario: «// Arrange y Act — …» marca dos. */
export function fasesDelComentario(linea: string): string[] {
  const marca = /^\s*\/\/\s*((?:Arrange|Act|Assert)(?:\s+y\s+(?:Arrange|Act|Assert))*)\b/.exec(linea)
  return marca ? marca[1].split(/\s+y\s+/) : []
}

/** true si el cuerpo marca Arrange, Act y Assert, cada una una sola vez y en ese orden. */
export function tieneAaa(cuerpo: string): boolean {
  const fases = cuerpo.split('\n').flatMap(fasesDelComentario)
  return fases.join(',') === FASES.join(',')
}

function esPrueba(expresion: ts.Expression): boolean {
  if (ts.isIdentifier(expresion)) return expresion.text === 'it' || expresion.text === 'test'
  if (ts.isCallExpression(expresion)) return esPrueba(expresion.expression) // it.each([...])('nombre', fn)
  if (!ts.isPropertyAccessExpression(expresion)) return false
  return VARIANTES.has(expresion.name.text) && esPrueba(expresion.expression)
}

/** Nombres de las pruebas del archivo cuyo cuerpo no sigue AAA (las que no tienen cuerpo, p. ej. `test.todo`, no cuentan). */
export function pruebasSinAaa(ruta: string, codigo: string): string[] {
  const archivo = ts.createSourceFile(ruta, codigo, ts.ScriptTarget.Latest, true)
  const faltan: string[] = []
  const visitar = (nodo: ts.Node) => {
    if (ts.isCallExpression(nodo) && esPrueba(nodo.expression)) {
      const funcion = nodo.arguments.find((argumento) => ts.isArrowFunction(argumento) || ts.isFunctionExpression(argumento))
      const nombre = nodo.arguments[0]
      if (funcion && !tieneAaa(funcion.body.getFullText(archivo))) faltan.push(nombre && ts.isStringLiteralLike(nombre) ? nombre.text : '(sin nombre)')
    }
    ts.forEachChild(nodo, visitar)
  }
  visitar(archivo)
  return faltan
}
