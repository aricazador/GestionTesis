using GestionTesis.Contratos.Dtos;
using GestionTesis.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace GestionTesis.Web.Pages;

public class RegistrarModel(IHttpClientFactory fabrica) : BasePageModel(fabrica)
{
    [BindProperty]
    public RegistrarDto Entrada { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        Entrada.Rol = null; // el auto-registro SIEMPRE queda como "Usuario"
        if (!ModelState.IsValid) return Page();

        var respuesta = await Api.PostAsJsonAsync("api/auth/registrar", Entrada);
        if (!respuesta.IsSuccessStatusCode)
        {
            await AgregarErroresApiAsync(respuesta, nameof(Entrada));
            return Page();
        }

        TempData["Exito"] = "Cuenta creada. Ahora iniciá sesión.";
        return RedirectToPage("/Login");   // PRG: Post -> Redirect -> Get
    }
}
