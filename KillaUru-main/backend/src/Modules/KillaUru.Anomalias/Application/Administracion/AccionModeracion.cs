using System.Text.Json.Serialization;
namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Comando del adaptador; se traduce explícitamente a la tabla del dominio.</summary>
[JsonConverter(typeof(ConversorAccionModeracion))]
internal enum AccionModeracion { MarcarReparada, Descartar, Reabrir, MantenerDescartada }
