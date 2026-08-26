using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.DataAccess.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roller");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RolAdi).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Aciklama).HasMaxLength(200);
        builder.Property(x => x.Aktif).HasDefaultValue(true);

        builder.HasIndex(x => x.RolAdi).IsUnique();
    }
}
