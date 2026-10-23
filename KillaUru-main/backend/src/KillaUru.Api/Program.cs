using KillaUru.Api.Configuracion;
using KillaUru.Api.Documentacion;
using KillaUru.Api.Errores;
using KillaUru.Api.Modulos;
using KillaUru.Api.Observabilidad;
using KillaUru.Api.Salud;
using KillaUru.Api.Seguridad;

// El host solo compone: cada asunto transversal vive en su extensión y cada módulo en su Add<Modulo>Module.
var builder = WebApplication.CreateBuilder(args);
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
