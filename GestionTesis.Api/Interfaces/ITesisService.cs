using GestionTesis.Api.Services;
using GestionTesis.Contratos.Dtos;

namespace GestionTesis.Api.Interfaces;

public interface ITesisService
{
    Task<List<TesisDto>> ListarAsync();
    Task<List<TesisDto>> BuscarAsync(string? texto);
    Task<Resultado<TesisDto>> ObtenerAsync(Guid id);
    Task<Resultado<TesisDto>> CrearAsync(TesisGuardarDto dto);
    Task<Resultado<TesisDto>> EditarAsync(Guid id, TesisGuardarDto dto);
    Task<Resultado<bool>> EliminarAsync(Guid id);
    Task<Resultado<TesisDto>> SubirImagenAsync(Guid id, IFormFile? archivo);
}
