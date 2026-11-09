// T068 — E2E moderación: las 4 acciones y un 409 (FR-027…FR-030, SC-008)
import { test } from '@playwright/test'

test.skip('marcar reparada desde detalle Activa', async () => {
  // 1. Login como administrador
  // 2. Abrir /mapa?anomalia=<id-activa>
  // 3. Click «Marcar como reparada» → confirmar
  // 4. El estado cambia a Reparada en el detalle
})

test.skip('descartar desde Expirada', async () => {
  // 1. Abrir /mapa?anomalia=<id-expirada>
  // 2. Click «Descartar» → confirmar
  // 3. Estado = Descartada
})

test.skip('mantener descartada desde Por revisar', async () => {
  // 1. Abrir /moderacion
  // 2. Pestaña «Por revisar» → botón «Mantener descartada» (sin diálogo)
  // 3. Fila sale de la lista
})

test.skip('reabrir desde Reparada en detalle', async () => {
  // 1. Abrir /mapa?anomalia=<id-reparada>
  // 2. Click «Reabrir» → confirmar en diálogo
  // 3. Estado = Activa
})

test.skip('409 transicion_no_permitida → aviso y recarga del detalle', async () => {
  // 1. Abrir dos pestañas con el mismo detalle Activa
  // 2. Pestaña A: marcar reparada
  // 3. Pestaña B: intentar marcar reparada → 409 → ver aviso y estado actualizado
})
