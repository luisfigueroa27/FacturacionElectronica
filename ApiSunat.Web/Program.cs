using ApiSunat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. Registrar el DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        // 3. Indicarle dónde buscar las migraciones
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

var env = app.Services.GetRequiredService<IWebHostEnvironment>();

RotativaConfiguration.Setup(env.WebRootPath);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();