using System.Diagnostics;
using System.Text.Json;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>Prueba explícita: Kestrel, PostgreSQL y k6 reales, aislados de producción.</summary>
public sealed class RendimientoIdentidadTests(BaseDatosPrueba bd)
{
    [Fact(Explicit = true)]
    public async Task K6_con_300_cuentas_cumple_login_y_renovacion()
    {
        using var api = new FabricaApi(bd, "Pruebas");
        api.UseKestrel(0);
        using var cliente = api.CreateClient();
        var backend = Directory.GetParent(Rutas.Migraciones)!.Parent!.FullName;
        var resultados = Path.Combine(backend, "TestResults");
        Directory.CreateDirectory(resultados);
        var archivo = Path.Combine(resultados, "identidad-cuentas-sinteticas.json");
        await PrepararAsync(api.Services, archivo);
        var direccion = api.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var inicio = new ProcessStartInfo("k6") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var argumento in new[] { "run", "--summary-export", Path.Combine(resultados, "identidad-k6.json"), Path.Combine(backend, "tests", "carga", "identidad.k6.js") })
            inicio.ArgumentList.Add(argumento);
        inicio.Environment["BASE_URL"] = direccion;
        inicio.Environment["USUARIOS_ARCHIVO"] = archivo;
        inicio.Environment["CONFIRMAR_PRUEBAS"] = "si";
        using var proceso = Process.Start(inicio)!;
        var salida = proceso.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var errores = proceso.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await proceso.WaitForExitAsync(TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(resultados, "identidad-k6.log"), await salida + await errores, TestContext.Current.CancellationToken);
        Assert.True(proceso.ExitCode == 0, "k6 no cumplió los umbrales; revisar TestResults/identidad-k6.log.");
    }

    private static async Task PrepararAsync(IServiceProvider servicios, string archivo)
    {
        await using var scope = servicios.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var hash = scope.ServiceProvider.GetRequiredService<IHashContrasenas>().Hash(EscenarioIdentidad.Contrasena);
        var ahora = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow();
        var cuentas = Enumerable.Range(0, 300).Select(_ => new { correo = Guid.NewGuid() + "@example.com", contrasena = EscenarioIdentidad.Contrasena }).ToArray();
        db.Usuarios.AddRange(cuentas.Select(x => Usuario.RegistrarConductor(Correo.Crear(x.correo)!, hash, ahora)));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(archivo, JsonSerializer.Serialize(cuentas), TestContext.Current.CancellationToken);
    }
}
