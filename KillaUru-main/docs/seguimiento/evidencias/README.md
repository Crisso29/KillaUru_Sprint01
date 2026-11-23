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
| 2026-11-23 | ML (spec 009): pruebas, cobertura y auditoría de tareas | [ML](pruebas/2026-11-23-ml/README.md) | Python 3.11, pytest | 174 aprobadas; cobertura 97 %; 0 funciones > 20 líneas, 0 pruebas sin AAA, 0 nombres de una letra; pipeline alineado con su contrato (características p1, dos salidas, puerta de 7 condiciones, manifiesto válido); 12 tareas corregidas, T036 reabierta |
| 2026-11-23 | Backend, parte 3: DT-03, DT-04 y la prueba intermitente de 50 eventos | [Parte 3](pruebas/2026-11-23-backend-parte3/README.md) | .NET 10, Docker, Testcontainers | 1 155 pruebas, 0 fallidas; chequeo de PostgreSQL por TCP con prueba que revisa todo compose; sink de logs seguro entre hilos (rojo 3/3 → verde 5/5); tope del lote de 50 basado en mediciones |
| 2026-11-23 | Colección de Postman de la API | [Postman](pruebas/2026-11-23-postman/README.md) | Newman 6.2.1, Docker | 62 peticiones y 281 comprobaciones sin fallas; las 32 operaciones de los contratos; encontró y se corrigieron 3 defectos de la API (mapa con JSON inválido, errores sin problem+json, reintentos de correo) |
| 2026-11-23 | Mapa: recorte por rol y filtros (006 T021 y T024) | [Mapa API](pruebas/2026-11-23-mapa-api/README.md) | .NET 10, Testcontainers | 9 pruebas de integración nuevas; 4 mutaciones, 3 detectadas y 1 equivalente justificada; 264 de integración sin fallas |

Pendiente: repetir en `killauru-pruebas` (VM) cuando esté desplegado, con k6 fuera de la VM.
