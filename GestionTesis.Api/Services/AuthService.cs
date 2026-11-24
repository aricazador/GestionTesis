using System.Security.Claims;
using System.Text;
using GestionTesis.Api.Data;
using GestionTesis.Api.Interfaces;
using GestionTesis.Api.Models;
using GestionTesis.Contratos.Constantes;
using GestionTesis.Contratos.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GestionTesis.Api.Services;

/// <summary>Login (genera el token JWT) y registro de usuarios con contraseña encriptada.</summary>
public class AuthService(AppDbContext db, IPasswordHasher<Usuario> hasher, IConfiguration config) : IAuthService
{
    public async Task<Resultado<TokenDto>> LoginAsync(LoginDto dto)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == dto.NombreUsuario.Trim());

        // Mismo mensaje para "no existe" y "clave incorrecta" (no damos pistas a un atacante)
        if (usuario is null ||
            hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
        {
            return Resultado<TokenDto>.Falla(TipoError.NoAutorizado, "Usuario o contraseña incorrectos.");
        }

        return Resultado<TokenDto>.Ok(GenerarToken(usuario));
    }

    public async Task<Resultado<UsuarioDto>> RegistrarAsync(RegistrarDto dto, bool quienLlamaEsAdmin)
    {
        var rol = string.IsNullOrWhiteSpace(dto.Rol) ? Roles.Usuario : dto.Rol;

        // Regla: solo un Administrador puede crear administradores
        if (rol == Roles.Administrador && !quienLlamaEsAdmin)
            return Resultado<UsuarioDto>.Falla(TipoError.Prohibido, "Solo un Administrador puede crear administradores.", "Rol");

        var nombre = dto.NombreUsuario.Trim();
        if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == nombre))
            return Resultado<UsuarioDto>.Falla(TipoError.Conflicto, "Ese nombre de usuario ya existe.", "NombreUsuario");

        var usuario = new Usuario { Id = Guid.NewGuid(), NombreUsuario = nombre, Rol = rol };
        usuario.PasswordHash = hasher.HashPassword(usuario, dto.Password);

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        return Resultado<UsuarioDto>.Ok(new UsuarioDto { Id = usuario.Id, NombreUsuario = usuario.NombreUsuario, Rol = usuario.Rol });
    }

    public async Task<List<UsuarioDto>> ListarUsuariosAsync() =>
        await db.Usuarios.AsNoTracking()
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioDto { Id = u.Id, NombreUsuario = u.NombreUsuario, Rol = u.Rol })
            .ToListAsync();

    private TokenDto GenerarToken(Usuario usuario)
    {
        var jwt = config.GetSection("Jwt");
        var expira = DateTime.UtcNow.AddMinutes(jwt.GetValue("ExpiraMinutos", 120));
        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt["Issuer"],
            Audience = jwt["Audience"],
            Expires = expira,
            SigningCredentials = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256),
            // Claims = "datos" que viajan dentro del token. El rol es el importante para [Authorize(Roles=...)]
            Subject = new ClaimsIdentity(new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new("name", usuario.NombreUsuario),
                new("role", usuario.Rol)
            })
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new TokenDto
        {
            Token = token,
            NombreUsuario = usuario.NombreUsuario,
            Rol = usuario.Rol,
            Expira = expira
        };
    }
}
