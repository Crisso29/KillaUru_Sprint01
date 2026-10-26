# Runbook · Inventario de secretos (RNF-34)

Ningún secreto vive en el repositorio. Los de la VM están en `deploy/.env` (permisos 600); los de GitHub, en el entorno `produccion`. Todos tienen una **copia cifrada fuera de la VM** (gestor de contraseñas del autor) y se rotan al menos una vez al año o ante cualquier sospecha.

| Secreto | Dónde | Para qué | Cómo se rota |
|---|---|---|---|
| `KU_PG_SUPERUSUARIO` | `.env` | arranque de la base y respaldo | `ALTER ROLE postgres PASSWORD …` + `.env` |
| `KU_DB_MIGRADOR_PASSWORD` | `.env` | migraciones | `ALTER ROLE killauru_migrador …` |
| `KU_DB_APP_PASSWORD` | `.env` | la API | `ALTER ROLE killauru_app …` + reiniciar la API |
| `KU_JWT_CLAVE_FIRMA` | `.env` | firmar tokens (≥ 32 bytes) | cambiar y reiniciar: todos reinician sesión (también al restaurar, D9) |
| `KU_CORREO_API_KEY` | `.env` | correo transaccional (003) | panel del proveedor |
| `KU_R2_ACCESS_KEY_ID` / `KU_R2_SECRET_ACCESS_KEY` | `.env` | subir respaldos | panel de Cloudflare R2 |
| `KU_BACKUP_CLAVE_PUBLICA` | `.env` | cifrar respaldos | nuevo par con `age-keygen`; la **privada** solo fuera de la VM |
| `KU_GRAFANA_*` | `.env` | telemetría y Alloy | Grafana Cloud → Access Policies |
| `KU_TUNEL_TOKEN` | `.env` | túnel de Cloudflare | Cloudflare Zero Trust → Tunnels |
| `KU_CF_ACCESS_CLIENT_ID` / `KU_CF_ACCESS_CLIENT_SECRET` | GitHub `produccion` | SSH del despliegue (D10) | Cloudflare Zero Trust → Service Auth |
| `KU_DEPLOY_SSH_KEY` | GitHub `produccion` | SSH del despliegue | nueva llave + `authorized_keys` del usuario `killauru` |
| `KU_URL_PRUEBAS` | GitHub | `carga.yml` | — |

## Cloudflare Access para el SSH del despliegue (T072)

1. Zero Trust → Access → Applications → *Self-hosted* `ssh.killauru.pe` con política **Service Auth** que solo acepta el token de servicio del despliegue.
2. En el túnel, una ruta `ssh.killauru.pe` → `ssh://localhost:22`. La VM sigue sin puertos abiertos.
3. Usuario `killauru` en la VM, sin contraseña, con la llave pública de `KU_DEPLOY_SSH_KEY` y permiso solo sobre Docker.

## Secretos añadidos por Identidad (003)

- `KU_IDENTIDAD_CLAVE_ROTACION`: mínimo 32 bytes aleatorios, independiente de JWT; deriva sucesores refresh.
- `KU_TURNSTILE_SECRETO`: clave privada del widget del panel, nunca enviada al navegador.
- `KU_CORREO_API_KEY`: API key de Resend con permiso de envío restringido al dominio verificado.

Compose los mapea a Identidad__ClaveRotacion, Identidad__TurnstileSecreto e Identidad__CorreoApiKey.
Rotar la clave HMAC requiere revocar las sesiones existentes; de otro modo no se puede reconstruir el
sucesor de una rotación previa durante la gracia. Tras restaurar, revocar sesiones y rotar siempre JWT.
Conservar las nuevas claves en la copia cifrada del inventario y validar entrega/anti-bot según correo.md.
