using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.DataAccess.Configurations;

public class KullaniciConfiguration : IEntityTypeConfiguration<Kullanici>
{
    public void Configure(EntityTypeBuilder<Kullanici> builder)
    {
        builder.ToTable("Kullanicilar");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.KullaniciAdi).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AdSoyad).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Eposta).HasMaxLength(100);
        builder.Property(x => x.SifreHash).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Aktif).HasDefaultValue(true);

        builder.HasIndex(x => x.KullaniciAdi).IsUnique();

        builder.HasOne(x => x.Rol)
               .WithMany(r => r.Kullanicilar)
               .HasForeignKey(x => x.RolId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
