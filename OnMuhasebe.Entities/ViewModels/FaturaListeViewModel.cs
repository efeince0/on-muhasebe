using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Fatura listesindeki bir tablo satiri.</summary>
public class FaturaListeViewModel
{
    public int        Id          { get; set; }
    public string     FaturaNo    { get; set; } = null!;
    public FaturaTipi FaturaTipi  { get; set; }
    public DateTime   Tarih       { get; set; }
    public int        CariId      { get; set; }
    public string     CariKodu    { get; set; } = null!;
    public string     Unvan       { get; set; } = null!;
    public decimal    AraToplam   { get; set; }
    public decimal    ToplamKdv   { get; set; }
    public decimal    GenelToplam { get; set; }
    public int        SatirSayisi { get; set; }
    public bool       Aktif       { get; set; }
}

/// <summary>Liste sonucu + ekranin ustunde gosterilen donem toplamlari.</summary>
public class FaturaListeSonucu : SayfaliListe<FaturaListeViewModel>
{
    public decimal ToplamAlis  { get; set; }
    public decimal ToplamSatis { get; set; }
}
