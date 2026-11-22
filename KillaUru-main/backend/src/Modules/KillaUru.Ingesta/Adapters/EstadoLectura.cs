namespace KillaUru.Ingesta.Adapters;

/// <summary>Resultado de leer el cuerpo HTTP del lote, antes de evaluar los eventos.</summary>
internal enum EstadoLectura
{
    /// <summary>Cuerpo `{ "eventos": [...] }` dentro del tope; cada evento se evalúa después por separado.</summary>
    Leido,

    /// <summary>No es JSON, está vacío o no trae el arreglo `eventos` → 400 `lote_invalido`.</summary>
    Invalido,

    /// <summary>Más bytes que `Ingesta:TamanoMaximoCuerpoKb` → 413 `lote_demasiado_grande`.</summary>
    DemasiadoGrande,
}
