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
            .Include(k => k.Rol)
                .ThenInclude(r => r.Izinler)
            // Rolu pasife alinmis kullanici da giris yapamaz: RolService pasife
            // alirken "bu roldeki kullanicilar giris yapamayacak" diye uyariyor,
            // kural burada uygulaniyor.
            .FirstOrDefaultAsync(k =>
                k.KullaniciAdi == kullaniciAdi && k.Aktif && k.Rol.Aktif);

        if (kullanici == null)
            return null;

        var sonuc = new PasswordHasher<Kullanici>()
            .VerifyHashedPassword(kullanici, kullanici.SifreHash, sifre);

        // Kullanici yoksa da sifre yanlissa da ayni cevap doner.
        if (sonuc == PasswordVerificationResult.Failed)
            return null;

        kullanici.SonGiris = DateTime.Now;
        await _context.SaveChangesAsync();

        return kullanici;
    }
}
