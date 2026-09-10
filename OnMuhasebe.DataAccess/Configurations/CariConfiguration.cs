using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.DataAccess.Configurations;

public class CariConfiguration : IEntityTypeConfiguration<Cari>
{
    public void Configure(EntityTypeBuilder<Cari> builder)
    {
        builder.ToTable("Cariler");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CariKodu).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Unvan).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CariTipi).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(x => x.VergiDairesi).HasMaxLength(80);
        builder.Property(x => x.VergiNo).HasMaxLength(15);
        builder.Property(x => x.Telefon).HasMaxLength(20);
        builder.Property(x => x.Eposta).HasMaxLength(100);
        builder.Property(x => x.Adres).HasMaxLength(250);
        builder.Property(x => x.AcilisBakiye).HasPrecision(18, 2);
        builder.Property(x => x.Aktif).HasDefaultValue(true);

        builder.HasIndex(x => x.CariKodu).IsUnique();

        // SQL Server unique index'te NULL'lari esit sayar; filtre olmadan
        // vergi no'su bos yalnizca bir cari kaydedilebilirdi.
        builder.HasIndex(x => x.VergiNo)
               .IsUnique()
               .HasFilter("[VergiNo] IS NOT NULL");
    }
}
