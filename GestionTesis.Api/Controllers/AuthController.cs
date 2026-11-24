using GestionTesis.Api.Interfaces;
using GestionTesis.Contratos.Constantes;
using GestionTesis.Contratos.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTesis.Api.Controllers;

/// <summary>Login, registro y lista de usuarios.</summary>
[Route("api/[controller]")]
[Authorize]
public class AuthController(IAuthService servicio) : ApiBaseController
{
    // POST api/auth/login -> 200 con token, o 401
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<TokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorDto>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var r = await servicio.LoginAsync(dto);
        return r.Exito ? Ok(r.Datos) : Fallo(r);
    }

    // POST api/auth/registrar -> 201. Anónimo = queda como "Usuario". Para crear Administrador hay que enviar token de admin.
    [HttpPost("registrar")]
    [AllowAnonymous]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorDto>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorDto>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarDto dto)
    {
        var esAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole(Roles.Administrador);
        var r = await servicio.RegistrarAsync(dto, esAdmin);
        return r.Exito ? StatusCode(StatusCodes.Status201Created, r.Datos) : Fallo(r);
    }

    // GET api/auth/usuarios -> solo admin
    [HttpGet("usuarios")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Usuarios() => Ok(await servicio.ListarUsuariosAsync());
}
