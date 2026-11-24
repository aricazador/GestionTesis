using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestionTesis.Web.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Index");

    public IActionResult OnPost()
    {
        HttpContext.Session.Clear();   // borramos el token: "devolvemos el pase"
        TempData["Exito"] = "Sesión cerrada.";
        return RedirectToPage("/Login");
    }
}
