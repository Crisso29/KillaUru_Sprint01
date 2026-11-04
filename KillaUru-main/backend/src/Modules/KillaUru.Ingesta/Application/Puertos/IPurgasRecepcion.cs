namespace KillaUru.Ingesta.Application.Puertos;

/// <summary>Puerto de purgas de la tabla evento_recibido (FR-017, FR-018, data-model §3).</summary>
internal interface IPurgasRecepcion
{
    /// <summary>Pone seudonimo=NULL, lote_id=NULL y redondea recibido_utc a la hora para filas antiguas; devuelve filas afectadas.</summary>
    Task<int> PurgarSeudonimosAsync(DateTimeOffset antesDe, CancellationToken ct);

    /// <summary>Borra las filas Procesado/Descartado recibidas antes de <paramref name="antesDe"/>; devuelve filas afectadas.</summary>
    Task<int> BorrarFinalesAsync(DateTimeOffset antesDe, CancellationToken ct);
}
