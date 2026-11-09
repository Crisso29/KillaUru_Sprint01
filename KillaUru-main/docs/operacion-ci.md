# Integración y verificación

La integración conserva la historia del `main` original y las once ramas locales
encontradas (nueve ramas remotas, sin contar `origin/HEAD`). No se borraron ramas
ni worktrees. La rama `fix/integracion-specs-ci` reúne los avances de specs 001–010
y las correcciones necesarias para verificarlos antes de fusionarlos.

## Workflows

Todos viven en `.github/workflows` de la raíz del repositorio. GitHub no ejecuta
los workflows situados dentro de `KillaUru-main/.github`.

| Workflow | Verificación |
| --- | --- |
| CI | .NET 10, arquitectura, pruebas con PostgreSQL/Testcontainers y cobertura de dominio ≥90 % |
| App Android | Java 21, API 35, APK, Lint, Detekt, pruebas, Kover ≥90 % y archivos <200 líneas |
| ML CI | Python 3.11, dependencias con hashes, Ruff, pytest y dos entrenamientos reproducibles |
| Panel CI | Contratos generados, TypeScript, ESLint, dependencias, cobertura, presupuesto JS y Playwright |

Las versiones de Android están fijadas; los avisos de nuevas versiones de
dependencias no bloquean el mismo commit según el día de ejecución. Las reglas
de permisos, manifiesto, arquitectura y cobertura continúan activas.

Para verificar localmente, usar los comandos de los workflows desde su
`working-directory`. Backend necesita Docker activo para las pruebas de integración.
Android necesita **Java 21**: Robolectric no admite el Java 25 incluido en algunas
instalaciones de Android Studio. ML usa un entorno Python 3.11 independiente.

## Despliegue

CD y Panel Deploy solo procesan un push a `main` cuyo CI terminó correctamente.
Publican el SHA comprobado. Un pull request no despliega.

Configurar los siguientes secretos en el entorno GitHub `produccion`:

- Backend: `SSH_PRIVATE_KEY`, `VM_HOSTNAME`, `VM_USER`,
  `CF_ACCESS_CLIENT_ID`, `CF_ACCESS_CLIENT_SECRET`.
- Panel: `CLOUDFLARE_API_TOKEN`, `CLOUDFLARE_ACCOUNT_ID`.

Variables del panel: `VITE_URL_API`, `VITE_URL_ESTILO_MAPA`, `VITE_URL_PMTILES`,
`VITE_URL_ESTADO`, `VITE_TURNSTILE_SITEKEY`, `VITE_URL_CDN` y `PANEL_URL`.
El build fija `VITE_ENTORNO=produccion` y genera `dist/_headers`.
Backend acepta `KU_GHCR_CUENTA`; por defecto utiliza el propietario en minúsculas.

Sin configuración, el resumen del workflow enumera los nombres faltantes y omite
el despliegue. **CI verde no significa infraestructura desplegada.** No guardar
credenciales en archivos versionados. Los scripts de despliegue requieren LF y
permiso de ejecución en Linux.

## Alcance pendiente de las specs

Integrar las ramas no certifica la aceptación funcional de las diez specs:

- El dataset real, el modelo aprobado y su publicación siguen requiriendo los
  entregables de campo de spec 009. El humo sintético comprueba reproducibilidad;
  no acredita calidad predictiva y nunca se publica como modelo de producción.
- El APK contiene un manifiesto de ejemplo, no un modelo TFLite aprobado. Sin
  archivo y umbral de prefiltro, el servicio no inicia la detección.
- El panel conserva escenarios E2E pendientes y algunos esqueletos de pruebas
  existentes. Los escenarios omitidos no cuentan como verificados; las pruebas
  contra servicios reales requieren configurar el entorno de pruebas.
- `publicar-modelo` es manual: exige un artefacto de una versión aprobada y la
  implementación de la puerta de publicación del CLI. No sustituir ese requisito
  por el modelo sintético ni considerar completada la publicación con CI verde.

Después de una fusión, comprobar los runs del SHA de `main` y, por separado, el
resultado de publicación de imágenes y despliegue. Conservar las ramas originales
hasta que se haya revisado la aceptación funcional.
