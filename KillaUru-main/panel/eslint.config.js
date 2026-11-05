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
    files: ['src/**/modelo/**/*.{ts,tsx}', 'src/sesion/**/*.{ts,tsx}'],
    rules: { ...noDirectDateInModelos },
  },
  {
    ignores: ['src/api/generado/**', 'dist/**', 'node_modules/**'],
  },
)
