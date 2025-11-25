namespace ApiSunat.Domain.Entities
{
    public class DocumentoDetalle
    {
        public int DocumentoDetalleId { get; set; }
        public int DocumentoId { get; set; }
        public int? ProductoId { get; set; } // Opcional
        public string? Codigo { get; set; }
        public string Descripcion { get; set; }
        public string UnidadMedida { get; set; }
        public decimal Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal TotalItem { get; set; }
        public decimal TotalIGVItem { get; set; }

        public virtual Documento Documento { get; set; }
    }
}