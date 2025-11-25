using ApiSunat.Domain.Entities;
using ApiSunat.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ApiSunat.Web.Controllers
{
    public class EmpresasController : Controller
    {
        private readonly ApplicationDbContext _context;
        // 1. Declarar la variable del entorno
        private readonly IWebHostEnvironment _hostEnvironment;

        // . Actualizar el constructor para inyectarlo
        public EmpresasController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment; // Inicializar la variable
        }

        // 2. GET: /Empresas (o /Empresas/Index)
        // Muestra la página principal "Mis Empresas" (Imagen 1)
        public async Task<IActionResult> Index()
        {
            // Busca todas las empresas en la BD y las envía a la vista
            var empresas = await _context.Empresas.ToListAsync();
            return View(empresas);
        }

        // 3. GET: /Empresas/Create
        // Muestra el formulario "Nueva Empresa" (Imagen 2)
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Empresas/Create
        // Recibe los datos del formulario "Nueva Empresa"
        // POST: Empresas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. Agregamos el parámetro 'logoFile' que coincide con el 'name' del input en la vista
        public async Task<IActionResult> Create([FromForm] Empresa empresa, IFormFile? logoFile)
        {
            // Removemos LogoUrl del ModelState porque lo vamos a llenar nosotros, no el usuario directamente
            ModelState.Remove("LogoUrl");

            if (ModelState.IsValid)
            {
                // --- INICIO LÓGICA DE SUBIDA DE IMAGEN ---
                if (logoFile != null && logoFile.Length > 0)
                {
                    // a. Definir la ruta donde se guardará (wwwroot/images/logos)
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string uploadsFolder = Path.Combine(wwwRootPath, "images", "logos");

                    // b. Asegurar que el directorio exista
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // c. Generar un nombre de archivo único para evitar duplicados (usando GUID)
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(logoFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // d. Guardar el archivo en el servidor
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(fileStream);
                    }

                    // e. Guardar la RUTA RELATIVA en la base de datos
                    empresa.LogoUrl = "/images/logos/" + uniqueFileName;
                }
                // --- FIN LÓGICA DE SUBIDA DE IMAGEN ---

                _context.Add(empresa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(empresa);
        }
    }
}