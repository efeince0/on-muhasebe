using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.DataAccess.Configurations;

public class FaturaConfiguration : IEntityTypeConfiguration<Fatura>
{
    public void Configure(EntityTypeBuilder<Fatura> builder)
    {
        builder.ToTable("Faturalar");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FaturaNo).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FaturaTipi).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.Tarih).HasColumnType("date");
        builder.Property(x => x.AraToplam).HasPrecision(18, 2);
        builder.Property(x => x.ToplamKdv).HasPrecision(18, 2);
        builder.Property(x => x.GenelToplam).HasPrecision(18, 2);
        builder.Property(x => x.Aciklama).HasMaxLength(250);
        builder.Property(x => x.Aktif).HasDefaultValue(true);

        builder.HasIndex(x => x.FaturaNo).IsUnique();

        builder.HasOne(x => x.Cari)
               .WithMany(c => c.Faturalar)
               .HasForeignKey(x => x.CariId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
