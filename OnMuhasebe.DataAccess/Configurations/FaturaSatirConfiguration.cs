using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.DataAccess.Configurations;

public class FaturaSatirConfiguration : IEntityTypeConfiguration<FaturaSatir>
{
    public void Configure(EntityTypeBuilder<FaturaSatir> builder)
    {
        builder.ToTable("FaturaSatirlari");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Miktar).HasPrecision(18, 3);
        builder.Property(x => x.BirimFiyat).HasPrecision(18, 2);
        builder.Property(x => x.KdvOrani).HasPrecision(5, 2);
        builder.Property(x => x.SatirTutari).HasPrecision(18, 2);
        builder.Property(x => x.Aktif).HasDefaultValue(true);

        // Satir faturasiz var olamaz: fatura silinirse satirlar da silinir.
        builder.HasOne(x => x.Fatura)
               .WithMany(f => f.Satirlar)
               .HasForeignKey(x => x.FaturaId)
               .OnDelete(DeleteBehavior.Cascade);

        // Fatura satiri olan stok kalici silinemez.
        builder.HasOne(x => x.Stok)
               .WithMany(s => s.FaturaSatirlari)
               .HasForeignKey(x => x.StokId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
