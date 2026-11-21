using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.PuntaAPunta;

/// <summary>
/// Spec 001 / FR-044 / T080: escenarios de sistema pendientes de implementar.
/// El host de prueba y PostgreSQL no sustituyen la aceptación del despliegue.
/// </summary>
[Collection("BaseDatosPrueba")]
public sealed class FlujoPuntaAPuntaTests(BaseDatosPrueba bd)
{
    private const string Pendiente =
        "T080: implementar datos, recorrido y aserciones entre módulos; no acredita E2E";

    /// <summary>Recepción durable, procesamiento y consulta de anomalías.</summary>
    [Fact(Skip = Pendiente)]
    public void Conductor_envia_lote_pipeline_procesa_y_anomalia_aparece_en_mapa()
    {
        using var api = new FabricaApi(bd);
        // TODO 003: preparar un conductor verificado y su sesión.
        // TODO 005: POST /api/v1/anomalies/batch; comprobar recepción durable.
        // TODO 004/005: preparar eventos que satisfagan las reglas de publicación.
        // No existe una ruta de polling por lote: observar el resultado con espera acotada.
        // TODO 006: GET /api/v1/anomalies; comprobar datos y recorte por rol.
        // Los descartes/recepciones no generan una auditoría "LoteEnviado".
        Assert.Fail("Implementar T080 antes de retirar Skip.");
    }

    /// <summary>Un anónimo puede enviar lotes, pero no consultar usuarios administrativos.</summary>
    [Fact(Skip = Pendiente)]
    public void Anonimo_puede_enviar_lotes_pero_no_consultar_usuarios()
    {
        using var api = new FabricaApi(bd);
        // TODO 005: lote anónimo válido a POST /api/v1/anomalies/batch → 200 durable.
        // TODO 003: GET /api/v1/usuarios sin sesión → 401.
        // Afirmar recepción y control de acceso; no inferir permisos desde la interfaz.
        Assert.Fail("Implementar T080 antes de retirar Skip.");
    }
}
