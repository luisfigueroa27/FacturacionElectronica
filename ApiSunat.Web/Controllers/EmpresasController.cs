using ApiSunat.Domain.Entities;
using ApiSunat.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ApiSunat.Web.Controllers
{
    public class EmpresasController : Controller
    {
        private readonly ApplicationDbContext _context;

        // 1. Inyectamos el DbContext para hablar con la BD
        public EmpresasController(ApplicationDbContext context)
        {
            _context = context;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Ruc,NombreLegal,NombreComercial,DireccionFiscal,MostrarEnTicket")] Empresa empresa)
        {
            if (ModelState.IsValid)
            {
                // --- INICIO DE LA VALIDACIÓN ---

                // 1. Buscamos en la BD si ya existe una empresa con ese RUC
                var rucExistente = await _context.Empresas
                                            .AnyAsync(e => e.Ruc == empresa.Ruc);

                // 2. Si existe, agregamos un error al modelo y volvemos a la vista
                if (rucExistente)
                {
                    // Agregamos el error al campo "Ruc" para que se muestre junto a él
                    ModelState.AddModelError("Ruc", "La empresa con este RUC ya existe.");
                    return View(empresa); // Devolvemos la vista con el error
                }

                // --- FIN DE LA VALIDACIÓN ---

                // Si el RUC no existe, continuamos normalmente
                _context.Add(empresa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(empresa);
        }
    }
}