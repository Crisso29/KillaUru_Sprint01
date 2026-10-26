namespace KillaUru.Identidad.Application.Resultados;

/// <summary>Página acotada de cuentas consultadas.</summary>
internal sealed record PaginaUsuarios(IReadOnlyList<UsuarioVista> Elementos, int Pagina, int Tamano, long Total);
