using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Integracion.Tests.Migraciones;
using Npgsql;
namespace KillaUru.Integracion.Tests.Auditoria;

/// <summary>La cuenta de aplicación no puede alterar el rastro aunque un programador escriba SQL directo.</summary>
public sealed class InmutabilidadTests(BaseDatosPrueba bd)
{
    [Theory]
    [InlineData("UPDATE auditoria.registro SET objetivo = 'alterado' WHERE false")]
    [InlineData("DELETE FROM auditoria.registro WHERE false")]
    [InlineData("TRUNCATE auditoria.registro")]
    public async Task La_app_no_puede_actualizar_ni_borrar(string sql)
    {
        var error = await Assert.ThrowsAsync<PostgresException>(() => Consulta.EjecutarAsync(bd.CadenaApp, sql));
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, error.SqlState);
    }
}
