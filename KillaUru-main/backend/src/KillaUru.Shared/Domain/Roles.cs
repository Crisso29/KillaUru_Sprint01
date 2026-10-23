namespace KillaUru.Shared.Domain;

/// <summary>
/// Nombres de los roles (10-glosario.md §4). Viven en Shared para que cada módulo proteja sus
/// endpoints sin depender del módulo de Identidad.
/// </summary>
public static class Roles
{
    /// <summary>Petición sin token.</summary>
    public const string Anonimo = "Anonimo";

    /// <summary>Conductor con cuenta.</summary>
    public const string ConductorRegistrado = "ConductorRegistrado";

    /// <summary>Personal que opera el panel.</summary>
    public const string Administrador = "Administrador";
}
