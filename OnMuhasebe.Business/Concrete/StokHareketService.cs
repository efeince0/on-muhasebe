using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.DataAccess.Context;
using OnMuhasebe.Entities.Tables;
using OnMuhasebe.Entities.ViewModels;
using System.Security.Claims;

namespace OnMuhasebe.Business.Concrete;

public class StokHareketService : IStokHareketService
{
    private readonly OnMuhasebeContext _context;
    private readonly IStokService      _stokService;

    // Miktar formulu StokService'te; buraya ikinci bir kopyasini yazmak
    // yerine oradan soruyoruz. Formul degisirse tek yerde degisir.
    public StokHareketService(
        OnMuhasebeContext context,
        IStokService stokService,
        IHttpContextAccessor erisim)
    {
        _context     = context;
        _stokService = stokService;

        var idMetni = erisim.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idMetni, out var kullaniciId))
            _context.AktifKullaniciId = kullaniciId;
    }

    public async Task<StokHareketListeSonucu> ListeleAsync(
        int? stokId = null, DateTime? baslangic = null, DateTime? bitis = null,
        StokHareketTipi? hareketTipi = null, bool sadeceAktif = true,
        int sayfaNo = 1, int sayfaBoyutu = 20,
        string sirala = "tarih", string yon = "desc")
    {
        if (sayfaNo < 1) sayfaNo = 1;
        if (sayfaBoyutu is < 5 or > 200) sayfaBoyutu = 20;

        var sorgu = _context.StokHareketleri.AsNoTracking().AsQueryable();

        if (sadeceAktif)         sorgu = sorgu.Where(h => h.Aktif);
        if (stokId is > 0)       sorgu = sorgu.Where(h => h.StokId == stokId);
        if (baslangic != null)   sorgu = sorgu.Where(h => h.Tarih >= baslangic);
        if (bitis != null)       sorgu = sorgu.Where(h => h.Tarih <= bitis);
        if (hareketTipi != null) sorgu = sorgu.Where(h => h.HareketTipi == hareketTipi);

        var toplam = await sorgu.CountAsync();

        // Toplamlar sayfaya degil, suzgecin tamamina ait olmali.
        var toplamGiris = await sorgu.Where(h => h.HareketTipi == StokHareketTipi.Giris).SumAsync(h => (decimal?)h.Miktar) ?? 0;
        var toplamCikis = await sorgu.Where(h => h.HareketTipi == StokHareketTipi.Cikis).SumAsync(h => (decimal?)h.Miktar) ?? 0;
        var toplamSayim = await sorgu.Where(h => h.HareketTipi == StokHareketTipi.Sayim).SumAsync(h => (decimal?)h.Miktar) ?? 0;

        var toplamSayfa = toplam == 0 ? 1 : (int)Math.Ceiling(toplam / (double)sayfaBoyutu);
        if (sayfaNo > toplamSayfa) sayfaNo = toplamSayfa;

        var projeksiyon = sorgu.Select(h => new StokHareketListeViewModel
        {
            Id          = h.Id,
            HareketNo   = h.HareketNo,
            Tarih       = h.Tarih,
            StokId      = h.StokId,
            StokKodu    = h.Stok.StokKodu,
            StokAdi     = h.Stok.StokAdi,
            Birim       = h.Stok.Birim,
            HareketTipi = h.HareketTipi,
            Miktar      = h.Miktar,
            Aciklama    = h.Aciklama,
            Aktif       = h.Aktif
        });

        var kayitlar = await SiralamaUygula(projeksiyon, sirala, yon)
            .Skip((sayfaNo - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .ToListAsync();

        return new StokHareketListeSonucu
        {
            Kayitlar    = kayitlar,
            ToplamKayit = toplam,
            SayfaNo     = sayfaNo,
            SayfaBoyutu = sayfaBoyutu,
            ToplamGiris = toplamGiris,
            ToplamCikis = toplamCikis,
            ToplamSayim = toplamSayim
        };
    }

    /// <summary>
    /// Siralama adres cubugundan geliyor; bilinmeyen deger varsayilana duser.
    /// Kolon adini dogrudan sorguya gecirmek yerine beyaz liste kullaniyoruz.
    /// </summary>
    private static IQueryable<StokHareketListeViewModel> SiralamaUygula(
        IQueryable<StokHareketListeViewModel> sorgu, string sirala, string yon)
    {
        var azalan = yon == "desc";

        return (sirala, azalan) switch
        {
            ("urun",   true)  => sorgu.OrderByDescending(x => x.StokAdi),
            ("urun",   false) => sorgu.OrderBy(x => x.StokAdi),
            ("miktar", true)  => sorgu.OrderByDescending(x => x.Miktar),
            ("miktar", false) => sorgu.OrderBy(x => x.Miktar),
            ("no",     true)  => sorgu.OrderByDescending(x => x.HareketNo),
            ("no",     false) => sorgu.OrderBy(x => x.HareketNo),

            // Ayni gune birden fazla hareket girilebilir; Id ikincil sira olmadan
            // sayfalar arasinda kayit tekrar edebilir veya atlanabilir.
            (_,        false) => sorgu.OrderBy(x => x.Tarih).ThenBy(x => x.Id),
            _                 => sorgu.OrderByDescending(x => x.Tarih).ThenByDescending(x => x.Id)
        };
    }

    public async Task<StokHareketFormViewModel?> FormGetirAsync(int id)
    {
        var hareket = await _context.StokHareketleri
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hareket == null)
            return null;

        var miktar = hareket.Miktar;

        // Sayimda tabloda FARK duruyor. Kullaniciya farki degil, o farkin
        // isaret ettigi sayilan miktari gosteriyoruz: kendi hareketi haric
        // mevcut miktarin uzerine farki ekliyoruz.
        if (hareket.HareketTipi == StokHareketTipi.Sayim)
        {
            var kendisiHaricMevcut = await _stokService.MiktarGetirAsync(hareket.StokId, hareket.Id);
            miktar = kendisiHaricMevcut + hareket.Miktar;
        }

        return new StokHareketFormViewModel
        {
            Id          = hareket.Id,
            HareketNo   = hareket.HareketNo,
            StokId      = hareket.StokId,
            HareketTipi = hareket.HareketTipi,
            Tarih       = hareket.Tarih,
            Miktar      = miktar,
            Aciklama    = hareket.Aciklama,
            Aktif       = hareket.Aktif
        };
    }

    public async Task<string> SonrakiHareketNoOnerAsync(StokHareketTipi hareketTipi)
    {
        var onEk = hareketTipi switch
        {
            StokHareketTipi.Giris => "GIR",
            StokHareketTipi.Cikis => "CIK",
            _                     => "SAY"
        };

        var numaralar = await _context.StokHareketleri
            .AsNoTracking()
            .Where(h => h.HareketNo.StartsWith(onEk))
            .Select(h => h.HareketNo)
            .ToListAsync();

        // "GIR000007" -> "000007" -> 7. Elle girilmis farkli bicimler elenir.
        var enBuyuk = numaralar
            .Select(n => n[onEk.Length..])
            .Where(son => son.Length > 0 && son.All(char.IsDigit))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();

        return $"{onEk}{enBuyuk + 1:D6}";
    }

    public async Task<(bool Basarili, string? Hata)> KaydetAsync(StokHareketFormViewModel model)
    {
        var stok = await _context.Stoklar
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == model.StokId);

        if (stok == null)
            return (false, "Seçilen ürün bulunamadı.");

        // Pasif karta yeni hareket girmek kapatilmis kartin yeniden
        // hareketlenmesi demek; mevcut kaydin duzeltilmesi ise serbest.
        if (!stok.Aktif && model.Id == 0)
            return (false, "Pasif ürüne yeni hareket girilemez. Önce kartı aktif edin.");

        var hareketNo = model.HareketNo.Trim();

        var noKullanimda = await _context.StokHareketleri
            .AnyAsync(h => h.HareketNo == hareketNo && h.Id != model.Id);

        if (noKullanimda)
            return (false, "Bu hareket numarası başka bir kayıtta kullanılıyor.");

        // Guncellemede kaydin kendi etkisi disindaki miktara bakiyoruz;
        // yoksa hareket kendi kendini dogrularken sayilmis olur.
        var mevcut = await _stokService.MiktarGetirAsync(
            model.StokId, model.Id == 0 ? null : model.Id);

        decimal kaydedilecek;

        if (model.HareketTipi == StokHareketTipi.Sayim)
        {
            // Kullanici sayilan miktari girdi; tabloya farki yaziyoruz.
            var fark = model.Miktar - mevcut;

            if (fark == 0)
                return (false, $"Sayılan miktar mevcut miktarla aynı ({mevcut:N3} {stok.Birim}). Düzeltme kaydına gerek yok.");

            kaydedilecek = fark;
        }
        else
        {
            if (model.Miktar <= 0)
                return (false, "Giriş ve çıkış miktarı sıfırdan büyük olmalıdır.");

            // Negatif stok engellenir: cikis, eldeki maldan fazla olamaz.
            if (model.HareketTipi == StokHareketTipi.Cikis && model.Miktar > mevcut)
                return (false, $"Yetersiz stok. Mevcut: {mevcut:N3} {stok.Birim}, çıkışı istenen: {model.Miktar:N3} {stok.Birim}");

            kaydedilecek = model.Miktar;
        }

        if (model.Id == 0)
        {
            _context.StokHareketleri.Add(new StokHareket
            {
                HareketNo   = hareketNo,
                StokId      = model.StokId,
                HareketTipi = model.HareketTipi,
                Tarih       = model.Tarih,
                Miktar      = kaydedilecek,
                Aciklama    = model.Aciklama?.Trim()
            });
        }
        else
        {
            var kayit = await _context.StokHareketleri.FirstOrDefaultAsync(h => h.Id == model.Id);
            if (kayit == null)
                return (false, "Kayıt bulunamadı.");

            kayit.HareketNo   = hareketNo;
            kayit.StokId      = model.StokId;
            kayit.HareketTipi = model.HareketTipi;
            kayit.Tarih       = model.Tarih;
            kayit.Miktar      = kaydedilecek;
            kayit.Aciklama    = model.Aciklama?.Trim();
            kayit.Aktif       = model.Aktif;
        }

        // Miktar kolonu yok; hareket kaydedildigi anda hesaplamaya dahil olur.
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Mesaj)> PasifeAlAsync(int id)
    {
        var hareket = await _context.StokHareketleri.FirstOrDefaultAsync(h => h.Id == id);
        if (hareket == null)
            return (false, "Kayıt bulunamadı.");

        if (!hareket.Aktif)
            return (false, "Kayıt zaten pasif durumda.");

        hareket.Aktif = false;
        await _context.SaveChangesAsync();

        // Bir girisi kaldirmak miktari negatife dusurebilir. Pasife alma
        // kuralimiz engellemek degil bildirmek; kullanici sonucu gorsun.
        var kalan = await _stokService.MiktarGetirAsync(hareket.StokId);

        return kalan < 0
            ? (true, $"Hareket pasife alındı. Dikkat: ürünün miktarı {kalan:N3} oldu, negatif.")
            : (true, null);
    }

    public async Task<(bool Basarili, string? Hata)> AktifYapAsync(int id)
    {
        var hareket = await _context.StokHareketleri.FirstOrDefaultAsync(h => h.Id == id);
        if (hareket == null)
            return (false, "Kayıt bulunamadı.");

        hareket.Aktif = true;
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
