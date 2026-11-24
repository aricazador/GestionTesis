using System.Net;
using System.Net.Http.Headers;
using GestionTesis.Contratos.Constantes;
using GestionTesis.Contratos.Dtos;
using GestionTesis.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace GestionTesis.Web.Pages.Tesis;

public class IndexModel(IHttpClientFactory fabrica) : BasePageModel(fabrica)
{
    public List<TesisDto> Lista { get; set; } = [];
    public List<TutorDto> Tutores { get; set; } = [];

    /// <summary>Texto del buscador (viene por la URL: /Tesis?Texto=redes).</summary>
    [BindProperty(SupportsGet = true)]
    public string? Texto { get; set; }

    public TesisGuardarDto Nuevo { get; set; } = new();
    public TesisGuardarDto Edicion { get; set; } = new();
    public Guid EdicionId { get; set; }
    public IFormFile? ImagenCrear { get; set; }
    public IFormFile? ImagenEditar { get; set; }

    public async Task<IActionResult> OnGetAsync() => await CargarAsync() ?? Page();

    // POST ?handler=Crear  (punto 2 + punto 4)
    public async Task<IActionResult> OnPostCrearAsync(IFormFile? imagenCrear)
    {
        var valido = await TryUpdateModelAsync(Nuevo, nameof(Nuevo));
        valido &= ValidarImagen(imagenCrear, nameof(ImagenCrear));

        if (valido)
        {
            var respuesta = await Api.PostAsJsonAsync("api/tesis", Nuevo);
            if (respuesta.IsSuccessStatusCode)
            {
                var creada = await respuesta.Content.ReadFromJsonAsync<TesisDto>();
                TempData["Exito"] = $"Tesis «{Nuevo.Titulo}» creada correctamente.";

                if (imagenCrear is not null && creada is not null)
                {
                    var rImagen = await SubirImagenAsync(creada.Id, imagenCrear);
                    if (!rImagen.IsSuccessStatusCode)
                        TempData["Error"] = "La tesis se creó, pero la imagen no se pudo subir: " + await LeerMensajeErrorAsync(rImagen);
                }
                return RedirectToPage();   // PRG
            }
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();
            await AgregarErroresApiAsync(respuesta, nameof(Nuevo));   // ej: 409 "el tutor ya tiene 3 tesis"
        }

        ViewData["AbrirModal"] = "modalCrear";
        return await CargarAsync() ?? Page();
    }

    // POST ?handler=Editar
    public async Task<IActionResult> OnPostEditarAsync(Guid edicionId, IFormFile? imagenEditar)
    {
        EdicionId = edicionId;
        var valido = await TryUpdateModelAsync(Edicion, nameof(Edicion));
        valido &= ValidarImagen(imagenEditar, nameof(ImagenEditar));

        if (valido)
        {
            var respuesta = await Api.PutAsJsonAsync($"api/tesis/{edicionId}", Edicion);
            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = $"Tesis «{Edicion.Titulo}» actualizada.";
                if (imagenEditar is not null)
                {
                    var rImagen = await SubirImagenAsync(edicionId, imagenEditar);
                    if (!rImagen.IsSuccessStatusCode)
                        TempData["Error"] = "Los datos se guardaron, pero la imagen no: " + await LeerMensajeErrorAsync(rImagen);
                }
                return RedirectToPage();
            }
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = "La tesis ya no existe.";
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
        var respuesta = await Api.DeleteAsync($"api/tesis/{id}");
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();

        if (respuesta.IsSuccessStatusCode) TempData["Exito"] = "Tesis eliminada.";
        else TempData["Error"] = await LeerMensajeErrorAsync(respuesta);

        return RedirectToPage();
    }

    // ---------------------------------------------------------------

    /// <summary>Envía la imagen a la API como multipart/form-data (campo "archivo").</summary>
    private async Task<HttpResponseMessage> SubirImagenAsync(Guid id, IFormFile imagen)
    {
        using var contenido = new MultipartFormDataContent();
        await using var stream = imagen.OpenReadStream();
        var archivo = new StreamContent(stream);
        archivo.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(imagen.ContentType) ? "application/octet-stream" : imagen.ContentType);
        contenido.Add(archivo, "archivo", imagen.FileName);

        return await Api.PostAsync($"api/tesis/{id}/imagen", contenido);
    }

    /// <summary>Validación previa en la Web para no crear la tesis si la imagen es inválida.</summary>
    private bool ValidarImagen(IFormFile? imagen, string campo)
    {
        if (imagen is null) return true;

        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
        if (!Reglas.ExtensionesImagen.Contains(extension))
        {
            ModelState.AddModelError(campo, "Formato no permitido. Use: " + string.Join(", ", Reglas.ExtensionesImagen));
            return false;
        }
        if (imagen.Length > Reglas.TamanoMaximoImagen)
        {
            ModelState.AddModelError(campo, "La imagen no puede pesar más de 2 MB.");
            return false;
        }
        return true;
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var url = string.IsNullOrWhiteSpace(Texto)
            ? "api/tesis"
            : $"api/tesis/buscar?texto={Uri.EscapeDataString(Texto)}";

        var rTesis = await Api.GetAsync(url);
        if (rTesis.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();
        Lista = rTesis.IsSuccessStatusCode ? await rTesis.Content.ReadFromJsonAsync<List<TesisDto>>() ?? [] : [];

        var rTutores = await Api.GetAsync("api/tutores");
        Tutores = rTutores.IsSuccessStatusCode ? await rTutores.Content.ReadFromJsonAsync<List<TutorDto>>() ?? [] : [];

        return null;
    }
}
