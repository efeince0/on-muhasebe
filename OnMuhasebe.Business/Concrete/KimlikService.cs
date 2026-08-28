using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.DataAccess.Context;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.Business.Concrete;

public class KimlikService : IKimlikService
{
    private readonly OnMuhasebeContext _context;

    public KimlikService(OnMuhasebeContext context)
    {
        _context = context;
    }

    public async Task<Kullanici?> GirisDogrulaAsync(string kullaniciAdi, string sifre)
    {
        var kullanici = await _context.Kullanicilar
            .Include(k => k.Rol)                     // rolunu getir
                .ThenInclude(r => r.Izinler)         // ve o rolun izin satirlarini
            .FirstOrDefaultAsync( k => 
            k.KullaniciAdi== kullaniciAdi
            && k.Aktif);
        if(kullanici==null)
            return null;
        
        var sonuc = new PasswordHasher<Kullanici>()
            .VerifyHashedPassword(
                kullanici,
                kullanici.SifreHash,
                sifre);
        if(sonuc== PasswordVerificationResult.Failed)
            return null;
        
        return kullanici;

    }
}
