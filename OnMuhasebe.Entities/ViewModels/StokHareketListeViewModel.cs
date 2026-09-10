using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Stok hareketleri listesindeki bir tablo satiri.</summary>
public class StokHareketListeViewModel
{
    public int             Id          { get; set; }
    public string          HareketNo   { get; set; } = null!;
    public DateTime        Tarih       { get; set; }
    public int             StokId      { get; set; }
    public string          StokKodu    { get; set; } = null!;
    public string          StokAdi     { get; set; } = null!;
    public string          Birim       { get; set; } = null!;
    public StokHareketTipi HareketTipi { get; set; }
    public decimal         Miktar      { get; set; }
    public string?         Aciklama    { get; set; }
    public bool            Aktif       { get; set; }
}

/// <summary>Liste sonucu + ekranin ustunde gosterilen donem toplamlari.</summary>
public class StokHareketListeSonucu : SayfaliListe<StokHareketListeViewModel>
{
    public decimal ToplamGiris { get; set; }
    public decimal ToplamCikis { get; set; }

    /// <summary>Sayim farklarinin net toplami; eksi olabilir.</summary>
    public decimal ToplamSayim { get; set; }
}
