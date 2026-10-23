# No aplicado en 006, 007, README y prototipo (2026-10-21)

| Hallazgo | Motivo |
|---|---|
| QA-07 (006: SC «p95 < 300 ms desde Lima» y prueba sintética) | La carga y su medición son de 001 (D11); se agrega allí si el dueño lo decide. 006 solo referencia esas pruebas |
| QA-20 / T-QA20 (prueba de punta a punta lote → mapa → indicador) | El plan la asigna a 001 (`PuntaAPunta/`); no se duplica en 007 |
| SEG-22 (regla de caché de Cloudflare con clave normalizada y prueba a través de Cloudflare) | Sin decisión del autor; T030 ya exige «respetar Cache-Control del origen». La prueba por el borde es de 001 |
| BD-18 (`TramosTotales` calculado con la polilínea) | Lo calcula 004 en `ResumenDia`; 007 solo guarda el número |
| ARQ-16 (nombres de `Nivel4` §5-§6) | Es un cambio de docs; lo hace el agente de `referencia/docs` |
| PO-21 («Servicio operativo · 99,9 %» del Resumen) | Fuera de las decisiones pedidas para el prototipo (cambios mínimos D13/D6) |
| Prototipo «Eventos ayer» vs. spec «Eventos válidos (ayer)» | Se respetó el texto exacto del plan para el prototipo (c); la ayuda y el gráfico del prototipo dicen «eventos válidos» |
