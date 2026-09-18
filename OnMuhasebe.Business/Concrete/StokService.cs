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

    public async Task<StokListeSonucu> ListeleAsync(
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
            Id = s.Id,
            StokKodu = s.StokKodu,
            StokAdi = s.StokAdi,
            Kategori = s.Kategori,
            Birim = s.Birim,
            SatisFiyati = s.SatisFiyati,
            AlisFiyati = s.AlisFiyati,
            KritikStok = s.KritikStok,
            Aktif = s.Aktif,

            // Miktar kolonu yok: stok hareketleri ve fatura satirlarindan hesaplanir.
            // Giris ve alis faturasi artirir, cikis ve satis faturasi azaltir.
            // Sayim farki isaretli kaydedilir.
            MevcutMiktar =
                  s.Hareketler.Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Giris).Sum(h => h.Miktar)
                - s.Hareketler.Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Cikis).Sum(h => h.Miktar)
                + s.Hareketler.Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Sayim).Sum(h => h.Miktar)
                + s.FaturaSatirlari.Where(fs => fs.Aktif && fs.Fatura.Aktif && fs.Fatura.FaturaTipi == FaturaTipi.Alis).Sum(fs => fs.Miktar)
                - s.FaturaSatirlari.Where(fs => fs.Aktif && fs.Fatura.Aktif && fs.Fatura.FaturaTipi == FaturaTipi.Satis).Sum(fs => fs.Miktar)
        });

        // Kritik suzgeci hesaplanmis kolona bakiyor; projeksiyondan sonra uygulanmali.
        if (sadeceKritik)
            projeksiyon = projeksiyon.Where(x => x.KritikStok != null && x.MevcutMiktar <= x.KritikStok);

        var toplam = await projeksiyon.CountAsync();

        var toplamDeger = await projeksiyon.SumAsync(x => x.MevcutMiktar * x.AlisFiyati);
        var kritikSayisi = await projeksiyon.CountAsync(x => x.KritikStok != null && x.MevcutMiktar <= x.KritikStok);

        var toplamSayfa = toplam == 0 ? 1 : (int)Math.Ceiling(toplam / (double)sayfaBoyutu);
        if (sayfaNo > toplamSayfa) sayfaNo = toplamSayfa;

        var kayitlar = await SiralamaUygula(projeksiyon, sirala, yon)
            .Skip((sayfaNo - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .ToListAsync();

        return new StokListeSonucu
        {
            Kayitlar = kayitlar,
            ToplamKayit = toplam,
            SayfaNo = sayfaNo,
            SayfaBoyutu = sayfaBoyutu,
            ToplamStokDegeri = toplamDeger,
            KritikKartSayisi = kritikSayisi
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
            ("ad", true) => sorgu.OrderByDescending(x => x.StokAdi),
            ("ad", false) => sorgu.OrderBy(x => x.StokAdi),
            ("kategori", true) => sorgu.OrderByDescending(x => x.Kategori),
            ("kategori", false) => sorgu.OrderBy(x => x.Kategori),
            ("miktar", true) => sorgu.OrderByDescending(x => x.MevcutMiktar),
            ("miktar", false) => sorgu.OrderBy(x => x.MevcutMiktar),
            ("fiyat", true) => sorgu.OrderByDescending(x => x.SatisFiyati),
            ("fiyat", false) => sorgu.OrderBy(x => x.SatisFiyati),
            (_, true) => sorgu.OrderByDescending(x => x.StokKodu),
            _ => sorgu.OrderBy(x => x.StokKodu)
        };
    }

    public async Task<StokFormViewModel?> FormGetirAsync(int id)
    {
        return await _context.Stoklar
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StokFormViewModel
            {
                Id               = s.Id,
                StokKodu         = s.StokKodu,
                StokAdi          = s.StokAdi,
                Kategori         = s.Kategori,
                Birim            = s.Birim,
                AlisFiyati       = s.AlisFiyati,
                SatisFiyati      = s.SatisFiyati,
                KdvOrani         = s.KdvOrani,
                KritikStok       = s.KritikStok
            })
            .FirstOrDefaultAsync();
    }

    public async Task<StokDetayViewModel?> DetayGetirAsync(int id, int sonHareketSayisi = 10)
    {
        var model = await _context.Stoklar
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StokDetayViewModel
            {
                Id               = s.Id,
                StokKodu         = s.StokKodu,
                StokAdi          = s.StokAdi,
                Kategori         = s.Kategori,
                Birim            = s.Birim,
                AlisFiyati       = s.AlisFiyati,
                SatisFiyati      = s.SatisFiyati,
                KdvOrani         = s.KdvOrani,
                KritikStok       = s.KritikStok,
                Aktif            = s.Aktif,

                ToplamGiris = s.Hareketler
                    .Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Giris)
                    .Sum(h => h.Miktar),
                ToplamCikis = s.Hareketler
                    .Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Cikis)
                    .Sum(h => h.Miktar),
                ToplamSayim = s.Hareketler
                    .Where(h => h.Aktif && h.HareketTipi == StokHareketTipi.Sayim)
                    .Sum(h => h.Miktar),

                ToplamFaturaGiris = s.FaturaSatirlari
                    .Where(fs => fs.Aktif && fs.Fatura.Aktif && fs.Fatura.FaturaTipi == FaturaTipi.Alis)
                    .Sum(fs => fs.Miktar),
                ToplamFaturaCikis = s.FaturaSatirlari
                    .Where(fs => fs.Aktif && fs.Fatura.Aktif && fs.Fatura.FaturaTipi == FaturaTipi.Satis)
                    .Sum(fs => fs.Miktar),

                OlusturmaTarihi  = s.OlusturmaTarihi,
                GuncellemeTarihi = s.GuncellemeTarihi
            })
            .FirstOrDefaultAsync();

        if (model == null)
            return null;

        model.SonHareketler = await HareketDokumuAsync(id, sonHareketSayisi);
        model.HareketSayisi = await HareketSayisiAsync(id);

        return model;
    }

    private async Task<int> HareketSayisiAsync(int stokId)
    {
        var elle = await _context.StokHareketleri
            .CountAsync(h => h.StokId == stokId && h.Aktif);

        var faturali = await _context.FaturaSatirlari
            .CountAsync(fs => fs.StokId == stokId && fs.Aktif && fs.Fatura.Aktif);

        return elle + faturali;
    }

    /// <summary>
    /// Urunun hareket dokumu: elle girilen hareketler ve fatura satirlari
    /// tek zaman cizgisinde. Miktar formulu iki kaynaktan topladigi icin
    /// gecmis de iki kaynaktan okunmali; yoksa ekrandaki miktar ile listedeki
    /// hareketler birbirini tutmaz.
    /// </summary>
    private async Task<List<StokHareketSatirViewModel>> HareketDokumuAsync(int stokId, int sonHareketSayisi)
    {
        var hareketler = await _context.StokHareketleri
            .AsNoTracking()
            .Where(h => h.StokId == stokId && h.Aktif)
            .Select(h => new StokHareketSatirViewModel
            {
                Tarih    = h.Tarih,
                Belge    = h.HareketNo,
                Tur      = h.HareketTipi == StokHareketTipi.Giris ? "Giriş"
                         : h.HareketTipi == StokHareketTipi.Cikis ? "Çıkış"
                         : "Sayım",
                Aciklama = h.Aciklama,

                // Giris artirir, cikis azaltir; sayim farki zaten isaretli saklaniyor.
                Degisim  = h.HareketTipi == StokHareketTipi.Giris ?  h.Miktar
                         : h.HareketTipi == StokHareketTipi.Cikis ? -h.Miktar
                         : h.Miktar
            })
            .ToListAsync();

        var faturaSatirlari = await _context.FaturaSatirlari
            .AsNoTracking()
            .Where(fs => fs.StokId == stokId && fs.Aktif && fs.Fatura.Aktif)
            .Select(fs => new StokHareketSatirViewModel
            {
                Tarih    = fs.Fatura.Tarih,
                Belge    = fs.Fatura.FaturaNo,
                Tur      = fs.Fatura.FaturaTipi == FaturaTipi.Alis ? "Alış Faturası" : "Satış Faturası",
                Aciklama = fs.Fatura.Cari.Unvan,
                Degisim  = fs.Fatura.FaturaTipi == FaturaTipi.Alis ? fs.Miktar : -fs.Miktar
            })
            .ToListAsync();

        // Iki liste bellekte birlestiriliyor; kolonlari farkli oldugu icin
        // veritabaninda birlestirmek sorguyu okunmaz hale getirirdi.
        var tumu = hareketler
            .Concat(faturaSatirlari)
            .OrderBy(x => x.Tarih)
            .ThenBy(x => x.Belge)
            .ToList();

        // Yuruyen miktar bastan hesaplanir, sonra son N satir gosterilir.
        decimal yuruyen = 0;
        foreach (var satir in tumu)
        {
            yuruyen += satir.Degisim;
            satir.YuruyenMiktar = yuruyen;
        }

        return tumu
            .OrderByDescending(x => x.Tarih)
            .ThenByDescending(x => x.Belge)
            .Take(sonHareketSayisi)
            .ToList();
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
            // TryParse: sayi olmayan ya da int'e sigmayacak kadar uzun bir
            // son ek 0 sayilir. Parse olsaydi elle girilmis tek bir bozuk
            // numara, oneri ucunu herkes icin kalici olarak patlatirdi.
            .Select(son => int.TryParse(son, out var no) ? no : 0)
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
                StokKodu = model.StokKodu.Trim(),
                StokAdi = model.StokAdi.Trim(),
                Kategori = model.Kategori?.Trim(),
                Birim = model.Birim.Trim(),
                AlisFiyati = model.AlisFiyati,
                SatisFiyati = model.SatisFiyati,
                KdvOrani = model.KdvOrani,
                KritikStok = model.KritikStok,
                Aktif = true
            });
        }
        else
        {
            var mevcut = await _context.Stoklar.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (mevcut == null)
                return (false, "Güncellenecek kayıt bulunamadı.");

            mevcut.StokKodu = model.StokKodu.Trim();
            mevcut.StokAdi = model.StokAdi.Trim();
            mevcut.Kategori = model.Kategori?.Trim();
            mevcut.Birim = model.Birim.Trim();
            mevcut.AlisFiyati = model.AlisFiyati;
            mevcut.SatisFiyati = model.SatisFiyati;
            mevcut.KdvOrani = model.KdvOrani;
            mevcut.KritikStok = model.KritikStok;
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Mesaj)> PasifeAlAsync(int id)
    {
        var stok = await _context.Stoklar.FirstOrDefaultAsync(s => s.Id == id);
        if (stok == null)
            return (false, "Kayıt bulunamadı.");

        if (!stok.Aktif)
            return (false, "Kayıt zaten pasif durumda.");

        // Mevcut miktar pasife almayi engellemez; dokuman kalici silme yerine
        // pasife almayi oneriyor. Depodaki bakiye kullaniciya bildirilir.
        var miktar = await MiktarHesaplaAsync(id);

        // Kayitlar kalici silinmez; gecmis hareketlerin bagli oldugu kart korunur.
        stok.Aktif = false;
        await _context.SaveChangesAsync();

        return miktar != 0
            ? (true, $"Stok kartı pasife alındı. Dikkat: {miktar:N3} {stok.Birim} stok görünüyor.")
            : (true, null);
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

    public async Task<List<StokSecimViewModel>> SecimListesiAsync()
    {
        // Acilir listede yalnizca aktif kartlar; pasif karta yeni hareket girilmez.
        return await _context.Stoklar
            .AsNoTracking()
            .Where(s => s.Aktif)
            .OrderBy(s => s.StokKodu)
            .Select(s => new StokSecimViewModel
            {
                Id          = s.Id,
                StokKodu    = s.StokKodu,
                StokAdi     = s.StokAdi,
                Birim       = s.Birim,
                AlisFiyati  = s.AlisFiyati,
                SatisFiyati = s.SatisFiyati,
                KdvOrani    = s.KdvOrani
            })
            .ToListAsync();
    }

    /// <summary>
    /// Miktar formulunun tek sahibi burasi. Stok hareketleri ve fatura servisleri
    /// kendi kopyalarini yazmak yerine bunu cagirir; formul degisirse tek yer degisir.
    /// haricHareketId / haricFaturaId: o kaydi hesaba katma. Mevcut bir hareketi
    /// veya faturayi guncellerken "kendi etkisi disinda ne var" sorusunu cevaplar.
    /// </summary>
    public async Task<decimal> MiktarGetirAsync(
        int stokId, int? haricHareketId = null, int? haricFaturaId = null)
    {
        return await _context.Stoklar
            .AsNoTracking()
            .Where(s => s.Id == stokId)
            .Select(s =>
                  s.Hareketler.Where(h => h.Aktif && h.Id != haricHareketId && h.HareketTipi == StokHareketTipi.Giris).Sum(h => h.Miktar)
                - s.Hareketler.Where(h => h.Aktif && h.Id != haricHareketId && h.HareketTipi == StokHareketTipi.Cikis).Sum(h => h.Miktar)
                + s.Hareketler.Where(h => h.Aktif && h.Id != haricHareketId && h.HareketTipi == StokHareketTipi.Sayim).Sum(h => h.Miktar)
                + s.FaturaSatirlari.Where(fs => fs.Aktif && fs.Fatura.Aktif && fs.FaturaId != haricFaturaId && fs.Fatura.FaturaTipi == FaturaTipi.Alis).Sum(fs => fs.Miktar)
                - s.FaturaSatirlari.Where(fs => fs.Aktif && fs.Fatura.Aktif && fs.FaturaId != haricFaturaId && fs.Fatura.FaturaTipi == FaturaTipi.Satis).Sum(fs => fs.Miktar))
            // Olmayan bir stokId icin FirstAsync istisna atip 500 dondururdu;
            // miktar sorusunun dogru cevabi bu durumda sifir.
            .FirstOrDefaultAsync();
    }

    private Task<decimal> MiktarHesaplaAsync(int stokId) => MiktarGetirAsync(stokId);
}
