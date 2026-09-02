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

    public async Task<List<CariListeViewModel>> ListeleAsync(string? arama = null, bool sadeceAktif = true)
    {
        var sorgu = _context.Cariler.AsNoTracking().AsQueryable();

        if (sadeceAktif)
            sorgu = sorgu.Where(c => c.Aktif);

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var a = arama.Trim();
            sorgu = sorgu.Where(c => c.CariKodu.Contains(a) || c.Unvan.Contains(a));
        }

        return await sorgu
            .OrderBy(c => c.CariKodu)
            .Select(c => new CariListeViewModel
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
            })
            .ToListAsync();
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

    public async Task<(bool Basarili, string? Hata)> KaydetAsync(CariFormViewModel model)
    {
        var kodKullanimda = await _context.Cariler
            .AnyAsync(c => c.CariKodu == model.CariKodu && c.Id != model.Id);

        if (kodKullanimda)
            return (false, "Bu cari kodu başka bir kayıtta kullanılıyor.");

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

    public async Task<(bool Basarili, string? Hata)> PasifeAlAsync(int id)
    {
        var cari = await _context.Cariler.FirstOrDefaultAsync(c => c.Id == id);
        if (cari == null)
            return (false, "Kayıt bulunamadı.");

        if (!cari.Aktif)
            return (false, "Kayıt zaten pasif durumda.");

        // Kayitlar kalici silinmez; gecmis hareketlerin bagli oldugu cari korunur.
        cari.Aktif = false;
        await _context.SaveChangesAsync();
        return (true, null);
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
}
