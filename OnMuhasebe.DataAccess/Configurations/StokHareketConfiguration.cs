using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.DataAccess.Configurations;

public class StokHareketConfiguration : IEntityTypeConfiguration<StokHareket>
{
    public void Configure(EntityTypeBuilder<StokHareket> builder)
    {
        // Giris ve Cikis miktari pozitif olmali; Sayim farki negatif olabilir.
        builder.ToTable("StokHareketleri", t =>
            t.HasCheckConstraint("CK_StokHareketleri_Miktar",
                "[HareketTipi] = 'Sayim' OR [Miktar] > 0"));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.HareketNo).HasMaxLength(20).IsRequired();
        builder.Property(x => x.HareketTipi).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(x => x.Miktar).HasPrecision(18, 3);
        builder.Property(x => x.Tarih).HasColumnType("date");
        builder.Property(x => x.Aciklama).HasMaxLength(250);
        builder.Property(x => x.Aktif).HasDefaultValue(true);

        builder.HasIndex(x => x.HareketNo).IsUnique();

        builder.HasOne(x => x.Stok)
               .WithMany(s => s.Hareketler)
               .HasForeignKey(x => x.StokId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Fatura)
               .WithMany(f => f.StokHareketleri)
               .HasForeignKey(x => x.FaturaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
