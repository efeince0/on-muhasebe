using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.ViewModels;

namespace OnMuhasebe.Business.Abstract;

public interface ICariIslemService
{
    /// <summary>Tahsilat/odeme listesi: cari, tarih araligi ve tip suzgecleriyle.</summary>
    Task<CariIslemListeSonucu> ListeleAsync(
        int? cariId = null,
        DateTime? baslangic = null,
        DateTime? bitis = null,
        IslemTipi? islemTipi = null,
        bool sadeceAktif = true,
        int sayfaNo = 1,
        int sayfaBoyutu = 20,
        string sirala = "tarih",
        string yon = "desc");

    /// <summary>Guncelleme formunu doldurmak icin. Bulunamazsa null.</summary>
    Task<CariIslemFormViewModel?> FormGetirAsync(int id);

    /// <summary>Tipe gore bir sonraki bos makbuz numarasini onerir (TAH0001 / ODE0001).</summary>
    Task<string> SonrakiIslemNoOnerAsync(IslemTipi islemTipi);

    /// <summary>Id = 0 ise ekler, degilse gunceller.</summary>
    Task<(bool Basarili, string? Hata)> KaydetAsync(CariIslemFormViewModel model);

    /// <summary>Kalici silmez; Aktif = false yapar. Bakiye pasif kayitlari saymaz.</summary>
    Task<(bool Basarili, string? Hata)> PasifeAlAsync(int id);

    Task<(bool Basarili, string? Hata)> AktifYapAsync(int id);

    /// <summary>Cari hesap ekstresi. Cari bulunamazsa null.</summary>
    Task<EkstreViewModel?> EkstreGetirAsync(int cariId, DateTime? baslangic = null, DateTime? bitis = null);
}
