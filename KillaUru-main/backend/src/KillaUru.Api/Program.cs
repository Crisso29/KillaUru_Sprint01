using KillaUru.Api.Configuracion;
using KillaUru.Api.Documentacion;
using KillaUru.Api.Errores;
using KillaUru.Api.Modulos;
using KillaUru.Api.Observabilidad;
using KillaUru.Api.Salud;
using KillaUru.Api.Seguridad;
using KillaUru.Identidad;

// Comando especial: "crear-admin <correo>" crea el primer administrador sin levantar el servidor.
// Cuando se detecta, el builder recibe args vacíos para no procesar flags de Kestrel.
const string ComandoCrearAdmin = "crear-admin";

var builder = WebApplication.CreateBuilder(
    args.Length > 0 && args[0] == ComandoCrearAdmin ? [] : args);

// El host solo compone: cada asunto transversal vive en su extensión y cada módulo en su Add<Modulo>Module.
builder.AgregarLogs();
builder.Services
    .AgregarConfiguracion(builder.Configuration)
    .AgregarProblemDetails()
    .AddExceptionHandler<ManejadorErrores>()
    .AgregarIpReal()
    .AgregarCorsPanel()
    .AgregarAutenticacion()
    .AgregarLimiteTasa()
    .AgregarSalud()
    .AgregarTelemetria(builder.Configuration)
    .AgregarDocumentacion()
    .AgregarModulos(builder.Configuration);

var app = builder.Build();

if (args.Length > 0 && args[0] == ComandoCrearAdmin)
{
    if (args.Length != 2)
    {
        await Console.Error.WriteLineAsync("Uso: crear-admin <correo>");
        Environment.ExitCode = 1;
    }
    else
    {
        Environment.ExitCode = await IdentidadModule.CrearAdministradorInicialAsync(app.Services, args[1], Console.Out, CancellationToken.None);
    }
    await app.DisposeAsync();
    return;
}

// El orden importa: IP real → errores → cabeceras → logs → CORS → quién es → token inválido = 401 → límites → permisos.
app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<CabecerasMiddleware>();
app.UsarLogsDePeticiones();
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<RechazoTokenInvalido>();
app.UseRateLimiter();
app.UseAuthorization();

app.MapearSalud();
app.MapearDocumentacion();
app.MapControllers();

await app.RunAsync();
