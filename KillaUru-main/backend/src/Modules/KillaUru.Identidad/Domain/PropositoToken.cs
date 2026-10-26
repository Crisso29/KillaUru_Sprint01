namespace KillaUru.Identidad.Domain;

/// <summary>Finalidades separadas para impedir usar un enlace como otra credencial.</summary>
internal enum PropositoToken : short { Verificacion = 1, Recuperacion = 2, Invitacion = 3 }
