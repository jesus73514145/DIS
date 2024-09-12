using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using proyecto.Data;
using proyecto.Models;
using DinkToPdf;
using DinkToPdf.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = Environment.GetEnvironmentVariable("RENDER_POSTGRES_CONNECTION") ??
                       builder.Configuration.GetConnectionString("PostgresSQLConnection") ??
                       throw new InvalidOperationException("Connection string 'PostgresSQLConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configuración de la autenticación y autorización
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configura la cookie de autenticación para redirigir a la página de inicio de sesión
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Ruta para la página de inicio de sesión
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Ruta para acceso denegado
    options.LogoutPath = "/Identity/Account/Logout"; // Ruta para cerrar sesión
});

builder.Services.AddTransient<IMyEmailSender, EmailSender>(i =>
    new EmailSender(
        builder.Configuration["Email:SmtpServer"],
        int.Parse(builder.Configuration["Email:SmtpPort"]),
        builder.Configuration["Email:SmtpUsername"],
        builder.Configuration["Email:SmtpPassword"]
    )
);

// Agregar servicios de Health Checks
builder.Services.AddHealthChecks();

// Registro del convertidor de DinkToPdf
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Asegúrate de incluir esto
app.UseAuthorization();  // Asegúrate de incluir esto

app.MapControllerRoute(
    name: "custom",
    pattern: "{company}/{controller}/{action}/{id?}",
    defaults: new { company = "Textil", controller = "Home", action = "Index" });

app.MapRazorPages();

app.MapHealthChecks("/health");

app.Run();



