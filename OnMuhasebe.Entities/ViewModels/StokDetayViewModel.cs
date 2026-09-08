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

    public decimal MevcutMiktar => ToplamGiris - ToplamCikis + ToplamSayim;

    /// <summary>Elde kalan malin alis maliyeti uzerinden degeri.</summary>
    public decimal StokDegeri => MevcutMiktar * AlisFiyati;

    public decimal KdvDahilSatis => SatisFiyati * (1 + KdvOrani / 100);

    /// <summary>Alis fiyati sifirsa marj hesaplanamaz; null doner.</summary>
    public decimal? KarMarjiYuzde =>
        AlisFiyati > 0 ? (SatisFiyati - AlisFiyati) / AlisFiyati * 100 : null;

    public bool KritikSeviyede => KritikStok.HasValue && MevcutMiktar <= KritikStok.Value;

    public int HareketSayisi { get; set; }
    public List<StokHareketSatirViewModel> SonHareketler { get; set; } = [];

    public DateTime  OlusturmaTarihi  { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
}

public class StokHareketSatirViewModel
{
    public int             Id          { get; set; }
    public string          HareketNo   { get; set; } = null!;
    public DateTime        Tarih       { get; set; }
    public StokHareketTipi HareketTipi { get; set; }
    public decimal         Miktar      { get; set; }
    public string?         Aciklama    { get; set; }

    /// <summary>Faturadan doğan hareket mi, elle mi girildi.</summary>
    public bool FaturadanMi { get; set; }
}
