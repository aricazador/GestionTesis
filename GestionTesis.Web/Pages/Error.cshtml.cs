using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestionTesis.Web.Pages;

[IgnoreAntiforgeryToken]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ErrorModel : PageModel
{
    public string Mensaje { get; private set; } = "Ocurrió un error inesperado.";

    public void OnGet() => Cargar();
    public void OnPost() => Cargar();

    private void Cargar()
    {
        var error = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        if (error is HttpRequestException or TaskCanceledException)
            Mensaje = "No se pudo conectar con la API. Verificá que el proyecto GestionTesis.Api esté ejecutándose (https://localhost:7101).";
    }
}
