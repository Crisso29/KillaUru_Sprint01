using KillaUru.Auditoria.Contracts;
using KillaUru.Auditoria.Domain;
namespace KillaUru.Auditoria.Application;

/// <summary>Entrada independiente de HTTP; valida también el desplazamiento para evitar desbordamientos.</summary>
internal sealed record FiltrosAuditoria(DateTimeOffset? Desde = null, DateTimeOffset? Hasta = null,
    string? Categoria = null, string? Accion = null, Guid? ActorId = null, int Pagina = 1, int Tamano = 50)
{
    internal static IEnumerable<string> CategoriasPermitidas => Enum.GetNames<CategoriaAuditoria>();
    internal static IEnumerable<string> AccionesPermitidas => Enum.GetNames<AccionAuditada>();
    internal bool EsValido => Pagina >= 1 && Tamano is >= 1 and <= 200
        && (long)(Pagina - 1) * Tamano <= int.MaxValue
        && !(Desde > Hasta)
        && (Categoria is null || Enumerable.Range(1, 14).Any(i => CatalogoAcciones.Categoria((short)i) == Categoria))
        && (Accion is null || Enum.GetNames<AccionAuditada>().Contains(Accion));

    internal short[] Acciones => Enumerable.Range(1, 14).Select(i => (short)i)
        .Where(i => (Categoria is null || CatalogoAcciones.Categoria(i) == Categoria)
            && (Accion is null || ((AccionAuditada)i).ToString() == Accion)).ToArray();
}
