import js from '@eslint/js'
import tseslint from 'typescript-eslint'
import reactHooks from 'eslint-plugin-react-hooks'

// FR-006: sin tokens en almacenamiento del navegador
// FR-047: archivos < 200 líneas
const noStorageRule = {
  'no-restricted-globals': ['error',
    { name: 'localStorage', message: 'FR-006: sin almacenamiento de sesión en el navegador' },
    { name: 'sessionStorage', message: 'FR-006: sin almacenamiento de sesión en el navegador' },
    { name: 'indexedDB', message: 'FR-006: sin almacenamiento de sesión en el navegador' },
  ],
  'no-restricted-syntax': [
    'error',
    {
      selector: "MemberExpression[object.name='document'][property.name='cookie']",
      message: 'FR-006: no leer ni escribir document.cookie',
    },
    {
      selector: "JSXAttribute[name.name='dangerouslySetInnerHTML']",
      message: 'Prohibido dangerouslySetInnerHTML — riesgo XSS',
    },
  ],
}

const noDirectDateInModelos = {
  'no-restricted-syntax': [
    'error',
    {
      selector: "CallExpression[callee.object.name='Date'][callee.property.name='now']",
      message: 'Usar reloj inyectado en modelos/sesion — no Date.now() directo',
    },
    {
      selector: "NewExpression[callee.name='Date'][arguments.length=0]",
      message: 'Usar reloj inyectado en modelos/sesion — no new Date() sin argumento',
    },
  ],
}

export default tseslint.config(
  js.configs.recommended,
  ...tseslint.configs.recommended,
  {
    files: ['src/**/*.{ts,tsx}', 'pruebas/**/*.{ts,tsx}'],
    plugins: { 'react-hooks': reactHooks },
    rules: {
      ...reactHooks.configs.recommended.rules,
      ...noStorageRule,
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
    },
  },
  {
    // Producción usa el reloj inyectado; las pruebas controlan el tiempo con vi.useFakeTimers (ahí Date.now() es falso)
    files: ['src/**/modelo/**/*.{ts,tsx}', 'src/sesion/**/*.{ts,tsx}'],
    ignores: ['**/*.test.{ts,tsx}'],
    rules: { ...noDirectDateInModelos },
  },
  {
    // Constitución VIII (igual que detekt en la app y la suite de arquitectura del backend): funciones cortas, de una
    // responsabilidad. En las pruebas no aplica: un describe agrupa muchos casos; ahí rige el patrón AAA.
    files: ['src/**/*.{ts,tsx}'],
    ignores: ['**/*.test.{ts,tsx}'],
    rules: { 'max-lines-per-function': ['error', { max: 20, skipBlankLines: true, skipComments: true }] },
  },
  {
    // Nombres que se entienden sin leer el resto: nada de `r`, `e`, `p`… salvo los convenidos de coordenadas e índices
    files: ['src/**/*.{ts,tsx}', 'pruebas/**/*.{ts,tsx}'],
    rules: { 'id-length': ['error', { min: 2, exceptions: ['_', 'x', 'y', 'i', 'j'], properties: 'never' }] },
  },
  {
    // FR-006 prohíbe al PANEL usar el almacenamiento del navegador. Las E2E de seguridad lo leen justamente para
    // comprobar que el panel lo dejó vacío; solo se les permite leerlo (dangerouslySetInnerHTML sigue prohibido).
    files: ['pruebas/e2e/seguridad/**/*.ts'],
    rules: {
      'no-restricted-globals': 'off',
      'no-restricted-syntax': ['error', noStorageRule['no-restricted-syntax'][2]],
    },
  },
  {
    ignores: ['src/api/generado/**', 'dist/**', 'node_modules/**'],
  },
)
