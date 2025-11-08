using ApiSunat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiSunat.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<DocumentoDetalle> DocumentoDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de Fluent API (mejora el mapeo)

            modelBuilder.Entity<Empresa>(e =>
            {
                e.ToTable("Empresas");
                e.HasKey(p => p.EmpresaId);
                e.Property(p => p.Ruc).IsRequired().HasMaxLength(11);
                e.HasIndex(p => p.Ruc).IsUnique();
            });

            modelBuilder.Entity<Cliente>(e =>
            {
                e.ToTable("Clientes");
                e.HasKey(p => p.ClienteId);
                e.Property(p => p.NumeroDocumentoIdentidad).IsRequired().HasMaxLength(15);
                e.HasOne(p => p.Empresa)
                 .WithMany(e => e.Clientes)
                 .HasForeignKey(p => p.EmpresaId);
            });

            modelBuilder.Entity<Producto>(e =>
            {
                e.ToTable("Productos");
                e.HasKey(p => p.ProductoId);
                e.Property(p => p.ValorUnitario).HasColumnType("decimal(18,5)");
                e.HasOne(p => p.Empresa)
                 .WithMany(e => e.Productos)
                 .HasForeignKey(p => p.EmpresaId);
            });

            modelBuilder.Entity<Documento>(e =>
            {
                e.ToTable("Documentos");
                e.HasKey(p => p.DocumentoId);
                e.Property(p => p.TotalOperacionGravada).HasColumnType("decimal(18,2)");
                e.Property(p => p.TotalIGV).HasColumnType("decimal(18,2)");
                e.Property(p => p.ImporteTotal).HasColumnType("decimal(18,2)");

                e.HasOne(p => p.Empresa)
                 .WithMany(e => e.Documentos)
                 .HasForeignKey(p => p.EmpresaId)
                 .OnDelete(DeleteBehavior.Restrict); // Evitar borrado en cascada

                e.HasOne(p => p.Cliente)
                 .WithMany(e => e.Documentos)
                 .HasForeignKey(p => p.ClienteId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DocumentoDetalle>(e =>
            {
                e.ToTable("DocumentoDetalles");
                e.HasKey(p => p.DocumentoDetalleId);
                e.Property(p => p.ValorUnitario).HasColumnType("decimal(18,5)");
                e.Property(p => p.PrecioUnitario).HasColumnType("decimal(18,5)");
                e.Property(p => p.Cantidad).HasColumnType("decimal(18,2)");
                e.Property(p => p.TotalItem).HasColumnType("decimal(18,2)");
                e.Property(p => p.TotalIGVItem).HasColumnType("decimal(18,2)");

                e.HasOne(p => p.Documento)
                 .WithMany(d => d.Detalles)
                 .HasForeignKey(p => p.DocumentoId)
                 .OnDelete(DeleteBehavior.Cascade); // Si se borra el doc, se borra el detalle
            });
        }
    }
}