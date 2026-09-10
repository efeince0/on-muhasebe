using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.DataAccess.Context;
using OnMuhasebe.Entities.Tables;
using OnMuhasebe.Entities.ViewModels;
using System.Security.Claims;

namespace OnMuhasebe.Business.Concrete;

public class CariIslemService : ICariIslemService
{
    private readonly OnMuhasebeContext _context;

    public CariIslemService(OnMuhasebeContext context, IHttpContextAccessor erisim)
    {
        _context = context;

        var idMetni = erisim.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idMetni, out var kullaniciId))
            _context.AktifKullaniciId = kullaniciId;
    }

    public async Task<CariIslemListeSonucu> ListeleAsync(
        int? cariId = null, DateTime? baslangic = null, DateTime? bitis = null,
        IslemTipi? islemTipi = null, bool sadeceAktif = true,
        int sayfaNo = 1, int sayfaBoyutu = 20,
        string sirala = "tarih", string yon = "desc")
    {
        if (sayfaNo < 1) sayfaNo = 1;
        if (sayfaBoyutu is < 5 or > 200) sayfaBoyutu = 20;

        var sorgu = _context.CariIslemler.AsNoTracking().AsQueryable();

        if (sadeceAktif)      sorgu = sorgu.Where(i => i.Aktif);
        if (cariId is > 0)    sorgu = sorgu.Where(i => i.CariId == cariId);
        if (baslangic != null) sorgu = sorgu.Where(i => i.Tarih >= baslangic);
        if (bitis != null)     sorgu = sorgu.Where(i => i.Tarih <= bitis);
        if (islemTipi != null) sorgu = sorgu.Where(i => i.IslemTipi == islemTipi);

        var toplam = await sorgu.CountAsync();

        // Toplamlar sayfaya degil, suzgecin tamamina ait olmali.
        var toplamTahsilat = await sorgu.Where(i => i.IslemTipi == IslemTipi.Tahsilat).SumAsync(i => (decimal?)i.Tutar) ?? 0;
        var toplamOdeme    = await sorgu.Where(i => i.IslemTipi == IslemTipi.Odeme).SumAsync(i => (decimal?)i.Tutar) ?? 0;

        var toplamSayfa = toplam == 0 ? 1 : (int)Math.Ceiling(toplam / (double)sayfaBoyutu);
        if (sayfaNo > toplamSayfa) sayfaNo = toplamSayfa;

        var projeksiyon = sorgu.Select(i => new CariIslemListeViewModel
        {
            Id         = i.Id,
            IslemNo    = i.IslemNo,
            Tarih      = i.Tarih,
            CariId     = i.CariId,
            CariKodu   = i.Cari.CariKodu,
            Unvan      = i.Cari.Unvan,
            IslemTipi  = i.IslemTipi,
            Tutar      = i.Tutar,
            OdemeSekli = i.OdemeSekli,
            Aciklama   = i.Aciklama,
            Aktif      = i.Aktif
        });

        var kayitlar = await SiralamaUygula(projeksiyon, sirala, yon)
            .Skip((sayfaNo - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .ToListAsync();

        return new CariIslemListeSonucu
        {
            Kayitlar       = kayitlar,
            ToplamKayit    = toplam,
            SayfaNo        = sayfaNo,
            SayfaBoyutu    = sayfaBoyutu,
            ToplamTahsilat = toplamTahsilat,
            ToplamOdeme    = toplamOdeme
        };
    }

    /// <summary>
    /// Siralama adres cubugundan geliyor; bilinmeyen deger varsayilana duser.
    /// Kolon adini dogrudan sorguya gecirmek yerine beyaz liste kullaniyoruz.
    /// </summary>
    private static IQueryable<CariIslemListeViewModel> SiralamaUygula(
        IQueryable<CariIslemListeViewModel> sorgu, string sirala, string yon)
    {
        var azalan = yon == "desc";

        return (sirala, azalan) switch
        {
            ("cari",  true)  => sorgu.OrderByDescending(x => x.Unvan),
            ("cari",  false) => sorgu.OrderBy(x => x.Unvan),
            ("tutar", true)  => sorgu.OrderByDescending(x => x.Tutar),
            ("tutar", false) => sorgu.OrderBy(x => x.Tutar),
            ("no",    true)  => sorgu.OrderByDescending(x => x.IslemNo),
            ("no",    false) => sorgu.OrderBy(x => x.IslemNo),

            // Ayni gune birden fazla islem girilebilir; Id ikincil sira olmadan
            // sayfalar arasinda kayit tekrar edebilir veya atlanabilir.
            (_,       false) => sorgu.OrderBy(x => x.Tarih).ThenBy(x => x.Id),
            _                => sorgu.OrderByDescending(x => x.Tarih).ThenByDescending(x => x.Id)
        };
    }

    public async Task<CariIslemFormViewModel?> FormGetirAsync(int id)
    {
        return await _context.CariIslemler
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new CariIslemFormViewModel
            {
                Id         = i.Id,
                IslemNo    = i.IslemNo,
                CariId     = i.CariId,
                IslemTipi  = i.IslemTipi,
                Tarih      = i.Tarih,
                Tutar      = i.Tutar,
                OdemeSekli = i.OdemeSekli,
                Aciklama   = i.Aciklama,
                Aktif      = i.Aktif
            })
            .FirstOrDefaultAsync();
    }

    public async Task<string> SonrakiIslemNoOnerAsync(IslemTipi islemTipi)
    {
        var onEk = islemTipi == IslemTipi.Tahsilat ? "TAH" : "ODE";

        var numaralar = await _context.CariIslemler
            .AsNoTracking()
            .Where(i => i.IslemNo.StartsWith(onEk))
            .Select(i => i.IslemNo)
            .ToListAsync();

        // "TAH000007" -> "000007" -> 7. Elle girilmis farkli bicimler elenir.
        var enBuyuk = numaralar
            .Select(n => n[onEk.Length..])
            .Where(son => son.Length > 0 && son.All(char.IsDigit))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();

        return $"{onEk}{enBuyuk + 1:D6}";
    }

    public async Task<(bool Basarili, string? Hata)> KaydetAsync(CariIslemFormViewModel model)
    {
        // Dokuman kurali: tutar sifir veya negatif olamaz.
        if (model.Tutar <= 0)
            return (false, "Tutar sıfırdan büyük olmalıdır.");

        var cari = await _context.Cariler
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == model.CariId);

        if (cari == null)
            return (false, "Seçilen cari bulunamadı.");

        // Pasif cariye yeni hareket girilmesi, kapatilmis hesabin yeniden
        // hareketlenmesi demek olur; mevcut kaydin duzeltilmesi ise serbest.
        if (!cari.Aktif && model.Id == 0)
            return (false, "Pasif cariye yeni işlem girilemez. Önce cariyi aktif edin.");

        var islemNo = model.IslemNo.Trim();

        var noKullanimda = await _context.CariIslemler
            .AnyAsync(i => i.IslemNo == islemNo && i.Id != model.Id);

        if (noKullanimda)
            return (false, "Bu işlem numarası başka bir kayıtta kullanılıyor.");

        if (model.Id == 0)
        {
            _context.CariIslemler.Add(new CariIslem
            {
                IslemNo    = islemNo,
                CariId     = model.CariId,
                IslemTipi  = model.IslemTipi,
                Tarih      = model.Tarih,
                Tutar      = model.Tutar,
                OdemeSekli = model.OdemeSekli,
                Aciklama   = model.Aciklama?.Trim()
            });
        }
        else
        {
            var mevcut = await _context.CariIslemler.FirstOrDefaultAsync(i => i.Id == model.Id);
            if (mevcut == null)
                return (false, "Kayıt bulunamadı.");

            mevcut.IslemNo    = islemNo;
            mevcut.CariId     = model.CariId;
            mevcut.IslemTipi  = model.IslemTipi;
            mevcut.Tarih      = model.Tarih;
            mevcut.Tutar      = model.Tutar;
            mevcut.OdemeSekli = model.OdemeSekli;
            mevcut.Aciklama   = model.Aciklama?.Trim();
            mevcut.Aktif      = model.Aktif;
        }

        // Bakiye kolonu yok; hareket kaydedildigi anda hesaplamaya dahil olur.
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Hata)> PasifeAlAsync(int id)
    {
        var islem = await _context.CariIslemler.FirstOrDefaultAsync(i => i.Id == id);
        if (islem == null)
            return (false, "Kayıt bulunamadı.");

        if (!islem.Aktif)
            return (false, "Kayıt zaten pasif durumda.");

        // Kalici silme yok; pasif kayit bakiye hesabina girmedigi icin
        // cari bakiyesi bu satirdan sonra kendiliginden duzelir.
        islem.Aktif = false;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Hata)> AktifYapAsync(int id)
    {
        var islem = await _context.CariIslemler.FirstOrDefaultAsync(i => i.Id == id);
        if (islem == null)
            return (false, "Kayıt bulunamadı.");

        islem.Aktif = true;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<EkstreViewModel?> EkstreGetirAsync(
        int cariId, DateTime? baslangic = null, DateTime? bitis = null)
    {
        var cari = await _context.Cariler
            .AsNoTracking()
            .Where(c => c.Id == cariId)
            .Select(c => new { c.Id, c.CariKodu, c.Unvan, c.AcilisBakiye })
            .FirstOrDefaultAsync();

        if (cari == null)
            return null;

        // Bitis suzgeci sorguya giriyor; baslangic girmiyor, cunku donem
        // oncesi hareketler devir bakiyesini olusturmak icin gerekli.
        var faturalar = await _context.Faturalar
            .AsNoTracking()
            .Where(f => f.CariId == cariId && f.Aktif && (bitis == null || f.Tarih <= bitis))
            .Select(f => new EkstreSatirViewModel
            {
                Tarih    = f.Tarih,
                Belge    = f.FaturaNo,
                Tur      = f.FaturaTipi == FaturaTipi.Satis ? "Satış Faturası" : "Alış Faturası",
                Aciklama = f.Aciklama,
                Borc     = f.FaturaTipi == FaturaTipi.Satis ? f.GenelToplam : 0,
                Alacak   = f.FaturaTipi == FaturaTipi.Alis  ? f.GenelToplam : 0
            })
            .ToListAsync();

        var islemler = await _context.CariIslemler
            .AsNoTracking()
            .Where(i => i.CariId == cariId && i.Aktif && (bitis == null || i.Tarih <= bitis))
            .Select(i => new EkstreSatirViewModel
            {
                Tarih    = i.Tarih,
                Belge    = i.IslemNo,
                Tur      = i.IslemTipi == IslemTipi.Tahsilat ? "Tahsilat" : "Ödeme",
                Aciklama = i.Aciklama,
                Borc     = i.IslemTipi == IslemTipi.Odeme    ? i.Tutar : 0,
                Alacak   = i.IslemTipi == IslemTipi.Tahsilat ? i.Tutar : 0
            })
            .ToListAsync();

        // Iki tablo tek zaman cizgisinde birlestiriliyor; siralama bellekte yapilir.
        var tumSatirlar = faturalar
            .Concat(islemler)
            .OrderBy(s => s.Tarih)
            .ThenBy(s => s.Belge)
            .ToList();

        var devir    = cari.AcilisBakiye;
        var satirlar = new List<EkstreSatirViewModel>();

        foreach (var satir in tumSatirlar)
        {
            if (baslangic != null && satir.Tarih < baslangic)
            {
                devir += satir.Borc - satir.Alacak;
                continue;
            }

            satirlar.Add(satir);
        }

        var yuruyen = devir;
        foreach (var satir in satirlar)
        {
            yuruyen += satir.Borc - satir.Alacak;
            satir.YuruyenBakiye = yuruyen;
        }

        return new EkstreViewModel
        {
            CariId      = cari.Id,
            CariKodu    = cari.CariKodu,
            Unvan       = cari.Unvan,
            Baslangic   = baslangic,
            Bitis       = bitis,
            DevirBakiye = devir,
            Satirlar    = satirlar
        };
    }
}
