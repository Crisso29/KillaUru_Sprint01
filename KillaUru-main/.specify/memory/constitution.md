<!--
Sync Impact Report
- Versión: 1.0.0 → 1.1.0 (2026-10-21, MENOR: se amplían reglas existentes, ningún principio se quita ni se redefine)
- Principios modificados: II (regla Adapters → solo Application; Domain sin Contracts de otro módulo; pruebas de arquitectura con la tabla de capas completa),
  VI (RegistrarAsync exige la transacción del que actúa; RegistrarLecturaAsync solo para lecturas sensibles),
  VII (CI en cada PR con cobertura coverlet ≥ 90 % del dominio; carga, picos, estrés, resistencia y caos con k6 fuera de la VM),
  VIII (la prueba de 100 líneas cubre .cs, .sql de consultas, .sh y .js, excepto migraciones V*.sql, generados y OpenAPI; BannedApiAnalyzers),
  X (Grafana Alloy como agente que solo envía)
- Restricciones: Despliegue (CI/CD con GitHub Actions + GHCR y despliegue automático al unir a main); Entorno de pruebas (admite carga y estrés con límites de CPU/RAM)
- Origen: specs/revision/decisiones.md D4, D10, D11, D12, D15 y plan-de-cambios.md T-QA16, T-QA17; docs 02–04 revisados el 2026-10-21
- Plantillas: sin cambios
- Pendientes: ninguno
-- Historial --
- Versión: (plantilla) → 1.0.0 (primera ratificación)
- Principios añadidos: I a X (todos nuevos)
- Secciones añadidas: Restricciones de tecnología y operación · Flujo de trabajo SDD · Gobierno
- Plantillas revisadas: plan-template.md ✅ (su «Constitution Check» se llena con la tabla de la sección «Puertas»)
                        spec-template.md ✅ · tasks-template.md ✅ · checklist-template.md ✅ (sin cambios)
- Pendientes: ninguno
-->

# Constitución de KillaUru

> **Fuente de verdad del dominio y de la arquitectura:** `referencia/docs/` (copia de `docs/` del repositorio del curso, revisión 2026-10-21).
> Esta constitución **no** inventa reglas: convierte en reglas verificables lo que ya decidieron los ADR-001…ADR-015, las reglas RN-01…RN-22, los requisitos RF/RNF y los principios de `03-diseño-de-software`. Suma dos reglas pedidas por el autor: archivos de código cortos (principio VIII) y API documentada (principio IX).

## Principios fundamentales

### I. `referencia/docs/` manda; cada artefacto es trazable
- Ninguna spec, plan, tarea ni línea de código puede contradecir `referencia/docs/`. Si aparece una contradicción, **se detiene el trabajo**, se corrige primero el documento de `docs/` (y su copia en `referencia/`) y solo después la spec.
- Cada requisito funcional (FR) de una spec cita su origen (`HU-xx`, `RF-xx`, `RN-xx`, `RNF-xx`, `ADR-xxx`). Cada tarea cita el FR o la regla que implementa. Lo que **no** viene de docs (por ejemplo, algo tomado del prototipo) se marca como **«Derivado del prototipo»** o **«Decisión de diseño SDD»** y se justifica en `research.md`.
- Los valores numéricos (umbrales, plazos, metas) **no se copian** en el código ni se reinventan: se toman de RN (`09-reglas-de-negocio.md`) y RNF (`04-atributos-de-calidad.md`) y viven en configuración.

### II. Monolito modular con Clean Architecture (ADR-001, ADR-002, ADR-012)
- Un solo proceso ASP.NET Core 10 con **6 módulos**: Identidad y Accesos, Ingesta, Anomalías, Consulta de Mapa, Dashboard y Auditoría. No se agregan módulos, servicios ni contenedores sin un ADR nuevo.
- **Un proyecto (`.csproj`) por módulo.** Todo es `internal` salvo `Contracts/` y el registro `<Modulo>Module.cs`. Un módulo usa a otro **solo** por su `Contracts/`.
- Cuatro capas por módulo: Domain ← Application ← Adapters; Infrastructure implementa los puertos de Application y conoce Domain. Los puertos viven en Application. **Adapters solo referencia Application**: los tipos de entrada y salida de los casos de uso viven en Application y los controladores no usan ni exponen entidades. **Domain no usa `Contracts/` de otro módulo.**
- **Un esquema de PostgreSQL y un `DbContext` por módulo**, sin llaves foráneas entre esquemas; solo el dueño escribe. Consulta de Mapa no tiene tablas.
- Grafo sin ciclos: Ingesta → Anomalías → Auditoría; Mapa y Dashboard → Anomalías; Dashboard → Identidad; Identidad → Auditoría.
- Se verifica con el compilador (visibilidad) y con **NetArchTest** en cada build (RNF-28), con la tabla de capas de `enfoque-arquitectonico.md` §3 regla por regla.

### III. Reglas de negocio en el dominio, calibrables sin recompilar (ADR-012, S-04)
- Toda regla de `09-reglas-de-negocio.md` vive en una clase de dominio con nombre de negocio (`ReglasCalidad`, `PoliticaConfirmacion`, `PoliticaPlausibilidad`, `ReglaPersistencia`, agregado `Anomalia`, política de bloqueo).
- Ningún umbral es una constante: va en `<Modulo>Options` (una clase por módulo, solo su sección de `appsettings.json`).
- Ningún código usa `DateTime.UtcNow` ni `now()` de SQL para reglas: el reloj es `TimeProvider`. Todas las ventanas usan la **hora de detección**; el día local es UTC−5 fijo.

### IV. Ningún dato aceptado se pierde ni se duplica (ADR-003, ADR-005, ADR-015)
- El HTTP 200 de un lote se responde **solo después del commit** en `ingesta.evento_recibido`.
- Idempotencia de extremo a extremo por `eventoId` (`evento_recibido` y `reporte_anomalia`). Entrega al menos una vez + receptor idempotente.
- El servidor juzga cada evento **por su contenido** (ubicación y hora de detección), nunca por el momento en que llega. Los descartes son métricas, no auditoría.

### V. Privacidad por diseño (R-06, ADR-007, Ley 29733, D.S. 016-2024-JUS)
- Ningún dato almacenado vincula una cuenta con eventos o trayectorias (RNF-10). `con_sesion` es solo sí/no.
- El seudónimo vive en una **columna** (nunca en el `JSONB`) y se pone en `NULL` a las 72 h de recibido, en **todas** las tablas (RN-09).
- La exportación nunca lleva seudónimos. Las métricas nunca usan seudónimo, IP ni `eventoId` como etiqueta (RNF-32). La auditoría guarda el id interno, nunca el correo de quien eliminó su cuenta.

### VI. El servidor decide el acceso y deja rastro (ADR-009, ADR-013, R-07)
- RBAC y recorte por rol (RN-08) se aplican **en la API**; la app y el panel no son frontera de seguridad.
- Tokens de acceso de 5 min para todos los roles; tokens de actualización revocables y rotativos (8 h admin, 30 días conductor, 60 s de gracia).
- Toda acción sensible (RF-15) y su registro de auditoría se confirman **en la misma transacción** (RNF-31): `IAuditoriaService.RegistrarAsync` **exige** la transacción del que actúa unida en `IContextoTransaccion` (si falta, lanza `InvalidOperationException`). `RegistrarLecturaAsync` (transacción propia) se usa solo para lecturas sensibles; las audita el módulo dueño del dato.
- Solo las respuestas anónimas pueden ir a la caché de borde; las demás son `private, no-store`.

### VII. Pruebas primero en las reglas (NO NEGOCIABLE para el dominio)
- Cada regla de `09` se escribe primero como prueba con **valores límite** (por ejemplo 0.74/0.75/0.76; 19/20/21 m; 2:59/3:00 min) y debe fallar antes de implementarse. Cobertura del dominio ≥ 90 %.
- Pirámide: Domain (xUnit puro) → Application (NSubstitute + `FakeTimeProvider`) → Infrastructure (Testcontainers PostgreSQL + PostGIS) → Adapters (`WebApplicationFactory`) → Arquitectura (NetArchTest) → Sistema (k6: carga, picos, estrés, resistencia y caos; campo).
- **CI en cada PR (GitHub Actions):** build con advertencias como errores, unitarias, arquitectura, integración, contrato OpenAPI y cobertura **coverlet ≥ 90 %** en los proyectos `Domain`; sin CI en verde no se une a `main`. Carga y estrés corren fuera del CI y fuera de la VM, contra el entorno de pruebas.

### VIII. Código pequeño y legible (pedido del autor + `principios-de-diseño.md`)
- **Ningún archivo de código fuente supera 100 líneas** (C#, SQL de consultas, scripts). Si crece, se divide por responsabilidad (otra clase, otra política o un `partial` con nombre de negocio). Excepciones: migraciones SQL versionadas (`V*.sql`), archivos generados y contratos OpenAPI. Se verifica con una prueba en `KillaUru.Arquitectura.Tests` que recorre `.cs`, `.sql` de consultas, `.sh` y `.js`.
- `DateTime.Now/UtcNow` y `DateTimeOffset.Now/UtcNow` están prohibidos en el build (`Microsoft.CodeAnalysis.BannedApiAnalyzers`).
- Métodos ≤ 20 líneas; controladores ≤ 15 líneas por acción; un caso de uso = una clase = un archivo con un único `EjecutarAsync`.
- Sin excepciones para el flujo de negocio (se usa *Result*); todo I/O es `async` con `CancellationToken`; `Nullable` activo y advertencias como errores; sin *Mediator* ni mapeo automático; sin repositorio genérico.
- Nombres de negocio en español (glosario `10-glosario.md`); términos técnicos de capa en inglés.

### IX. API REST documentada y dirigida por contrato
- Cada endpoint nace en `specs/<feature>/contracts/*.openapi.yaml` (OpenAPI 3.1) **antes** de programarse, y el código debe coincidir con él (prueba de contrato en `KillaUru.Integracion.Tests`).
- La API genera su documento con `Microsoft.AspNetCore.OpenApi` (`/openapi/v1.json`) y lo muestra con **Scalar** en `/docs`. Cada endpoint declara resumen, descripción, códigos de respuesta, ejemplos y el rol que lo puede usar. Comentarios XML obligatorios en controladores y DTOs públicos.
- Rutas versionadas `/api/v1`; errores con `ProblemDetails` (RFC 9457) y un `codigo` estable en español.
- Cada módulo tiene su `README.md` con propósito, endpoints y ejemplos (RNF-19).

### X. Costo cero y simplicidad (DA-06, DA-08, RNF-16, RNF-32, ADR-014)
- Sin Redis, sin broker, sin base NoSQL, sin servidores extra en v1.0: PostgreSQL + PostGIS, `Channel` en memoria, HybridCache sobre `IMemoryCache`, R2, Grafana Cloud gratis sin tarjeta (con el agente Grafana Alloy en la VM, que solo envía), UptimeRobot gratis.
- KISS y YAGNI: una abstracción entra con dos usos reales o con una métrica que la exige.

## Restricciones de tecnología y operación

| Tema | Decisión fija | Fuente |
|---|---|---|
| Backend | ASP.NET Core sobre **.NET 10 LTS**, EF Core + NetTopologySuite + Npgsql, Serilog, OpenTelemetry | R-02, ADR-001, ADR-012 |
| Base de datos | PostgreSQL + PostGIS (imagen arm64), `geography(Point, 4326)`, índices GiST, bloqueos consultivos por celda | R-03, RS-02 |
| Infraestructura | Oracle Cloud ARM Always Free (Santiago) · Docker Compose (`api`, `postgres`, `cloudflared`, `backup`, `alloy`) · Cloudflare Tunnel, Pages y R2 · `killauru.pe` | R-04, R-05, ADR-010 |
| Despliegue | CI/CD con **GitHub Actions**: al unir a `main`, imagen arm64 en GHCR con etiqueta = commit y **despliegue automático** a la VM por SSH a través de Cloudflare Access (respaldo → migraciones → `docker compose up -d`); migraciones como paso explícito, precedidas de `pg_dump`, en dos fases; rollback = desplegar la etiqueta anterior (< 5 min) | RNF-17, RNF-18, ADR-010 |
| Entorno de pruebas | Segundo `docker compose` en la misma VM, con datos sintéticos, para simulacros y pruebas de carga y estrés (k6 desde fuera de la VM), con límites de CPU y memoria, límites de tasa relajados y en horario de poco tráfico | R-11 |
| Tiempo y equipo | Una persona, 6–8 semanas: se respeta el orden de recorte MoSCoW de `02-historias-de-usuario.md` | R-09, R-10 |
| Interfaz de referencia | El prototipo `referencia/prototipo/killauru-diseno.html` guía qué datos necesita cada pantalla de la app y del panel; **no** cambia reglas | — |

## Flujo de trabajo SDD

1. Orden de features del backend: el de `specs/README.md`. No se empieza una feature si la anterior no pasó su *checkpoint*.
2. Por cada feature: `spec.md` → (`/speckit-clarify` si hay dudas) → `plan.md` + `research.md` + `data-model.md` + `contracts/` + `quickstart.md` → `tasks.md` → `/speckit-analyze` → `/speckit-implement`.
3. **Puertas** (sección «Constitution Check» de cada `plan.md`): cada principio I–X se marca ✅ o se justifica en «Complexity Tracking».
4. Definición de Hecho (de `02-historias-de-usuario.md`): código con pruebas, RF/RNF trazados, criterio de aceptación verificado, OpenAPI y README del módulo actualizados, C4 actualizado si cambia un componente.

## Gobierno

- Esta constitución está por encima de cualquier otra práctica del repositorio de código. Si una spec la contradice, gana la constitución; si la constitución contradice `referencia/docs/`, gana `docs/` y se enmienda la constitución.
- Enmiendas: se documentan en el *Sync Impact Report* de este archivo con versión semántica (MAYOR: se quita o redefine un principio; MENOR: se agrega uno; PARCHE: redacción).
- Toda revisión de código verifica los principios II, III, V, VI y VIII de forma explícita.

**Version**: 1.1.0 | **Ratified**: 2026-10-20 | **Last Amended**: 2026-10-21
