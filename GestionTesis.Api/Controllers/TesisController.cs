using GestionTesis.Api.Interfaces;
using GestionTesis.Contratos.Constantes;
using GestionTesis.Contratos.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTesis.Api.Controllers;

/// <summary>Endpoints de Tesis (ruta: api/tesis).</summary>
[Route("api/[controller]")]
[Authorize]
public class TesisController(ITesisService servicio) : ApiBaseController
{
    // GET api/tesis
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await servicio.ListarAsync());

    // GET api/tesis/buscar?texto=redes   (punto 3: buscar tesis y mostrar tutor)
    [HttpGet("buscar")]
    [ProducesResponseType<List<TesisDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] string? texto) => Ok(await servicio.BuscarAsync(texto));

    // GET api/tesis/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtener(Guid id)
    {
        var r = await servicio.ObtenerAsync(id);
        return r.Exito ? Ok(r.Datos) : Fallo(r);
    }

    // POST api/tesis  (punto 2 de la consigna)
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType<TesisDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorDto>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] TesisGuardarDto dto)
    {
        var r = await servicio.CrearAsync(dto);
        return r.Exito ? CreatedAtAction(nameof(Obtener), new { id = r.Datos!.Id }, r.Datos) : Fallo(r);
    }

    // PUT api/tesis/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Editar(Guid id, [FromBody] TesisGuardarDto dto)
    {
        var r = await servicio.EditarAsync(id, dto);
        return r.Exito ? Ok(r.Datos) : Fallo(r);
    }

    // DELETE api/tesis/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var r = await servicio.EliminarAsync(id);
        return r.Exito ? Ok(new { mensaje = "Tesis eliminada." }) : Fallo(r);
    }

    // POST api/tesis/{id}/imagen   (punto 4: subir imágenes) - multipart/form-data, campo "archivo"
    [HttpPost("{id:guid}/imagen")]
    [Authorize(Roles = Roles.Administrador)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> SubirImagen(Guid id, IFormFile archivo)
    {
        var r = await servicio.SubirImagenAsync(id, archivo);
        return r.Exito ? Ok(r.Datos) : Fallo(r);
    }
}
