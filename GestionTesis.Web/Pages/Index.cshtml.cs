using System.Net;
using GestionTesis.Contratos.Dtos;
using GestionTesis.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace GestionTesis.Web.Pages;

public class IndexModel(IHttpClientFactory fabrica) : BasePageModel(fabrica)
{
    public List<TutorDto> Tutores { get; set; } = [];
    public List<TesisDto> Tesis { get; set; } = [];
    public string? Usuario => HttpContext.Session.GetString(SesionClaves.Usuario);
    public string? Rol => HttpContext.Session.GetString(SesionClaves.Rol);

    [BindProperty(SupportsGet = true)]
    public bool Denegado { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var rTutores = await Api.GetAsync("api/tutores");
        if (rTutores.StatusCode == HttpStatusCode.Unauthorized) return SesionExpirada();
        if (rTutores.IsSuccessStatusCode)
            Tutores = await rTutores.Content.ReadFromJsonAsync<List<TutorDto>>() ?? [];

        var rTesis = await Api.GetAsync("api/tesis");
        if (rTesis.IsSuccessStatusCode)
            Tesis = await rTesis.Content.ReadFromJsonAsync<List<TesisDto>>() ?? [];

        return Page();
    }
}
