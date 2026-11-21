# Guía de mantenimiento de KillaUru

Leer la constitución en `.specify/memory/constitution.md` y la spec afectada antes de cambiar comportamiento.
La documentación de dominio en `referencia/docs/` y las decisiones vigentes gobiernan las reglas.

## Calidad permanente

- Mantener SOLID, Clean Architecture y nombres de negocio en español.
- Menos de 200 líneas por archivo fuente; métodos hasta 20; acciones de controlador hasta 15.
- Respetar las excepciones de la constitución. No comprimir código ni usar partial para eludir límites.
- Un archivo por clase/responsabilidad. Introducir patrones solo con un problema concreto que resolver.
- Documentar contratos públicos y decisiones difíciles; explicar propósito, unidades y efectos.
- Mantener reglas puras, reloj inyectado, parámetros configurables e I/O cancelable.
- Para dominio, observar primero una prueba fallida antes de implementar la regla.

## Pruebas y seguimiento

- Entrada común: `docs/guias/pruebas.md`; cada spec tiene `pruebas.md`.
- Mantener suites en su tecnología; integración del backend por spec y propósito.
- No duplicar casos entre seguridad e integración; enlazar un único propietario.
- Conservar Arrange / Act / Assert y nombres que expliquen escenario y resultado.
- Actualizar rutas de documentación y CI al mover tests; comprobar descubrimiento y ejecución.
- Nunca sustituir aserciones de negocio por existencia de page/browser u otros objetos.
- Registrar skip/fixme como pendientes; diferenciar simulación de integración real.
- Marcar tareas completadas solo con evidencia adecuada; no deducir CI verde por cantidad de archivos.
- Al cerrar o reabrir tareas, conciliar `tasks.md`, `checklists/trazabilidad.md`, `cierre.md` y `docs/seguimiento/estado-implementacion.md` en el mismo cambio.
- Seguir `docs/guias/cierre-y-despliegue.md`; distinguir validación local, entorno de pruebas y aceptación de producción. Conservar informes históricos con su fecha.

## Seguridad

- Revisar autorización en API, validación, consultas parametrizadas, secretos y privacidad.
- No concatenar entradas en SQL ni introducir identificadores dinámicos sin lista permitida.
- Mantener cuotas, límites de entrada y controles de confianza del proxy.
- Distinguir protección de aplicación de protección DDoS del borde.
- No declarar seguridad certificada, cobertura o despliegue validado sin evidencia.
- Carga y caos solo en el entorno de pruebas designado, con límites y datos sintéticos.

La guía `docs/guias/calidad-y-seguridad.md` explica patrones, herramientas y criterios para revisores.

