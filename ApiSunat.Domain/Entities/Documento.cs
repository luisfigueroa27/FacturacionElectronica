using System;
using System.Collections.Generic;

namespace ApiSunat.Domain.Entities
{
    public class Documento
    {
        public int DocumentoId { get; set; }
        public int EmpresaId { get; set; }
        public int ClienteId { get; set; }
        public string TipoDocumento { get; set; }
        public string Serie { get; set; }
        public int Correlativo { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Moneda { get; set; }
        public string FormaPago { get; set; }
        public decimal TotalOperacionGravada { get; set; }
        public decimal TotalIGV { get; set; }
        public decimal ImporteTotal { get; set; }
        public string ImporteEnLetras { get; set; }
        public string? Observaciones { get; set; }
        public string? EstadoSunat { get; set; }
        public string? HashCdr { get; set; }

        public virtual Empresa Empresa { get; set; }
        public virtual Cliente Cliente { get; set; }
        public virtual ICollection<DocumentoDetalle> Detalles { get; set; } = new List<DocumentoDetalle>();
    }
}