using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.Business.Abstract;

public interface IKimlikService
{
    //dorguysa kullanıcı döner , yanlışsa null
    Task<Kullanici?> GirisDogrulaAsync(string kullaniciAdi, string sifre);
    
}