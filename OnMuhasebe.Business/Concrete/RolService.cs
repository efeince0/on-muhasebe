using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.DataAccess.Context;
using OnMuhasebe.Entities.Tables;
using OnMuhasebe.Entities.ViewModels;
using System.Security.Claims;

namespace OnMuhasebe.Business.Concrete;

public class RolService : IRolService
{
    private readonly OnMuhasebeContext _context;

    /// <summary>Matriste gosterilecek modul sirasi ve okunabilir adlari.</summary>
    private static readonly (Modul Modul, string Ad)[] Moduller =
    [
        (Modul.Cari,        "Cari"),
        (Modul.CariIslem,   "Cari İşlemleri (Tahsilat/Ödeme)"),
        (Modul.Stok,        "Stok"),
        (Modul.StokHareket, "Stok İşlemleri (Giriş/Çıkış/Sayım)"),
        (Modul.Fatura,      "Fatura"),
        (Modul.Kullanici,   "Kullanıcı Yönetimi"),
        (Modul.Rol,         "Rol ve İzin Yönetimi")
    ];

    public RolService(OnMuhasebeContext context, IHttpContextAccessor erisim)
    {
        _context = context;

        var idMetni = erisim.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idMetni, out var kullaniciId))
            _context.AktifKullaniciId = kullaniciId;
    }

    public async Task<List<RolListeViewModel>> ListeleAsync(bool sadeceAktif = true)
    {
        var sorgu = _context.Roller.AsNoTracking().AsQueryable();

        if (sadeceAktif)
            sorgu = sorgu.Where(r => r.Aktif);

        var roller = await sorgu
            .OrderBy(r => r.RolAdi)
            .Select(r => new RolListeViewModel
            {
                Id              = r.Id,
                RolAdi          = r.RolAdi,
                Aciklama        = r.Aciklama,
                KullaniciSayisi = r.Kullanicilar.Count(k => k.Aktif),
                IzinSayisi      = r.Izinler.Count(i => i.Aktif && i.IzinVar),
                Aktif           = r.Aktif
            })
            .ToListAsync();

        foreach (var rol in roller)
            rol.YonetimKaynagi = !await BaskaYonetimKaynagiVarMi(rol.Id) && rol.KullaniciSayisi > 0;

        return roller;
    }

    /// <summary>
    /// Verilen rol disinda, kullanici yonetme yetkisini fiilen saglayan
    /// baska bir kaynak var mi: aktif rol + aktif kullanici + Kullanici.Guncelle izni.
    /// </summary>
    private async Task<bool> BaskaYonetimKaynagiVarMi(int haricRolId)
    {
        return await _context.Kullanicilar.AnyAsync(k =>
            k.Aktif &&
            k.RolId != haricRolId &&
            k.Rol.Aktif &&
            k.Rol.Izinler.Any(i => i.Aktif && i.IzinVar &&
                                   i.Modul == Modul.Kullanici &&
                                   i.Islem == Islem.Guncelle));
    }

    /// <summary>
    /// Dokuman [352]: sistem her zaman yonetilebilir kalmali. Degisiklikten sonra
    /// kullanici yonetebilen aktif bir hesap kalmiyorsa false doner.
    /// </summary>
    private async Task<bool> YonetimSurdurulebilirMi(int rolId, bool yeniAktif, bool yeniYetkiVar)
    {
        if (await BaskaYonetimKaynagiVarMi(rolId))
            return true;

        // Tek kaynak bu rol olabilir; ama rolun aktif kullanicisi yoksa
        // zaten bir sey kaybedilmiyor demektir.
        var aktifKullanicisiVar = await _context.Kullanicilar
            .AnyAsync(k => k.Aktif && k.RolId == rolId);

        if (!aktifKullanicisiVar)
            return true;

        return yeniAktif && yeniYetkiVar;
    }

    public async Task<RolFormViewModel?> FormGetirAsync(int id)
    {
        return await _context.Roller
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RolFormViewModel
            {
                Id       = r.Id,
                RolAdi   = r.RolAdi,
                Aciklama = r.Aciklama
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(bool Basarili, string? Hata)> KaydetAsync(RolFormViewModel model)
    {
        var rolAdi = model.RolAdi.Trim();

        var adKullanimda = await _context.Roller
            .AnyAsync(r => r.RolAdi == rolAdi && r.Id != model.Id);

        if (adKullanimda)
            return (false, "Bu rol adı başka bir rolde kullanılıyor.");

        if (model.Id == 0)
        {
            // Yeni rol izinsiz baslar; yetkiler matris ekranindan verilir.
            _context.Roller.Add(new Rol
            {
                RolAdi   = rolAdi,
                Aciklama = model.Aciklama?.Trim()
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        var mevcut = await _context.Roller.FirstOrDefaultAsync(r => r.Id == model.Id);
        if (mevcut == null)
            return (false, "Kayıt bulunamadı.");

        var yetkiVar = await _context.RolIzinleri.AnyAsync(i =>
            i.RolId == model.Id && i.Aktif && i.IzinVar &&
            i.Modul == Modul.Kullanici && i.Islem == Islem.Guncelle);

        if (!await YonetimSurdurulebilirMi(model.Id, mevcut.Aktif, yetkiVar))
            return (false, "Bu rol, kullanıcı yönetebilen tek kaynak. Pasife alınamaz. Önce başka bir yönetici rolü tanımlayın.");

        mevcut.RolAdi   = rolAdi;
        mevcut.Aciklama = model.Aciklama?.Trim();

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<YetkiMatrisiViewModel?> MatrisGetirAsync(int rolId)
    {
        var rol = await _context.Roller
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == rolId);

        if (rol == null)
            return null;

        // Yalnizca IZIN VERILEN kombinasyonlar tabloda tutuluyor;
        // kayit yoksa izin yok demektir.
        var izinler = await _context.RolIzinleri
            .AsNoTracking()
            .Where(i => i.RolId == rolId && i.Aktif && i.IzinVar)
            .Select(i => new { i.Modul, i.Islem })
            .ToListAsync();

        bool Var(Modul modul, Islem islem) =>
            izinler.Any(i => i.Modul == modul && i.Islem == islem);

        return new YetkiMatrisiViewModel
        {
            RolId    = rol.Id,
            RolAdi   = rol.RolAdi,
            Satirlar = Moduller.Select(m => new MatrisSatirViewModel
            {
                Modul     = m.Modul,
                ModulAdi  = m.Ad,
                Goruntule = Var(m.Modul, Islem.Goruntule),
                Ekle      = Var(m.Modul, Islem.Ekle),
                Guncelle  = Var(m.Modul, Islem.Guncelle),
                Sil       = Var(m.Modul, Islem.Sil)
            }).ToList()
        };
    }

    public async Task<(bool Basarili, string? Hata)> MatrisKaydetAsync(YetkiMatrisiViewModel model)
    {
        var rol = await _context.Roller.FirstOrDefaultAsync(r => r.Id == model.RolId);
        if (rol == null)
            return (false, "Kayıt bulunamadı.");

        // Formdan gelen satirlar tekrarli ya da tanimsiz bir modul tasiyabilir.
        // Tekrar, RolIzinleri'ndeki (RolId, Modul, Islem) benzersiz indeksini
        // ihlal edip 500 dondururdu. Bilinen modullere indirgenip her modul
        // bir kez aliniyor.
        var satirlar = model.Satirlar
            .Where(s => Enum.IsDefined(s.Modul))
            .GroupBy(s => s.Modul)
            .Select(g => g.First())
            .ToList();

        var kullaniciSatiri = satirlar.FirstOrDefault(s => s.Modul == Modul.Kullanici);
        var yeniYetkiVar    = kullaniciSatiri?.Guncelle == true;

        if (!await YonetimSurdurulebilirMi(model.RolId, rol.Aktif, yeniYetkiVar))
            return (false, "Bu rol, kullanıcı yönetebilen tek kaynak. Kullanıcı Yönetimi / Güncelle iznini kaldıramazsınız.");

        // Fatura satirlarindaki desenin aynisi: eskiyi silip yeniden yaz.
        // Hangi izin eklendi, hangisi kaldirildi diye fark hesaplamaktan
        // hem daha kisa hem daha az hata acik.
        var eskiler = await _context.RolIzinleri.Where(i => i.RolId == model.RolId).ToListAsync();
        _context.RolIzinleri.RemoveRange(eskiler);

        foreach (var satir in satirlar)
        {
            void Ekle(Islem islem, bool secili)
            {
                if (!secili) return;

                _context.RolIzinleri.Add(new RolIzin
                {
                    RolId   = model.RolId,
                    Modul   = satir.Modul,
                    Islem   = islem,
                    IzinVar = true
                });
            }

            Ekle(Islem.Goruntule, satir.Goruntule);
            Ekle(Islem.Ekle,      satir.Ekle);
            Ekle(Islem.Guncelle,  satir.Guncelle);
            Ekle(Islem.Sil,       satir.Sil);
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Basarili, string? Mesaj)> PasifeAlAsync(int id)
    {
        var rol = await _context.Roller.FirstOrDefaultAsync(r => r.Id == id);
        if (rol == null)
            return (false, "Kayıt bulunamadı.");

        if (!rol.Aktif)
            return (false, "Kayıt zaten pasif durumda.");

        if (!await YonetimSurdurulebilirMi(id, yeniAktif: false, yeniYetkiVar: false))
            return (false, "Bu rol, kullanıcı yönetebilen tek kaynak. Pasife alınamaz.");

        // Rol pasife alininca o roldeki kullanicilar menude hicbir modul goremez.
        var kullaniciSayisi = await _context.Kullanicilar.CountAsync(k => k.Aktif && k.RolId == id);

        rol.Aktif = false;
        await _context.SaveChangesAsync();

        return kullaniciSayisi > 0
            ? (true, $"Rol pasife alındı. Dikkat: bu roldeki {kullaniciSayisi} aktif kullanıcı artık hiçbir modüle erişemez.")
            : (true, null);
    }

    public async Task<(bool Basarili, string? Hata)> AktifYapAsync(int id)
    {
        var rol = await _context.Roller.FirstOrDefaultAsync(r => r.Id == id);
        if (rol == null)
            return (false, "Kayıt bulunamadı.");

        rol.Aktif = true;
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
