using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.DataAccess.Context;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.Business.Seed;

/// <summary>
/// Bos veritabanina baslangic kayitlarini ekler: roller, yetki matrisi, yonetici hesabi.
/// Uygulama her acilista calisir ama veritabani doluysa hicbir sey yapmaz.
/// </summary>
public static class VeritabaniBaslatici
{
    // Ilk giris bilgileri. Ilk girsten sonra sifre degistirilmelidir.
    private const string AdminKullaniciAdi = "admin";
    private const string AdminSifre        = "Admin!2345";

    public static async Task BaslangicVerisiEkleAsync(this IServiceProvider services)
    {
        using var kapsam = services.CreateScope();
        var context = kapsam.ServiceProvider.GetRequiredService<OnMuhasebeContext>();

        // Zaten doluysa dokunma. Uygulama her acilista bu metot cagrilir.
        if (await context.Roller.AnyAsync())
            return;

        var roller = RolleriOlustur();
        context.Roller.AddRange(roller);
        await context.SaveChangesAsync();          // RolId'ler burada olusur

        context.RolIzinleri.AddRange(IzinleriOlustur(roller));
        context.Kullanicilar.Add(YoneticiOlustur(roller));
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────
    private static List<Rol> RolleriOlustur() =>
    [
        new Rol { RolAdi = "Yönetici",      Aciklama = "Tüm modüllere ve kullanıcı yönetimine tam erişim." },
        new Rol { RolAdi = "Muhasebe",      Aciklama = "Cari, cari işlemleri ve fatura modüllerinde tam yetki; stok görüntüleme." },
        new Rol { RolAdi = "Depo",          Aciklama = "Stok ve stok işlemleri modüllerinde tam yetki; cari görüntüleme." },
        new Rol { RolAdi = "Görüntüleyici", Aciklama = "Yalnızca görüntüleme; değişiklik yapamaz." }
    ];

    /// <summary>
    /// Kaynak dokumanin 4.7 maddesindeki yetki matrisi.
    /// Yalnizca IZIN VERILEN kombinasyonlar kaydedilir; kayit yoksa izin yok demektir.
    /// </summary>
    private static List<RolIzin> IzinleriOlustur(List<Rol> roller)
    {
        var tumIslemler = new[] { Islem.Goruntule, Islem.Ekle, Islem.Guncelle, Islem.Sil };
        var sadeceGoruntule = new[] { Islem.Goruntule };

        var matris = new (string RolAdi, Modul Modul, Islem[] Islemler)[]
        {
            // Yönetici — her modülde her işlem
            ("Yönetici", Modul.Cari,        tumIslemler),
            ("Yönetici", Modul.CariIslem,   tumIslemler),
            ("Yönetici", Modul.Stok,        tumIslemler),
            ("Yönetici", Modul.StokHareket, tumIslemler),
            ("Yönetici", Modul.Fatura,      tumIslemler),
            ("Yönetici", Modul.Kullanici,   tumIslemler),
            ("Yönetici", Modul.Rol,         tumIslemler),

            // Muhasebe — cari ve fatura tam, stok sadece görüntüleme
            ("Muhasebe", Modul.Cari,        tumIslemler),
            ("Muhasebe", Modul.CariIslem,   tumIslemler),
            ("Muhasebe", Modul.Fatura,      tumIslemler),
            ("Muhasebe", Modul.Stok,        sadeceGoruntule),

            // Depo — stok tam, cari ve fatura sadece görüntüleme
            ("Depo",     Modul.Stok,        tumIslemler),
            ("Depo",     Modul.StokHareket, tumIslemler),
            ("Depo",     Modul.Cari,        sadeceGoruntule),
            ("Depo",     Modul.Fatura,      sadeceGoruntule),

            // Görüntüleyici — her modülde yalnızca görüntüleme
            ("Görüntüleyici", Modul.Cari,        sadeceGoruntule),
            ("Görüntüleyici", Modul.CariIslem,   sadeceGoruntule),
            ("Görüntüleyici", Modul.Stok,        sadeceGoruntule),
            ("Görüntüleyici", Modul.StokHareket, sadeceGoruntule),
            ("Görüntüleyici", Modul.Fatura,      sadeceGoruntule)
        };

        var izinler = new List<RolIzin>();

        foreach (var (rolAdi, modul, islemler) in matris)
        {
            var rol = roller.First(r => r.RolAdi == rolAdi);

            foreach (var islem in islemler)
            {
                izinler.Add(new RolIzin
                {
                    RolId   = rol.Id,
                    Modul   = modul,
                    Islem   = islem,
                    IzinVar = true
                });
            }
        }

        return izinler;
    }

    private static Kullanici YoneticiOlustur(List<Rol> roller)
    {
        var yonetici = new Kullanici
        {
            KullaniciAdi = AdminKullaniciAdi,
            AdSoyad      = "Sistem Yöneticisi",
            Eposta       = "admin@onmuhasebe.local",
            RolId        = roller.First(r => r.RolAdi == "Yönetici").Id
        };

        // Sifre asla duz metin saklanmaz. PasswordHasher salt uretir,
        // hash'in icine gomer ve islemi bilerek yavaslatir.
        var hasher = new PasswordHasher<Kullanici>();
        yonetici.SifreHash = hasher.HashPassword(yonetici, AdminSifre);

        return yonetici;
    }
}
