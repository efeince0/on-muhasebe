using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.DataAccess.Configurations;

public class StokConfiguration : IEntityTypeConfiguration<Stok>
{
    public void Configure(EntityTypeBuilder<Stok> builder)
    {
        builder.ToTable("Stoklar");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StokKodu).HasMaxLength(20).IsRequired();
        builder.Property(x => x.StokAdi).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Kategori).HasMaxLength(50);
        builder.Property(x => x.Birim).HasMaxLength(15).IsRequired();
        builder.Property(x => x.AlisFiyati).HasPrecision(18, 2);
        builder.Property(x => x.SatisFiyati).HasPrecision(18, 2);
        builder.Property(x => x.KdvOrani).HasPrecision(5, 2);
        builder.Property(x => x.KritikStok).HasPrecision(18, 3);
        builder.Property(x => x.Aktif).HasDefaultValue(true);

        builder.HasIndex(x => x.StokKodu).IsUnique();
    }
}
