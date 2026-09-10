namespace OnMuhasebe.Entities.ViewModels;

/// <summary>
/// Cari hesap ekstresi: secilen tarih araligindaki borc/alacak hareketleri
/// ve her satirdan sonraki yuruyen bakiye.
/// </summary>
public class EkstreViewModel
{
    public int    CariId   { get; set; }
    public string CariKodu { get; set; } = null!;
    public string Unvan    { get; set; } = null!;

    public DateTime? Baslangic { get; set; }
    public DateTime? Bitis     { get; set; }

    /// <summary>Acilis bakiyesi + donem basindan onceki tum hareketler.</summary>
    public decimal DevirBakiye { get; set; }

    public List<EkstreSatirViewModel> Satirlar { get; set; } = [];

    public decimal ToplamBorc     => Satirlar.Sum(s => s.Borc);
    public decimal ToplamAlacak   => Satirlar.Sum(s => s.Alacak);
    public decimal KapanisBakiye  => DevirBakiye + ToplamBorc - ToplamAlacak;
}

public class EkstreSatirViewModel
{
    public DateTime Tarih    { get; set; }
    public string   Belge    { get; set; } = null!;
    public string   Tur      { get; set; } = null!;
    public string?  Aciklama { get; set; }
    public decimal  Borc     { get; set; }
    public decimal  Alacak   { get; set; }

    /// <summary>Satir islendikten sonraki bakiye; serviste doldurulur.</summary>
    public decimal YuruyenBakiye { get; set; }
}
