# Revisión de implementación · 003-modulo-identidad-accesos

Fecha: 2026-10-24. Base: requisitos de referencia, constitución 1.3.0, spec, plan, modelo de datos y contrato de 003; integración con 001/002.

**Estado: implementación local completada; activación y validación en el entorno desplegado pendientes.** No se enviaron correos reales ni se alteraron servicios externos.

## Resultado

Las ocho historias tienen implementación: registro y consentimiento; login con bloqueo progresivo; JWT y refresh rotativo; verificación, invitación y recuperación; acceso y eliminación de datos del titular; administración y revisión mensual; consulta agregada para Dashboard; retención y restauración.

V0003 crea cinco tablas, índices de cascada y ninguna relación cuenta-evento. Puertos separan aplicación de EF, correo, criptografía y anti-bot. Adaptadores HTTP sin Domain, tipos internos, comentarios de responsabilidad, un tipo por archivo y límites de legibilidad verificados automáticamente.

Escrituras y auditoría confirman o revierten juntas. Bloqueos por cuenta impiden dobles canjes y rotaciones incompatibles; un bloqueo separado protege el conjunto de administradores. Se respeta la precisión de microsegundos de PostgreSQL en el límite inclusivo de sesenta segundos. Recuperación evita revelar si existe el correo y aplica cuotas por correo/IP sin conservarlos en claro en memoria.

## Evidencia

Informes de cobertura: `backend/TestResults/003-final` y `003-integracion-final`; logs correspondientes en `backend/TestResults/`. Contratos generados comparados con YAML, incluidos esquemas, seguridad, respuestas y ejemplos. Integración usa PostgreSQL real, migraciones y roles reales; sustituye correo y anti-bot.

| Validación | Resultado |
|---|---|
| Unitarias Identidad | 45 correctas |
| Unitarias Shared / Auditoría | 62 / 22 correctas |
| Arquitectura y legibilidad | 265 correctas |
| Integración y contratos (001/002/003) | 111 correctas; carga explícita omitida en esta suite y ejecutada aparte |
| Cobertura Domain Identidad | 100 % (92/92 líneas) |
| Cobertura Domain Shared / Auditoría | 100 % (41/41 y 36/36) |
| k6 explícito | 1 prueba correcta; umbrales cumplidos |
| Compose de semilla | Configuración combinada válida con variables sintéticas y `--no-env-resolution` |

La ejecución global detectó cuatro proyectos vacíos de módulos futuros (Ingesta, Anomalías, Mapa y Dashboard), que MTP reporta como salida no exitosa por ausencia de pruebas. No se modificaron ni ocultaron esos proyectos. Los cinco proyectos con pruebas se validaron; la captura Serilog inicialmente falló en su configuración de prueba, se corrigió y pasó. La integración final se ejecutó por separado: salida 0, sin fallos. Total de las cinco suites: **505 pruebas correctas**, más la prueba explícita de carga.

La secuencia inicial del dominio tuvo 22 pruebas fallidas antes de implementarse y después pasó. Se añadieron pruebas durante la revisión; no se afirma que toda la implementación siguiera una secuencia test-first.

Carga local k6: **300 cuentas distintas, 5 usuarios virtuales concurrentes, 600 peticiones comprobadas, 0 fallos**. Login p95 **307,10 ms** (<500); renovación p95 **36,37 ms** (<50). Evidencia: `backend/TestResults/identidad-k6.json`; reproducción con `RendimientoIdentidadTests` explícito y `tests/carga/identidad.k6.js`. No mide 300 sesiones simultáneas ni hardware Oracle.

Las pruebas se agrupan en archivos por responsabilidad: `CuentaApiTests`, `SesionApiTests`, `UsuariosApiTests`, `DesactivacionRestauracionTests`, `PersistenciaTests`, `PurgaIdentidadTests`, `ConsultasYPrivacidadTests`, `HorasGuardadasTests`, `RespuestasComunesApiTests` y `ConsolaPrivacidadTests`. Sustituyen nombres tentativos del plan. La prohibición de emitir métricas de riesgo desde Identidad se verifica en arquitectura; 002 conserva esa responsabilidad.

## Mapa de evidencia de tareas

Rutas de pruebas relativas a `backend/tests/`; Id = `KillaUru.Identidad.Tests`; Int = `KillaUru.Integracion.Tests/Identidad`.

| Tareas | Evidencia principal |
|---|---|
| T001–T003, T014–T016, T056 | V0003, opciones y secretos; Int/PersistenciaTests |
| T004–T012 | Id/Domain: Correo, UsuarioBloqueo, PoliticaSesion, HorasGuardadas, SesionRenovacion, TokenCorreo, EntidadesTests |
| T013, T017–T022 | Puertos y composición; Id/Application/SeguridadTests; Int/SesionApiTests |
| T023–T027 | Id/Application/RegistroTests; Int/CuentaApiTests; contratos de Legal y registro |
| T028–T036 | Id/Domain/UsuarioBloqueoTests y SesionRenovacionTests; Int/SesionApiTests, UsuariosApiTests, RespuestasComunesApiTests, DesactivacionRestauracionTests |
| T037–T042 | Int/CuentaApiTests (incluido canje concurrente); Id/Application/SeguridadTests y AdaptadoresExternosTests; runbook de correo |
| T043–T046 | Int/CuentaApiTests y ConsultasYPrivacidadTests; casos de uso Titular y log estructurado CuentaEliminada tras commit |
| T047–T050 | Id/Application/GestionUsuariosTests y RevisionTests; Int/UsuariosApiTests y RespuestasComunesApiTests |
| T051 | Id/Application/ConsolaYProgramaTests; Int/ConsolaPrivacidadTests con sink JSON; runbook y override Compose |
| T052 | Int/ConsultasYPrivacidadTests: cuenta distinta, exclusión de administradores y ventana temporal |
| T053–T057 | README; Contratos/ContratoIdentidadTests y ContratoOpenApiTests; k6; matriz de trazabilidad |
| T058–T062 | Int/PurgaIdentidadTests, HorasGuardadasTests, DesactivacionRestauracionTests; Id/Application/PurgaTareaTests y ConsolaYProgramaTests; SQL/runbook de restauración |
| T063–T065 | Arquitectura/ReglasIdentidadTests; calendario de simulacros; Int/RespuestasComunesApiTests y pruebas de contrato |

## Continuidad de 001 y 002

El host valida vigencia JWT con TimeProvider y sin tolerancia, exige `sub` UUID y rol conocido, y permite renovar con la cookie de acceso vencida en rutas anónimas de autenticación. Las consultas EF dejan de registrarse a nivel Information. OpenAPI conserva tipos y errores comunes de 001.

No se cierran pendientes de plataforma: privacidad integral de telemetría (R01), distribución de migraciones/scripts del mismo SHA (R02), ventana de tasa (R07), backups, observabilidad y simulacros externos siguen según revisión de 001. **Resolver R02 antes de desplegar V0003.** De 002 se verifica consumo transaccional de auditoría y ausencia de duplicación de métricas en 003; el consumo futuro desde 004 y sus pendientes operativos permanecen abiertos.

## Checklist operativo separado

- [ ] Configurar dominio/remitente reales, SPF/DKIM/DMARC y comprobar entrega de los tres correos. Resend, reintentos e idempotencia probados con HTTP simulado; instrucciones en `deploy/runbooks/correo.md`.
- [ ] Configurar widget/secretos de Turnstile y comprobar panel desplegado. Adaptador probado con respuestas simuladas.
- [ ] Aplicar migraciones del mismo SHA que la imagen y ejecutar semilla en consola privada con `docker-compose.semilla.yml`.
- [ ] Repetir carga en entorno objetivo; comprobar purga, telemetría y revisión mensual en operación.
- [ ] Ejecutar simulacro real de restauración y rotación de firma. Revocación SQL y rechazo HTTP probados localmente.

Los checks de tareas/trazabilidad significan implementación y comprobación local o documentación preparada, no ejecución de este checklist operativo. No se declara producción validada.
