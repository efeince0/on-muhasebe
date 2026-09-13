using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Fatura detay ve yazdirma ekrani: ust bilgi, cari kimligi, satirlar ve toplamlar.</summary>
public class FaturaDetayViewModel
{
    public int        Id          { get; set; }
    public string     FaturaNo    { get; set; } = null!;
    public FaturaTipi FaturaTipi  { get; set; }
    public DateTime   Tarih       { get; set; }
    public string?    Aciklama    { get; set; }
    public bool       Aktif       { get; set; }

    public int     CariId       { get; set; }
    public string  CariKodu     { get; set; } = null!;
    public string  Unvan        { get; set; } = null!;
    public string? VergiDairesi { get; set; }
    public string? VergiNo      { get; set; }
    public string? Telefon      { get; set; }
    public string? Adres        { get; set; }

    public decimal AraToplam   { get; set; }
    public decimal ToplamKdv   { get; set; }
    public decimal GenelToplam { get; set; }

    public List<FaturaDetaySatirViewModel> Satirlar { get; set; } = [];

    // Dokuman 2.1: kayitlarda olusturan/guncelleyen kullanici ve tarih tutulmali.
    // Fatura guncellenebildigi icin bu iz ekranda gorunur olmali.
    public DateTime  OlusturmaTarihi  { get; set; }
    public string?   OlusturanAdi     { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
    public string?   GuncelleyenAdi   { get; set; }
}

public class FaturaDetaySatirViewModel
{
    public string  StokKodu    { get; set; } = null!;
    public string  StokAdi     { get; set; } = null!;
    public string  Birim       { get; set; } = null!;
    public decimal Miktar      { get; set; }
    public decimal BirimFiyat  { get; set; }
    public decimal KdvOrani    { get; set; }
    public decimal SatirTutari { get; set; }

    public decimal KdvTutari   => SatirTutari * KdvOrani / 100;
    public decimal ToplamTutar => SatirTutari + KdvTutari;
}
