# Restricciones

> **Proyecto:** KillaUru · **Versión:** 1.1 · **Revisión:** 2026-10-12
> Las restricciones son condiciones que no podemos cambiar: límites fijos que la arquitectura debe respetar. Las **decisiones** que se derivan de ellas están en `07-decisiones-arquitectonicas.md`.
> Los supuestos y riesgos que acompañan a las restricciones se registran en las secciones 2 y 3.

## 1. Restricciones

| ID | Tipo | Restricción | Decisión o impacto asociado |
|---|---|---|---|
| R-01 | Tecnología | La app móvil debe ser **Android nativo (Kotlin)**. No se permite desarrollo híbrido (Flutter, React Native): se necesita acceso directo a los sensores a 50 Hz y ejecución confiable en segundo plano. | ADR-004; servicio en primer plano |
| R-02 | Tecnología | El backend debe desarrollarse en **ASP.NET Core (.NET 8)**, tecnología principal del autor y del proyecto. | ADR-001 |
| R-03 | Tecnología | La base de datos debe ser **PostgreSQL con la extensión PostGIS**: es el motor open source más maduro para consultas geoespaciales en producción y el que domina el equipo. | Índices GiST (`geography`); ADR-001 |
| R-04 | Infraestructura | El servidor debe ser **Oracle Cloud ARM (Always Free)**. Costo operativo cercano a cero, con tope de **RNF-16**. | ADR-006, ADR-010 |
| R-05 | Infraestructura | El dominio debe ser **`killauru.pe`**, registrado en Punto.pe. El TLD .pe comunica identidad peruana. Subdominios previstos: `api`, `panel`, `status` (confirmar si habrá un cuarto). | ADR-010 |
| R-06 | Legal | El sistema debe cumplir la **Ley N.º 29733** (Protección de Datos Personales del Perú) y su reglamento vigente. **No debe almacenarse ni inferirse un vínculo entre la identidad de una persona o cuenta y sus eventos o trayectorias.** Los datos de cuenta (correo) se tratan con consentimiento y finalidad definidos. El uso de Cloudflare y de una región de nube fuera de Perú implica flujo transfronterizo y debe documentarse. | ADR-007; RNF-10, RNF-25 |
| R-07 | Legal | Los datos agregados son propiedad intelectual de CRISSO DEV S.A.C. **No se exponen en internet abierto**: la exposición pública se limita al tramo demo y se hace cumplir en el servidor. | RNF-27; DA-10 |
| R-08 | Conectividad | El sistema debe operar en zonas sin cobertura celular durante 4 a 5 horas continuas. La arquitectura no puede depender de conexión permanente. Los eventos pueden llegar al servidor hasta 7 días después de generarse. | ADR-003; ventana de timestamp en RF-12 |
| R-09 | Equipo | El proyecto es desarrollado por **una sola persona**, que asume los roles PO, Dev, QA y SM. La arquitectura debe ser simple de operar y desplegar. | ADR-001 |
| R-10 | Tiempo | El sistema debe estar desplegado en producción en **6 a 8 semanas**, para la sustentación del curso IS488. | Priorización MoSCoW en `02`; DA-12 |

## 2. Supuestos

| ID | Supuesto | Si deja de cumplirse |
|---|---|---|
| S-01 | El piloto cuenta con **hasta ~300 dispositivos** simultáneos. | Revisar el dimensionamiento (RNF-03) y los umbrales de confirmación |
| S-02 | La app se distribuye como **APK** (sin Play Store en v1.0). | Evaluar un canal de actualización del modelo y de la app |
| S-03 | Se despliega **una sola instancia** del backend. | Si hay más de una instancia, la caché en memoria y la cola en proceso dejan de bastar (ver ADR-006) |
| S-04 | Los umbrales de RN-11 y RN-12 se **calibran con datos del piloto**. | Parametrizarlos por configuración (ver `09-reglas-de-negocio.md`) |
| S-05 | La penetración de la app es baja, por lo que muchas anomalías reales pueden tardar en confirmarse. | Medir la tasa de confirmación (RNF-22) y ajustar |

## 3. Riesgos de la infraestructura gratuita *(verificar vigencia con cada proveedor)*

| ID | Riesgo | Mitigación sugerida |
|---|---|---|
| RS-01 | Oracle puede **recuperar instancias Always Free con baja utilización**. | Evaluar pasar la cuenta a Pay As You Go sin salir de los recursos gratuitos |
| RS-02 | La imagen de PostGIS debe tener **soporte arm64**. | Verificar antes de fijar `docker-compose` |
| RS-03 | UptimeRobot Free tiene restricciones de **uso comercial y de dominio propio** en la página de estado. | Revisar términos; alternativa de monitoreo y página de estado |
| RS-04 | SendGrid **retiró su plan gratuito permanente**. | Elegir un proveedor de correo con costo marginal antes de implementar HU-18b |
| RS-05 | **La región de Oracle no está en Perú**, lo que añade latencia a RNF-01. | Registrar la región elegida y apoyarse en la caché de borde |
| RS-06 | **Android restringe la ejecución en segundo plano** (servicios en primer plano, ahorro de batería). | Declarar el servicio en primer plano y probar en campo (RNF-15) |