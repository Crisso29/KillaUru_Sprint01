namespace KillaUru.Anomalias.Domain;

/// <summary>Catálogo RN-01; Normal nunca se almacena.</summary>
internal enum TipoAnomalia : short
{
    /// <summary>Hueco en el pavimento: golpe vertical fuerte y corto, con rebote (RN-01).</summary>
    Bache = 1,

    /// <summary>Asentamiento, desnivel o grieta ancha en la calzada: caída de la rueda sin rebote marcado (RN-01).</summary>
    Hundimiento = 2,
}
