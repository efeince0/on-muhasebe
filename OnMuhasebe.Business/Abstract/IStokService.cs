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

    /// <summary>Acilir listeler icin aktif stok kartlarinin kod + ad + birim listesi.</summary>
    Task<List<StokSecimViewModel>> SecimListesiAsync();

    /// <summary>
    /// Bir kartin mevcut miktari. haricHareketId verilirse o hareket sayilmaz;
    /// mevcut bir hareketi guncellerken "kendisi haric" miktara bakmak icin.
    /// </summary>
    Task<decimal> MiktarGetirAsync(int stokId, int? haricHareketId = null);

    Task<(bool Basarili, string? Hata)> KaydetAsync(StokFormViewModel model);

    /// <summary>Kalici silmez; Aktif = false yapar. Stok varsa engellemez, mesajla bildirir.</summary>
    Task<(bool Basarili, string? Mesaj)> PasifeAlAsync(int id);

    Task<(bool Basarili, string? Hata)> AktifYapAsync(int id);
}
