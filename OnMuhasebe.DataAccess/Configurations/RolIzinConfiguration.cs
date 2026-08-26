using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.DataAccess.Configurations;

public class RolIzinConfiguration : IEntityTypeConfiguration<RolIzin>
{
    public void Configure(EntityTypeBuilder<RolIzin> builder)
    {
        builder.ToTable("RolIzinleri");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Modul).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Islem).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Aktif).HasDefaultValue(true);

        // Ayni role, ayni modul icin, ayni islem iki kez tanimlanamaz
        builder.HasIndex(x => new { x.RolId, x.Modul, x.Islem }).IsUnique();

        builder.HasOne(x => x.Rol)
               .WithMany(r => r.Izinler)
               .HasForeignKey(x => x.RolId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
