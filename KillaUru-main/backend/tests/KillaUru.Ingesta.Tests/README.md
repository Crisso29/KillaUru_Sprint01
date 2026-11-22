# Pruebas de Ingesta

Esta suite pertenece a la spec 005. Consulta [su guía](../../../specs/005-modulo-ingesta/pruebas.md)
para requisitos, integración, seguridad, carga y comandos.

Las carpetas Domain, Application, Adapters e Infrastructure reflejan las responsabilidades bajo prueba.
Adapters prueba `LectorCuerpoLote` (tope de bytes, 400/413) sin levantar el host.
Las pruebas de adaptadores con dobles no equivalen a ejecutar contra PostgreSQL real.
Sigue [las convenciones comunes](../../../docs/guias/pruebas.md) al añadir casos.

