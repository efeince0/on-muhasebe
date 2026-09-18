using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

public class StokDetayViewModel
{
    public int     Id       { get; set; }
    public string  StokKodu { get; set; } = null!;
    public string  StokAdi  { get; set; } = null!;
    public string? Kategori { get; set; }
    public string  Birim    { get; set; } = null!;
    public bool    Aktif    { get; set; }

    public decimal  AlisFiyati  { get; set; }
    public decimal  SatisFiyati { get; set; }
    public decimal  KdvOrani    { get; set; }
    public decimal? KritikStok  { get; set; }

    public decimal ToplamGiris { get; set; }
    public decimal ToplamCikis { get; set; }
    public decimal ToplamSayim { get; set; }

    /// <summary>Alis faturalarindan gelen miktar.</summary>
    public decimal ToplamFaturaGiris { get; set; }

    /// <summary>Satis faturalariyla cikan miktar.</summary>
    public decimal ToplamFaturaCikis { get; set; }

    public decimal MevcutMiktar =>
        ToplamGiris - ToplamCikis + ToplamSayim + ToplamFaturaGiris - ToplamFaturaCikis;

    /// <summary>Elde kalan malin alis maliyeti uzerinden degeri.</summary>
    public decimal StokDegeri => MevcutMiktar * AlisFiyati;

    public decimal KdvDahilSatis => SatisFiyati * (1 + KdvOrani / 100);

    /// <summary>Alis fiyati sifirsa marj hesaplanamaz; null doner.</summary>
    public decimal? KarMarjiYuzde =>
        AlisFiyati > 0 ? (SatisFiyati - AlisFiyati) / AlisFiyati * 100 : null;

    public bool KritikSeviyede => KritikStok.HasValue && MevcutMiktar <= KritikStok.Value;

    /// <summary>Elle girilen hareketler + fatura satirlari.</summary>
    public int HareketSayisi { get; set; }

    /// <summary>
    /// Urun hareket dokumu: stok hareketleri ve fatura satirlari tek zaman
    /// cizgisinde, her satirdan sonraki miktarla birlikte. Cari ekstresinin
    /// stok karsiligi.
    /// </summary>
    public List<StokHareketSatirViewModel> SonHareketler { get; set; } = [];

    public DateTime  OlusturmaTarihi  { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
}

/// <summary>
/// Hareket dokumundeki bir satir. Kaynak elle girilen bir stok hareketi de
/// olabilir, bir fatura satiri da; ikisi de miktari ayni sekilde etkiler.
/// </summary>
public class StokHareketSatirViewModel
{
    public DateTime Tarih    { get; set; }

    /// <summary>Hareket numarasi veya fatura numarasi.</summary>
    public string   Belge    { get; set; } = null!;

    /// <summary>Giriş / Çıkış / Sayım / Alış Faturası / Satış Faturası.</summary>
    public string   Tur      { get; set; } = null!;

    public string?  Aciklama { get; set; }

    /// <summary>Miktara etkisi; isaretli. Sayim farki eksi de olabilir.</summary>
    public decimal  Degisim  { get; set; }

    /// <summary>Satir islendikten sonraki miktar; serviste doldurulur.</summary>
    public decimal  YuruyenMiktar { get; set; }

    public decimal Giris => Degisim > 0 ?  Degisim : 0;
    public decimal Cikis => Degisim < 0 ? -Degisim : 0;
}
