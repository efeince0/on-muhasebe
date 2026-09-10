using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Tahsilat/odeme listesindeki bir tablo satiri.</summary>
public class CariIslemListeViewModel
{
    public int        Id         { get; set; }
    public string     IslemNo    { get; set; } = null!;
    public DateTime   Tarih      { get; set; }
    public int        CariId     { get; set; }
    public string     CariKodu   { get; set; } = null!;
    public string     Unvan      { get; set; } = null!;
    public IslemTipi  IslemTipi  { get; set; }
    public decimal    Tutar      { get; set; }
    public OdemeSekli OdemeSekli { get; set; }
    public string?    Aciklama   { get; set; }
    public bool       Aktif      { get; set; }
}

/// <summary>Liste sonucu + ekranin ustunde gosterilen donem toplamlari.</summary>
public class CariIslemListeSonucu : SayfaliListe<CariIslemListeViewModel>
{
    public decimal ToplamTahsilat { get; set; }
    public decimal ToplamOdeme    { get; set; }
}
