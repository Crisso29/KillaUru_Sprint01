namespace KillaUru.Auditoria.Application;

/// <summary>Página y total de una misma instantánea de lectura.</summary>
internal sealed record PaginaAuditoria(IReadOnlyList<RegistroConsultado> Elementos, int Pagina, int Tamano, long Total);
