using System.Net;
using GestionTesis.Contratos.Dtos;
using GestionTesis.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace GestionTesis.Web.Pages;

public class LoginModel(IHttpClientFactory fabrica) : BasePageModel(fabrica)
{
    [BindProperty]
    public LoginDto Entrada { get; set; } = new();

    public IActionResult OnGet()
    {
        // Si ya tiene sesión, no tiene sentido mostrar el login
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SesionClaves.Token)))
            return RedirectToPage("/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var respuesta = await Api.PostAsJsonAsync("api/auth/login", Entrada);

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return Page();
        }
        if (!respuesta.IsSuccessStatusCode)
        {
            await AgregarErroresApiAsync(respuesta, nameof(Entrada));
            return Page();
        }

        var token = await respuesta.Content.ReadFromJsonAsync<TokenDto>();

        // Guardamos el "pase" en la Session: el TokenHandler lo usará en cada pedido
        HttpContext.Session.SetString(SesionClaves.Token, token!.Token);
        HttpContext.Session.SetString(SesionClaves.Usuario, token.NombreUsuario);
        HttpContext.Session.SetString(SesionClaves.Rol, token.Rol);

        TempData["Exito"] = $"¡Bienvenido, {token.NombreUsuario}!";
        return RedirectToPage("/Index");
    }
}
