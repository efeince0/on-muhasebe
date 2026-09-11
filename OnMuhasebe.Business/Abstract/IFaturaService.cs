using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.ViewModels;

namespace OnMuhasebe.Business.Abstract;

public interface IFaturaService
{
    /// <summary>Fatura listesi: tur, cari, tarih araligi ve tutar suzgecleriyle.</summary>
    Task<FaturaListeSonucu> ListeleAsync(
        FaturaTipi? faturaTipi = null,
        int? cariId = null,
        DateTime? baslangic = null,
        DateTime? bitis = null,
        decimal? minTutar = null,
        decimal? maxTutar = null,
        bool sadeceAktif = true,
        int sayfaNo = 1,
        int sayfaBoyutu = 20,
        string sirala = "tarih",
        string yon = "desc");

    /// <summary>Guncelleme formunu doldurmak icin; satirlariyla birlikte. Bulunamazsa null.</summary>
    Task<FaturaFormViewModel?> FormGetirAsync(int id);

    /// <summary>Tipe gore bir sonraki bos fatura numarasini onerir (ALF / SAT).</summary>
    Task<string> SonrakiFaturaNoOnerAsync(FaturaTipi faturaTipi);

    /// <summary>
    /// Id = 0 ise ekler, degilse gunceller. Guncellemede eski satirlar silinip
    /// form verisinden yeniden yazilir; toplamlar her zaman sunucuda hesaplanir.
    /// </summary>
    Task<(bool Basarili, string? Hata)> KaydetAsync(FaturaFormViewModel model);

    /// <summary>
    /// Kalici silmez; Aktif = false yapar. Bakiye ve stok miktari yalnizca
    /// aktif faturalari saydigi icin hareketler kendiliginden geri alinir.
    /// </summary>
    Task<(bool Basarili, string? Hata)> PasifeAlAsync(int id);

    Task<(bool Basarili, string? Mesaj)> AktifYapAsync(int id);
}
