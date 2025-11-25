using System.Collections.Generic;

namespace ApiSunat.Domain.Entities
{
    public class Empresa
    {
        public int EmpresaId { get; set; }
        public string Ruc { get; set; }
        public string NombreLegal { get; set; }
        public string? NombreComercial { get; set; }
        public string? DireccionFiscal { get; set; }
        public string? LogoUrl { get; set; }
        public bool MostrarEnTicket { get; set; }

        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
        public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
    }
}
