# Gestión de Tesis (consigna B) - ASP.NET Core .NET 10

Solución con 3 proyectos:

| Proyecto | Tipo | Qué hace |
|---|---|---|
| `GestionTesis.Api` | ASP.NET Core Web API | Base de datos (EF Core + SQL Server), reglas de negocio, JWT, Swagger, imágenes |
| `GestionTesis.Contratos` | Biblioteca de clases | DTOs con Data Annotations y constantes que comparten la API y la Web |
| `GestionTesis.Web` | Razor Pages | Pantallas (Bootstrap 5, color magenta) que consumen la API |

## Requisitos
- Visual Studio 2022 17.14+ o Visual Studio 2026, con la carga de trabajo **ASP.NET y desarrollo web**.
- .NET 10 SDK.
- SQL Server **LocalDB** (`(localdb)\MSSQLLocalDB`). Viene con Visual Studio.
- Internet la primera vez: para restaurar los paquetes NuGet y cargar Bootstrap/GLightbox por CDN.

## Pasos para ejecutar
1. Descomprimí el ZIP y abrí **`GestionTesis.slnx`** con Visual Studio.
2. Esperá a que termine "Restaurando paquetes NuGet" (abajo a la izquierda).
3. Clic derecho en la **Solución** → **Configurar proyectos de inicio...**
   - Elegí **Varios proyectos de inicio**.
   - `GestionTesis.Api` → **Iniciar**
   - `GestionTesis.Web` → **Iniciar**
   - `GestionTesis.Contratos` → Ninguna
   - Usá las flechas para dejar **la API arriba**, así arranca primero. Luego **Aceptar**.
4. Presioná **F5** (o el botón Iniciar).
   - Si Visual Studio pregunta por el **certificado HTTPS de desarrollo**, respondé **Sí** a todo.
5. Se abren dos pestañas:
   - **Swagger** de la API: https://localhost:7101/swagger
   - **Web**: https://localhost:7102
6. Entrá a la Web con:
   - **admin / 123456**: Administrador (puede crear, editar y eliminar).
   - **usuario / 123456**: Usuario (solo puede ver).

> **No hace falta `Update-Database`.** Al arrancar, la API ejecuta `context.Database.Migrate()`: crea la base `GestionTesisDb` y aplica la migración. Después la llena con datos de prueba (Bogus) si las tablas están vacías.

## Tablas

**Tutor** (`Tutores`)
- Id (Guid)
- Nombre: 100, **único**
- Especialidad: 100
- Correo: 150, **único**

**Tesis** (`Tesis`)
- Id (Guid)
- Titulo: 200, **único**
- Estudiante: 100
- Estado: 30
- ImagenRuta: 300, opcional
- TutorId: FK → Tutores, `ON DELETE RESTRICT`

**Usuario** (`Usuarios`)
- Id (Guid)
- NombreUsuario: 50, **único**
- PasswordHash
- Rol

## Reglas de la consigna (dónde están)

| Regla | Dónde | Respuesta |
|---|---|---|
| 1) POST Tutor | `POST api/tutores` | 201 |
| 2) POST Tesis | `POST api/tesis` | 201 |
| 3) Buscar tesis y mostrar tutor | `GET api/tesis/buscar?texto=...` (trae nombre, especialidad y correo del tutor) | 200 |
| 4) Subir imágenes | `POST api/tesis/{id}/imagen` (multipart, campo `archivo`) → `wwwroot/imagenes` | 200 / 400 |
| 5) Frontend magenta | `GestionTesis.Web/wwwroot/css/site.css` | - |
| No duplicar tutor | `TutorService` (nombre y correo) + índice único en la BD | 409 |
| No duplicar tesis | `TesisService` (título) + índice único en la BD | 409 |
| Tutor máximo 3 tesis | `TesisService.ValidarAsync` (al crear y al cambiar de tutor) | 409 |
| No borrar tutor con tesis | `TutorService.EliminarAsync` + `DeleteBehavior.Restrict` | 409 |

## Probar en Swagger
1. `POST /api/auth/login` con `{ "nombreUsuario": "admin", "password": "123456" }`.
2. Copiá el `token` de la respuesta.
3. Clic en **Authorize** (el candado) y pegá **solo el token**.
4. Ya podés probar todos los endpoints.
   - Sin token → **401**.
   - Con el token de `usuario` intentando un POST → **403**.

## Si algo falla
- **"No se pudo conectar con la API"** en la Web: la API no está corriendo. Revisá el paso 3 (varios proyectos de inicio).
- **Error con la migración**:
  1. Borrá la carpeta `GestionTesis.Api/Migrations`.
  2. En la *Consola del Administrador de paquetes*, con el proyecto predeterminado `GestionTesis.Api`, ejecutá `Add-Migration Inicial`.
  3. Volvé a ejecutar.
- **Empezar con la base limpia**: en el *Explorador de objetos de SQL Server* borrá `GestionTesisDb` y ejecutá de nuevo. Se recrea sola.
- **Cambiar los puertos**: API en `GestionTesis.Api/Properties/launchSettings.json`. Si la cambiás, actualizá `ApiBaseUrl` en `GestionTesis.Web/appsettings.json`.
