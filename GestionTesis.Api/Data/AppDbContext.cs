using GestionTesis.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionTesis.Api.Data;

/// <summary>
/// El DbContext es el "puente" entre las clases C# y las tablas de SQL Server.
/// Aquí, con Fluent API, definimos largos, índices únicos y relaciones.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tutor> Tutores => Set<Tutor>();
    public DbSet<Tesis> Tesis => Set<Tesis>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----- TUTOR -----
        modelBuilder.Entity<Tutor>(e =>
        {
            e.ToTable("Tutores");
            e.HasKey(t => t.Id);

            e.Property(t => t.Nombre).IsRequired().HasMaxLength(100);
            e.Property(t => t.Especialidad).IsRequired().HasMaxLength(100);
            e.Property(t => t.Correo).IsRequired().HasMaxLength(150);

            // "No puede duplicarse tutor": nombre y correo únicos también en la BD
            e.HasIndex(t => t.Nombre).IsUnique();
            e.HasIndex(t => t.Correo).IsUnique();
        });

        // ----- TESIS -----
        modelBuilder.Entity<Tesis>(e =>
        {
            e.ToTable("Tesis");
            e.HasKey(t => t.Id);

            e.Property(t => t.Titulo).IsRequired().HasMaxLength(200);
            e.Property(t => t.Estudiante).IsRequired().HasMaxLength(100);
            e.Property(t => t.Estado).IsRequired().HasMaxLength(30);
            e.Property(t => t.ImagenRuta).HasMaxLength(300);

            // "No puede duplicarse tesis": título único
            e.HasIndex(t => t.Titulo).IsUnique();

            // Relación 1 Tutor -> N Tesis. Restrict: no se puede borrar un tutor con tesis.
            e.HasOne(t => t.Tutor)
             .WithMany(tu => tu.Tesis)
             .HasForeignKey(t => t.TutorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ----- USUARIO -----
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(u => u.Id);

            e.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            e.Property(u => u.Rol).IsRequired().HasMaxLength(20);

            e.HasIndex(u => u.NombreUsuario).IsUnique();
        });
    }
}
