using OnMuhasebe.Entities.ViewModels;

namespace OnMuhasebe.Business.Abstract;

public interface IRolService
{
    Task<List<RolListeViewModel>> ListeleAsync(bool sadeceAktif = true);

    Task<RolFormViewModel?> FormGetirAsync(int id);

    /// <summary>Id = 0 ise ekler, degilse gunceller.</summary>
    Task<(bool Basarili, string? Hata)> KaydetAsync(RolFormViewModel model);

    /// <summary>Rolun yetki matrisi; izni olmayan kombinasyonlar isaretsiz gelir.</summary>
    Task<YetkiMatrisiViewModel?> MatrisGetirAsync(int rolId);

    /// <summary>
    /// Matrisi bastan yazar. Sistem kullanici yonetebilen son kaynagini
    /// kaybedecekse degisiklik reddedilir.
    /// </summary>
    Task<(bool Basarili, string? Hata)> MatrisKaydetAsync(YetkiMatrisiViewModel model);

    /// <summary>
    /// Kalici silmez; Aktif = false yapar. Rolde aktif kullanici varsa
    /// engellemez ama sonucu mesajla bildirir.
    /// </summary>
    Task<(bool Basarili, string? Mesaj)> PasifeAlAsync(int id);

    Task<(bool Basarili, string? Hata)> AktifYapAsync(int id);
}
