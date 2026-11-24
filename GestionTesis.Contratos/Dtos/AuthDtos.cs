using System.ComponentModel.DataAnnotations;

namespace GestionTesis.Contratos.Dtos;

public class LoginDto
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo {1} caracteres.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegistrarDto
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre {2} y {1} caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9_.]+$", ErrorMessage = "Solo letras, números, punto y guion bajo (sin espacios).")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarPassword { get; set; } = string.Empty;

    /// <summary>Opcional. Si no se envía, el usuario queda como "Usuario". Solo un Administrador puede mandar "Administrador".</summary>
    [RegularExpression("^(Administrador|Usuario)$", ErrorMessage = "Rol inválido.")]
    public string? Rol { get; set; }
}

/// <summary>Respuesta del login: el "pase" (token) y quién es.</summary>
public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
}

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

/// <summary>Formato de error de negocio que devuelve la API (por ejemplo, un 409).</summary>
public class ErrorDto
{
    public string Mensaje { get; set; } = string.Empty;
    /// <summary>Campo al que pertenece el error (para mostrarlo debajo del input). Puede ser null.</summary>
    public string? Campo { get; set; }
}
