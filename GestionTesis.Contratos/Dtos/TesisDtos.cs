using System.ComponentModel.DataAnnotations;
using GestionTesis.Contratos.Constantes;

namespace GestionTesis.Contratos.Dtos;

/// <summary>Lo que la API DEVUELVE de una tesis, incluyendo los datos de su tutor.</summary>
public class TesisDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Estudiante { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? ImagenRuta { get; set; }

    public Guid TutorId { get; set; }
    public string TutorNombre { get; set; } = string.Empty;
    public string TutorEspecialidad { get; set; } = string.Empty;
    public string TutorCorreo { get; set; } = string.Empty;
}

/// <summary>Lo que la API RECIBE para crear o editar una tesis.</summary>
public class TesisGuardarDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del estudiante es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El estudiante debe tener entre {2} y {1} caracteres.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü ]+$", ErrorMessage = "El nombre del estudiante solo puede tener letras y espacios.")]
    public string Estudiante { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression(EstadosTesis.Patron, ErrorMessage = "Estado inválido. Use: Propuesta, En desarrollo, Aprobada o Rechazada.")]
    public string Estado { get; set; } = EstadosTesis.Propuesta;

    [Required(ErrorMessage = "Debe elegir un tutor.")]
    public Guid? TutorId { get; set; }
}
