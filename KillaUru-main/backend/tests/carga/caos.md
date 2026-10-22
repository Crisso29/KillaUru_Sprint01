# Prueba de caos (T077, EQ-09)

Comprueba que un reinicio brusco del servidor no pierde ni duplica eventos ya aceptados (RNF-06).

1. En la VM: `docker compose -f deploy/docker-compose.pruebas.yml up -d` con la versión a probar.
2. Fuera de la VM: `KU_URL_PRUEBAS=http://<vm>:18080 k6 run tests/carga/carga-combinada.k6.js`.
3. En la VM, mientras corre k6: `tests/carga/caos.sh`.
4. Anotar el resultado en `deploy/runbooks/simulacros.md`.

> Requiere las features 004, 005 y 006 (tablas `ingesta.evento_recibido` y `anomalias.reporte_anomalia`, y los endpoints del lote y del mapa).
