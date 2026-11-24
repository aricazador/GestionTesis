using GestionTesis.Api.Services;
using GestionTesis.Contratos.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GestionTesis.Api.Controllers;

/// <summary>Controlador base: traduce el error del servicio al código HTTP correcto.</summary>
[ApiController]
public abstract class ApiBaseController : ControllerBase
{
    protected IActionResult Fallo<T>(Resultado<T> r)
    {
        var cuerpo = new ErrorDto { Mensaje = r.Mensaje, Campo = r.Campo };
        return r.Error switch
        {
            TipoError.Invalido => BadRequest(cuerpo),                                     // 400
            TipoError.NoAutorizado => Unauthorized(cuerpo),                               // 401
            TipoError.Prohibido => StatusCode(StatusCodes.Status403Forbidden, cuerpo),     // 403
            TipoError.NoEncontrado => NotFound(cuerpo),                                   // 404
            TipoError.Conflicto => Conflict(cuerpo),                                      // 409
            _ => StatusCode(StatusCodes.Status500InternalServerError, cuerpo)
        };
    }
}
