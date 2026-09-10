using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.ViewModels;

namespace OnMuhasebe.Business.Abstract;

public interface IStokHareketService
{
    /// <summary>Hareket listesi: urun, tarih araligi ve tip suzgecleriyle.</summary>
    Task<StokHareketListeSonucu> ListeleAsync(
        int? stokId = null,
        DateTime? baslangic = null,
        DateTime? bitis = null,
        StokHareketTipi? hareketTipi = null,
        bool sadeceAktif = true,
        int sayfaNo = 1,
        int sayfaBoyutu = 20,
        string sirala = "tarih",
        string yon = "desc");

    /// <summary>
    /// Guncelleme formunu doldurmak icin. Bulunamazsa null.
    /// Sayim kayitlarinda saklanan fark degil, o hareketin isaret ettigi
    /// sayilan miktar geri verilir; kullanici ne girdiyse onu gorur.
    /// </summary>
    Task<StokHareketFormViewModel?> FormGetirAsync(int id);

    /// <summary>Tipe gore bir sonraki bos hareket numarasini onerir (GIR/CIK/SAY).</summary>
    Task<string> SonrakiHareketNoOnerAsync(StokHareketTipi hareketTipi);

    /// <summary>Id = 0 ise ekler, degilse gunceller.</summary>
    Task<(bool Basarili, string? Hata)> KaydetAsync(StokHareketFormViewModel model);

    /// <summary>
    /// Kalici silmez; Aktif = false yapar. Miktar pasif kayitlari saymaz,
    /// yani hareket kaldirilinca stok kendiliginden eski haline doner.
    /// Sonuc negatif kaliyorsa engellemez, mesajla bildirir.
    /// </summary>
    Task<(bool Basarili, string? Mesaj)> PasifeAlAsync(int id);

    Task<(bool Basarili, string? Hata)> AktifYapAsync(int id);
}
