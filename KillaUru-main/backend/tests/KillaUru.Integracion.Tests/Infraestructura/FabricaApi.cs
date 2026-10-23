using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Infraestructura;

/// <summary>
/// La API real en memoria, conectada como killauru_app al PostgreSQL de pruebas (T021).
/// Agrega <see cref="ControladorDePrueba"/> para probar la seguridad común sin depender de los módulos.
/// Cada instancia tiene sus propios límites de tasa: créala por prueba cuando los cuente.
/// </summary>
/// <param name="bd">Base de datos de pruebas ya migrada.</param>
/// <param name="entorno">Development (con /docs) o Production.</param>
internal sealed class FabricaApi(BaseDatosPrueba bd, string entorno = "Development") : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(entorno);
        builder.UseSetting("ConnectionStrings:KillaUru", bd.CadenaApp);
        builder.UseSetting("Api:Jwt:ClaveFirma", Tokens.Clave);
        builder.UseSetting("Api:RedTunel", null);
        builder.ConfigureTestServices(servicios => servicios.AddControllers()
            .ConfigureApplicationPartManager(partes => partes.ApplicationParts.Add(new AssemblyPart(typeof(ControladorDePrueba).Assembly))));
    }
}
