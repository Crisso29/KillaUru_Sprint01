# Protocolo de campo — grabación de recorridos KillaUru

**T016 / FR-002, FR-005, FR-006 · Versión 1.0 · 2026-10-22**

---

## 1. Equipo requerido (R-020)

| Elemento | Cantidad | Notas |
|---|---|---|
| Vehículo liviano (auto, camioneta) | 1 | Nunca pesado ni moto (RN-18, S-06) |
| Celulares Android ≥ gama media, minSdk 26 | 3 (mínimo) | Los tres a la vez en el mismo vehículo |
| Soportes fijos (rejilla, parabrisas o tablero) | 3 | Uno por celular; no sostener en la mano |
| Cable USB + laptop | 1 | Para copiar la grabación después |
| Cinta métrica o distanciómetro | 1 | Medir baches patrón fuera de la vía |
| Formulario de baches patrón | Impreso | Ver §6 |

---

## 2. Seguridad — REGLAS NO NEGOCIABLES (R-003)

1. **Iniciar y detener solo estacionado**: el grabador solo acepta inicio/stop con GPS < 1 km/h. No tocar el celular si el vehículo se mueve.
2. **Solo el copiloto marca en marcha**: el conductor no toca ningún celular mientras el vehículo está en movimiento.
3. **Sin copiloto = sin marcas en marcha**: se etiqueta después con el video.
4. **Velocidad legal en todo momento**: el recorrido es de trabajo, no de prueba de velocidad.
5. **Medir baches patrón fuera de la calzada**: detener el vehículo en un lugar seguro; el copiloto mide a pie.

---

## 3. Montaje de los soportes

1. Colocar el soporte **antes de salir** del estacionamiento.
2. Posición recomendada: **centro del tablero** o rejilla de ventilación central, con el celular vertical (pantalla hacia el conductor).
3. El soporte debe quedar fijo durante todo el recorrido; si se afloja, detener y reajustar estacionado.
4. Registrar en `meta.json`: `soporte.tipo` (rejilla/parabrisas/tablero/cd) y `soporte.posicion`.
5. Verificar que los tres celulares tienen la misma orientación (todos verticales).

---

## 4. Checklist de salida

- [ ] Los tres celulares cargados (> 80 %) y con espacio libre (> 5 GB)
- [ ] Grabador APK instalado en los tres celulares
- [ ] Soportes fijos, celulares verticales
- [ ] GPS activo y con señal (esperar fix antes de salir)
- [ ] `meta.json` llenado con alias de celular, vehículo, tramo y sentido
- [ ] Golpe de sincronización al inicio: el copiloto da un golpe firme sobre el tablero para alinear los `t_ns` de los tres celulares (el grabador lo detecta automáticamente)
- [ ] Uno de los tres celulares con `esCelularVideo = true` y cámara activa

---

## 5. Plan de recorridos (R-020)

| Recorrido | Sentido | Meta |
|---|---|---|
| R01–R05 | Ayacucho → Vinchos | 5 recorridos |
| R06–R10 | Vinchos → Ayacucho | 5 recorridos |

- Tramo: km 268.0 – 335.073 (~67 km, una sola ida)
- Hora preferida: 07:00–10:00 y 15:00–18:00 (luz natural, tráfico moderado)
- Si Hundimiento < 60 % de la meta al día 7: programar pasadas dirigidas por los km del catálogo de baches patrón (R-020)

---

## 6. Medición de baches patrón (R-006)

Para cada bache o hundimiento que merezca ser catalogado como patrón:

1. Detener el vehículo en lugar seguro **fuera de la calzada**.
2. Medir:
   - Profundidad máxima (cm)
   - Ancho en la dirección de marcha (cm)
   - Km del corredor (leer del GPS)
3. Asignar severidad provisional según la tabla de `guia-etiquetado.md`.
4. Fotografiar sin personas ni placas visibles; guardar en R2 privado.
5. Llenar `ml/datos/baches-patron.csv`.

---

## 7. Consentimiento y privacidad (R-004, Ley 29733)

- Ningún participante aparece en el video ni en los datos.
- El video vive **solo** en el bucket R2 privado y se borra a los 90 días de congelar la versión del dataset (ver `scripts/borrar_videos.sh`).
- El dataset derivado contiene km y velocidad, **nunca** latitud ni longitud.
- Antes de cada grabación: todo el equipo confirma verbalmente que conoce y acepta el protocolo.

---

## 8. Después del recorrido

1. Estacionar y detener la grabación (GPS < 1 km/h).
2. Copiar la carpeta de grabación al portátil por USB.
3. Ejecutar `python -m killauru_ml ingerir <carpeta> --subir` para verificar e ingerir.
4. Si el estado es `NoApta`, revisar el motivo y decidir si repetir el recorrido.
5. Cargar los celulares para el siguiente recorrido.
