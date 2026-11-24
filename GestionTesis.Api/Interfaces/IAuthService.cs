using GestionTesis.Api.Services;
using GestionTesis.Contratos.Dtos;

namespace GestionTesis.Api.Interfaces;

public interface IAuthService
{
    Task<Resultado<TokenDto>> LoginAsync(LoginDto dto);
    Task<Resultado<UsuarioDto>> RegistrarAsync(RegistrarDto dto, bool quienLlamaEsAdmin);
    Task<List<UsuarioDto>> ListarUsuariosAsync();
}
