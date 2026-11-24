using System.Net.Http.Headers;

namespace GestionTesis.Web.Infraestructura;

/// <summary>
/// "Cartero" que intercepta CADA pedido a la API y le pega el token:
/// Authorization: Bearer {token}. Así las páginas no tienen que acordarse de hacerlo.
/// </summary>
public class TokenHandler(IHttpContextAccessor accesor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = accesor.HttpContext?.Session.GetString(SesionClaves.Token);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
