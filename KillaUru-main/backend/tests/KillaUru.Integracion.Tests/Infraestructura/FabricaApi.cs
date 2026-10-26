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
        builder.UseSetting("Identidad:ClaveRotacion", "clave-de-rotacion-solo-para-pruebas-003");
        builder.UseSetting("Identidad:TurnstileSecreto", "secreto-ficticio-pruebas");
        builder.UseSetting("Identidad:CorreoApiKey", "secreto-ficticio-pruebas");
        builder.UseSetting("Api:RedTunel", null);
        builder.ConfigureTestServices(servicios =>
        {
            // Cada host comparte la BD sintética: las tareas se prueban aparte para no alterar datos de otro caso.
            var purga = servicios.SingleOrDefault(x => x.ImplementationType == typeof(KillaUru.Identidad.Infrastructure.Tareas.PurgaIdentidadTarea));
            if (purga is not null) servicios.Remove(purga);
            var carga = servicios.SingleOrDefault(x => x.ImplementationType == typeof(KillaUru.Anomalias.Infrastructure.CargadorCorredor));
            if (carga is not null) servicios.Remove(carga);
            foreach (var tarea in servicios.Where(x => x.ImplementationType?.Namespace == "KillaUru.Anomalias.Infrastructure.Tareas").ToArray()) servicios.Remove(tarea);
            // El corredor real se valida en sus pruebas; otros módulos no dependen de datos cartográficos externos.
            var corredor = new KillaUru.Anomalias.Application.Procesamiento.EstadoCorredor();
            corredor.Cargar(new([KillaUru.Shared.Domain.Coordenada.Crear(-13, -76)!.Value,
                KillaUru.Shared.Domain.Coordenada.Crear(-13, -75)!.Value], 50));
            servicios.AddSingleton(corredor);
            servicios.AddControllers().ConfigureApplicationPartManager(partes =>
                partes.ApplicationParts.Add(new AssemblyPart(typeof(ControladorDePrueba).Assembly)));
        });
    }
}
