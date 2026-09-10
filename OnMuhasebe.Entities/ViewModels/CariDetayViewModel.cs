using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

public class CariDetayViewModel
{
    public int      Id           { get; set; }
    public string   CariKodu     { get; set; } = null!;
    public string   Unvan        { get; set; } = null!;
    public CariTipi CariTipi     { get; set; }
    public string?  VergiDairesi { get; set; }
    public string?  VergiNo      { get; set; }
    public string?  Telefon      { get; set; }
    public string?  Eposta       { get; set; }
    public string?  Adres        { get; set; }
    public bool     Aktif        { get; set; }

    public decimal AcilisBakiye { get; set; }
    public decimal ToplamBorc   { get; set; }
    public decimal ToplamAlacak { get; set; }
    public decimal GuncelBakiye => AcilisBakiye + ToplamBorc - ToplamAlacak;

    public int HareketSayisi { get; set; }
    public List<CariHareketSatirViewModel> SonHareketler { get; set; } = [];

    public DateTime  OlusturmaTarihi  { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
}

public class CariHareketSatirViewModel
{
    public int        Id         { get; set; }
    public string     IslemNo    { get; set; } = null!;
    public DateTime   Tarih      { get; set; }
    public IslemTipi  IslemTipi  { get; set; }
    public decimal    Tutar      { get; set; }
    public OdemeSekli OdemeSekli { get; set; }
    public string?    Aciklama   { get; set; }
}
