using ApiSunat.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApiSunat.Web.Controllers
{
    public class DocumentosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Documentos?empresaId=1&fechaInicio=...&fechaFin=...
        public async Task<IActionResult> Index(int empresaId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            // 1. Buscamos la empresa
            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e => e.EmpresaId == empresaId);

            if (empresa == null)
            {
                return RedirectToAction("Index", "Empresas");
            }

            // 2. Creamos la consulta base de documentos para esa empresa
            IQueryable<ApiSunat.Domain.Entities.Documento> query = _context.Documentos
                .Where(d => d.EmpresaId == empresaId)
                .Include(d => d.Cliente) // <-- ¡Muy importante para mostrar el nombre del cliente!
                .OrderByDescending(d => d.FechaEmision)
                .ThenByDescending(d => d.Correlativo);

            // 3. Aplicamos los filtros de fecha si existen
            if (fechaInicio.HasValue)
            {
                query = query.Where(d => d.FechaEmision.Date >= fechaInicio.Value.Date);
            }
            if (fechaFin.HasValue)
            {
                query = query.Where(d => d.FechaEmision.Date <= fechaFin.Value.Date);
            }

            // 4. Pasamos los datos a la vista
            ViewBag.Empresa = empresa; // Pasamos la empresa para mostrar su logo/nombre
            ViewBag.FechaInicio = fechaInicio; // Para rellenar los campos de filtro
            ViewBag.FechaFin = fechaFin;

            // 5. Ejecutamos la consulta y pasamos la lista de documentos como Modelo
            var documentos = await query.ToListAsync();
            return View(documentos);
        }
    }
}