# Evidencias de validación

> Resultados de ejecución que se conservan en el repositorio. [Volver al estado del proyecto](../estado-implementacion.md).

`backend/TestResults/` es la salida local que se regenera con cada `dotnet test` o k6 y está en `.gitignore`.
Se genera completa, con nombres legibles, con `pwsh backend/tests/generar-resultados.ps1`.
Aquí solo se copian las ejecuciones que sirven como evidencia, una carpeta por ejecución con su informe
(fecha, entorno, SHA, criterio y lectura) y sus archivos de datos.

| Fecha | Atributo | Ejecución | Entorno | Resultado |
|---|---|---|---|---|
| 2026-11-21 | Rendimiento y disponibilidad (RNF-01, RNF-02, RNF-03, DA-05) | [Carga progresiva hasta 2 000 usuarios](carga/2026-11-21-local-progresiva-2000/README.md) | Local, misma máquina | Metas de 300 ✅; 0 5xx; 503 de defensa desde 1 000; procesamiento por debajo de la llegada (riesgo RNF-23) |
| 2026-11-22 | Pruebas automatizadas y cobertura (constitución VII) | [Backend: 1 153 pruebas y cobertura](pruebas/2026-11-22-backend/README.md) | Local, PostgreSQL real | 1 150 aprobadas, 0 fallidas, 3 pendientes; dominio ≥ 97 %; backend 78,8 % de líneas |
| 2026-11-22 | Pruebas automatizadas y cobertura del panel | [Panel: Vitest, cobertura y Playwright](pruebas/2026-11-22-panel/README.md) | Local, API simulada (MSW) | 389 Vitest aprobadas; 75,1 % de líneas, `modelo/` ≥ 90 %; E2E: 1 de 13 archivos implementado (147 casos pendientes) |
| 2026-11-22 | Rendimiento (RNF-23) y conformidad con el diseño (§3.7) | [Trabajadores y pool de conexiones](carga/2026-11-22-local-trabajadores-y-pool/README.md) | Local, misma máquina | Más trabajadores empeoran; 5xx por pool sin tope corregidos (0 tras el cambio); quedan 3 pools |

Pendiente: repetir en `killauru-pruebas` (VM) cuando esté desplegado, con k6 fuera de la VM.
