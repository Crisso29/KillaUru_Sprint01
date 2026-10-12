# Drivers Arquitectónicos

> **Proyecto:** KillaUru · **Versión:** 1.1 · **Revisión:** 2026-10-12
> Un driver arquitectónico es un requisito, atributo o restricción que **obliga** a tomar una decisión de arquitectura. Criterio de inclusión: *si este driver no existiera, la arquitectura sería diferente.*
> **Relacionados:** `04-atributos-de-calidad.md`, `05-restricciones.md`, `07-decisiones-arquitectonicas.md`

| ID | Driver | Origen | ¿Por qué cambia la arquitectura? | Decisión (ADR) | Dónde se ve (C2–C4) |
|---|---|---|---|---|---|
| DA-01 | El sistema debe operar sin internet hasta 5 horas continuas. | R-08, AC-01 | Obliga a Store-and-Forward con SQLite local. Sin este driver bastaría una arquitectura cliente-servidor normal. | ADR-003 | App: buffer SQLite y sincronizador |
| DA-02 | Ningún evento aceptado puede perderse, ni siquiera por un reinicio del servidor. | AC-02, RNF-06 | Obliga a entrega *at-least-once* con reintentos, **idempotencia por `eventoId`** y **persistencia durable antes de responder 200**. | ADR-003, ADR-005 | Ingesta: tabla de recepción; app: reintentos |
| DA-03 | La detección debe ser automática y pasiva, sin acción del conductor. | RF-01, AC-08 | Obliga a un modelo ML embebido (TFLite) en lugar de reportes manuales. | ADR-004 | App: Detector |
| DA-04 | El modelo ML debe clasificar en < 50 ms en un móvil de gama media. | RNF-04, AC-03 | Obliga a inferencia local (Edge AI) en lugar de enviar la señal cruda al servidor. | ADR-004 | App: Detector |
| DA-05 | 300 dispositivos pueden sincronizar al mismo tiempo. | RNF-03, RNF-23 | Obliga a procesamiento asíncrono interno (.NET Channels) con contrapresión por lote. | ADR-005 | Ingesta: cola y `BackgroundService` |
| DA-06 | Una sola persona desarrolla y opera el sistema. | R-09 | Obliga a monolito modular en lugar de microservicios. La complejidad operacional debe ser mínima. | ADR-001, ADR-002, ADR-006 | API: proceso único |
| DA-07 | La identidad del conductor no puede almacenarse ni vincularse a sus eventos. | R-06, AC-04 | Obliga a seudónimo rotativo, purga a 72 h e historial solo local. | ADR-007 | App: seudónimo; Clasificación: purga |
| DA-08 | El costo operativo debe ser mínimo en este ciclo 2026-II. | R-04, RNF-16 | Obliga a infraestructura Free Tier (Oracle ARM, Cloudflare Free) y a evitar Redis y brokers. | ADR-006, ADR-010 | C2: despliegue |
| DA-09 | El sistema debe escalar a más módulos en el futuro (empresas, autoridades). | AC-06 | Obliga a separar el backend en *bounded contexts* internos desde el inicio, aunque corran en un solo proceso. | ADR-001, ADR-002 | C3: módulos y `Contracts/` |
| DA-10 | Los datos no pueden exponerse en internet abierto más allá del tramo demo. | R-07, RNF-27 | Obliga a **autorización por rol en el servidor** y a recortar el `bbox` de los anónimos. | ADR-009 | Consulta de Mapa; API REST |
| DA-11 | Las fallas deben detectarse antes de que las reporten los usuarios. | AC-07, RNF-20…22 | Obliga a `/health`, logs estructurados y métricas internas desde el inicio. | ADR-010 | API REST; Dashboard |
| DA-12 | El plazo es de 6–8 semanas con una sola persona. | R-10, R-09 | Obliga a **recortar el alcance** (MoSCoW) y a no introducir componentes de infraestructura opcionales. | ADR-001, ADR-012 | `02`: prioridades |
| DA-13 | El mapa debe reflejar la realidad de la vía en el tiempo (reparaciones y obstáculos temporales). | AC-02, HU-06, HU-09 | Obliga a un **ciclo de vida** de la anomalía con caducidad parametrizable. | ADR-008 | Clasificación |
| DA-14 | El mapa y las alertas deben funcionar sin señal y sin gastar datos. | EQ-01, RNF-15 | Obliga a un **mapa base vectorial offline** recortado al corredor y a descarga incremental. | ADR-011 | App: Mapa |

## Requisitos importantes que **no** son drivers

| Requisito | Por qué no es un driver |
|---|---|
| RNF-12 (español y ≤ 3 toques) | Es un requisito de interfaz; no cambia la estructura del sistema |
| RNF-13 (Chrome, Firefox, Edge) | Restringe pruebas, no la arquitectura |
| RNF-14 (tiempo relativo) | Es una regla de presentación |
| RNF-16 como porcentaje de ingresos | No es medible en 2026-II; su efecto se captura en DA-08 mediante el tope absoluto |