using System.ComponentModel.DataAnnotations;

namespace GestionTesis.Contratos.Dtos;

/// <summary>Lo que la API DEVUELVE cuando pedimos un tutor.</summary>
public class TutorDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    /// <summary>Cuántas tesis tiene asignadas (máximo 3).</summary>
    public int CantidadTesis { get; set; }
}

/// <summary>Lo que la API RECIBE para crear o editar un tutor (con sus validaciones).</summary>
public class TutorGuardarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü. ]+$", ErrorMessage = "El nombre solo puede tener letras, espacios y puntos.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "La especialidad debe tener entre {2} y {1} caracteres.")]
    public string Especialidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede pasar de {1} caracteres.")]
    public string Correo { get; set; } = string.Empty;
}
