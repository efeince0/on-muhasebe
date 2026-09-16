using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.DataAccess.Context;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.Business.Seed;

/// <summary>
/// Dokuman 7. maddesi: "Ornek/test verileri ile doldurulmus bir demo".
/// Yalnizca bos veritabaninda ve ayar acikken calisir; gercek kuruluma
/// veri bulastirmamasi icin appsettings'teki DemoVerisi:Ekle bayragina bagli.
/// </summary>
public static class DemoVerisi
{
    public static async Task EkleAsync(OnMuhasebeContext context)
    {
        // Cari tablosu doluysa demo daha once eklenmis veya gercek veri var.
        if (await context.Cariler.AnyAsync())
            return;

        var bugun = DateTime.Today;

        var kullanicilar = KullanicilariOlustur(await RolHaritasi(context));
        context.Kullanicilar.AddRange(kullanicilar);

        var cariler = CarileriOlustur();
        context.Cariler.AddRange(cariler);

        var stoklar = StoklariOlustur();
        context.Stoklar.AddRange(stoklar);

        // Id'ler burada olusuyor; hareketler ve faturalar bunlara baglanacak.
        await context.SaveChangesAsync();

        context.StokHareketleri.AddRange(AcilisStoklari(stoklar, bugun));
        context.CariIslemler.AddRange(TahsilatOdemeler(cariler, bugun));
        await context.SaveChangesAsync();

        context.Faturalar.AddRange(Faturalar(cariler, stoklar, bugun));
        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, int>> RolHaritasi(OnMuhasebeContext context) =>
        await context.Roller.AsNoTracking().ToDictionaryAsync(r => r.RolAdi, r => r.Id);

    private static List<Kullanici> KullanicilariOlustur(Dictionary<string, int> roller)
    {
        var hasher = new PasswordHasher<Kullanici>();

        // Demo hesaplari; sifreler asgari karmasiklik kuralina uyar.
        (string Ad, string AdSoyad, string Rol, string Sifre)[] tanimlar =
        [
            ("muhasebe", "Ayşe Yılmaz",  "Muhasebe",      "Muhasebe123"),
            ("depo",     "Mehmet Kaya",  "Depo",          "Depo12345"),
            ("bakis",    "Zeynep Demir", "Görüntüleyici", "Bakis12345")
        ];

        var liste = new List<Kullanici>();

        foreach (var t in tanimlar)
        {
            if (!roller.TryGetValue(t.Rol, out var rolId))
                continue;

            var kullanici = new Kullanici
            {
                KullaniciAdi = t.Ad,
                AdSoyad      = t.AdSoyad,
                Eposta       = $"{t.Ad}@onmuhasebe.local",
                RolId        = rolId
            };

            kullanici.SifreHash = hasher.HashPassword(kullanici, t.Sifre);
            liste.Add(kullanici);
        }

        return liste;
    }

    private static List<Cari> CarileriOlustur() =>
    [
        new() { CariKodu = "C0001", Unvan = "Anadolu Market Ltd. Şti.", CariTipi = CariTipi.Musteri,
                VergiDairesi = "Kadıköy", VergiNo = "1234567890", Telefon = "0216 555 10 01",
                Eposta = "info@anadolumarket.com", Adres = "Caferağa Mah. No:12 Kadıköy/İstanbul",
                AcilisBakiye = 5_000m },

        new() { CariKodu = "C0002", Unvan = "Yıldız Gıda A.Ş.", CariTipi = CariTipi.Musteri,
                VergiDairesi = "Şişli", VergiNo = "2345678901", Telefon = "0212 555 20 02",
                Eposta = "muhasebe@yildizgida.com", Adres = "Mecidiyeköy Mah. No:45 Şişli/İstanbul",
                AcilisBakiye = 0m },

        new() { CariKodu = "C0003", Unvan = "Deniz Restoran", CariTipi = CariTipi.Musteri,
                VergiDairesi = "Beşiktaş", VergiNo = "3456789012", Telefon = "0212 555 30 03",
                Adres = "Ortaköy Mah. Sahil Yolu No:8 Beşiktaş/İstanbul",
                AcilisBakiye = 1_250m },

        new() { CariKodu = "C0004", Unvan = "Ege Toptan Gıda", CariTipi = CariTipi.Tedarikci,
                VergiDairesi = "Bornova", VergiNo = "4567890123", Telefon = "0232 555 40 04",
                Eposta = "siparis@egetoptan.com", Adres = "Kazımdirik Mah. No:101 Bornova/İzmir",
                AcilisBakiye = 0m },

        new() { CariKodu = "C0005", Unvan = "Marmara Ambalaj San.", CariTipi = CariTipi.Tedarikci,
                VergiDairesi = "Gebze", VergiNo = "5678901234", Telefon = "0262 555 50 05",
                Adres = "Organize Sanayi Böl. 5. Cad. Gebze/Kocaeli",
                AcilisBakiye = -2_000m },

        new() { CariKodu = "C0006", Unvan = "Başak Ticaret", CariTipi = CariTipi.Ikisi,
                VergiDairesi = "Çankaya", VergiNo = "6789012345", Telefon = "0312 555 60 06",
                Eposta = "basak@basakticaret.com", Adres = "Kızılay Mah. No:7 Çankaya/Ankara",
                AcilisBakiye = 0m }
    ];

    private static List<Stok> StoklariOlustur() =>
    [
        new() { StokKodu = "S0001", StokAdi = "Ayçiçek Yağı 5 L",   Kategori = "Gıda",    Birim = "Adet",
                AlisFiyati = 180m, SatisFiyati = 225m, KdvOrani = 10m, KritikStok = 20m },

        new() { StokKodu = "S0002", StokAdi = "Toz Şeker 50 kg",    Kategori = "Gıda",    Birim = "Çuval",
                AlisFiyati = 750m, SatisFiyati = 890m, KdvOrani = 1m,  KritikStok = 10m },

        new() { StokKodu = "S0003", StokAdi = "Un 25 kg",           Kategori = "Gıda",    Birim = "Çuval",
                AlisFiyati = 320m, SatisFiyati = 395m, KdvOrani = 1m,  KritikStok = 15m },

        new() { StokKodu = "S0004", StokAdi = "Zeytinyağı 1 L",     Kategori = "Gıda",    Birim = "Adet",
                AlisFiyati = 240m, SatisFiyati = 310m, KdvOrani = 10m, KritikStok = 30m },

        new() { StokKodu = "S0005", StokAdi = "Karton Koli 40x30",  Kategori = "Ambalaj", Birim = "Adet",
                AlisFiyati = 12m,  SatisFiyati = 18m,  KdvOrani = 20m, KritikStok = 200m },

        new() { StokKodu = "S0006", StokAdi = "Streç Film 500 m",   Kategori = "Ambalaj", Birim = "Rulo",
                AlisFiyati = 95m,  SatisFiyati = 135m, KdvOrani = 20m, KritikStok = 25m },

        new() { StokKodu = "S0007", StokAdi = "Temizlik Bezi",      Kategori = "Sarf",    Birim = "Paket",
                AlisFiyati = 45m,  SatisFiyati = 70m,  KdvOrani = 20m, KritikStok = 40m },

        new() { StokKodu = "S0008", StokAdi = "Nakliye Hizmeti",    Kategori = "Hizmet",  Birim = "Sefer",
                AlisFiyati = 0m,   SatisFiyati = 1_500m, KdvOrani = 20m, KritikStok = null }
    ];

    /// <summary>Faturalardan once depoda mal olmasi icin acilis girisleri.</summary>
    private static List<StokHareket> AcilisStoklari(List<Stok> stoklar, DateTime bugun)
    {
        (string Kod, decimal Miktar)[] girisler =
        [
            ("S0001", 150m), ("S0002", 60m),  ("S0003", 80m),  ("S0004", 200m),
            ("S0005", 1_500m), ("S0006", 120m), ("S0007", 250m)
        ];

        var hareketler = new List<StokHareket>();
        var sira = 1;

        foreach (var (kod, miktar) in girisler)
        {
            var stok = stoklar.First(s => s.StokKodu == kod);

            hareketler.Add(new StokHareket
            {
                HareketNo   = $"GIR{sira:D6}",
                StokId      = stok.Id,
                HareketTipi = StokHareketTipi.Giris,
                Miktar      = miktar,
                Tarih       = bugun.AddDays(-45),
                Aciklama    = "Dönem başı devir"
            });

            sira++;
        }

        // Fire ve sayim ornekleri: her hareket tipi demoda gorunsun.
        hareketler.Add(new StokHareket
        {
            HareketNo   = "CIK000001",
            StokId      = stoklar.First(s => s.StokKodu == "S0004").Id,
            HareketTipi = StokHareketTipi.Cikis,
            Miktar      = 6m,
            Tarih       = bugun.AddDays(-20),
            Aciklama    = "Kırılma / fire"
        });

        hareketler.Add(new StokHareket
        {
            HareketNo   = "SAY000001",
            StokId      = stoklar.First(s => s.StokKodu == "S0005").Id,
            HareketTipi = StokHareketTipi.Sayim,
            Miktar      = -25m,
            Tarih       = bugun.AddDays(-10),
            Aciklama    = "Yıl sonu sayım farkı"
        });

        return hareketler;
    }

    private static List<CariIslem> TahsilatOdemeler(List<Cari> cariler, DateTime bugun)
    {
        (string Kod, IslemTipi Tip, decimal Tutar, OdemeSekli Sekil, int GunOnce, string Aciklama)[] tanimlar =
        [
            ("C0001", IslemTipi.Tahsilat, 5_000m,  OdemeSekli.Havale,     30, "Devir bakiye tahsilatı"),
            ("C0001", IslemTipi.Tahsilat, 12_000m, OdemeSekli.Nakit,      12, "Kısmi tahsilat"),
            ("C0002", IslemTipi.Tahsilat, 8_500m,  OdemeSekli.KrediKarti, 18, "Fatura tahsilatı"),
            ("C0003", IslemTipi.Tahsilat, 1_250m,  OdemeSekli.Nakit,       7, "Açık hesap kapama"),
            ("C0004", IslemTipi.Odeme,    15_000m, OdemeSekli.Havale,     25, "Mal bedeli ödemesi"),
            ("C0005", IslemTipi.Odeme,    2_000m,  OdemeSekli.CekSenet,   15, "Ambalaj alımı ödemesi")
        ];

        var islemler = new List<CariIslem>();
        var tahsilat = 1;
        var odeme    = 1;

        foreach (var t in tanimlar)
        {
            var numara = t.Tip == IslemTipi.Tahsilat ? $"TAH{tahsilat++:D6}" : $"ODE{odeme++:D6}";

            islemler.Add(new CariIslem
            {
                IslemNo    = numara,
                CariId     = cariler.First(c => c.CariKodu == t.Kod).Id,
                IslemTipi  = t.Tip,
                Tarih      = bugun.AddDays(-t.GunOnce),
                Tutar      = t.Tutar,
                OdemeSekli = t.Sekil,
                Aciklama   = t.Aciklama
            });
        }

        return islemler;
    }

    private static List<Fatura> Faturalar(List<Cari> cariler, List<Stok> stoklar, DateTime bugun)
    {
        var faturalar = new List<Fatura>();
        var alis  = 1;
        var satis = 1;

        Fatura Olustur(string cariKodu, FaturaTipi tip, int gunOnce, string aciklama,
                       params (string Kod, decimal Miktar, decimal Fiyat)[] satirlar)
        {
            var fatura = new Fatura
            {
                FaturaNo   = tip == FaturaTipi.Alis ? $"ALF{alis++:D6}" : $"SAT{satis++:D6}",
                FaturaTipi = tip,
                CariId     = cariler.First(c => c.CariKodu == cariKodu).Id,
                Tarih      = bugun.AddDays(-gunOnce),
                Aciklama   = aciklama
            };

            foreach (var s in satirlar)
            {
                var stok = stoklar.First(x => x.StokKodu == s.Kod);

                fatura.Satirlar.Add(new FaturaSatir
                {
                    StokId      = stok.Id,
                    Miktar      = s.Miktar,
                    BirimFiyat  = s.Fiyat,
                    KdvOrani    = stok.KdvOrani,
                    SatirTutari = s.Miktar * s.Fiyat
                });
            }

            // Toplamlar serviste nasil hesaplaniyorsa burada da ayni: satir
            // tutari KDV haric, KDV ayri toplaniyor.
            fatura.AraToplam   = fatura.Satirlar.Sum(x => x.SatirTutari);
            fatura.ToplamKdv   = fatura.Satirlar.Sum(x => x.SatirTutari * x.KdvOrani / 100);
            fatura.GenelToplam = fatura.AraToplam + fatura.ToplamKdv;

            return fatura;
        }

        faturalar.Add(Olustur("C0004", FaturaTipi.Alis, 40, "Aylık gıda alımı",
            ("S0001", 60m, 180m), ("S0002", 20m, 750m), ("S0003", 30m, 320m)));

        faturalar.Add(Olustur("C0005", FaturaTipi.Alis, 28, "Ambalaj malzemesi",
            ("S0005", 800m, 12m), ("S0006", 60m, 95m)));

        faturalar.Add(Olustur("C0006", FaturaTipi.Alis, 14, "Sarf malzeme",
            ("S0007", 100m, 45m)));

        faturalar.Add(Olustur("C0001", FaturaTipi.Satis, 35, "Haftalık sevkiyat",
            ("S0001", 40m, 225m), ("S0004", 25m, 310m), ("S0005", 200m, 18m)));

        faturalar.Add(Olustur("C0002", FaturaTipi.Satis, 22, "Toplu sipariş",
            ("S0002", 15m, 890m), ("S0003", 20m, 395m), ("S0008", 1m, 1_500m)));

        faturalar.Add(Olustur("C0003", FaturaTipi.Satis, 9, "Restoran tedariği",
            ("S0004", 30m, 310m), ("S0007", 20m, 70m)));

        faturalar.Add(Olustur("C0001", FaturaTipi.Satis, 3, "Ek sipariş",
            ("S0006", 15m, 135m), ("S0005", 150m, 18m)));

        return faturalar;
    }
}
