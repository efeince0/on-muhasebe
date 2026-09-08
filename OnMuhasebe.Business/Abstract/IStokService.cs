using OnMuhasebe.Entities.ViewModels;

namespace OnMuhasebe.Business.Abstract;

public interface IStokService
{
    /// <summary>Liste ekrani icin suzulmus, siralanmis ve sayfalanmis sonuc.</summary>
    Task<StokListeSonucu> ListeleAsync(
        string? arama = null,
        bool sadeceAktif = true,
        bool sadeceKritik = false,
        int sayfaNo = 1,
        int sayfaBoyutu = 20,
        string sirala = "kod",
        string yon = "asc");

    Task<StokFormViewModel?> FormGetirAsync(int id);

    Task<StokDetayViewModel?> DetayGetirAsync(int id, int sonHareketSayisi = 10);

    /// <summary>Form ekraninda kategori onerisi icin mevcut kategoriler.</summary>
    Task<List<string>> KategorileriGetirAsync();

    /// <summary>Yeni kayit icin bir sonraki bos kodu onerir. Garanti degil, yalnizca oneri.</summary>
    Task<string> SonrakiKodOnerAsync(string onEk = "S");

    Task<(bool Basarili, string? Hata)> KaydetAsync(StokFormViewModel model);

    /// <summary>Kalici silmez; Aktif = false yapar. Stogu sifir degilse reddeder.</summary>
    Task<(bool Basarili, string? Hata)> PasifeAlAsync(int id);

    Task<(bool Basarili, string? Hata)> AktifYapAsync(int id);
}
