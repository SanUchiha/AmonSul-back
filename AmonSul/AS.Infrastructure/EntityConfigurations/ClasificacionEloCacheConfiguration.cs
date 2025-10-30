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
        entity.Property(e => e.Email)
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("Email")
            .IsRequired();
        entity.Property(e => e.Nick)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("Nick")
            .IsRequired();
        entity.Property(e => e.IdFaccion).HasColumnName("ID_Faccion");
        entity.Property(e => e.Elo).HasColumnName("Elo");
        entity.Property(e => e.Partidas).HasColumnName("Partidas");
        entity.Property(e => e.Ganadas).HasColumnName("Ganadas");
        entity.Property(e => e.Empatadas).HasColumnName("Empatadas");
        entity.Property(e => e.Perdidas).HasColumnName("Perdidas");
        entity.Property(e => e.NumeroPartidasJugadas).HasColumnName("Numero_Partidas_Jugadas");
        entity.Property(e => e.AnioClasificacion).HasColumnName("Anio_Clasificacion");
        entity.Property(e => e.FechaActualizacion)
            .HasColumnName("Fecha_Actualizacion")
            .HasDefaultValueSql("GETDATE()");
        entity.Property(e => e.Activo)
            .HasColumnName("Activo")
            .HasDefaultValue(true);

        // Relaciones
        entity.HasOne(d => d.Usuario)
            .WithMany()
            .HasForeignKey(d => d.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ClasificacionEloCache_Usuario");

        entity.HasOne(d => d.Faccion)
            .WithMany()
            .HasForeignKey(d => d.IdFaccion)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_ClasificacionEloCache_Faccion");

        // Índices
        entity.HasIndex(e => new { e.IdUsuario, e.AnioClasificacion })
            .IsUnique()
            .HasDatabaseName("IX_ClasificacionEloCache_Usuario_Anio");

        entity.HasIndex(e => e.AnioClasificacion)
            .HasDatabaseName("IX_ClasificacionEloCache_Anio");

        entity.HasIndex(e => e.FechaActualizacion)
            .HasDatabaseName("IX_ClasificacionEloCache_FechaActualizacion");
    }
}