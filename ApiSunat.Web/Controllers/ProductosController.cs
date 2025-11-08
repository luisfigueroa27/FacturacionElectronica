using ApiSunat.Domain.Entities;
using ApiSunat.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ApiSunat.Web.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: /Productos?empresaId=1  (Muestra la Lista de Precios)
        public async Task<IActionResult> Index(int empresaId)
        {
            // Buscamos la empresa y nos "traemos" (Include) su lista de productos
            var empresa = await _context.Empresas
                .Include(e => e.Productos) // <-- Carga los productos asociados
                .FirstOrDefaultAsync(e => e.EmpresaId == empresaId);

            if (empresa == null)
            {
                // Si no se encuentra la empresa, volvemos al inicio
                return RedirectToAction("Index", "Empresas");
            }

            return View(empresa); // Pasamos el objeto Empresa completo a la vista
        }

        // 2. GET: /Productos/_ProductoModal?empresaId=1&productoId=5 
        // (Devuelve el HTML del formulario para el modal)
        public async Task<IActionResult> _ProductoModal(int empresaId, int? productoId = null)
        {
            if (productoId == null)
            {
                // Es un CREATE (Agregar ítem)
                var newProducto = new Producto { EmpresaId = empresaId, UnidadMedida = "NIU" };
                return PartialView("_ProductoModal", newProducto);
            }

            // Es un EDIT (Editar ítem)
            var producto = await _context.Productos.FindAsync(productoId.Value);
            if (producto == null)
            {
                return NotFound();
            }
            return PartialView("_ProductoModal", producto);
        }

        // 3. POST: /Productos/Save
        // (Recibe los datos del modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Producto producto)
        {
            // Ocultamos el EmpresaId del formulario para que no sea modificado
            ModelState.Remove("Empresa");

            if (ModelState.IsValid)
            {
                if (producto.ProductoId == 0)
                {
                    // Es un CREATE nuevo
                    _context.Add(producto);
                }
                else
                {
                    // Es un EDIT
                    _context.Update(producto);
                }
                await _context.SaveChangesAsync();
            }

            // Redirigimos de vuelta a la lista de precios de esa empresa
            return RedirectToAction(nameof(Index), new { empresaId = producto.EmpresaId });
        }

        // 4. POST: /Productos/Delete
        // (Para el botón de eliminar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int productoId)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                return NotFound();
            }

            var empresaId = producto.EmpresaId; // Guardamos el Id para la redirección
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { empresaId = empresaId });
        }
    }
}