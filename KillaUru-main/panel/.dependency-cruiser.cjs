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
    // Código fuera de funcionalidades solo puede importar su index.ts
    {
      name: 'funcionalidades-solo-via-index',
      severity: 'error',
      comment: 'código externo a funcionalidades importa solo su index.ts',
      from: {
        path: '^src/',
        pathNot: [
          'src/funcionalidades/',          // archivos dentro de cualquier funcionalidad
          'src/api/generado/',
        ],
      },
      to: {
        path: 'src/funcionalidades/[^/]+/',
        pathNot: 'src/funcionalidades/[^/]+/index\\.ts',
      },
    },
    // Una funcionalidad solo importa internals de otra via su index.ts
    {
      name: 'funcionalidades-cruzadas-via-index',
      severity: 'error',
      comment: 'una funcionalidad no importa los internals de otra',
      from: { path: '^src/funcionalidades/login/' },
      to: {
        path: '^src/funcionalidades/(mapa|resumen|moderacion|usuarios|auditoria|exportacion|cuenta)/',
        pathNot: 'index\\.ts$',
      },
    },
    {
      name: 'funcionalidades-cruzadas-via-index-mapa',
      severity: 'error',
      from: { path: '^src/funcionalidades/mapa/' },
      to: {
        path: '^src/funcionalidades/(login|resumen|moderacion|usuarios|auditoria|exportacion|cuenta)/',
        pathNot: 'index\\.ts$',
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
