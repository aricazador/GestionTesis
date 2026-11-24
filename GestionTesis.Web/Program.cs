using System.Text.Encodings.Web;
using System.Text.Unicode;
using GestionTesis.Web.Infraestructura;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Para que las tildes y la ñ se escriban tal cual en el HTML
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// Session: aquí guardamos el token, el usuario y el rol
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromMinutes(120);
    opciones.Cookie.Name = ".GestionTesis.Sesion";
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// HttpClient con nombre "Api" + TokenHandler que agrega "Authorization: Bearer ..."
builder.Services.AddTransient<TokenHandler>();
builder.Services.AddHttpClient(ApiCliente.Nombre, cliente =>
{
    cliente.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
    cliente.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<TokenHandler>();

var app = builder.Build();

app.UseExceptionHandler("/Error");   // página amigable si la API no responde
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseMiddleware<SesionMiddleware>();   // el "portero": login y permisos por rol

app.MapRazorPages();

app.Run();
