using System.Text;
using GestionTesis.Api.Data;
using GestionTesis.Api.Factories;
using GestionTesis.Api.Interfaces;
using GestionTesis.Api.Models;
using GestionTesis.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1) Base de datos (EF Core + SQL Server) ----------
builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            // Evita que Migrate() se detenga por diferencias mínimas del snapshot
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

// ---------- 2) Inyección de dependencias: Interfaz -> Servicio ----------
builder.Services.AddScoped<ITutorService, TutorService>();
builder.Services.AddScoped<ITesisService, TesisService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// ---------- 3) Autenticación JWT ----------
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false; // usamos los nombres cortos "name" y "role" tal cual
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            NameClaimType = "name",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();

// ---------- 4) Swagger con botón Authorize (candado) - sintaxis Swashbuckle 10 ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gestión de Tesis (UPDS)",
        Version = "v1",
        Description = "Tesis y Tutores. Primero haga login en /api/auth/login y pegue el token en Authorize."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue SOLO el token (sin escribir 'Bearer')."
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

// ---------- 5) Crear/actualizar la BD y sembrar datos al arrancar ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // crea la base si no existe y aplica migraciones (no hace falta Update-Database)

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
    await DbSeeder.SembrarAsync(db, hasher, app.Environment);
}

// ---------- 6) Pipeline HTTP ----------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gestión de Tesis v1");
    c.DocumentTitle = "API Gestión de Tesis";
});

app.UseHttpsRedirection();
app.UseStaticFiles();       // sirve wwwroot/imagenes para que la Web muestre las fotos

app.UseAuthentication();    // ¿quién sos? (lee el token)
app.UseAuthorization();     // ¿qué podés hacer? (revisa roles)

app.MapControllers();

app.Run();
