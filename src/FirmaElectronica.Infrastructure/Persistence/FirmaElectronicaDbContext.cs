using FirmaElectronica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FirmaElectronica.Infrastructure.Persistence;

public sealed class FirmaElectronicaDbContext(DbContextOptions<FirmaElectronicaDbContext> options) : DbContext(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<Firmante> Firmantes => Set<Firmante>();
    public DbSet<SolicitudFirma> SolicitudesFirma => Set<SolicitudFirma>();
    public DbSet<EventoAuditoria> EventosAuditoria => Set<EventoAuditoria>();
    public DbSet<UsuarioApi> UsuariosApi => Set<UsuarioApi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(x => x.IdEmpresa);
            entity.Property(x => x.RazonSocial).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CUIT).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Dominio).HasMaxLength(200);
            entity.Property(x => x.LogoUrl).HasMaxLength(500);
            entity.Property(x => x.ColorPrincipal).HasMaxLength(20);
            entity.Property(x => x.Estado).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.CUIT).IsUnique();
        });

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.HasKey(x => x.IdDocumento);
            entity.Property(x => x.IdSistemaOrigen).HasMaxLength(100);
            entity.Property(x => x.IdComprobanteOrigen).HasMaxLength(100);
            entity.Property(x => x.TipoDocumento).HasMaxLength(100).IsRequired();
            entity.Property(x => x.NombreArchivoOriginal).HasMaxLength(260).IsRequired();
            entity.Property(x => x.HashOriginal).HasMaxLength(64).IsRequired();
            entity.Property(x => x.HashFirmado).HasMaxLength(64);
            entity.Property(x => x.Estado).HasMaxLength(40).IsRequired();
            entity.Property(x => x.RutaPdfOriginal).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.RutaPdfFirmado).HasMaxLength(1000);
            entity.Property(x => x.RutaConstancia).HasMaxLength(1000);
            entity.Property(x => x.CreadoPor).HasMaxLength(120);
            entity.Property(x => x.UltimoError).HasMaxLength(1000);
            entity.HasOne(x => x.Empresa)
                .WithMany(x => x.Documentos)
                .HasForeignKey(x => x.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.IdEmpresa, x.IdSistemaOrigen, x.IdComprobanteOrigen });
        });

        modelBuilder.Entity<Firmante>(entity =>
        {
            entity.HasKey(x => x.IdFirmante);
            entity.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CUIT_DNI).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.Celular).HasMaxLength(40);
            entity.Property(x => x.EstadoFirma).HasMaxLength(40).IsRequired();
            entity.Property(x => x.RutaFirmaImagen).HasMaxLength(1000);
            entity.HasOne(x => x.Documento)
                .WithMany(x => x.Firmantes)
                .HasForeignKey(x => x.IdDocumento)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudFirma>(entity =>
        {
            entity.HasKey(x => x.IdSolicitud);
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Estado).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasOne(x => x.Documento)
                .WithMany(x => x.SolicitudesFirma)
                .HasForeignKey(x => x.IdDocumento)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Firmante)
                .WithMany(x => x.SolicitudesFirma)
                .HasForeignKey(x => x.IdFirmante)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EventoAuditoria>(entity =>
        {
            entity.HasKey(x => x.IdEvento);
            entity.Property(x => x.TipoEvento).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();
            entity.Property(x => x.IP).HasMaxLength(80);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
            entity.Property(x => x.HashDocumentoEnEvento).HasMaxLength(64);
            entity.HasOne(x => x.Documento)
                .WithMany(x => x.EventosAuditoria)
                .HasForeignKey(x => x.IdDocumento)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Firmante)
                .WithMany()
                .HasForeignKey(x => x.IdFirmante)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.IdDocumento, x.FechaHoraUTC });
        });

        modelBuilder.Entity<UsuarioApi>(entity =>
        {
            entity.HasKey(x => x.IdUsuarioApi);
            entity.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ApiKeyHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Permisos).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Estado).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.ApiKeyHash).IsUnique();
            entity.HasOne(x => x.Empresa)
                .WithMany(x => x.UsuariosApi)
                .HasForeignKey(x => x.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
