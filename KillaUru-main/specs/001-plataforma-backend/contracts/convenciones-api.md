# Convenciones de la API REST de KillaUru

Todos los contratos de las features 002–007 siguen estas reglas. Si un contrato no las cumple, el contrato está mal.

## 1. Rutas y versiones

| Regla | Ejemplo |
|---|---|
| Todo endpoint de negocio cuelga de `/api/v1` | `/api/v1/anomalies/batch` |
| Las rutas que la app ya usa en los documentos se mantienen en inglés: `/api/v1/anomalies` (RF-11, RF-14) | — |
| Las rutas nuevas van en español, en minúsculas y con guiones | `/api/v1/usuarios/{id}/desactivar` |
| Acciones que no son CRUD: `POST` a un sub-recurso con verbo | `POST /api/v1/anomalies/{id}/moderacion` |
| `/health/live` y `/health/ready` no llevan versión | RF-28 |

## 2. Formato

- JSON en `camelCase`; fechas en ISO 8601 **UTC** con `Z` (`2026-10-20T14:32:05Z`); ids `uuid`.
- Enumeraciones como texto con el nombre del glosario: `Bache`, `Hundimiento`, `Activa`, `PendienteConfirmacion`…
- Geodatos del mapa en **GeoJSON** (`application/geo+json`), coordenadas `[longitud, latitud]`.
- Listas paginadas: `?pagina=1&tamano=50` (máximo 200); respuesta `{ elementos: [], pagina, tamano, total }`.

## 3. Errores

- Siempre `application/problem+json` con el esquema `Problema` de `plataforma.openapi.yaml`.
- `codigo` estable en español y `snake_case`. Catálogo común: `token_invalido` (401), `sin_permiso` (403), `no_encontrado` (404), `datos_invalidos` (400), `conflicto` (409), `limite_excedido` (429), `saturado` (503), `error_interno` (500). Cada módulo agrega los suyos en su contrato.
- Nunca se devuelven trazas, nombres de tablas ni datos de otra cuenta.
- Respuestas que no deben revelar si una cuenta existe (RN-19, RF-32) responden **igual** en ambos casos.

## 4. Seguridad por endpoint

Cada operación declara en su descripción una línea **«Rol: …»** y su `security`:

| Rol permitido | `security` |
|---|---|
| Público (incluido anónimo) | `[]` o `[{}, {tokenAcceso: []}]` si el token cambia la respuesta |
| Conductor registrado | `[{tokenAcceso: []}]` |
| Administrador | `[{tokenAcceso: []}, {cookieAcceso: []}]` |

Los códigos 401 y 403 se declaran en **todas** las operaciones que pueden darlos, y el **429 en todas** las operaciones de `/api/v1` (el límite de tasa es global, FR-010, FR-030; T-SEG19), con las respuestas comunes (`$ref`).

**Regla única 401/403 (FR-045, T-SEG20):**

| Situación | Código |
|---|---|
| Token inválido, vencido o con rol desconocido (en cualquier endpoint) | 401 `token_invalido` |
| Sin token en un endpoint que exige sesión | 401 `token_invalido` |
| Con token y rol insuficiente | 403 `sin_permiso` |
| Parámetro reservado (`estado`) usado por un rol que no es administrador, **con o sin token** | 403 `sin_permiso` |

Ningún contrato ni prueba dice «403 o 401»: cada escenario espera un solo código.

## 5. Caché

- Por defecto: `Cache-Control: private, no-store` (lo pone el host).
- Solo `GET /api/v1/anomalies` **sin token** puede responder `public, max-age=60` con `ETag` (ADR-006, feature 006).

## 6. Documentación en el código (para que `/docs` quede completo)

- Cada acción: `[EndpointSummary]`, `[EndpointDescription]` con la línea «Rol: …» y los RF/RN que cumple, y un `[ProducesResponseType]` por código.
- Cada DTO público: comentario XML en la clase y en cada propiedad, con un `<example>`.
- El `operationId` del contrato es el nombre de la acción en `camelCase`.

## 7. Prueba de contrato

`KillaUru.Integracion.Tests/Contratos/ContratoOpenApiTests` descarga `/openapi/v1.json` y verifica, para cada YAML de `specs/*/contracts/`: misma ruta y método, mismos códigos de respuesta y mismas propiedades requeridas en los cuerpos.
