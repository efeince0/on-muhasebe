using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Entities.Common;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.DataAccess.Context;

public class OnMuhasebeContext : DbContext
{
    public OnMuhasebeContext(DbContextOptions<OnMuhasebeContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// O anda islem yapan kullanicinin kimligi. Denetim alanlarini doldurmak icin kullanilir.
    /// Giris sistemi devreye girdiginde (Gun 4) servis katmani bu degeri set edecek.
    /// Seed ve migration kayitlarinda null kalir.
    /// </summary>
    public int? AktifKullaniciId { get; set; }

    public DbSet<Rol>         Roller          { get; set; }
    public DbSet<Kullanici>   Kullanicilar    { get; set; }
    public DbSet<RolIzin>     RolIzinleri     { get; set; }
    public DbSet<Cari>        Cariler         { get; set; }
    public DbSet<CariIslem>   CariIslemler    { get; set; }
    public DbSet<Stok>        Stoklar         { get; set; }
    public DbSet<StokHareket> StokHareketleri { get; set; }
    public DbSet<Fatura>      Faturalar       { get; set; }
    public DbSet<FaturaSatir> FaturaSatirlari { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OnMuhasebeContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        DenetimAlanlariniDoldur();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        DenetimAlanlariniDoldur();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Dokumanin 2.1 maddesi: tum ana tablolarda denetim alanlari tutulur.
    /// Bu alanlar elle doldurulmaz, kayit sirasinda merkezi olarak set edilir.
    /// </summary>
    private void DenetimAlanlariniDoldur()
    {
        var simdi = DateTime.Now;

        foreach (var girdi in ChangeTracker.Entries<BaseEntity>())
        {
            if (girdi.State == EntityState.Added)
            {
                girdi.Entity.OlusturmaTarihi      = simdi;
                girdi.Entity.OlusturanKullaniciId = AktifKullaniciId;
            }
            else if (girdi.State == EntityState.Modified)
            {
                // Olusturma bilgisi guncellemede degistirilemez
                girdi.Property(x => x.OlusturmaTarihi).IsModified      = false;
                girdi.Property(x => x.OlusturanKullaniciId).IsModified = false;

                girdi.Entity.GuncellemeTarihi       = simdi;
                girdi.Entity.GuncelleyenKullaniciId = AktifKullaniciId;
            }
        }
    }
}
