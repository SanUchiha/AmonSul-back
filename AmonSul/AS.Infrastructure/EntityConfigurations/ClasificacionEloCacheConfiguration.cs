using AS.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AS.Infrastructure.EntityConfigurations;

public class ClasificacionEloCacheConfiguration : IEntityTypeConfiguration<ClasificacionEloCache>
{
    public void Configure(EntityTypeBuilder<ClasificacionEloCache> entity)
    {
        entity.HasKey(e => e.IdClasificacion).HasName("PK__ClasificacionEloCache__IdClasificacion");

        entity.ToTable("Clasificacion_Elo_Cache");

        entity.Property(e => e.IdClasificacion).HasColumnName("ID_Clasificacion");
        entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
        entity.Property(e => e.Nick)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("Nick")
            .IsRequired();
        entity.Property(e => e.IdFaccion).HasColumnName("ID_Faccion");
        entity.Property(e => e.Elo).HasColumnName("Elo");

        // Índices para rendimiento en consultas de lectura
        entity.HasIndex(e => e.Elo)
            .HasDatabaseName("IX_ClasificacionEloCache_Elo");
    }
}