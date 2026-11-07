# Panel de administración «KillaUru Ops»

Panel web de administración para el sistema KillaUru. SPA React + TypeScript en Cloudflare Pages.

## Propósito

Interfaz exclusiva para administradores: moderación de anomalías, gestión de usuarios, auditoría de acciones, exportación de datos y resumen del sistema.

URL de producción: `https://panel.killauru.pe`

## Arranque local

```bash
# Instalar dependencias
npm ci

# Servidor de desarrollo (con MSW en el navegador)
npm run dev

# Build simulado (MSW activado en el bundle)
npm run build:simulado
npx vite preview --port 4173

# Build de producción
npm run build
```

Requiere Node 22 LTS.

## Variables de entorno

Copia `.env.example` como `.env.local` para el desarrollo:

```bash
cp .env.example .env.local
```

Variables disponibles:

| Variable | Descripción | Ejemplo |
|---|---|---|
| `VITE_URL_API` | URL base de la API | `https://api.killauru.pe` |
| `VITE_URL_ESTILO_MAPA` | URL del estilo MapLibre | `https://cdn.killauru.pe/estilo/v1/style.json` |
| `VITE_URL_PMTILES` | URL del archivo PMTiles en R2 | `https://cdn.killauru.pe/mapa/corredor.pmtiles` |
| `VITE_URL_ESTADO` | URL de la página de estado (UptimeRobot) | `https://status.killauru.pe` |
| `VITE_TURNSTILE_SITE_KEY` | Clave pública de Turnstile | (ver H-02) |

## Modos de prueba

| Modo | Descripción |
|---|---|
| `npm run dev` | Desarrollo con MSW interceptando peticiones en el navegador |
| `npm run build:simulado` | Bundle con MSW activado para Playwright E2E simulado |
| `npm run test` | Vitest: unitarias + componentes |
| `npm run e2e` | Playwright E2E simulado (modo simulado) |
| `npm run e2e:pruebas` | Playwright E2E real contra «killauru-pruebas» (ver H-02) |

## Cuentas de prueba para E2E

Entorno: `killauru-pruebas` (ver H-02 para configuración del backend).

### Administradores

| Correo | Contraseña | Notas |
|---|---|---|
| `admin1@killauru-pruebas.pe` | `Admin1!Test` | Administrador principal; realiza las acciones de moderación |
| `admin2@killauru-pruebas.pe` | `Admin2!Test` | Segundo administrador; para probar revisión de privilegios |

### Conductores

| Correo | Contraseña | Notas |
|---|---|---|
| `conductor@killauru-pruebas.pe` | `Cond1!Test` | ConductorRegistrado; sin acceso al panel |

### Anomalías de prueba

El entorno «killauru-pruebas» incluye al menos una anomalía en cada estado del ciclo de vida:

| Estado | Descripción |
|---|---|
| `Activa` | Anomalía reportada y confirmada por segundo vehículo |
| `PendienteConfirmacion` | Reportada, esperando confirmación |
| `Expirada` | Sin confirmar dentro del plazo |
| `Reparada` | Marcada como reparada por el administrador |
| `Descartada` | Descartada por el administrador |

Además: 1 anomalía `Descartada` con `porRevisar = true` (para probar «Por revisar» en moderación).

## Despliegue

El despliegue es automático al unir a `main` si hay cambios en `panel/**`.

Ver `.github/workflows/panel-deploy.yml`.

Requisitos (ver H-03):
- Secreto `CLOUDFLARE_API_TOKEN` con permiso «Pages: editar»
- Secreto `CLOUDFLARE_ACCOUNT_ID`

## Volver atrás

En Cloudflare Pages: Dashboard → killauru-panel → Deployments → seleccionar versión anterior → «Rollback».

O redesplegar el commit anterior:

```bash
git checkout <commit-anterior>
# El workflow de deploy se activa al push a main
```

Tiempo estimado: < 5 minutos.
