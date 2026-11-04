using System.Text.Json;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>Datos del POST armados por el controlador antes de entrar al caso de uso (D4).</summary>
/// <param name="Eventos">Elementos crudos; cada uno se deserializa por separado (FR-002).</param>
/// <param name="ConSesion">True si el token es de ConductorRegistrado con verificado=true (FR-007).</param>
/// <param name="SeudonimoCabecera">UUID de la cabecera X-KillaUru-Seudonimo, o null si no vino (FR-024).</param>
internal sealed record LoteEntrante(
    IReadOnlyList<JsonElement> Eventos,
    bool ConSesion,
    Guid? SeudonimoCabecera);
