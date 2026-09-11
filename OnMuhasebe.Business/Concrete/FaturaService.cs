using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.DataAccess.Context;
using OnMuhasebe.Entities.Tables;
using OnMuhasebe.Entities.ViewModels;
using System.Security.Claims;

namespace OnMuhasebe.Business.Concrete;

public class FaturaService : IFaturaService
{
    private readonly OnMuhasebeContext _context;
    private readonly IStokService      _stokService;

    public FaturaService(
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

    public async Task<FaturaListeSonucu> ListeleAsync(
        FaturaTipi? faturaTipi = null, int? cariId = null,
        DateTime? baslangic = null, DateTime? bitis = null,
        decimal? minTutar = null, decimal? maxTutar = null,
        bool sadeceAktif = true, int sayfaNo = 1, int sayfaBoyutu = 20,
        string sirala = "tarih", string yon = "desc")
    {
        if (sayfaNo < 1) sayfaNo = 1;
        if (sayfaBoyutu is < 5 or > 200) sayfaBoyutu = 20;

        var sorgu = _context.Faturalar.AsNoTracking().AsQueryable();

        if (sadeceAktif)        sorgu = sorgu.Where(f => f.Aktif);
        if (faturaTipi != null) sorgu = sorgu.Where(f => f.FaturaTipi == faturaTipi);
        if (cariId is > 0)      sorgu = sorgu.Where(f => f.CariId == cariId);
        if (baslangic != null)  sorgu = sorgu.Where(f => f.Tarih >= baslangic);
        if (bitis != null)      sorgu = sorgu.Where(f => f.Tarih <= bitis);
        if (minTutar != null)   sorgu = sorgu.Where(f => f.GenelToplam >= minTutar);
        if (maxTutar != null)   sorgu = sorgu.Where(f => f.GenelToplam <= maxTutar);

        var toplam = await sorgu.CountAsync();

        // Toplamlar sayfaya degil, suzgecin tamamina ait olmali.
        var toplamAlis  = await sorgu.Where(f => f.FaturaTipi == FaturaTipi.Alis).SumAsync(f => (decimal?)f.GenelToplam) ?? 0;
        var toplamSatis = await sorgu.Where(f => f.FaturaTipi == FaturaTipi.Satis).SumAsync(f => (decimal?)f.GenelToplam) ?? 0;

        var toplamSayfa = toplam == 0 ? 1 : (int)Math.Ceiling(toplam / (double)sayfaBoyutu);
        if (sayfaNo > toplamSayfa) sayfaNo = toplamSayfa;

        var projeksiyon = sorgu.Select(f => new FaturaListeViewModel
        {
            Id          = f.Id,
            FaturaNo    = f.FaturaNo,
            FaturaTipi  = f.FaturaTipi,
            Tarih       = f.Tarih,
            CariId      = f.CariId,
            CariKodu    = f.Cari.CariKodu,
            Unvan       = f.Cari.Unvan,
            AraToplam   = f.AraToplam,
            ToplamKdv   = f.ToplamKdv,
            GenelToplam = f.GenelToplam,
            SatirSayisi = f.Satirlar.Count(sa => sa.Aktif),
            Aktif       = f.Aktif
        });

        var kayitlar = await SiralamaUygula(projeksiyon, sirala, yon)
            .Skip((sayfaNo - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .ToListAsync();

        return new FaturaListeSonucu
        {
            Kayitlar    = kayitlar,
            ToplamKayit = toplam,
            SayfaNo     = sayfaNo,
            SayfaBoyutu = sayfaBoyutu,
            ToplamAlis  = toplamAlis,
            ToplamSatis = toplamSatis
        };
    }

    /// <summary>
    /// Siralama adres cubugundan geliyor; bilinmeyen deger varsayilana duser.
    /// Kolon adini dogrudan sorguya gecirmek yerine beyaz liste kullaniyoruz.
    /// </summary>
    private static IQueryable<FaturaListeViewModel> SiralamaUygula(
        IQueryable<FaturaListeViewModel> sorgu, string sirala, string yon)
    {
        var azalan = yon == "desc";

        return (sirala, azalan) switch
        {
            ("cari",  true)  => sorgu.OrderByDescending(x => x.Unvan),
            ("cari",  false) => sorgu.OrderBy(x => x.Unvan),
            ("tutar", true)  => sorgu.OrderByDescending(x => x.GenelToplam),
            ("tutar", false) => sorgu.OrderBy(x => x.GenelToplam),
            ("no",    true)  => sorgu.OrderByDescending(x => x.FaturaNo),
            ("no",    false) => sorgu.OrderBy(x => x.FaturaNo),

            // Ayni gune birden fazla fatura kesilebilir; Id ikincil sira olmadan
            // sayfalar arasinda kayit tekrar edebilir veya atlanabilir.
            (_,       false) => sorgu.OrderBy(x => x.Tarih).ThenBy(x => x.Id),
            _                => sorgu.OrderByDescending(x => x.Tarih).ThenByDescending(x => x.Id)
        };
    }

    public async Task<FaturaFormViewModel?> FormGetirAsync(int id)
    {
        return await _context.Faturalar
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FaturaFormViewModel
            {
                Id         = f.Id,
                FaturaNo   = f.FaturaNo,
                FaturaTipi = f.FaturaTipi,
                CariId     = f.CariId,
                Tarih      = f.Tarih,
                Aciklama   = f.Aciklama,
                Aktif      = f.Aktif,

                Satirlar = f.Satirlar
                    .Where(sa => sa.Aktif)
                    .OrderBy(sa => sa.Id)
                    .Select(sa => new FaturaSatirFormViewModel
                    {
                        Id         = sa.Id,
                        StokId     = sa.StokId,
                        Miktar     = sa.Miktar,
                        BirimFiyat = sa.BirimFiyat,
                        KdvOrani   = sa.KdvOrani
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<string> SonrakiFaturaNoOnerAsync(FaturaTipi faturaTipi)
    {
        var onEk = faturaTipi == FaturaTipi.Alis ? "ALF" : "SAT";

        var numaralar = await _context.Faturalar
            .AsNoTracking()
            .Where(f => f.FaturaNo.StartsWith(onEk))
            .Select(f => f.FaturaNo)
            .ToListAsync();

        // "SAT000007" -> "000007" -> 7. Elle girilmis farkli bicimler elenir.
        var enBuyuk = numaralar
            .Select(n => n[onEk.Length..])
            .Where(son => son.Length > 0 && son.All(char.IsDigit))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();

        return $"{onEk}{enBuyuk + 1:D6}";
    }

    public async Task<(bool Basarili, string? Hata)> KaydetAsync(FaturaFormViewModel model)
    {
        // Urun secilmemis satirlar kullanicinin doldurmadigi bos satirlardir, elenir.
        var satirlar = model.Satirlar
            .Where(sa => sa.StokId > 0)
            .ToList();

        // Dokuman kurali: bir faturada en az bir urun satiri bulunmalidir.
        if (satirlar.Count == 0)
            return (false, "Faturada en az bir ürün satırı olmalıdır.");

        // Urun secilip miktar girilmemis satir sessizce dusurulmez; kullanici
        // urunu faturaya koydugunu sanip eksik tutar odenmesine yol acabilir.
        if (satirlar.Any(sa => sa.Miktar <= 0))
            return (false, "Satır miktarı sıfırdan büyük olmalıdır.");

        if (satirlar.Any(sa => sa.BirimFiyat < 0))
            return (false, "Birim fiyat negatif olamaz.");

        if (satirlar.Any(sa => sa.KdvOrani is < 0 or > 100))
            return (false, "KDV oranı 0 ile 100 arasında olmalıdır.");

        var cari = await _context.Cariler
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == model.CariId);

        if (cari == null)
            return (false, "Seçilen cari bulunamadı.");

        if (!cari.Aktif && model.Id == 0)
            return (false, "Pasif cariye yeni fatura kesilemez. Önce cariyi aktif edin.");

        var faturaNo = model.FaturaNo.Trim();

        // Dokuman kurali: fatura numarasi benzersiz olmali ve tekrar edememelidir.
        var noKullanimda = await _context.Faturalar
            .AnyAsync(f => f.FaturaNo == faturaNo && f.Id != model.Id);

        if (noKullanimda)
            return (false, "Bu fatura numarası başka bir kayıtta kullanılıyor.");

        var stokIdler = satirlar.Select(sa => sa.StokId).Distinct().ToList();

        var stoklar = await _context.Stoklar
            .AsNoTracking()
            .Where(s => stokIdler.Contains(s.Id))
            .ToListAsync();

        if (stoklar.Count != stokIdler.Count)
            return (false, "Satırlardan birinde geçersiz ürün seçilmiş.");

        if (model.FaturaTipi == FaturaTipi.Satis)
        {
            var hata = await YetersizStokMesajiAsync(satirlar, stoklar, model.Id);
            if (hata != null)
                return (false, hata);
        }

        // Toplamlar her zaman sunucuda hesaplanir; istemciden gelen tutara guvenilmez.
        var araToplam = satirlar.Sum(sa => sa.Miktar * sa.BirimFiyat);
        var toplamKdv = satirlar.Sum(sa => sa.Miktar * sa.BirimFiyat * sa.KdvOrani / 100);

        Fatura fatura;

        if (model.Id == 0)
        {
            fatura = new Fatura();
            _context.Faturalar.Add(fatura);
        }
        else
        {
            var mevcut = await _context.Faturalar
                .Include(f => f.Satirlar)
                .FirstOrDefaultAsync(f => f.Id == model.Id);

            if (mevcut == null)
                return (false, "Kayıt bulunamadı.");

            // Guncellemede eski satirlar silinip yenileri yazilir. Satir baglamsiz
            // bir kayit degil; faturanin parcasi.
            // ToList() sart: RemoveRange listeyi gezerken EF degisiklik takibi
            // ayni koleksiyonu duzeltiyor ve "collection was modified" firlatiyor.
            _context.FaturaSatirlari.RemoveRange(mevcut.Satirlar.ToList());
            fatura = mevcut;
        }

        fatura.FaturaNo    = faturaNo;
        fatura.FaturaTipi  = model.FaturaTipi;
        fatura.CariId      = model.CariId;
        fatura.Tarih       = model.Tarih;
        fatura.Aciklama    = model.Aciklama?.Trim();
        fatura.AraToplam   = araToplam;
        fatura.ToplamKdv   = toplamKdv;
        fatura.GenelToplam = araToplam + toplamKdv;

        if (model.Id != 0)
            fatura.Aktif = model.Aktif;

        foreach (var satir in satirlar)
        {
            fatura.Satirlar.Add(new FaturaSatir
            {
                StokId      = satir.StokId,
                Miktar      = satir.Miktar,
                BirimFiyat  = satir.BirimFiyat,
                KdvOrani    = satir.KdvOrani,
                SatirTutari = satir.Miktar * satir.BirimFiyat
            });
        }

        // Tek SaveChanges: EF bunu tek transaction'da calistirir. Ust bilgi,
        // silinen satirlar ve yeni satirlar ya hep birlikte yazilir ya hicbiri.
        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Satis faturasinda stok yeterliligini kontrol eder. Sorun varsa hata
    /// mesajini, yoksa null doner. Ayni urun birden fazla satirda olabilecegi
    /// icin once urun bazinda toplanir; satir satir bakmak 6 + 6 adetlik iki
    /// satiri 10 adetlik stoktan gecirirdi.
    /// </summary>
    private async Task<string?> YetersizStokMesajiAsync(
        List<FaturaSatirFormViewModel> satirlar, List<Stok> stoklar, int faturaId)
    {
        var istenenler = satirlar
            .GroupBy(sa => sa.StokId)
            .Select(g => new { StokId = g.Key, Miktar = g.Sum(sa => sa.Miktar) });

        foreach (var istenen in istenenler)
        {
            // Guncellemede faturanin kendi satirlari hesaba katilmaz; yoksa
            // fatura kendi cikardigi mali eksik gorup kendini reddeder.
            var mevcut = await _stokService.MiktarGetirAsync(
                istenen.StokId, haricFaturaId: faturaId == 0 ? null : faturaId);

            if (istenen.Miktar > mevcut)
            {
                var stok = stoklar.First(s => s.Id == istenen.StokId);
                return $"Yetersiz stok: {stok.StokAdi}. Mevcut: {mevcut:N3} {stok.Birim}, faturada istenen: {istenen.Miktar:N3} {stok.Birim}";
            }
        }

        return null;
    }

    public async Task<(bool Basarili, string? Hata)> PasifeAlAsync(int id)
    {
        var fatura = await _context.Faturalar.FirstOrDefaultAsync(f => f.Id == id);
        if (fatura == null)
            return (false, "Kayıt bulunamadı.");

        if (!fatura.Aktif)
            return (false, "Kayıt zaten pasif durumda.");

        // Satirlara dokunmuyoruz: bakiye ve miktar formulleri zaten
        // fs.Fatura.Aktif sartini tasiyor, ust bilgi pasife alininca
        // hem cari bakiyesi hem stok miktari kendiliginden geri doner.
        fatura.Aktif = false;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Mesaj)> AktifYapAsync(int id)
    {
        var fatura = await _context.Faturalar
            .Include(f => f.Satirlar)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fatura == null)
            return (false, "Kayıt bulunamadı.");

        fatura.Aktif = true;
        await _context.SaveChangesAsync();

        // Pasifken baska cikislar yapilmis olabilir; geri acilinca stok
        // negatife dusebilir. Engellemiyoruz ama kullanici sonucu gorsun.
        if (fatura.FaturaTipi != FaturaTipi.Satis)
            return (true, null);

        var stokIdler = fatura.Satirlar.Where(sa => sa.Aktif).Select(sa => sa.StokId).Distinct();

        foreach (var stokId in stokIdler)
        {
            var kalan = await _stokService.MiktarGetirAsync(stokId);
            if (kalan < 0)
                return (true, "Fatura yeniden aktif edildi. Dikkat: bazı ürünlerin miktarı negatife düştü.");
        }

        return (true, null);
    }
}
