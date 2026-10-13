# Actores del Sistema

> **Proyecto:** KillaUru · **Versión:** 1.1 · **Revisión:** 2026-10-12 · **Estado:** Vigente (alineado con C4 Nivel 1)
> **Relacionados:** `Nivel1-diagrama-de-contexto.md`, `02-historias-de-usuario.md`, `03-requisitos-funcionales.md`

El sistema tiene **tres actores humanos** (dos tipos de conductor y el administrador) y **sistemas externos de infraestructura**. Las empresas y las autoridades viales están identificadas solo para versiones futuras, para limitar el alcance del ciclo 2026-II.

## 1. Actores humanos

| Actor (nombre en C1) | Rol RBAC | Tipo | Descripción | Interfaz | Alcance de datos | Autenticación |
|---|---|---|---|---|---|---|
| **Conductor No Registrado** | `Anonimo` (sin token) | Principal (v1.0) | Instala la app y no crea cuenta. Sus sensores detectan anomalías de forma pasiva y aporta eventos de forma anónima. | App Android | Solo el tramo demo Ayacucho–Vinchos (~65 km) | Ninguna. Se identifica solo con un seudónimo de instalación rotativo (RF-27) |
| **Conductor Registrado** | `ConductorRegistrado` | Principal (v1.0) | Conductor con cuenta (correo y contraseña). Ve el corredor completo, recibe alertas anticipadas y consulta su historial local. | App Android | Corredor completo de 333 km (PE-28A, Vía Los Libertadores) | JWT de acceso con renovación (RF-21) |
| **Administrador** | `Administrador` | Principal (v1.0) · Personal interno de CRISSO DEV S.A.C. | Gestiona usuarios e identidad, administra anomalías, consulta estadísticas globales, exporta datos y revisa la auditoría. | Panel web | Corredor completo y datos de operación | Correo y contraseña, JWT de 8 h, bloqueo temporal tras 3 intentos fallidos (RF-16) |

> **Nota:** los dos tipos de conductor usan **la misma app (mismo APK)**. Cambian el alcance de los datos y las funciones disponibles, no la aplicación.

## 2. Sistemas externos (infraestructura)

| Sistema | Tipo | Responsabilidad | Estado |
|---|---|---|---|
| Cloudflare | Infraestructura externa | DNS, terminación TLS, proxy HTTPS y protección perimetral | v1.0 |
| UptimeRobot | Infraestructura externa | Consulta `GET /health` cada 5 minutos y alimenta `status.killauru.pe` | v1.0 |
| Proveedor de correo transaccional (p. ej. Amazon SES / Resend) | Infraestructura externa | Correo de verificación de registro (HU-18b) | Roadmap (*Could*) |

> El mapa base offline se aloja como archivo estático propio (ADR-011), por lo que no añade un sistema externo en v1.0.

## 3. Actores futuros (fuera de alcance en este ciclo)

| Actor | Versión | Descripción | Interfaz |
|---|---|---|---|
| Empresa suscriptora | v1.5+ | Empresas de transporte y aseguradoras. Acceden a analítica de flota y tramos. | Panel web |
| Autoridad Vial | v2.0+ | Provías Nacional, SUTRAN y gobiernos regionales. Acceden bajo convenio institucional. | Panel web |

Ambos requerirán nuevos roles RBAC y una API comercial (ver patrón *Facade* en `patrones-de-diseño.md`).

## 4. Matriz de permisos por rol

| Capacidad | Anónimo | Conductor Registrado | Administrador |
|---|:---:|:---:|:---:|
| Detectar anomalías y guardarlas localmente | ✅ | ✅ | — |
| Sincronizar eventos (`POST /api/v1/anomalies/batch`) | ✅ | ✅ | — |
| Ver mapa del tramo demo (~65 km) | ✅ | ✅ | ✅ |
| Ver corredor completo (333 km) | ❌ | ✅ | ✅ |
| Recibir alertas anticipadas | ❌ | ✅ | — |
| Ver historial de recorridos (local, 6 meses) | ❌ | ✅ | — |
| Registrarse e iniciar sesión | — | ✅ | ✅ (login) |
| Aceptar la política de privacidad (RF-29) | ✅ (aviso) | ✅ | — |
| Ver mis datos personales (RF-31) | — | ✅ | — |
| Eliminar mi cuenta (RF-30) | — | ✅ | — |
| Mapa con filtros, incluidas anomalías sin confirmar | ❌ | ❌ | ✅ |
| Dashboard de estadísticas | ❌ | ❌ | ✅ |
| Gestionar usuarios e identidad (Identidad y Accesos) | ❌ | ❌ | ✅ |
| Moderar anomalías (reparada, descartar) | ❌ | ❌ | ✅ |
| Exportar dataset (CSV y GeoJSON) | ❌ | ❌ | ✅ |
| Consultar log de auditoría | ❌ | ❌ | ✅ |
| Límite de tasa (RNF-11) | 60 req/h | 1 000 req/h | Definido en RNF-11 |

> **Regla de aplicación:** los permisos se **hacen cumplir en el servidor** (RBAC en la API REST), nunca solo en la app. Un anónimo que consulte fuera del tramo demo recibe la respuesta recortada al polígono demo (RF-14, RNF-27).

## 5. Reglas de identidad y privacidad por actor

| Actor | Qué datos se guardan | Qué **no** se guarda |
|---|---|---|
| Conductor No Registrado | Eventos de anomalía con seudónimo rotativo (se purga en 72 h) | Correo, nombre o cualquier dato personal |
| Conductor Registrado | Correo, credenciales y versión aceptada de la política de privacidad (RF-29). Puede verlos (RF-31) y borrarlos (RF-30) | Vínculo entre su cuenta y sus eventos o trayectorias. El historial vive solo en su dispositivo (RF-25) |
| Administrador | Correo, rol y acciones auditadas | — |

Detalle normativo: R-06 (`05-restricciones.md`), RNF-10 y RNF-25 (`04-atributos-de-calidad.md`), ADR-007.

## 6. Trazabilidad

| Actor | Historias de usuario |
|---|---|
| Conductor No Registrado | HU-01, HU-02, HU-03, HU-04, HU-07, HU-21, HU-22 (aviso) |
| Conductor Registrado | HU-01…HU-04, HU-08, HU-09, HU-10, HU-18, HU-18b, HU-21, HU-22, HU-23, HU-24 |
| Administrador | HU-06, HU-11, HU-12, HU-13, HU-14, HU-15, HU-19, HU-20 |
| Sistema (historia técnica) | HU-05 |
| Equipo KillaUru | HU-16, HU-17 |