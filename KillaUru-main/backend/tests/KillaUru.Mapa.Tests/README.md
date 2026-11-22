# Pruebas de Consulta de mapa

Esta suite pertenece a la spec 006. Consulta [su guía](../../../specs/006-modulo-consulta-mapa/pruebas.md)
para requisitos, integración, seguridad, carga y comandos.

Las carpetas Domain, Application y Adapters reflejan las responsabilidades bajo prueba.
Adapters prueba la lectura de parámetros HTTP (listas separadas por comas) sin levantar el host.
Las pruebas de adaptadores con dobles no equivalen a ejecutar contra PostgreSQL real.
Sigue [las convenciones comunes](../../../docs/guias/pruebas.md) al añadir casos.

