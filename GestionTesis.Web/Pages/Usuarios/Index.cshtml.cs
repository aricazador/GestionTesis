using System.Net;
using GestionTesis.Contratos.Dtos;
using GestionTesis.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace GestionTesis.Web.Pages.Usuarios;

/// <summary>Página solo para Administrador (el SesionMiddleware bloquea al resto).</summary>
public class IndexModel(IHttpClientFactory fabrica) : BasePageModel(fabrica)
{
    public List<UsuarioDto> Lista { get; set; } = [];
    public RegistrarDto Nuevo { get; set; } = new();

    public async Task<IActionResult> OnGetAsync() => await CargarAsync() ?? Page();

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (await TryUpdateModelAsync(Nuevo, nameof(Nuevo)))
        {
            // Como el TokenHandler manda el token de admin, la API permite crear Administradores
            var respuesta = await Api.PostAsJsonAsync("api/auth/registrar", Nuevo);
            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = $"Usuario «{Nuevo.NombreUsuario}» creado como {Nuevo.Rol}.";
                return RedirectToPage();
            }
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();
            await AgregarErroresApiAsync(respuesta, nameof(Nuevo));
        }

        ViewData["AbrirModal"] = "modalCrear";
        return await CargarAsync() ?? Page();
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var respuesta = await Api.GetAsync("api/auth/usuarios");
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();
        Lista = respuesta.IsSuccessStatusCode ? await respuesta.Content.ReadFromJsonAsync<List<UsuarioDto>>() ?? [] : [];
        return null;
    }
}
