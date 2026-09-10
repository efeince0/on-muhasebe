using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.DataAccess.Configurations;

public class CariIslemConfiguration : IEntityTypeConfiguration<CariIslem>
{
    public void Configure(EntityTypeBuilder<CariIslem> builder)
    {
        builder.ToTable("CariIslemler", t =>
            t.HasCheckConstraint("CK_CariIslemler_Tutar", "[Tutar] > 0"));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IslemNo).HasMaxLength(20).IsRequired();
        builder.Property(x => x.IslemTipi).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.OdemeSekli).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Tarih).HasColumnType("date");
        builder.Property(x => x.Tutar).HasPrecision(18, 2);
        builder.Property(x => x.Aciklama).HasMaxLength(250);
        builder.Property(x => x.Aktif).HasDefaultValue(true);

        builder.HasIndex(x => x.IslemNo).IsUnique();

        // Ekstre ve bakiye sorgulari her zaman cari + tarih uzerinden gider.
        builder.HasIndex(x => new { x.CariId, x.Tarih });

        builder.HasOne(x => x.Cari)
               .WithMany(c => c.CariIslemler)
               .HasForeignKey(x => x.CariId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
