using GestionTesis.Api.Services;
using GestionTesis.Contratos.Dtos;

namespace GestionTesis.Api.Interfaces;

/// <summary>El "contrato" de lo que sabe hacer el servicio de tutores.</summary>
public interface ITutorService
{
    Task<List<TutorDto>> ListarAsync();
    Task<Resultado<TutorDto>> ObtenerAsync(Guid id);
    Task<Resultado<TutorDto>> CrearAsync(TutorGuardarDto dto);
    Task<Resultado<TutorDto>> EditarAsync(Guid id, TutorGuardarDto dto);
    Task<Resultado<bool>> EliminarAsync(Guid id);
}
