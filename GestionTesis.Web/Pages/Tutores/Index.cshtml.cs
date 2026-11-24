using System.Net;
using GestionTesis.Contratos.Dtos;
using GestionTesis.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace GestionTesis.Web.Pages.Tutores;

public class IndexModel(IHttpClientFactory fabrica) : BasePageModel(fabrica)
{
    public List<TutorDto> Tutores { get; set; } = [];

    // Formularios de los modales (se llenan con TryUpdateModelAsync solo en su handler)
    public TutorGuardarDto Nuevo { get; set; } = new();
    public TutorGuardarDto Edicion { get; set; } = new();
    public Guid EdicionId { get; set; }

    public async Task<IActionResult> OnGetAsync() => await CargarAsync() ?? Page();

    // POST ?handler=Crear
    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (await TryUpdateModelAsync(Nuevo, nameof(Nuevo)))
        {
            var respuesta = await Api.PostAsJsonAsync("api/tutores", Nuevo);
            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = $"Tutor «{Nuevo.Nombre}» creado correctamente.";
                return RedirectToPage();   // PRG
            }
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();
            await AgregarErroresApiAsync(respuesta, nameof(Nuevo));
        }

        ViewData["AbrirModal"] = "modalCrear";
        return await CargarAsync() ?? Page();
    }

    // POST ?handler=Editar
    public async Task<IActionResult> OnPostEditarAsync(Guid edicionId)
    {
        EdicionId = edicionId;
        if (await TryUpdateModelAsync(Edicion, nameof(Edicion)))
        {
            var respuesta = await Api.PutAsJsonAsync($"api/tutores/{edicionId}", Edicion);
            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = $"Tutor «{Edicion.Nombre}» actualizado.";
                return RedirectToPage();
            }
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = "El tutor ya no existe.";
                return RedirectToPage();
            }
            await AgregarErroresApiAsync(respuesta, nameof(Edicion));
        }

        ViewData["AbrirModal"] = "modalEditar";
        return await CargarAsync() ?? Page();
    }

    // POST ?handler=Eliminar&id=...
    public async Task<IActionResult> OnPostEliminarAsync(Guid id)
    {
        var respuesta = await Api.DeleteAsync($"api/tutores/{id}");
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();

        if (respuesta.IsSuccessStatusCode) TempData["Exito"] = "Tutor eliminado.";
        else TempData["Error"] = await LeerMensajeErrorAsync(respuesta);   // ej: 409 "tiene tesis asignadas"

        return RedirectToPage();
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var respuesta = await Api.GetAsync("api/tutores");
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();
        Tutores = respuesta.IsSuccessStatusCode
            ? await respuesta.Content.ReadFromJsonAsync<List<TutorDto>>() ?? []
            : [];
        return null;
    }
}
