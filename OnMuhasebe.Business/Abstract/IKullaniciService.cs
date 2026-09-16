using OnMuhasebe.Entities.ViewModels;

namespace OnMuhasebe.Business.Abstract;

public interface IKullaniciService
{
    /// <summary>Kullanici listesi; her satirda "son yonetici mi" bilgisiyle.</summary>
    Task<SayfaliListe<KullaniciListeViewModel>> ListeleAsync(
        string? arama = null,
        bool sadeceAktif = true,
        int sayfaNo = 1,
        int sayfaBoyutu = 20,
        string sirala = "kullaniciAdi",
        string yon = "asc");

    Task<KullaniciFormViewModel?> FormGetirAsync(int id);

    /// <summary>Sifirlama ekranini doldurmak icin kimlik bilgisi. Bulunamazsa null.</summary>
    Task<SifreSifirlaViewModel?> SifreFormuGetirAsync(int id);

    /// <summary>Acilir listeler icin aktif roller.</summary>
    Task<List<RolSecimViewModel>> RolSecimListesiAsync();

    /// <summary>Id = 0 ise ekler (sifre zorunlu), degilse gunceller (sifreye dokunmaz).</summary>
    Task<(bool Basarili, string? Hata)> KaydetAsync(KullaniciFormViewModel model);

    /// <summary>Yonetici baskasinin sifresini sifirlar; eski sifre sorulmaz.</summary>
    Task<(bool Basarili, string? Hata)> SifreSifirlaAsync(SifreSifirlaViewModel model);

    /// <summary>Kullanici kendi sifresini degistirir; mevcut sifre dogrulanir.</summary>
    Task<(bool Basarili, string? Hata)> SifreDegistirAsync(int kullaniciId, SifreDegistirViewModel model);

    /// <summary>
    /// Kalici silmez; Aktif = false yapar. Dokuman [352]: kullanici yonetebilen
    /// son aktif hesap pasife alinamaz.
    /// </summary>
    Task<(bool Basarili, string? Hata)> PasifeAlAsync(int id);

    Task<(bool Basarili, string? Hata)> AktifYapAsync(int id);
}
