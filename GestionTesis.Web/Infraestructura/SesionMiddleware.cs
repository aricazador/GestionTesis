using GestionTesis.Contratos.Constantes;

namespace GestionTesis.Web.Infraestructura;

/// <summary>
/// "Portero" de la Web:
///  - Sin sesión -> lo manda al Login.
///  - Rol Usuario intentando entrar a páginas de admin o modificar datos (POST) -> lo manda al Inicio.
/// </summary>
public class SesionMiddleware(RequestDelegate siguiente)
{
    private static readonly string[] RutasPublicas = ["/Login", "/Registrar", "/Error"];
    private static readonly string[] RutasSoloAdmin = ["/Usuarios"];

    public async Task InvokeAsync(HttpContext contexto)
    {
        var ruta = contexto.Request.Path;
        var token = contexto.Session.GetString(SesionClaves.Token);
        var esPublica = RutasPublicas.Any(r => ruta.StartsWithSegments(r, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(token))
        {
            if (!esPublica)
            {
                contexto.Response.Redirect("/Login");
                return;
            }
        }
        else
        {
            var esAdmin = contexto.Session.GetString(SesionClaves.Rol) == Roles.Administrador;
            var paginaAdmin = RutasSoloAdmin.Any(r => ruta.StartsWithSegments(r, StringComparison.OrdinalIgnoreCase));

            // El rol Usuario solo puede VER: cualquier POST (crear/editar/eliminar) se bloquea
            var intentaModificar = HttpMethods.IsPost(contexto.Request.Method)
                                   && !esPublica
                                   && !ruta.StartsWithSegments("/Logout", StringComparison.OrdinalIgnoreCase);

            if (!esAdmin && (paginaAdmin || intentaModificar))
            {
                contexto.Response.Redirect("/?denegado=true");
                return;
            }
        }

        await siguiente(contexto);
    }
}
