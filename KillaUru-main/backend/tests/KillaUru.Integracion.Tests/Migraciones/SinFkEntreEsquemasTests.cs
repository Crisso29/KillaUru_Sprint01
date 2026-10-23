using KillaUru.Integracion.Tests.Infraestructura;

namespace KillaUru.Integracion.Tests.Migraciones;

/// <summary>
/// T063 (FR-004, ADR-012): ninguna llave foránea cruza esquemas. Si un módulo apuntara a la tabla
/// de otro, eliminar una cuenta (RF-30) o mover un módulo rompería datos ajenos.
/// </summary>
public sealed class SinFkEntreEsquemasTests(BaseDatosPrueba bd)
{
    private const string FkQueCruzanEsquemas = """
        SELECT format('%s.%s → %s.%s', origen.nspname, tabla.relname, destino_ns.nspname, destino.relname)
        FROM pg_constraint fk
        JOIN pg_class tabla ON tabla.oid = fk.conrelid
        JOIN pg_namespace origen ON origen.oid = tabla.relnamespace
        JOIN pg_class destino ON destino.oid = fk.confrelid
        JOIN pg_namespace destino_ns ON destino_ns.oid = destino.relnamespace
        WHERE fk.contype = 'f' AND origen.nspname <> destino_ns.nspname
        """;

    [Fact]
    public async Task Ninguna_llave_foranea_apunta_a_otro_esquema()
    {
        var cruces = await Consulta.TextosAsync(bd.CadenaSuperusuario, FkQueCruzanEsquemas);

        Assert.Empty(cruces);
    }
}
