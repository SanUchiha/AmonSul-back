using AS.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AS.Infrastructure.EntityConfigurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.HasKey(e => e.IdRefreshToken).HasName("PK__Refresh_Token__IdRefreshToken");

        entity.ToTable("Refresh_Token");

        entity.Property(e => e.IdRefreshToken).HasColumnName("ID_Refresh_Token");
        entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
        entity.Property(e => e.Hash)
            .HasMaxLength(64)
            .IsUnicode(false)
            .HasColumnName("Hash")
            .IsRequired();
        entity.Property(e => e.Caducidad).HasColumnName("Caducidad");
        entity.Property(e => e.FechaCreacion).HasColumnName("Fecha_Creacion");

        entity.HasIndex(e => e.Hash)
            .IsUnique()
            .HasDatabaseName("IX_Refresh_Token_Hash");

        entity.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(e => e.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
