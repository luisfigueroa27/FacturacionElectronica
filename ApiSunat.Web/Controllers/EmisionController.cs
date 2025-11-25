using ApiSunat.Domain.Entities;
using ApiSunat.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ApiSunat.Web.Helpers;
using System.Globalization;
using Rotativa.AspNetCore;

namespace ApiSunat.Web.Controllers
{
    public class EmisionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmisionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearDocumento(
            [FromForm] Documento documento, 

            // Recibimos los datos del cliente
            string TipoDocumentoCliente,
            string NumeroDocumentoCliente,
            string NombreCliente,
            string? DireccionCliente)
        {

            // --- INICIO DE LA SOLUCIÓN ---

            // 1. Limpiamos los errores del modelo principal
            ModelState.Remove("Cliente");
            ModelState.Remove("Empresa");
            ModelState.Remove("ImporteEnLetras");
            ModelState.Remove("TotalOperacionGravada");
            ModelState.Remove("TotalIGV");
            ModelState.Remove("TotalTotal");

            // 2. [NUEVO] Limpiamos los errores de los Detalles
            //    Iteramos por todas las claves de error
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Detalles[")).ToList())
            {
                // Si el error NO es de una de las 3 propiedades que SÍ enviamos...
                if (!key.EndsWith(".Cantidad]") &&
                    !key.EndsWith(".Descripcion]") &&
                    !key.EndsWith(".ValorUnitario]"))
                {
                    // ...entonces es un error de una propiedad calculada (ej. TotalItem, PrecioUnitario).
                    // Lo eliminamos porque no nos importa, lo calcularemos en el backend.
                    ModelState.Remove(key);
                }
            }
            // --- FIN DE LA SOLUCIÓN ---


            // 3. Verificación de ModelState
            // (Ahora SÍ debería ser 'true')
            if (!ModelState.IsValid)
            {
                // Si AÚN falla, es un error real (ej. "abc" en un campo de número).
                // Coloca un breakpoint aquí y revisa 'ModelState.Errors'
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return RedirectToAction(documento.TipoDocumento == "Factura" ? "CrearFactura" : "CrearBoleta", new { empresaId = documento.EmpresaId });
            }

            // 4. Validar Cliente
            if (string.IsNullOrEmpty(NumeroDocumentoCliente) || string.IsNullOrEmpty(NombreCliente))
            {
                return RedirectToAction(documento.TipoDocumento == "Factura" ? "CrearFactura" : "CrearBoleta", new { empresaId = documento.EmpresaId });
            }

            // 5. Lógica del Cliente (Buscar o Crear)
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.EmpresaId == documento.EmpresaId && c.NumeroDocumentoIdentidad == NumeroDocumentoCliente);

            if (cliente == null)
            {
                cliente = new Cliente
                {
                    EmpresaId = documento.EmpresaId,
                    TipoDocumentoIdentidad = TipoDocumentoCliente,
                    NumeroDocumentoIdentidad = NumeroDocumentoCliente,
                    NombreRazonSocial = NombreCliente,
                    Direccion = DireccionCliente
                };
                _context.Clientes.Add(cliente);
            }
            else
            {
                cliente.NombreRazonSocial = NombreCliente;
                cliente.Direccion = DireccionCliente;
                _context.Clientes.Update(cliente);
            }

            documento.Cliente = cliente;


            // 6. Verificación de Detalles
            if (documento.Detalles == null || !documento.Detalles.Any())
            {
                // Si falla aquí, significa que NO agregaste ítems a la venta
                // O que el JavaScript está fallando al crear los hidden inputs
                return RedirectToAction(documento.TipoDocumento == "Factura" ? "CrearFactura" : "CrearBoleta", new { empresaId = documento.EmpresaId });
            }


            // 7. Recálculo de Totales (backend)
            decimal totalGeneral = 0;
            foreach (var item in documento.Detalles)
            {
                // Rellenamos los campos que faltaban y que causaban errores
                item.UnidadMedida = "NIU"; // <-- Asignamos un valor por defecto
                item.PrecioUnitario = item.ValorUnitario * 1.18m;
                item.TotalItem = item.Cantidad * item.PrecioUnitario;
                item.TotalIGVItem = item.TotalItem - (item.Cantidad * item.ValorUnitario);
                totalGeneral += item.TotalItem;
            }

            documento.ImporteTotal = totalGeneral;
            documento.TotalOperacionGravada = totalGeneral / 1.18m;
            documento.TotalIGV = totalGeneral - documento.TotalOperacionGravada;
            documento.ImporteEnLetras = ConversorMoneda.Convertir(documento.ImporteTotal, true, "SOLES", "CÉNTIMOS");


            // 8. Guardar en Base de Datos (con el try/catch activado)
            try
            {
                _context.Documentos.Add(documento);
                await _context.SaveChangesAsync();

                return RedirectToAction("EmisionExitosa", new { documentoId = documento.DocumentoId });
            }
            catch (Exception ex)
            {
                // Si algo falla aquí, te redirigirá.
                // (Puedes comentar el 'try/catch' de nuevo para ver el error de BD)
                return RedirectToAction(documento.TipoDocumento == "Factura" ? "CrearFactura" : "CrearBoleta", new { empresaId = documento.EmpresaId });
            }
        }

        // GET: /Emision/EmisionExitosa?documentoId=123
        // (Tu solicitud: "salga un mensaje de satisfaccion y despues opcion para compartir o impimir")
        public async Task<IActionResult> EmisionExitosa(int documentoId)
        {
            var documento = await _context.Documentos
                .Include(d => d.Empresa)
                .Include(d => d.Cliente)
                .FirstOrDefaultAsync(d => d.DocumentoId == documentoId);

            if (documento == null)
            {
                return NotFound();
            }

            return View(documento);
        }

        // GET: /Emision/_AgregarItemModal?empresaId=1
        // Devuelve el modal para buscar/agregar un ítem a la factura/boleta
        public async Task<IActionResult> _AgregarItemModal(int empresaId)
        {
            // Pasamos el ID de la empresa al modal
            ViewBag.EmpresaId = empresaId;

            // Obtenemos la lista de precios de la empresa para el buscador
            var listaDePrecios = await _context.Productos
                                        .Where(p => p.EmpresaId == empresaId)
                                        .ToListAsync();

            return PartialView("_AgregarItemModal", listaDePrecios);
        }


        // GET: /Emision?empresaId=1
        // Muestra el menú de selección de documentos 
        public async Task<IActionResult> Index(int empresaId)
        {
            var empresa = await _context.Empresas.FindAsync(empresaId);
            if (empresa == null)
            {
                return RedirectToAction("Index", "Empresas");
            }
            // Pasamos la empresa a la vista para saber a nombre de quién emitir
            return View(empresa);
        }

        // GET: /Emision/CrearFactura?empresaId=1
        // Muestra el formulario para emitir Factura 
        public async Task<IActionResult> CrearFactura(int empresaId)
        {
            var empresa = await _context.Empresas.FindAsync(empresaId);
            if (empresa == null)
            {
                return RedirectToAction("Index", "Empresas");
            }

            // --- Lógica para el Correlativo (Número de Factura) ---
            // Buscamos la última factura "F001" de esta empresa
            var ultimoCorrelativo = await _context.Documentos
                .Where(d => d.EmpresaId == empresaId && d.Serie == "F001" && d.TipoDocumento == "Factura")
                .MaxAsync(d => (int?)d.Correlativo) ?? 0;

            // Creamos un nuevo objeto Documento pre-llenado
            var nuevaFactura = new Documento
            {
                EmpresaId = empresa.EmpresaId,
                Empresa = empresa, // Para mostrar datos del emisor
                TipoDocumento = "Factura",
                Serie = "F001",
                Correlativo = ultimoCorrelativo + 1,
                FechaEmision = DateTime.Now,
                Moneda = "PEN",
                FormaPago = "Contado",
                // Pre-llenamos los totales en 0
                TotalOperacionGravada = 0,
                TotalIGV = 0,
                ImporteTotal = 0
            };

            return View(nuevaFactura);
        }

        // GET: /Emision/CrearBoleta?empresaId=1
        // Muestra el formulario para emitir Boleta 
        public async Task<IActionResult> CrearBoleta(int empresaId)
        {
            var empresa = await _context.Empresas.FindAsync(empresaId);
            if (empresa == null)
            {
                return RedirectToAction("Index", "Empresas");
            }

            // --- Lógica para el Correlativo (Número de Boleta) ---
            var ultimoCorrelativo = await _context.Documentos
                .Where(d => d.EmpresaId == empresaId && d.Serie == "B001" && d.TipoDocumento == "Boleta de Venta")
                .MaxAsync(d => (int?)d.Correlativo) ?? 0;

            var nuevaBoleta = new Documento
            {
                EmpresaId = empresa.EmpresaId,
                Empresa = empresa,
                TipoDocumento = "Boleta de Venta",
                Serie = "B001",
                Correlativo = ultimoCorrelativo + 1,
                FechaEmision = DateTime.Now,
                Moneda = "PEN",
                FormaPago = "Contado",
                TotalOperacionGravada = 0,
                TotalIGV = 0,
                ImporteTotal = 0
            };

            return View("CrearFactura", nuevaBoleta); // REUTILIZAMOS LA VISTA de Factura
        }

        // GET: /Emision/ConvertirALetras?monto=171.10
        [HttpGet]
        public IActionResult ConvertirALetras(string monto) // <-- CAMBIO 1: de 'decimal' a 'string'
        {
            try
            {
                // CAMBIO 2: Convertimos manualmente el string a decimal
                // CultureInfo.InvariantCulture le dice a C# que use '.' como separador decimal
                decimal montoDecimal = decimal.Parse(monto, CultureInfo.InvariantCulture);

                string letras = ConversorMoneda.Convertir(montoDecimal, true, "SOLES", "CÉNTIMOS");
                return Ok(new { letras = letras });
            }
            catch (Exception ex)
            {
                // Ahora este 'catch' sí atrapará un error si el 'monto' es inválido
                return BadRequest(ex.Message);
            }
        }

        // GET: /Emision/Imprimir?documentoId=123
        public async Task<IActionResult> Imprimir(int documentoId)
        {
            var documento = await _context.Documentos
                .Include(d => d.Empresa)
                .Include(d => d.Cliente)
                .Include(d => d.Detalles) // ¡MUY IMPORTANTE!
                .FirstOrDefaultAsync(d => d.DocumentoId == documentoId);

            if (documento == null)
            {
                return NotFound();
            }

            // Devolvemos una nueva vista llamada "Imprimir"
            return View("Imprimir", documento);
        }

        // GET: /Emision/ImprimirA4?documentoId=123
        public async Task<IActionResult> ImprimirA4(int documentoId)
        {
            var documento = await _context.Documentos
                .Include(d => d.Empresa)
                .Include(d => d.Cliente)
                .Include(d => d.Detalles) // ¡MUY IMPORTANTE!
                .FirstOrDefaultAsync(d => d.DocumentoId == documentoId);

            if (documento == null)
            {
                return NotFound();
            }

            return View("ImprimirA4", documento);
        }

        // GET: /Emision/DescargarPdfA4?documentoId=123
        public async Task<IActionResult> DescargarPdfA4(int documentoId)
        {
            var documento = await _context.Documentos
                .Include(d => d.Empresa)
                .Include(d => d.Cliente)
                .Include(d => d.Detalles)
                .FirstOrDefaultAsync(d => d.DocumentoId == documentoId);

            if (documento == null)
            {
                return NotFound();
            }

            // Nombre del archivo PDF
            string fileName = $"Factura-{documento.Serie}-{documento.Correlativo}.pdf";

            // Llama a Rotativa para convertir la vista "ImprimirA4" en un PDF
            return new ViewAsPdf("ImprimirA4", documento)
            {
                FileName = fileName,
                PageMargins = { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }

    }
}
