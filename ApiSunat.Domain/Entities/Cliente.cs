namespace ApiSunat.Domain.Entities
{
    public class Cliente
    {
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
        public string TipoDocumentoIdentidad { get; set; }
        public string NumeroDocumentoIdentidad { get; set; }
        public string NombreRazonSocial { get; set; }
        public string? Direccion { get; set; }

        public virtual Empresa Empresa { get; set; }
        public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
    }
}