namespace GestionTesis.Api.Models;

/// <summary>Usuario que inicia sesión. La contraseña NUNCA se guarda en texto plano, solo su hash.</summary>
public class Usuario
{
    public Guid Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
