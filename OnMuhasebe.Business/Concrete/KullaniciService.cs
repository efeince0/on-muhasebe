using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.DataAccess.Context;
using OnMuhasebe.Entities.Tables;
using OnMuhasebe.Entities.ViewModels;
using System.Security.Claims;

namespace OnMuhasebe.Business.Concrete;

public class KullaniciService : IKullaniciService
{
    private readonly OnMuhasebeContext _context;
    private readonly PasswordHasher<Kullanici> _hasher = new();

    public KullaniciService(OnMuhasebeContext context, IHttpContextAccessor erisim)
    {
        _context = context;

        var idMetni = erisim.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idMetni, out var kullaniciId))
            _context.AktifKullaniciId = kullaniciId;
    }

    /// <summary>
    /// "Yonetici" burada rol ADINA degil YETKISINE gore tanimlaniyor: kullanici
    /// kaydi guncelleyebilen rol. Rol adlari yonetim ekranindan degistirilebildigi
    /// icin kurali "Yönetici" metnine baglamak kirilgan olurdu.
    /// </summary>
    private IQueryable<Kullanici> YonetebilenAktifler() =>
        _context.Kullanicilar.Where(k =>
            k.Aktif &&
            k.Rol.Aktif &&
            k.Rol.Izinler.Any(i => i.Aktif && i.IzinVar &&
                                   i.Modul == Modul.Kullanici &&
                                   i.Islem == Islem.Guncelle));

    public async Task<SayfaliListe<KullaniciListeViewModel>> ListeleAsync(
        string? arama = null, bool sadeceAktif = true,
        int sayfaNo = 1, int sayfaBoyutu = 20,
        string sirala = "kullaniciAdi", string yon = "asc")
    {
        if (sayfaNo < 1) sayfaNo = 1;
        if (sayfaBoyutu is < 5 or > 200) sayfaBoyutu = 20;

        var sorgu = _context.Kullanicilar.AsNoTracking().AsQueryable();

        if (sadeceAktif)
            sorgu = sorgu.Where(k => k.Aktif);

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var a = arama.Trim();
            sorgu = sorgu.Where(k => k.KullaniciAdi.Contains(a) || k.AdSoyad.Contains(a));
        }

        var toplam = await sorgu.CountAsync();

        var toplamSayfa = toplam == 0 ? 1 : (int)Math.Ceiling(toplam / (double)sayfaBoyutu);
        if (sayfaNo > toplamSayfa) sayfaNo = toplamSayfa;

        // Tek yonetici kaldiysa listede o satirin dugmeleri gizlenecek.
        var yonetenSayisi = await YonetebilenAktifler().CountAsync();
        var yonetenIdler  = await YonetebilenAktifler().Select(k => k.Id).ToListAsync();

        var projeksiyon = sorgu.Select(k => new KullaniciListeViewModel
        {
            Id           = k.Id,
            KullaniciAdi = k.KullaniciAdi,
            AdSoyad      = k.AdSoyad,
            Eposta       = k.Eposta,
            RolAdi       = k.Rol.RolAdi,
            SonGiris     = k.SonGiris,
            Aktif        = k.Aktif
        });

        var kayitlar = await SiralamaUygula(projeksiyon, sirala, yon)
            .Skip((sayfaNo - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .ToListAsync();

        foreach (var kayit in kayitlar)
            kayit.SonYonetici = yonetenSayisi == 1 && yonetenIdler.Contains(kayit.Id);

        return new SayfaliListe<KullaniciListeViewModel>
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
    private static IQueryable<KullaniciListeViewModel> SiralamaUygula(
        IQueryable<KullaniciListeViewModel> sorgu, string sirala, string yon)
    {
        var azalan = yon == "desc";

        return (sirala, azalan) switch
        {
            ("adSoyad",  true)  => sorgu.OrderByDescending(x => x.AdSoyad),
            ("adSoyad",  false) => sorgu.OrderBy(x => x.AdSoyad),
            ("rol",      true)  => sorgu.OrderByDescending(x => x.RolAdi),
            ("rol",      false) => sorgu.OrderBy(x => x.RolAdi),
            ("sonGiris", true)  => sorgu.OrderByDescending(x => x.SonGiris),
            ("sonGiris", false) => sorgu.OrderBy(x => x.SonGiris),
            (_,          true)  => sorgu.OrderByDescending(x => x.KullaniciAdi),
            _                   => sorgu.OrderBy(x => x.KullaniciAdi)
        };
    }

    public async Task<KullaniciFormViewModel?> FormGetirAsync(int id)
    {
        return await _context.Kullanicilar
            .AsNoTracking()
            .Where(k => k.Id == id)
            .Select(k => new KullaniciFormViewModel
            {
                Id           = k.Id,
                KullaniciAdi = k.KullaniciAdi,
                AdSoyad      = k.AdSoyad,
                Eposta       = k.Eposta,
                RolId        = k.RolId,
                Aktif        = k.Aktif
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SifreSifirlaViewModel?> SifreFormuGetirAsync(int id)
    {
        return await _context.Kullanicilar
            .AsNoTracking()
            .Where(k => k.Id == id)
            .Select(k => new SifreSifirlaViewModel
            {
                Id           = k.Id,
                KullaniciAdi = k.KullaniciAdi,
                AdSoyad      = k.AdSoyad,
                YeniSifre    = string.Empty,
                YeniSifreTekrar = string.Empty
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<RolSecimViewModel>> RolSecimListesiAsync()
    {
        return await _context.Roller
            .AsNoTracking()
            .Where(r => r.Aktif)
            .OrderBy(r => r.RolAdi)
            .Select(r => new RolSecimViewModel
            {
                Id       = r.Id,
                RolAdi   = r.RolAdi,
                Aciklama = r.Aciklama
            })
            .ToListAsync();
    }

    public async Task<(bool Basarili, string? Hata)> KaydetAsync(KullaniciFormViewModel model)
    {
        var kullaniciAdi = model.KullaniciAdi.Trim();

        // Dokuman kurali: kullanici adi benzersiz olmalidir.
        var adKullanimda = await _context.Kullanicilar
            .AnyAsync(k => k.KullaniciAdi == kullaniciAdi && k.Id != model.Id);

        if (adKullanimda)
            return (false, "Bu kullanıcı adı başka bir hesapta kullanılıyor.");

        var rol = await _context.Roller.AsNoTracking().FirstOrDefaultAsync(r => r.Id == model.RolId);
        if (rol == null)
            return (false, "Seçilen rol bulunamadı.");

        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Sifre))
                return (false, "Yeni kullanıcı için şifre zorunludur.");

            var yeni = new Kullanici
            {
                KullaniciAdi = kullaniciAdi,
                AdSoyad      = model.AdSoyad.Trim(),
                Eposta       = model.Eposta?.Trim(),
                RolId        = model.RolId
            };

            // Sifre asla duz metin saklanmaz; hasher salt uretip hash'in icine gomer.
            yeni.SifreHash = _hasher.HashPassword(yeni, model.Sifre);

            _context.Kullanicilar.Add(yeni);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        var mevcut = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id == model.Id);
        if (mevcut == null)
            return (false, "Kayıt bulunamadı.");

        // Dokuman [352]: sistem kullanici yonetebilen tek hesabini kaybetmemeli.
        // Pasife almak kadar rolu yetkisiz bir role cevirmek de ayni sonucu dogurur.
        var hata = await SonYoneticiKorumasi(mevcut.Id, model.RolId, model.Aktif);
        if (hata != null)
            return (false, hata);

        mevcut.KullaniciAdi = kullaniciAdi;
        mevcut.AdSoyad      = model.AdSoyad.Trim();
        mevcut.Eposta       = model.Eposta?.Trim();
        mevcut.RolId        = model.RolId;
        mevcut.Aktif        = model.Aktif;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Verilen degisiklik uygulandiktan sonra kullanici yonetebilen aktif hesap
    /// kalmayacaksa hata mesaji doner, sorun yoksa null.
    /// </summary>
    private async Task<string?> SonYoneticiKorumasi(int kullaniciId, int yeniRolId, bool yeniAktif)
    {
        var yonetenIdler = await YonetebilenAktifler().Select(k => k.Id).ToListAsync();

        // Bu hesap zaten yonetici degilse, onu degistirmek kimseyi etkilemez.
        if (!yonetenIdler.Contains(kullaniciId))
            return null;

        if (yonetenIdler.Count > 1)
            return null;

        var yeniRolYonetebilir = await _context.Roller
            .AnyAsync(r => r.Id == yeniRolId && r.Aktif &&
                           r.Izinler.Any(i => i.Aktif && i.IzinVar &&
                                              i.Modul == Modul.Kullanici &&
                                              i.Islem == Islem.Guncelle));

        if (yeniAktif && yeniRolYonetebilir)
            return null;

        return "Bu, kullanıcı yönetebilen son aktif hesap. Pasife alınamaz ve yetkisiz bir role taşınamaz. Önce başka bir yönetici hesabı tanımlayın.";
    }

    public async Task<(bool Basarili, string? Hata)> SifreSifirlaAsync(SifreSifirlaViewModel model)
    {
        var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id == model.Id);
        if (kullanici == null)
            return (false, "Kayıt bulunamadı.");

        kullanici.SifreHash = _hasher.HashPassword(kullanici, model.YeniSifre);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Hata)> SifreDegistirAsync(
        int kullaniciId, SifreDegistirViewModel model)
    {
        var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id == kullaniciId);
        if (kullanici == null)
            return (false, "Kayıt bulunamadı.");

        var sonuc = _hasher.VerifyHashedPassword(kullanici, kullanici.SifreHash, model.MevcutSifre);
        if (sonuc == PasswordVerificationResult.Failed)
            return (false, "Mevcut şifre hatalı.");

        if (model.MevcutSifre == model.YeniSifre)
            return (false, "Yeni şifre eskisiyle aynı olamaz.");

        kullanici.SifreHash = _hasher.HashPassword(kullanici, model.YeniSifre);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Hata)> PasifeAlAsync(int id)
    {
        var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id == id);
        if (kullanici == null)
            return (false, "Kayıt bulunamadı.");

        if (!kullanici.Aktif)
            return (false, "Kayıt zaten pasif durumda.");

        var hata = await SonYoneticiKorumasi(id, kullanici.RolId, yeniAktif: false);
        if (hata != null)
            return (false, hata);

        // Dokuman [351]: kullanici silmek yerine pasife almak tercih edilir,
        // boylece kayitlardaki olusturan/guncelleyen izleri anlamli kalir.
        kullanici.Aktif = false;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Hata)> AktifYapAsync(int id)
    {
        var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id == id);
        if (kullanici == null)
            return (false, "Kayıt bulunamadı.");

        kullanici.Aktif = true;
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
