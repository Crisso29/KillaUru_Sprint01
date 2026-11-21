# Código y validación de KillaUru

Esta carpeta contiene la implementación. La [guía de documentación](docs/README.md)
explica dónde encontrar cada tema. Para revisar las pruebas, empieza por
[el índice de pruebas](docs/guias/pruebas.md); para conocer pendientes, abre
[el estado de implementación](docs/seguimiento/estado-implementacion.md).

## Dónde está cada responsabilidad

| Ubicación | Responsabilidad y fuente vigente |
|---|---|
| [backend](backend/) | API .NET, seis módulos, persistencia y tests xUnit |
| [app](app/) | Aplicación Kotlin, tests JVM y preparación de pruebas en dispositivo |
| [panel](panel/) | React, Vitest y recorridos Playwright |
| [ml](ml/) | Datos, modelo y pruebas pytest |
| [specs](specs/README.md) | Requisitos SDD, contratos, tareas y cierre por funcionalidad |
| [docs](docs/README.md) | Guías, seguimiento actual e historial de implementación |
| [referencia/docs](referencia/docs/) | Copia normativa de las decisiones de dominio y arquitectura del Sprint 1 |
| [../docs](../docs/) | Documentación académica original; cambios normativos deben sincronizar su copia |
| [backend/deploy](backend/deploy/) | Docker Compose, despliegue del backend, respaldos y runbooks operativos |
| [deploy/runbooks](deploy/runbooks/) | Simulacros transversales que coordinan varios componentes |
| [infra](infra/) | Configuración compartida de servicios externos, actualmente CORS de R2 |
| [../.github/workflows](../.github/workflows/) | Automatización CI/CD del repositorio completo |

Las carpetas de documentación no son alternativas intercambiables: el dominio se
define en la referencia normativa y el estado ejecutado se registra en las guías
de implementación. Un plan o una tarea marcada no sustituye un reporte de ejecución.

## Cómo defender la organización de pruebas

Cada tecnología conserva su ejecutor y estructura habitual. La spec enlaza sus
pruebas; no se duplica código para tener una copia bajo cada categoría.
Una prueba de seguridad puede usar integración: su ubicación expresa el propósito
principal y su documentación explica las dependencias reales.

Para cada caso, identificar requisito, datos, acción, resultado esperado y entorno.
Los helpers preparan escenarios; las aserciones ejecutables pertenecen a suites.
Los escenarios omitidos y los servicios simulados se declaran expresamente.
