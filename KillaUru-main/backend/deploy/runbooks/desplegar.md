# Runbook · Desplegar

## Primer despliegue (una sola vez, en la VM)

1. VM Oracle A1 (4 OCPU / 24 GB, arm64, Santiago) con Docker y el repositorio en `/opt/killauru`.
2. Copiar `deploy/.env.example` a `deploy/.env` y llenar los secretos (ver `secretos.md`). Nunca subir `.env`.
3. Construir las imágenes locales: `docker compose build postgres backup`.
4. Levantar solo la base: `docker compose up -d postgres`.
5. Arranque de la base como superusuario: `docker compose run --rm --entrypoint /killauru/deploy/migrar.sh migrador --arranque`.
6. Migraciones: `docker compose run --rm migrador` (hace respaldo previo y aplica V0001…).
7. Todo: `./desplegar.sh <sha>`. Termina bien solo si `/health/ready` responde 200.

> El orden es siempre **arranque → migraciones → levantar** (T-BD01): la API nueva arranca sobre un esquema ya migrado.

## Despliegues normales (automáticos, D10)

- Cada unión a `main` con el CI en verde corre `cd.yml`: imagen arm64 → `ghcr.io/<cuenta>/killauru-api:<sha>` → SSH por Cloudflare Access → `desplegar.sh <sha>`.
- `main` está protegida (T070): *check* `ci` obligatorio, sin uniones directas ni *force-push*. Configurar en GitHub → Settings → Branches → Branch protection rules.
- A mano, si hiciera falta: `ssh` por Cloudflare Access y `cd /opt/killauru/KillaUru-main/backend/deploy && ./desplegar.sh <sha>`.

## Reglas de migraciones (RNF-18)

- Cada script `V####__descripcion.sql` lleva en su primera línea `-- V#### · descripcion · fase 1|2`.
- Fase 1 = agregar (columnas, tablas, índices). Fase 2 = borrar, **en un despliegue posterior** a la versión que dejó de usarlo. Así la versión anterior siempre funciona con el esquema nuevo y la vuelta atrás es solo cambiar la imagen.

## Identidad: primer administrador (003)

Aplicar V0003 antes de arrancar esta versión y configurar los secretos de Identidad. Generar dos claves
independientes para firma JWT y rotación HMAC. No copiar sus valores a Git, terminal compartida ni tickets.

Validar la composición y ejecutar desde una terminal privada:

```sh
docker compose -f docker-compose.yml -f docker-compose.semilla.yml config --quiet
docker compose -f docker-compose.yml -f docker-compose.semilla.yml run --rm --no-deps api crear-admin admin@killauru.pe
```

El override usa `logging.driver=none` para que la consola con el enlace no alimente Docker ni Alloy.
No usar `tee`, redirección a archivos ni una sesión grabada. Canjear la invitación de 24 h en el panel.
El comando no inicia HTTP ni manda correo; audita UsuarioCreado con actor Sistema. Rechaza la ejecución
si ya existe cualquier administrador, incluso invitado o desactivado. No es un mecanismo de recuperación.
La página que canjea el enlace pertenece a 010; en pruebas se usa POST /api/v1/auth/fijar-contrasena.
