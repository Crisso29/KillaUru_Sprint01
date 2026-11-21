# Calidad y seguridad para contribuir a KillaUru

> Guía de trabajo. [Volver al índice de documentación](../README.md).

Esta guía aplica las reglas de la [constitución](../../.specify/memory/constitution.md).
Es un criterio de revisión y una guía de navegación; no certifica una auditoría de seguridad.

## Diseño que un compañero pueda mantener

| Principio | Aplicación concreta |
|---|---|
| SRP: una responsabilidad | Un archivo por clase. Un caso de uso realiza una operación de negocio |
| OCP: extender sin alterar reglas ajenas | Incorporar una política solo cuando exista una variación real |
| LSP: sustituciones compatibles | Un adaptador y su doble respetan el mismo contrato, errores y cancelación |
| ISP: interfaces pequeñas | Cada consumidor depende de los métodos que necesita; sin repositorio genérico |
| DIP: depender de contratos | Application define puertos; Infrastructure los implementa; Domain no depende de BD/HTTP |

SOLID no exige una interfaz para cada clase. La constitución exige dos usos reales o una métrica
antes de introducir una abstracción. Evitar fábricas, estrategias y capas que no resuelvan un problema.

## Patrones y frameworks del proyecto

| Pieza | Para qué se utiliza | Qué revisar |
|---|---|---|
| Monolito modular y Clean Architecture | Separar reglas, casos de uso, HTTP y persistencia | Dependencias entre módulos solo por Contracts |
| Puertos y adaptadores, inyección de dependencias | Sustituir reloj, BD y servicios externos sin alterar reglas | Vida scoped/singleton y contratos pequeños |
| Políticas de dominio y Result | Expresar reglas y rechazos esperados | Umbrales configurables; sin excepciones como flujo normal |
| Transacción e idempotencia | Confirmar efectos juntos y tolerar reintentos | Commit, auditoría atómica y ausencia de duplicados |
| ASP.NET Core, EF Core y Npgsql | HTTP y persistencia PostgreSQL/PostGIS | Autorización, consultas parametrizadas y cancelación |
| xUnit, NSubstitute y Testcontainers | Reglas aisladas y colaboración con PostgreSQL real | Dobles explícitos y datos de prueba independientes |
| NetArchTest y Roslyn | Revisar dependencias y estructura del código | No silenciar fallos para cerrar tareas |
| Kotlin, Compose, Room y WorkManager | Interfaz Android, almacenamiento y sincronización | Dominio independiente de Android; migraciones preservan datos |
| React, Vitest, MSW y Playwright | UI web, componentes y flujos de navegador | Identificar si HTTP está simulado o es real |
| pytest y pipeline ML | Señal, entrenamiento, conversión y validación | Reproducibilidad y separación de datos de evaluación |

Consulta las versiones fijadas en los archivos de dependencias del componente.
Esta reorganización no incorpora frameworks nuevos.

## Archivos, métodos y comentarios

- Archivos de código: **menos de 200 líneas**; métodos: **hasta 20**; acciones HTTP: **hasta 15**.
- Mantener las excepciones expresas de la constitución para migraciones, generados y OpenAPI.
- Si crece una clase, extraer una responsabilidad con nombre de negocio. No partirla con `partial`.
- Documentar contratos públicos, DTOs y controladores: propósito, entradas, salidas, unidades y errores.
- Explicar decisiones difíciles: orden de middleware, transacciones, concurrencia y fronteras de confianza.
- No añadir comentarios decorativos ni inventar requisitos. Citar spec/FR/RN/ADR cuando corresponda.
- Los tests también deben ser legibles. No comprimir líneas ni ocultar aserciones para cumplir el límite.

## Seguridad: control, prueba y límite de la evidencia

| Riesgo | Control que debe revisarse | Evidencia o pendiente |
|---|---|---|
| Inyección SQL | Parámetros EF/Npgsql; nunca concatenar entrada en SQL; identificadores dinámicos con lista permitida | Revisar SQL crudo y probar entradas hostiles contra consultas reales; no certificado por esta reorganización |
| Acceso indebido | Roles y acceso al recurso en la API; denegación por defecto | Suites de seguridad 001 y 004; revisar también operaciones por recurso |
| Tokens y sesiones | Firma, expiración, renovación, revocación, cookies y cierre de sesión | Integración 001/003; E2E de sesión del panel todavía pendientes |
| Abuso y disponibilidad | Cuotas, tamaño de entrada, límites de trabajo y cancelación | Pruebas de cuotas/IP 001 y escenarios k6; ejecución de carga pendiente de evidencia |
| DDoS | Controles de borde, aislamiento del origen, capacidad y operación | No lo garantiza un 429 local ni una prueba k6; falta validación del despliegue |
| XSS/CSRF | Tratamiento seguro de contenido, cookies y verificación del origen según el flujo | Revisar cabeceras y operaciones autenticadas; sin auditoría completa en este cambio |
| Filtración de datos | Redacción de logs, caché privada, purga, exportaciones y almacenamiento local | Suites de privacidad; E2E de almacenamiento del panel pendiente |
| Dependencias vulnerables | Revisar dependencias directas/transitivas y aplicar actualizaciones verificadas | Registrar herramienta, fecha y resultado; no asumir que una versión fijada es segura |
| Secretos | Configuración externa al código, privilegio mínimo y rotación | No usar credenciales de producción en fixtures, informes ni logs |
| Pérdida o duplicación | Migraciones, backups restaurables e idempotencia | Integración y simulacros; una copia de respaldo sin restauración probada no basta |

Una prueba de seguridad debe comprobar el rechazo, ausencia de efectos indebidos y funcionamiento
del caso legítimo. Un código 400 aislado no demuestra que la persistencia sea segura.
No usar búsquedas de texto como única prueba de ausencia de inyección SQL.

Ejecutar carga y caos únicamente en el entorno de pruebas designado, con datos sintéticos,
límites y criterios de parada. El [runbook de carga](../../backend/tests/carga/README.md) describe los escenarios.

## Antes de cerrar un cambio

1. Ejecutar las suites afectadas y sus controles de arquitectura/lint.
2. Comprobar contratos, permisos, entradas inválidas y efectos transaccionales del cambio.
3. Revisar archivos y métodos, comentarios útiles y dependencias.
4. Registrar comando, commit, entorno y resultados, incluidos omitidos.
5. Actualizar la guía de pruebas de la spec y su seguimiento con evidencia.

