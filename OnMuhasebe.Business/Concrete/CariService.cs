using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.DataAccess.Context;
using OnMuhasebe.Entities.Tables;
using OnMuhasebe.Entities.ViewModels;
using System.Security.Claims;

namespace OnMuhasebe.Business.Concrete;

public class CariService : ICariService
{
    private readonly OnMuhasebeContext _context;

    public CariService(OnMuhasebeContext context, IHttpContextAccessor erisim)
    {
        _context = context;

        // Denetim alanlarini SaveChanges dolduruyor; aktif kullanici Id'sini oradan alamiyor.
        var idMetni = erisim.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idMetni, out var kullaniciId))
            _context.AktifKullaniciId = kullaniciId;
    }

    public async Task<SayfaliListe<CariListeViewModel>> ListeleAsync(
        string? arama = null, bool sadeceAktif = true,
        int sayfaNo = 1, int sayfaBoyutu = 20,
        string sirala = "kod", string yon = "asc")
    {
        // Adres cubugundan gelen degerler guvenilmez; sinirlara cekiyoruz.
        if (sayfaNo < 1) sayfaNo = 1;
        if (sayfaBoyutu is < 5 or > 200) sayfaBoyutu = 20;

        var sorgu = _context.Cariler.AsNoTracking().AsQueryable();

        if (sadeceAktif)
            sorgu = sorgu.Where(c => c.Aktif);

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var a = arama.Trim();
            sorgu = sorgu.Where(c => c.CariKodu.Contains(a) || c.Unvan.Contains(a));
        }

        var toplam = await sorgu.CountAsync();

        // Son sayfadaki tek kayit silinince bos sayfada kalinmasin.
        var toplamSayfa = toplam == 0 ? 1 : (int)Math.Ceiling(toplam / (double)sayfaBoyutu);
        if (sayfaNo > toplamSayfa) sayfaNo = toplamSayfa;

        var projeksiyon = sorgu.Select(c => new CariListeViewModel
        {
            Id       = c.Id,
            CariKodu = c.CariKodu,
            Unvan    = c.Unvan,
            CariTipi = c.CariTipi,
            Telefon  = c.Telefon,
            Aktif    = c.Aktif,

            // Borc ve Odeme bakiyeyi artirir, Alacak ve Tahsilat azaltir.
            GuncelBakiye =
                  c.AcilisBakiye
                + c.CariIslemler
                    .Where(i => i.Aktif && (i.IslemTipi == IslemTipi.Borc || i.IslemTipi == IslemTipi.Odeme))
                    .Sum(i => i.Tutar)
                - c.CariIslemler
                    .Where(i => i.Aktif && (i.IslemTipi == IslemTipi.Alacak || i.IslemTipi == IslemTipi.Tahsilat))
                    .Sum(i => i.Tutar)
        });

        var kayitlar = await SiralamaUygula(projeksiyon, sirala, yon)
            .Skip((sayfaNo - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .ToListAsync();

        return new SayfaliListe<CariListeViewModel>
        {
            Kayitlar    = kayitlar,
            ToplamKayit = toplam,
            SayfaNo     = sayfaNo,
            SayfaBoyutu = sayfaBoyutu
        };
    }

    /// <summary>
    /// Siralama adres cubugundan geliyor; bilinmeyen deger varsayilana duser.
    /// Kolon adini dogrudan sorguya gecirmek yerine beyaz liste kullaniyoruz.
    /// </summary>
    private static IQueryable<CariListeViewModel> SiralamaUygula(
        IQueryable<CariListeViewModel> sorgu, string sirala, string yon)
    {
        var azalan = yon == "desc";

        return (sirala, azalan) switch
        {
            ("unvan",  true)  => sorgu.OrderByDescending(x => x.Unvan),
            ("unvan",  false) => sorgu.OrderBy(x => x.Unvan),
            ("bakiye", true)  => sorgu.OrderByDescending(x => x.GuncelBakiye),
            ("bakiye", false) => sorgu.OrderBy(x => x.GuncelBakiye),
            ("tip",    true)  => sorgu.OrderByDescending(x => x.CariTipi),
            ("tip",    false) => sorgu.OrderBy(x => x.CariTipi),
            (_,        true)  => sorgu.OrderByDescending(x => x.CariKodu),
            _                 => sorgu.OrderBy(x => x.CariKodu)
        };
    }

    public async Task<string> SonrakiKodOnerAsync(string onEk = "C")
    {
        var kodlar = await _context.Cariler
            .AsNoTracking()
            .Where(c => c.CariKodu.StartsWith(onEk))
            .Select(c => c.CariKodu)
            .ToListAsync();

        // "C0007" -> "0007" -> 7. Sayi olmayanlar (ornegin "CARI-X") elenir.
        var enBuyuk = kodlar
            .Select(k => k[onEk.Length..])
            .Where(son => son.Length > 0 && son.All(char.IsDigit))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();

        // D4: dort haneye tamamla -> 8 olur "0008"
        return $"{onEk}{enBuyuk + 1:D4}";
    }

    public async Task<CariFormViewModel?> FormGetirAsync(int id)
    {
        return await _context.Cariler
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CariFormViewModel
            {
                Id           = c.Id,
                CariKodu     = c.CariKodu,
                Unvan        = c.Unvan,
                CariTipi     = c.CariTipi,
                VergiDairesi = c.VergiDairesi,
                VergiNo      = c.VergiNo,
                Telefon      = c.Telefon,
                Eposta       = c.Eposta,
                Adres        = c.Adres,
                AcilisBakiye = c.AcilisBakiye,
                Aktif        = c.Aktif
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CariDetayViewModel?> DetayGetirAsync(int id, int sonHareketSayisi = 10)
    {
        return await _context.Cariler
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CariDetayViewModel
            {
                Id           = c.Id,
                CariKodu     = c.CariKodu,
                Unvan        = c.Unvan,
                CariTipi     = c.CariTipi,
                VergiDairesi = c.VergiDairesi,
                VergiNo      = c.VergiNo,
                Telefon      = c.Telefon,
                Eposta       = c.Eposta,
                Adres        = c.Adres,
                Aktif        = c.Aktif,

                AcilisBakiye = c.AcilisBakiye,
                ToplamBorc   = c.CariIslemler
                    .Where(i => i.Aktif && (i.IslemTipi == IslemTipi.Borc || i.IslemTipi == IslemTipi.Odeme))
                    .Sum(i => i.Tutar),
                ToplamAlacak = c.CariIslemler
                    .Where(i => i.Aktif && (i.IslemTipi == IslemTipi.Alacak || i.IslemTipi == IslemTipi.Tahsilat))
                    .Sum(i => i.Tutar),

                HareketSayisi = c.CariIslemler.Count(i => i.Aktif),

                SonHareketler = c.CariIslemler
                    .Where(i => i.Aktif)
                    .OrderByDescending(i => i.Tarih).ThenByDescending(i => i.Id)
                    .Take(sonHareketSayisi)
                    .Select(i => new CariHareketSatirViewModel
                    {
                        Id          = i.Id,
                        IslemNo     = i.IslemNo,
                        Tarih       = i.Tarih,
                        IslemTipi   = i.IslemTipi,
                        Tutar       = i.Tutar,
                        OdemeSekli  = i.OdemeSekli,
                        Aciklama    = i.Aciklama,
                        FaturadanMi = i.FaturaId != null
                    })
                    .ToList(),

                OlusturmaTarihi  = c.OlusturmaTarihi,
                GuncellemeTarihi = c.GuncellemeTarihi
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(bool Basarili, string? Hata)> KaydetAsync(CariFormViewModel model)
    {
        var kodKullanimda = await _context.Cariler
            .AnyAsync(c => c.CariKodu == model.CariKodu && c.Id != model.Id);

        if (kodKullanimda)
            return (false, "Bu cari kodu başka bir kayıtta kullanılıyor.");

        // Vergi no bos birakilabilir; bos olanlar benzersizlik disinda tutulur.
        if (!string.IsNullOrWhiteSpace(model.VergiNo))
        {
            var vergiNo = model.VergiNo.Trim();
            var vergiNoKullanimda = await _context.Cariler
                .AnyAsync(c => c.VergiNo == vergiNo && c.Id != model.Id);

            if (vergiNoKullanimda)
                return (false, "Bu vergi numarası başka bir kayıtta kullanılıyor.");
        }

        if (model.Id == 0)
        {
            _context.Cariler.Add(new Cari
            {
                CariKodu     = model.CariKodu.Trim(),
                Unvan        = model.Unvan.Trim(),
                CariTipi     = model.CariTipi,
                VergiDairesi = model.VergiDairesi?.Trim(),
                VergiNo      = model.VergiNo?.Trim(),
                Telefon      = model.Telefon?.Trim(),
                Eposta       = model.Eposta?.Trim(),
                Adres        = model.Adres?.Trim(),
                AcilisBakiye = model.AcilisBakiye,
                Aktif        = true
            });
        }
        else
        {
            var mevcut = await _context.Cariler.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (mevcut == null)
                return (false, "Güncellenecek kayıt bulunamadı.");

            mevcut.CariKodu     = model.CariKodu.Trim();
            mevcut.Unvan        = model.Unvan.Trim();
            mevcut.CariTipi     = model.CariTipi;
            mevcut.VergiDairesi = model.VergiDairesi?.Trim();
            mevcut.VergiNo      = model.VergiNo?.Trim();
            mevcut.Telefon      = model.Telefon?.Trim();
            mevcut.Eposta       = model.Eposta?.Trim();
            mevcut.Adres        = model.Adres?.Trim();
            mevcut.AcilisBakiye = model.AcilisBakiye;
            mevcut.Aktif        = model.Aktif;
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Mesaj)> PasifeAlAsync(int id)
    {
        var cari = await _context.Cariler.FirstOrDefaultAsync(c => c.Id == id);
        if (cari == null)
            return (false, "Kayıt bulunamadı.");

        if (!cari.Aktif)
            return (false, "Kayıt zaten pasif durumda.");

        // Bakiye pasife almayi engellemez; dokuman hareketi olan cari icin
        // pasife almayi tek cikis yolu olarak tanimliyor. Kullanici bilgilendirilir.
        var bakiye = await BakiyeHesaplaAsync(id);

        // Kayitlar kalici silinmez; gecmis hareketlerin bagli oldugu cari korunur.
        cari.Aktif = false;
        await _context.SaveChangesAsync();

        return bakiye != 0
            ? (true, $"Cari pasife alındı. Dikkat: {bakiye:N2} ₺ bakiyesi var.")
            : (true, null);
    }

    public async Task<(bool Basarili, string? Hata)> AktifYapAsync(int id)
    {
        var cari = await _context.Cariler.FirstOrDefaultAsync(c => c.Id == id);
        if (cari == null)
            return (false, "Kayıt bulunamadı.");

        cari.Aktif = true;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private async Task<decimal> BakiyeHesaplaAsync(int cariId)
    {
        return await _context.Cariler
            .AsNoTracking()
            .Where(c => c.Id == cariId)
            .Select(c =>
                  c.AcilisBakiye
                + c.CariIslemler
                    .Where(i => i.Aktif && (i.IslemTipi == IslemTipi.Borc || i.IslemTipi == IslemTipi.Odeme))
                    .Sum(i => i.Tutar)
                - c.CariIslemler
                    .Where(i => i.Aktif && (i.IslemTipi == IslemTipi.Alacak || i.IslemTipi == IslemTipi.Tahsilat))
                    .Sum(i => i.Tutar))
            .FirstAsync();
    }
}
