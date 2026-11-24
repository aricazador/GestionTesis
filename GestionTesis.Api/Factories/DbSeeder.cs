using System.Globalization;
using System.Text;
using Bogus;
using GestionTesis.Api.Data;
using GestionTesis.Api.Models;
using GestionTesis.Contratos.Constantes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionTesis.Api.Factories;

public static class DbSeeder
{
    private const int CantidadTutores = 10;
    private const int CantidadTesis = 10;

    private static readonly string[] Especialidades =
    [
        "Ingeniería de Software", "Bases de Datos", "Redes y Telecomunicaciones",
        "Inteligencia Artificial", "Seguridad Informática", "Sistemas de Información"
    ];

    private static readonly string[] Grados = ["Ing.", "Lic.", "MSc.", "Dr."];

    // {0} = tema, {1} = lugar
    private static readonly string[] Plantillas =
    [
        "Sistema web para la gestión de {0} en {1}",
        "Aplicación móvil para el control de {0} en {1}",
        "Base de datos para el análisis de {0} en {1}",
        "Sistema de información para el seguimiento de {0} en {1}"
    ];

    private static readonly string[] Temas =
    [
        "inventarios", "citas médicas", "inscripciones", "ventas", "reservas de canchas",
        "pedidos", "historiales clínicos", "préstamos de libros", "activos fijos", "recursos humanos"
    ];

    private static readonly string[] Lugares =
    [
        "la UPDS Tarija", "el Hospital San Juan de Dios", "la Alcaldía de Tarija", "una farmacia de Cercado",
        "el Mercado Campesino", "una bodega de Uriondo", "una clínica dental", "la Gobernación de Tarija",
        "una cooperativa de San Lorenzo", "una distribuidora de Yacuiba"
    ];

    public static async Task SembrarAsync(AppDbContext db, IPasswordHasher<Usuario> hasher, IWebHostEnvironment env)
    {
        // Semilla fija: siempre se generan los mismos datos
        Randomizer.Seed = new Random(2026);

        if (!await db.Usuarios.AnyAsync())
        {
            db.Usuarios.Add(CrearUsuario(hasher, "admin", "123456", Roles.Administrador));
            db.Usuarios.Add(CrearUsuario(hasher, "usuario", "123456", Roles.Usuario));
            await db.SaveChangesAsync();
        }

        // 2) Tutores y Tesis (solo si no hay tutores)
        if (await db.Tutores.AnyAsync()) return;

        var tutores = CrearTutores();
        var tesis = CrearTesis(tutores, ContarImagenesMuestra(env));

        db.Tutores.AddRange(tutores);
        db.Tesis.AddRange(tesis);
        await db.SaveChangesAsync();
    }

    // 
    private static List<Tutor> CrearTutores()
    {
        var faker = new Faker<Tutor>("es")
            .RuleFor(t => t.Id, f => f.Random.Guid())
            .RuleFor(t => t.Nombre, f => $"{f.PickRandom(Grados)} {SoloLetras(f.Name.FirstName())} {SoloLetras(f.Name.LastName())}")
            .RuleFor(t => t.Especialidad, f => f.PickRandom(Especialidades));

        var lista = new List<Tutor>();
        var nombresUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (lista.Count < CantidadTutores)
        {
            var nuevo = faker.Generate();
            if (!nombresUsados.Add(nuevo.Nombre)) continue;   // regla: no duplicar tutor

            var partes = nuevo.Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            nuevo.Correo = $"{ParaCorreo(partes[1])}.{ParaCorreo(partes[^1])}{lista.Count + 1}@upds.edu.bo";
            lista.Add(nuevo);
        }

        return lista;
    }


    private static List<Tesis> CrearTesis(List<Tutor> tutores, int cantidadImagenes)
    {
        var tesisPorTutor = tutores.ToDictionary(t => t.Id, _ => 0);

        var faker = new Faker<Tesis>("es")
            .RuleFor(t => t.Id, f => f.Random.Guid())
            .RuleFor(t => t.Titulo, f => string.Format(f.PickRandom(Plantillas), f.PickRandom(Temas), f.PickRandom(Lugares)))
            .RuleFor(t => t.Estudiante, f => $"{SoloLetras(f.Name.FirstName())} {SoloLetras(f.Name.LastName())}")
            .RuleFor(t => t.Estado, f => f.PickRandom(EstadosTesis.Todos))
            // Solo elige tutores que todavía tienen cupo (máximo 3 tesis)
            .RuleFor(t => t.TutorId, f => f.PickRandom(tutores.Where(tu => tesisPorTutor[tu.Id] < Reglas.MaximoTesisPorTutor)).Id)
            .RuleFor(t => t.ImagenRuta, f => cantidadImagenes > 0 && f.Random.Bool(0.7f)
                ? $"/imagenes/muestras/portada{f.Random.Int(1, cantidadImagenes)}.png"
                : null);

        var lista = new List<Tesis>();
        var titulosUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (lista.Count < CantidadTesis)
        {
            var nueva = faker.Generate();
            if (!titulosUsados.Add(nueva.Titulo)) continue;   // regla: no duplicar tesis

            tesisPorTutor[nueva.TutorId]++;                   // regla: máximo 3 por tutor
            lista.Add(nueva);
        }

        return lista;
    }

    private static Usuario CrearUsuario(IPasswordHasher<Usuario> hasher, string nombre, string clave, string rol)
    {
        var u = new Usuario { Id = Guid.NewGuid(), NombreUsuario = nombre, Rol = rol };
        u.PasswordHash = hasher.HashPassword(u, clave);   // encriptada, nunca en texto plano
        return u;
    }

   
    private static string SoloLetras(string texto) =>
        new string(texto.Where(c => char.IsAsciiLetter(c) || c == ' ' || "ÁÉÍÓÚáéíóúÑñÜü".Contains(c)).ToArray()).Trim();

    /// <summary>"José Ñuñez" -> "josenunez" (para armar correos sin tildes).</summary>
    private static string ParaCorreo(string texto)
    {
        var sb = new StringBuilder();
        foreach (var c in texto.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

    /// <summary>Cuenta las portadas de muestra (portada1.png, portada2.png, ...) en wwwroot/imagenes/muestras.</summary>
    private static int ContarImagenesMuestra(IWebHostEnvironment env)
    {
        var raiz = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var carpeta = Path.Combine(raiz, "imagenes", "muestras");
        if (!Directory.Exists(carpeta)) return 0;

        var n = 0;
        while (File.Exists(Path.Combine(carpeta, $"portada{n + 1}.png"))) n++;
        return n;
    }
}