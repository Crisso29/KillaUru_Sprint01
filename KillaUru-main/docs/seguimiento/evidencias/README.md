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
| 2026-11-22 | Funcionalidad y usabilidad de la app (FR-001, FR-006, FR-008) | [App: primer uso y navegación en emulador](pruebas/2026-11-22-app-navegacion/README.md) | Emulador Android 15 | Primer uso completo hasta el mapa; permisos reales en dos pasos; pestañas en 1 toque; tema de KillaUru en claro y oscuro; 4 de 6 diferencias corregidas |
| 2026-11-22 | Interfaz y experiencia de usuario del panel (prototipo) | [Panel: fidelidad al prototipo](pruebas/2026-11-22-panel-prototipo/README.md) | Local, API simulada (MSW) | 7 vistas junto al prototipo; Mapa funcional por primera vez; identidad alineada al contrato; 416 Vitest |
| 2026-11-22 | Interfaz y experiencia de usuario de la app (prototipo) | [App: fidelidad al prototipo](pruebas/2026-11-22-app-prototipo/README.md) | Emulador Android 15 | Marca «Señal», primer uso, Ajustes y Mis viajes como el prototipo; textos legales 1.2 iguales al backend; 302 pruebas JVM |
| 2026-11-22 | Funcionalidad, UX y seguridad de la cuenta en la app (FR-060…FR-069) | [App: pantallas de cuenta](pruebas/2026-11-22-app-cuenta/README.md) | Emulador Android 15 con la API local real | 7 pantallas de cuenta como el prototipo; registro 201, perfil 200, cerrar sesión 204, login 401/200, eliminar 403/204 contra la API; teclado que no tapa el botón; 353 pruebas JVM, lint y detekt sin hallazgos |
| 2026-11-23 | CDN del mapa: datos, publicación y mapa sin señal (FR-041…FR-044, FR-051, RN-08) | [CDN del mapa](pruebas/2026-11-23-cdn/README.md) | Emulador Android 15 + CDN local (nginx) | Datos reales según el contrato (PMTiles 2,3 MB, tramo demo de 115 vértices); paso 4 y mapa sin señal como el prototipo; actualización v1 → v2; la CDN ya no recibe credenciales; 391 pruebas JVM |
| 2026-11-23 | Calidad del código y de las pruebas (app y herramientas de la CDN) | [Calidad](pruebas/2026-11-23-calidad/README.md) | JVM, Python, .NET | 406 pruebas JVM sin fallas y todas con AAA; dominio 97,2 % de líneas; lint y detekt sin hallazgos; TDD contado prueba por prueba |

Pendiente: repetir en `killauru-pruebas` (VM) cuando esté desplegado, con k6 fuera de la VM.
