using System.Net;
using System.Text.Json;
using GestionTesis.Contratos.Constantes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestionTesis.Web.Infraestructura;

/// <summary>Página base con ayudas comunes: cliente de la API, rol actual y lectura de errores de la API.</summary>
public abstract class BasePageModel(IHttpClientFactory fabrica) : PageModel
{
    /// <summary>HttpClient con nombre "Api" (ya trae la URL base y el TokenHandler).</summary>
    protected HttpClient Api => fabrica.CreateClient(ApiCliente.Nombre);

    public bool EsAdmin => HttpContext.Session.GetString(SesionClaves.Rol) == Roles.Administrador;

    /// <summary>Arma la URL completa de una imagen guardada en la API.</summary>
    public string UrlImagen(string? ruta)
    {
        var baseApi = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["ApiBaseUrl"]!.TrimEnd('/');
        return $"{baseApi}{ruta}";
    }

    /// <summary>Si la API dice 401 (token vencido), cerramos la sesión y mandamos al Login.</summary>
    protected IActionResult SesionExpirada()
    {
        HttpContext.Session.Clear();
        TempData["Error"] = "Tu sesión expiró. Volvé a iniciar sesión.";
        return RedirectToPage("/Login");
    }

    /// <summary>
    /// Lee la respuesta de error de la API y pone cada mensaje DEBAJO del campo correspondiente.
    /// Soporta los dos formatos: ValidationProblemDetails ("errors") y ErrorDto ("mensaje"/"campo").
    /// </summary>
    protected async Task AgregarErroresApiAsync(HttpResponseMessage respuesta, string? prefijo)
    {
        var contenido = await respuesta.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(contenido);
            var raiz = doc.RootElement;

            if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == JsonValueKind.Object)
            {
                foreach (var campo in errores.EnumerateObject())
                {
                    var clave = Clave(prefijo, NormalizarCampo(campo.Name));
                    foreach (var mensaje in campo.Value.EnumerateArray())
                        ModelState.AddModelError(clave, mensaje.GetString() ?? "Dato inválido.");
                }
                return;
            }

            if (raiz.TryGetProperty("mensaje", out var msj))
            {
                string? campo = raiz.TryGetProperty("campo", out var c) && c.ValueKind == JsonValueKind.String
                    ? c.GetString()
                    : null;
                ModelState.AddModelError(Clave(prefijo, campo), msj.GetString() ?? "Error.");
                return;
            }
        }
        catch (JsonException)
        {
            // La respuesta no era JSON; usamos un mensaje según el código
        }

        ModelState.AddModelError(string.Empty, MensajePorCodigo(respuesta.StatusCode));
    }

    /// <summary>Devuelve solo el texto del error (para mostrarlo arriba con TempData).</summary>
    protected static async Task<string> LeerMensajeErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            using var doc = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("mensaje", out var m) && m.GetString() is { } texto)
                return texto;
        }
        catch (JsonException) { }

        return MensajePorCodigo(respuesta.StatusCode);
    }

    protected static string MensajePorCodigo(HttpStatusCode codigo) => codigo switch
    {
        HttpStatusCode.BadRequest => "Los datos enviados no son válidos.",
        HttpStatusCode.Unauthorized => "Tu sesión no es válida. Iniciá sesión de nuevo.",
        HttpStatusCode.Forbidden => "No tenés permiso para hacer esta acción.",
        HttpStatusCode.NotFound => "El registro no existe.",
        HttpStatusCode.Conflict => "El registro entra en conflicto con otro existente.",
        _ => $"Error inesperado de la API ({(int)codigo})."
    };

    private static string Clave(string? prefijo, string? campo) =>
        string.IsNullOrEmpty(campo) ? string.Empty
        : string.IsNullOrEmpty(prefijo) ? campo
        : $"{prefijo}.{campo}";

    // "$.nombre" o "nombre" -> "Nombre"
    private static string NormalizarCampo(string nombre)
    {
        var limpio = nombre.StartsWith("$.") ? nombre[2..] : nombre;
        return limpio.Length == 0 ? limpio : char.ToUpperInvariant(limpio[0]) + limpio[1..];
    }
}
