using AS.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AS.Infrastructure.EntityConfigurations;

public class DispositivoPushConfiguration : IEntityTypeConfiguration<DispositivoPush>
{
    public void Configure(EntityTypeBuilder<DispositivoPush> entity)
    {
        entity.HasKey(e => e.IdDispositivoPush).HasName("PK__Dispositivo_Push__IdDispositivoPush");

        entity.ToTable("Dispositivo_Push");

        entity.Property(e => e.IdDispositivoPush).HasColumnName("ID_Dispositivo_Push");
        entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
        entity.Property(e => e.Token)
            .HasMaxLength(512)
            .IsUnicode(false)
            .HasColumnName("Token")
            .IsRequired();
        entity.Property(e => e.Plataforma)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("Plataforma")
            .IsRequired();
        entity.Property(e => e.FechaCreacion).HasColumnName("Fecha_Creacion");

        entity.HasIndex(e => e.Token)
            .IsUnique()
            .HasDatabaseName("IX_Dispositivo_Push_Token");

        entity.HasIndex(e => e.IdUsuario)
            .HasDatabaseName("IX_Dispositivo_Push_ID_Usuario");

        entity.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(e => e.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
