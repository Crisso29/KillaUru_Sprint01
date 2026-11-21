# Panel de administración «KillaUru Ops»

Panel web de administración para el sistema KillaUru. SPA React + TypeScript en Cloudflare Pages.

## Propósito

Interfaz exclusiva para administradores: moderación de anomalías, gestión de usuarios, auditoría de acciones, exportación de datos y resumen del sistema.

URL prevista de producción: `https://panel.killauru.pe`. Su disponibilidad y despliegue no se verificaron en esta revisión local.

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
| `VITE_TURNSTILE_SITEKEY` | Clave pública de Turnstile; nombre leído por el código | (ver H-02) |
| `VITE_URL_CDN` | Origen CDN usado por la generación de cabeceras del despliegue | Configurar según entorno |
| `PANEL_URL` | URL comprobada por el script de cabeceras, fuera del bundle | Configurar según entorno |

## Modos de prueba

Consulta la [organización y pendientes de las pruebas](pruebas/README.md). Los E2E simulados y los escenarios omitidos no acreditan validación contra el backend real.

| Modo | Descripción |
|---|---|
| `npm run dev` | Desarrollo con MSW interceptando peticiones en el navegador |
| `npm run build:simulado` | Bundle con MSW activado para Playwright E2E simulado |
| `npm run test` | Vitest: unitarias + componentes |
| `npm run e2e` | Playwright E2E simulado (modo simulado) |
| `npm run e2e:pruebas` | Configuración desplegada separada, sin servidor local; requiere `PANEL_URL` HTTPS. Sin casos `*.pruebas.ts` todavía |

## Cuentas de prueba para E2E

Entorno previsto: `killauru-pruebas` (ver H-02 para configuración del backend).
Los datos siguientes son ejemplos sintéticos requeridos por T088; no se ha verificado
que estén provisionados. No reutilizar esas contraseñas en producción.

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

La semilla del entorno «killauru-pruebas» debe incluir al menos una anomalía en cada estado del ciclo de vida:

| Estado | Descripción |
|---|---|
| `Activa` | Anomalía reportada y confirmada por segundo vehículo |
| `PendienteConfirmacion` | Reportada, esperando confirmación |
| `Expirada` | Sin confirmar dentro del plazo |
| `Reparada` | Marcada como reparada por el administrador |
| `Descartada` | Descartada por el administrador |

Además: 1 anomalía `Descartada` con `porRevisar = true` (para probar «Por revisar» en moderación).

## Despliegue

El workflow de despliegue escucha la finalización correcta de `Panel CI` para un push a `main` del mismo repositorio. Publica el SHA verificado y requiere secretos y variables completos en el entorno `produccion`.

Ver `.github/workflows/panel-deploy.yml`.

Requisitos (ver H-03):
- Secreto `CLOUDFLARE_API_TOKEN` con permiso «Pages: editar»
- Secreto `CLOUDFLARE_ACCOUNT_ID`

## Volver atrás

En Cloudflare Pages: Dashboard → killauru-panel → Deployments → seleccionar versión anterior → «Rollback».

O preparar una reversión revisada y pasar nuevamente el CI de `main`. Un checkout local no publica por sí solo:

```bash
git checkout <commit-anterior>
# Esto solo cambia el checkout local; no dispara el despliegue.
```

Tiempo estimado: < 5 minutos.
