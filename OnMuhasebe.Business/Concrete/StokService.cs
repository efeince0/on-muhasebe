using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.DataAccess.Context;
using OnMuhasebe.Entities.Tables;
using OnMuhasebe.Entities.ViewModels;
using System.Security.Claims;

namespace OnMuhasebe.Business.Concrete;

public class StokService : IStokService
{
    private readonly OnMuhasebeContext _context;

    public StokService(OnMuhasebeContext context, IHttpContextAccessor erisim)
    {
        var idMetni = erisim.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _context = context;

        if (int.TryParse(idMetni, out var kullaniciId))
            _context.AktifKullaniciId = kullaniciId;
    }

    public async Task<SayfaliListe<StokListeViewModel>> ListeleAsync(
        string? arama = null, bool sadeceAktif = true, bool sadeceKritik = false,
        int sayfaNo = 1, int sayfaBoyutu = 20,
        string sirala = "kod", string yon = "asc")
    {
        if (sayfaNo < 1) sayfaNo = 1;
        if (sayfaBoyutu is < 5 or > 200) sayfaBoyutu = 20;

        var sorgu = _context.Stoklar.AsNoTracking().AsQueryable();

        if (sadeceAktif)
            sorgu = sorgu.Where(s => s.Aktif);

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var a = arama.Trim();
            sorgu = sorgu.Where(s =>
                s.StokKodu.Contains(a) || s.StokAdi.Contains(a) ||
                (s.Kategori != null && s.Kategori.Contains(a)));
        }

        var projeksiyon = sorgu.Select(s => new StokListeViewModel
        {
            Id          = s.Id,
            StokKodu    = s.StokKodu,
            StokAdi     = s.StokAdi,
            Kategori    = s.Kategori,
            Birim       = s.Birim,
            SatisFiyati = s.SatisFiyati,
            KritikStok  = s.KritikStok,
            Aktif       = s.Aktif,

            // Giris artirir, Cikis azaltir. Sayim farki isaretli kaydedilir.
            MevcutMiktar =
                  s.Hareketler.Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Giris).Sum(h => h.Miktar)
                - s.Hareketler.Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Cikis).Sum(h => h.Miktar)
                + s.Hareketler.Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Sayim).Sum(h => h.Miktar)
        });

        // Kritik suzgeci hesaplanmis kolona bakiyor; projeksiyondan sonra uygulanmali.
        if (sadeceKritik)
            projeksiyon = projeksiyon.Where(x => x.KritikStok != null && x.MevcutMiktar <= x.KritikStok);

        var toplam = await projeksiyon.CountAsync();

        var toplamSayfa = toplam == 0 ? 1 : (int)Math.Ceiling(toplam / (double)sayfaBoyutu);
        if (sayfaNo > toplamSayfa) sayfaNo = toplamSayfa;

        var kayitlar = await SiralamaUygula(projeksiyon, sirala, yon)
            .Skip((sayfaNo - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .ToListAsync();

        return new SayfaliListe<StokListeViewModel>
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
    private static IQueryable<StokListeViewModel> SiralamaUygula(
        IQueryable<StokListeViewModel> sorgu, string sirala, string yon)
    {
        var azalan = yon == "desc";

        return (sirala, azalan) switch
        {
            ("ad",       true)  => sorgu.OrderByDescending(x => x.StokAdi),
            ("ad",       false) => sorgu.OrderBy(x => x.StokAdi),
            ("kategori", true)  => sorgu.OrderByDescending(x => x.Kategori),
            ("kategori", false) => sorgu.OrderBy(x => x.Kategori),
            ("miktar",   true)  => sorgu.OrderByDescending(x => x.MevcutMiktar),
            ("miktar",   false) => sorgu.OrderBy(x => x.MevcutMiktar),
            ("fiyat",    true)  => sorgu.OrderByDescending(x => x.SatisFiyati),
            ("fiyat",    false) => sorgu.OrderBy(x => x.SatisFiyati),
            (_,          true)  => sorgu.OrderByDescending(x => x.StokKodu),
            _                   => sorgu.OrderBy(x => x.StokKodu)
        };
    }

    public async Task<StokFormViewModel?> FormGetirAsync(int id)
    {
        return await _context.Stoklar
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StokFormViewModel
            {
                Id          = s.Id,
                StokKodu    = s.StokKodu,
                StokAdi     = s.StokAdi,
                Kategori    = s.Kategori,
                Birim       = s.Birim,
                AlisFiyati  = s.AlisFiyati,
                SatisFiyati = s.SatisFiyati,
                KdvOrani    = s.KdvOrani,
                KritikStok  = s.KritikStok,
                Aktif       = s.Aktif
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<string>> KategorileriGetirAsync()
    {
        return await _context.Stoklar
            .AsNoTracking()
            .Where(s => s.Kategori != null && s.Kategori != "")
            .Select(s => s.Kategori!)
            .Distinct()
            .OrderBy(k => k)
            .ToListAsync();
    }

    public async Task<string> SonrakiKodOnerAsync(string onEk = "S")
    {
        var kodlar = await _context.Stoklar
            .AsNoTracking()
            .Where(s => s.StokKodu.StartsWith(onEk))
            .Select(s => s.StokKodu)
            .ToListAsync();

        var enBuyuk = kodlar
            .Select(k => k[onEk.Length..])
            .Where(son => son.Length > 0 && son.All(char.IsDigit))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();

        return $"{onEk}{enBuyuk + 1:D4}";
    }

    public async Task<(bool Basarili, string? Hata)> KaydetAsync(StokFormViewModel model)
    {
        var kodKullanimda = await _context.Stoklar
            .AnyAsync(s => s.StokKodu == model.StokKodu && s.Id != model.Id);

        if (kodKullanimda)
            return (false, "Bu stok kodu başka bir kayıtta kullanılıyor.");

        // Zarar uyarisi degil, engel: satis alistan dusukse fiyat girisi hatalidir.
        if (model.SatisFiyati > 0 && model.AlisFiyati > model.SatisFiyati)
            return (false, "Alış fiyatı satış fiyatından yüksek olamaz.");

        if (model.Id == 0)
        {
            _context.Stoklar.Add(new Stok
            {
                StokKodu    = model.StokKodu.Trim(),
                StokAdi     = model.StokAdi.Trim(),
                Kategori    = model.Kategori?.Trim(),
                Birim       = model.Birim.Trim(),
                AlisFiyati  = model.AlisFiyati,
                SatisFiyati = model.SatisFiyati,
                KdvOrani    = model.KdvOrani,
                KritikStok  = model.KritikStok,
                Aktif       = true
            });
        }
        else
        {
            var mevcut = await _context.Stoklar.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (mevcut == null)
                return (false, "Güncellenecek kayıt bulunamadı.");

            mevcut.StokKodu    = model.StokKodu.Trim();
            mevcut.StokAdi     = model.StokAdi.Trim();
            mevcut.Kategori    = model.Kategori?.Trim();
            mevcut.Birim       = model.Birim.Trim();
            mevcut.AlisFiyati  = model.AlisFiyati;
            mevcut.SatisFiyati = model.SatisFiyati;
            mevcut.KdvOrani    = model.KdvOrani;
            mevcut.KritikStok  = model.KritikStok;
            mevcut.Aktif       = model.Aktif;
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Hata)> PasifeAlAsync(int id)
    {
        var stok = await _context.Stoklar.FirstOrDefaultAsync(s => s.Id == id);
        if (stok == null)
            return (false, "Kayıt bulunamadı.");

        if (!stok.Aktif)
            return (false, "Kayıt zaten pasif durumda.");

        // Depo kurali: uzerinde mal duran kart kapatilamaz, once sayim/cikis yapilmali.
        var miktar = await MiktarHesaplaAsync(id);
        if (miktar != 0)
            return (false, $"Stoğu sıfır olmayan kart pasife alınamaz. Mevcut miktar: {miktar:N3} {stok.Birim}");

        stok.Aktif = false;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Hata)> AktifYapAsync(int id)
    {
        var stok = await _context.Stoklar.FirstOrDefaultAsync(s => s.Id == id);
        if (stok == null)
            return (false, "Kayıt bulunamadı.");

        stok.Aktif = true;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private async Task<decimal> MiktarHesaplaAsync(int stokId)
    {
        return await _context.Stoklar
            .AsNoTracking()
            .Where(s => s.Id == stokId)
            .Select(s =>
                  s.Hareketler.Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Giris).Sum(h => h.Miktar)
                - s.Hareketler.Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Cikis).Sum(h => h.Miktar)
                + s.Hareketler.Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Sayim).Sum(h => h.Miktar))
            .FirstAsync();
    }
}
