# Guía de etiquetado — KillaUru

**T028 / FR-009, FR-010, FR-011 · Versión 1.0 · 2026-10-22**

---

## 1. Tipos de evento (RN-01)

| Tipo | Descripción | ¿Lleva severidad? |
|---|---|---|
| **Bache** | Depresión repentina en la carpeta asfáltica | Sí (1–5) |
| **Hundimiento** | Deformación gradual, mayor extensión que el bache | Sí (1–5) |
| **Normal** | Superficie sin deterioro relevante | No |

> **Atención**: `Rompemuelles`, `Junta`, `Baden`, etc. son **subtipos de Normal**, no tipos independientes (RS-07).

---

## 2. Escala de severidad (RN-01)

| Nivel | Criterio orientativo |
|---|---|
| 1 | Apenas perceptible; sin riesgo para el vehículo |
| 2 | Molestia leve; no afecta la conducción |
| 3 | Moderado; puede dañar vehículos de baja suspensión |
| 4 | Grave; pone en riesgo la estabilidad del vehículo |
| 5 | Muy grave; provoca daño seguro y obliga a detenerse |

La severidad se asigna **por observación del video** en el segundo indicado por `segundo_video`.

---

## 3. Subtipos de Normal

Usar cuando la causa de la vibración es identificable pero no implica deterioro:

`Rompemuelles` · `Junta` · `Baden` · `Riel` · `Ripio` · `CambioPavimento`

---

## 4. Zona gris

Una ventana de 500 ms marcada como **zona gris** contiene un pico cuyo origen es ambiguo
(el evento real ocurre en la ventana adyacente, a < 250 ms del borde).
Las ventanas zona gris **pueden incluirse en entrenamiento** con precaución o excluirse
con la bandera `--excluir-zona-gris` del comando `ventanas`.

---

## 5. Fuente de la etiqueta

| Fuente | Cuándo usar |
|---|---|
| `video` | Revisión directa del video sincronizado |
| `patron` | Bache catalogado en `baches-patron.csv` |
| `sensor` | Solo del acelerómetro, sin video disponible |

---

## 6. Protocolo de doble etiquetado (FR-011)

- El **10 %** de las grabaciones se etiqueta por dos personas independientes.
- Se calcula el **κ de Cohen** sobre los tipos; debe ser **≥ 0.80**.
- El acuerdo de severidad **±1** debe ser **≥ 90 %**.
- Si no se alcanzan los umbrales, revisar los criterios de severidad en §2 y repetir.

```bash
python -m killauru_ml acuerdo <archivo_e1.json> <archivo_e2.json>
```

---

## 7. Flujo de trabajo

1. Ejecutar `python -m killauru_ml candidatos <carpeta>` para obtener la lista de candidatos.
2. Abrir el video en el segundo `segundo_video` de cada candidato.
3. Asignar tipo, severidad y subtipo según §1-§3.
4. Guardar en `<carpeta>/etiquetas.json` con formato:

```json
[
  {
    "t_pico_ns": 1234567890,
    "tipo": "Bache",
    "severidad": 3,
    "subtipo": "",
    "fuente": "video",
    "etiquetador": "E1",
    "bache_patron": ""
  }
]
```

5. Validar con `python -m killauru_ml validar-etiquetas <carpeta>/etiquetas.json`.
6. Si la grabación forma parte del 10 % de doble etiquetado, enviar al segundo etiquetador
   antes de continuar.
