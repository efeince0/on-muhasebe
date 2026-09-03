using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.Business.Abstract;

public interface IKimlikService
{
    /// <summary>Doğruysa kullanıcıyı döner, kullanıcı yoksa veya şifre yanlışsa null.</summary>
    Task<Kullanici?> GirisDogrulaAsync(string kullaniciAdi, string sifre);
    
}