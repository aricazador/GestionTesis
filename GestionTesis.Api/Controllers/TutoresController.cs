using GestionTesis.Api.Interfaces;
using GestionTesis.Contratos.Constantes;
using GestionTesis.Contratos.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTesis.Api.Controllers;

/// <summary>Endpoints de Tutores. Cualquiera logueado puede VER; solo el Administrador modifica.</summary>
[Route("api/[controller]")]
[Authorize]
public class TutoresController(ITutorService servicio) : ApiBaseController
{
    // GET api/tutores
    [HttpGet]
    [ProducesResponseType<List<TutorDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar() => Ok(await servicio.ListarAsync());

    // GET api/tutores/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtener(Guid id)
    {
        var r = await servicio.ObtenerAsync(id);
        return r.Exito ? Ok(r.Datos) : Fallo(r);
    }

    // POST api/tutores  (punto 1 de la consigna)
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<TutorDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorDto>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] TutorGuardarDto dto)
    {
        var r = await servicio.CrearAsync(dto);
        return r.Exito ? CreatedAtAction(nameof(Obtener), new { id = r.Datos!.Id }, r.Datos) : Fallo(r);
    }

    // PUT api/tutores/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Editar(Guid id, [FromBody] TutorGuardarDto dto)
    {
        var r = await servicio.EditarAsync(id, dto);
        return r.Exito ? Ok(r.Datos) : Fallo(r);
    }

    // DELETE api/tutores/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var r = await servicio.EliminarAsync(id);
        return r.Exito ? Ok(new { mensaje = "Tutor eliminado." }) : Fallo(r);
    }
}
