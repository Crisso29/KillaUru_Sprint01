# Drivers Arquitectónicos

> **Proyecto:** KillaUru · **Versión:** 2.0 · **Revisión:** 2026-10-13
> Un driver arquitectónico es un requisito, atributo o restricción que **obliga** a tomar una decisión de arquitectura. Criterio de inclusión: *si este driver no existiera, la arquitectura sería diferente.*
> **Método:** *Attribute-Driven Design* (ADD 3.0, SEI). Cada driver se clasifica por **tipo** y se prioriza con dos notas: **importancia para el negocio** y **dificultad técnica**: **A** (alta), **M** (media) o **B** (baja).
> **Regla de trazabilidad:** la columna «ADR» de esta tabla coincide exactamente con el campo «Drivers» de cada ADR en `07-decisiones-arquitectonicas.md`.
> **Relacionados:** `04-atributos-de-calidad.md`, `05-restricciones.md`, `07-decisiones-arquitectonicas.md`, `08-escenarios-y-atributos-de-calidad.md`, `09-reglas-de-negocio.md`

## 1. Tipos de driver (ADD)

| Tipo | Qué es | Drivers |
|---|---|---|
| **Funcionalidad principal** | Lo que el sistema debe hacer y que, por su naturaleza, condiciona la estructura | DA-03, DA-13 |
| **Atributo de calidad** | Una meta medible de calidad con su escenario | DA-01, DA-02, DA-04, DA-05, DA-11, DA-14, DA-15, DA-17 |
| **Restricción** | Una condición que no se puede cambiar (equipo, plazo, costo, ley, negocio) | DA-06, DA-07, DA-08, DA-10, DA-12, DA-16 |
| **Preocupación arquitectónica** | Un tema de diseño que no viene de un requisito puntual, pero que el arquitecto debe resolver | DA-09, DA-18 |

## 2. Catálogo de drivers

| ID | Driver | Tipo | Prioridad (negocio, dificultad) | Origen | Escenario | ¿Por qué cambia la arquitectura? | ADR | Dónde se ve |
|---|---|---|:---:|---|---|---|---|---|
| DA-01 | La app debe operar sin internet al menos 5 h continuas y conservar 7 días de eventos pendientes. | Atributo de calidad | **(A, A)** | R-08, AC-01, RNF-33 | EQ-01 | Obliga a *Store-and-Forward* con base local en el celular y sincronización en segundo plano. Sin este driver bastaría un cliente-servidor normal. | ADR-003, ADR-014 | App: Buffer (Room/SQLite) y Sincronizador (WorkManager) |
| DA-02 | Ningún evento aceptado puede perderse ni duplicarse, ni siquiera por un reinicio del servidor. | Atributo de calidad | **(A, A)** | AC-02, RNF-06 | EQ-02, EQ-09 | Obliga a entrega *at-least-once*, **persistencia durable antes de responder 200** e **idempotencia por `eventoId` de extremo a extremo** (Ingesta y Anomalías). | ADR-003, ADR-005 | Ingesta: `evento_recibido`; Anomalías: `reporte_anomalia` único por `eventoId` |
| DA-03 | La detección debe ser automática y pasiva, sin acción del conductor. | Funcionalidad principal | **(A, M)** | RF-01, HU-01, AC-08 | EQ-03, EQ-08 | Obliga a un modelo de ML embebido en lugar de reportes manuales. | ADR-004 | App: Sensores y Detector |
| DA-04 | El modelo debe clasificar una ventana en < 50 ms en un celular de gama media. | Atributo de calidad | **(A, A)** | RNF-04, AC-03 | EQ-03 | Obliga a inferencia local (*Edge AI*, LiteRT cuantizado) en lugar de enviar la señal cruda al servidor. | ADR-004 | App: Detector |
| DA-05 | 300 dispositivos deben poder sincronizar y consultar el mapa al mismo tiempo sin degradarse. | Atributo de calidad | **(M, A)** | AC-06, RNF-01, RNF-02, RNF-03, RNF-23 | EQ-06, EQ-14 | Obliga a procesamiento asíncrono interno con contrapresión, bloqueo por celda geográfica y caché por celdas más caché de borde. | ADR-005, ADR-006 | Ingesta: cola y `BackgroundService`; Consulta de Mapa: caché |
| DA-06 | Una sola persona desarrolla y opera el sistema. | Restricción | **(A, B)** | R-09 | EQ-05 | Obliga a monolito modular, despliegue único, ningún servicio de datos extra y soluciones pragmáticas (auditoría en la misma transacción). | ADR-001, ADR-002, ADR-013, ADR-014 | Un proceso API, una BD, `docker compose up -d` |
| DA-07 | La identidad del conductor no puede almacenarse ni vincularse a sus eventos. | Restricción | **(A, M)** | R-06, AC-04, RNF-10, RNF-25 | EQ-04 | Obliga a seudónimo rotativo, purga a 72 h, historial solo local e indicadores solo agregados (RN-17). | ADR-007 | App: Seudónimo; Anomalías: purga; Dashboard |
| DA-08 | El costo operativo debe ser ≤ US$ 10 al mes (MVP autofinanciado). | Restricción | **(A, M)** | R-04, AC-09, RNF-16, RNF-32 | EQ-15 | Obliga a planes gratuitos (Oracle ARM, Cloudflare, Grafana Cloud), a evitar Redis y brokers, y a telemetría con muestreo. | ADR-006, ADR-010, ADR-014 | Despliegue (C2) |
| DA-09 | El sistema debe poder crecer con nuevos módulos (empresas, autoridades, notificaciones) sin reescribirse. | Preocupación arquitectónica | **(M, M)** | AC-05, actores futuros (`01-actores.md`) | EQ-05 | Obliga a *bounded contexts* con `Contracts/`, un esquema de BD por módulo y eventos de dominio (Observer) desde el inicio. | ADR-001, ADR-002, ADR-006, ADR-012 | C3: módulos, `Contracts/`, esquemas |
| DA-10 | Los datos no pueden exponerse en internet abierto más allá del tramo demo. | Restricción | **(A, M)** | R-07, AC-04, RNF-27 | EQ-11 | Obliga a **autorización por rol en el servidor** y a recortar el `bbox` de los anónimos. | ADR-009 | Consulta de Mapa; middleware de la API |
| DA-11 | Las fallas y degradaciones deben detectarse antes de que las reporten los usuarios. | Atributo de calidad | **(M, M)** | AC-07, RNF-20, RNF-21, RNF-22, RNF-32 | EQ-07 | Obliga a `/health`, OpenTelemetry, logs estructurados y alertas desde el inicio, dentro del plan gratuito. | ADR-010 | API REST; Grafana Cloud; UptimeRobot |
| DA-12 | El plazo es de 6–8 semanas con una sola persona. | Restricción | **(A, B)** | R-10, R-09 | — | Obliga a **recortar el alcance** (MoSCoW) y a no introducir componentes de infraestructura opcionales. | ADR-001, ADR-012 | `02-historias-de-usuario.md`: prioridades |
| DA-13 | El mapa debe reflejar la realidad de la vía en el tiempo (reparaciones, obstáculos temporales, falsos positivos). | Funcionalidad principal | **(A, M)** | HU-06, HU-09, HU-19, RN-07 | — | Obliga a un **ciclo de vida** de la anomalía con caducidad configurable y moderación. | ADR-008 | Anomalías: agregado `Anomalia` |
| DA-14 | El mapa y las alertas deben funcionar sin señal y sin gastar datos móviles. | Atributo de calidad | **(A, M)** | AC-01, RNF-15, RNF-33 | EQ-01 | Obliga a un **mapa base vectorial offline** recortado al corredor y a descarga incremental. | ADR-011, ADR-014 | App: Mapa y Proximidad; R2 |
| DA-15 | Los datos falsos no deben llegar al mapa, sin castigar a los conductores reales. | Atributo de calidad | **(A, A)** | AC-02, AC-04, RN-13, RN-14, RN-15 | EQ-12 | Obliga a defensas por contenido en el dominio (polilínea de la vía, marca `con_sesion`, política de plausibilidad). | ADR-015 | Anomalías: `ReglasCalidad`, `PoliticaConfirmacion`, `PoliticaPlausibilidad` |
| DA-16 | Toda acción sensible debe quedar trazada para responder a un incidente en ≤ 48 h (D.S. 016-2024-JUS). | Restricción | **(A, M)** | R-06, AC-04, RNF-29, RNF-30, RNF-31 | EQ-13 | Obliga a que la acción y su auditoría se confirmen en la misma transacción. | ADR-013 | Auditoría; todos los módulos con acciones sensibles |
| DA-17 | Una cuenta desactivada o eliminada debe perder el acceso en ≤ 5 min. | Atributo de calidad | **(M, M)** | AC-04, RNF-26 | EQ-04 | Obliga a tokens de acceso cortos y tokens de actualización revocables en la BD. | ADR-009 | Identidad y Accesos; middleware |
| DA-18 | El modelo de IA debe poder mejorarse en campo sin reinstalar la app. | Preocupación arquitectónica | **(M, M)** | S-02, RS-07, RF-33 | EQ-03 | Obliga a un canal de distribución del modelo (manifiesto + SHA-256 en R2) y a aceptar varias versiones en el servidor. | ADR-004 | App: Actualizador de modelo; `cdn.killauru.pe` |

## 3. Drivers que más moldean la arquitectura

Los drivers **(A, A)** son los que más moldean la arquitectura y los que se presentan primero en la sustentación:

| Driver | Lo que obligó a hacer |
|---|---|
| **DA-01** Offline 5 h | App autónoma con *Store-and-Forward* |
| **DA-02** Ningún evento perdido ni duplicado | Ingesta durable e idempotencia de extremo a extremo |
| **DA-04** Clasificar en < 50 ms | *Edge AI* en el celular |
| **DA-15** Sin datos falsos en el mapa | Defensas por contenido en el dominio |

Las restricciones **(A, B)** (DA-06 y DA-12) son fáciles de cumplir, pero **limitan** todas las demás decisiones: explican por qué no hay microservicios, brokers ni bases de datos adicionales.

## 4. Requisitos importantes que **no** son drivers

| Requisito | Por qué no es un driver |
|---|---|
| RNF-12 (español y ≤ 3 toques) | Es un requisito de interfaz; no cambia la estructura del sistema |
| RNF-13 (Chrome, Firefox, Edge) | Restringe las pruebas, no la arquitectura |
| RNF-14 (tiempo relativo) | Es una regla de presentación |
| RNF-16 como porcentaje de ingresos | No es medible en 2026-II; su efecto se captura en DA-08 mediante el tope absoluto |
| RN-16 (distancia de aviso según velocidad) | Es una regla de negocio dentro de un componente existente (Proximidad); no cambia la estructura |
| RF-22 y RF-32 (correo de verificación y recuperación) | Añaden un adaptador dentro de Identidad, ya previsto por la regla de una integración externa por módulo |
