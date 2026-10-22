# Semillas

Datos **sintéticos** para desarrollo y para el entorno de pruebas (R-11). Nunca datos reales de conductores.

Cada feature agrega aquí sus semillas cuando las necesita:

| Archivo | Feature | Para qué |
|---|---|---|
| `lotes/` | 005 | lotes de ejemplo sobre la polilínea |
| `mapa-roles.sql` | 006 | anomalías dentro, en el borde y fuera del tramo demo |
| `dashboard-60-dias.sql` | 007 | indicadores de 60 días con huecos |

La semilla del primer administrador **no** va aquí: la crea un comando de Identidad (003) y su enlace se imprime solo en la consola (T-SEG14).
