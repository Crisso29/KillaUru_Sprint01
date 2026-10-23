namespace KillaUru.Auditoria.Domain;

/// <summary>Clasifica los códigos persistidos sin depender del contrato de otros módulos.</summary>
internal static class CatalogoAcciones
{
    internal static string? Categoria(short accion) => accion switch
    {
        1 or 2 or 3 or 4 or 11 => "Usuarios",
        5 or 9 or 10 => "CuentaDelTitular",
        6 or 7 or 8 => "Sesion",
        12 => "Moderacion",
        13 => "Exportacion",
        14 => "AccesoADatos",
        _ => null,
    };
    internal static bool EsDeRiesgo(short accion) => accion is 2 or 7 or 13;
}
