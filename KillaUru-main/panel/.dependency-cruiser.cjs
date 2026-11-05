/** @type {import('dependency-cruiser').IConfiguration} */
module.exports = {
  forbidden: [
    // Regla de capas: ui → api → modelo (sentido único)
    {
      name: 'modelo-no-importa-api',
      severity: 'error',
      comment: 'modelo no puede importar de api',
      from: { path: 'src/.*/modelo/' },
      to: { path: 'src/api/' },
    },
    {
      name: 'modelo-no-importa-ui',
      severity: 'error',
      comment: 'modelo no puede importar componentes UI',
      from: { path: 'src/.*/modelo/' },
      to: { path: 'src/.*/ui/' },
    },
    {
      name: 'api-no-importa-ui',
      severity: 'error',
      comment: 'api no puede importar componentes UI',
      from: { path: 'src/api/' },
      to: { path: 'src/.*/ui/' },
    },
    // Funcionalidades solo via index.ts
    {
      name: 'funcionalidades-solo-via-index',
      severity: 'error',
      comment: 'importar funcionalidades solo desde su index.ts',
      from: { path: 'src/', pathNot: 'src/funcionalidades/[^/]+/index\\.ts' },
      to: {
        path: 'src/funcionalidades/[^/]+/',
        pathNot: 'src/funcionalidades/[^/]+/index\\.ts',
      },
    },
    // Sin ciclos
    {
      name: 'sin-ciclos',
      severity: 'error',
      comment: 'no se permiten dependencias circulares',
      from: {},
      to: { circular: true },
    },
  ],
  options: {
    doNotFollow: { path: 'node_modules' },
    tsConfig: { fileName: 'tsconfig.app.json' },
    reporterOptions: {
      text: { highlightFocused: true },
    },
  },
}
