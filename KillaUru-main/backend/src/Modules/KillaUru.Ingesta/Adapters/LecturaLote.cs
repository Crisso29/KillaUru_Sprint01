using System.Text.Json;

namespace KillaUru.Ingesta.Adapters;

/// <summary>Cuerpo del lote ya leído: su estado y, solo si es <see cref="EstadoLectura.Leido"/>, sus eventos.</summary>
internal sealed record LecturaLote(EstadoLectura Estado, IReadOnlyList<JsonElement>? Eventos)
{
    internal static LecturaLote Invalido { get; } = new(EstadoLectura.Invalido, null);

    internal static LecturaLote DemasiadoGrande { get; } = new(EstadoLectura.DemasiadoGrande, null);

    internal static LecturaLote Leido(IReadOnlyList<JsonElement> eventos) => new(EstadoLectura.Leido, eventos);
}
