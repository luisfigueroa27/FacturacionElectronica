namespace ApiSunat.Domain.Entities
{
    public class Producto
    {
        public int ProductoId { get; set; }
        public int EmpresaId { get; set; }
        public string? Codigo { get; set; }
        public string Descripcion { get; set; }
        public string UnidadMedida { get; set; }
        public decimal ValorUnitario { get; set; }
        public string TipoOperacion { get; set; }
        public bool ImpuestoBolsas { get; set; }

        public virtual Empresa Empresa { get; set; }
    }
}