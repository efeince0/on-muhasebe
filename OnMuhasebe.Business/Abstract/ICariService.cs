using OnMuhasebe.Entities.ViewModels;

namespace OnMuhasebe.Business.Abstract;

public interface ICariService
{
    /// <summary>Liste ekrani icin suzulmus, siralanmis ve sayfalanmis sonuc.</summary>
    Task<SayfaliListe<CariListeViewModel>> ListeleAsync(
        string? arama = null,
        bool sadeceAktif = true,
        int sayfaNo = 1,
        int sayfaBoyutu = 20,
        string sirala = "kod",
        string yon = "asc");

    /// <summary>Guncelleme formunu doldurmak icin. Bulunamazsa null.</summary>
    Task<CariFormViewModel?> FormGetirAsync(int id);

    /// <summary>Detay ekrani: kart bilgileri, bakiye ozeti ve son hareketler.</summary>
    Task<CariDetayViewModel?> DetayGetirAsync(int id, int sonHareketSayisi = 10);

    /// <summary>Yeni kayit icin bir sonraki bos kodu onerir. Garanti degil, yalnizca oneri.</summary>
    Task<string> SonrakiKodOnerAsync(string onEk = "C");

    /// <summary>Id = 0 ise ekler, degilse gunceller.</summary>
    Task<(bool Basarili, string? Hata)> KaydetAsync(CariFormViewModel model);

    /// <summary>Kalici silmez; Aktif = false yapar. Bakiye varsa engellemez, mesajla bildirir.</summary>
    Task<(bool Basarili, string? Mesaj)> PasifeAlAsync(int id);

    Task<(bool Basarili, string? Hata)> AktifYapAsync(int id);
}
