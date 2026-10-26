// ============================================================
// BLOQUEADO — spec 001 FR-044 (T080)
//
// Este archivo es el esqueleto del test E2E de KillaUru.
// No puede ejecutarse hasta que los módulos de negocio estén implementados:
//   - Spec 004: Anomalías (registro y consulta de baches/huecos)
//   - Spec 005: Ingesta (pipeline de procesamiento de reportes)
//   - Spec 006: Mapa (visualización geoespacial de anomalías)
//
// Cuando esos módulos existan, este archivo debe:
//   1. Registrar un conductor (spec 003 — Identidad)
//   2. Enviar un lote de anomalías (spec 004)
//   3. Verificar que el lote fue procesado (spec 005)
//   4. Consultar el mapa para confirmar que la anomalía aparece (spec 006)
//   5. Generar el evento de auditoría correspondiente (spec 002)
// ============================================================

using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.PuntaAPunta;

/// <summary>
/// Spec 001 — FR-044 (T080):
/// flujo completo de extremo a extremo: registro → envío de anomalías → procesamiento → mapa.
/// Verifica que todos los módulos colaboran correctamente bajo condiciones reales.
/// </summary>
/// <remarks>
/// BLOQUEADO: requiere specs 004 (Anomalías), 005 (Ingesta) y 006 (Mapa) implementados.
/// Los tests están marcados con Skip para que no fallen en la suite actual.
/// Eliminar el Skip cuando los endpoints de negocio estén disponibles.
/// </remarks>
[Collection("BaseDatosPrueba")]
public sealed class FlujoPuntaAPuntaTests(BaseDatosPrueba bd)
{
    private const string Bloqueado =
        "BLOQUEADO: requiere specs 004, 005 y 006 implementados (FR-044, T080)";

    /// <summary>
    /// Flujo nominal: un conductor registrado envía un lote, el pipeline lo procesa
    /// y la anomalía aparece en el mapa dentro de 5 segundos (FR-044, T080).
    /// </summary>
    [Fact(Skip = Bloqueado)]
    public async Task Conductor_envia_lote_pipeline_procesa_y_anomalia_aparece_en_mapa()
    {
        // Arrange — conductor registrado con token válido
        using var api = new FabricaApi(bd);
        // TODO (spec 003): registrar conductor y obtener token
        // var token = await RegistrarConductorAsync(api);

        // Act — enviar lote de anomalías
        // TODO (spec 004): POST /api/v1/anomalias/lotes con el token del conductor
        // var loteId = await EnviarLoteAsync(api, token);

        // Act — esperar procesamiento del pipeline
        // TODO (spec 005): GET /api/v1/anomalias/lotes/{loteId}/estado hasta "Procesado"

        // Assert — la anomalía aparece en el mapa (spec 006)
        // TODO (spec 006): GET /api/v1/mapa/anomalias?bbox=... y verificar que el loteId está

        // Assert — el evento de auditoría fue registrado (spec 002)
        // TODO (spec 002): consultar auditoría y verificar acción "LoteEnviado"

        await Task.CompletedTask; // placeholder hasta implementar los pasos anteriores
    }

    /// <summary>
    /// Flujo de seguridad E2E: un anónimo no puede enviar lotes ni ver el panel de administración
    /// (FR-008, FR-009, T080).
    /// </summary>
    [Fact(Skip = Bloqueado)]
    public async Task Anonimo_no_puede_enviar_lotes_ni_acceder_al_panel_admin()
    {
        // Arrange — cliente sin token (rol Anónimo)
        using var api = new FabricaApi(bd);
        var cliente = api.CreateClient();

        // Act + Assert — envío de lote sin token → 401 (FR-008, spec 004)
        // TODO (spec 004): POST /api/v1/anomalias/lotes sin token → Assert 401

        // Act + Assert — acceso al panel de administración sin token → 401/403 (FR-009)
        // TODO (spec 003): GET /api/v1/admin/usuarios sin token → Assert 401

        await Task.CompletedTask; // placeholder hasta implementar los pasos anteriores
    }
}
